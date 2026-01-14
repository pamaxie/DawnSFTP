using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using dawn.ftp.ui.Database;
using dawn.ftp.ui.Extensions;
using dawn.ftp.ui.Models.FileModels;
using dawn.ftp.ui.Models.FileModels.Interfaces;
using dawn.ftp.ui.Models.TransferModels;
using dawn.ftp.ui.Models.TransferModels.Interfaces;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace dawn.ftp.ui.BusinessLogic
{
    internal class FileTransfer : ObservableObject
    {
        private static Dictionary<long, bool> LockList { get; } = new Dictionary<long, bool>();
        
        
        /// <summary>
        /// This starts a File Transfer in the direction that's set by <see cref="transferType"/>
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destinationPath"></param>
        /// <param name="transferType"></param>
        /// <param name="token"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task<Tuple<bool, string>> StartFileTransferAsync(IFileSystemObject sourceFile, string destinationPath, FileTransferType transferType, CancellationToken token, ISftpClient client)
        {
            if (!client.IsConnected) {
                client.Connect();
            }

            var sourcePath = Path.Combine(sourceFile.Path, sourceFile.Name);
            
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (transferType == FileTransferType.HostToRemote) {
                var exists = File.Exists(sourcePath) || Directory.Exists(sourceFile.Path);
                if (!exists) {
                    return new Tuple<bool, string>(false, "File / Folder could not be found on local machine. " +
                                                          "Please ensure that the file system was not modified locally," +
                                                          "and if it was please refresh the file explorer");
                }
                
                var attr = File.GetAttributes(sourcePath);
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory) {
                    UploadDirectory(client, sourcePath, destinationPath);
                }
                else {
                    //We need to set a fully qualified name for our destination folders here
                    destinationPath = Path.Combine(destinationPath, sourceFile.Name);
                    UploadFile(client, sourceFile, sourcePath, destinationPath);
                }
            } else if (transferType == FileTransferType.RemoteToHost) {
                var remotePath = Path.Combine(sourceFile.Path, sourceFile.Name);
                if (!await client.ExistsAsync(remotePath, token)) {
                    return new Tuple<bool, string>(false, "The file that was trying to be downloaded did not exist on the destination file system.");
                }

                var sftpFile = client.Get(remotePath);
                
                if (sftpFile.IsDirectory) {
                    DownloadDirectory(client, destinationPath, remotePath);
                }
                else {
                    //We need to set a fully qualified name for our destination folders here
                    destinationPath = Path.Combine(destinationPath, sourceFile.Name);
                    var file = client.Get(remotePath);
                    DownloadFile(client, file, destinationPath);
                }
            }

            return new Tuple<bool, string>(false, "Unknown File Transfer Type");
        }
        
        /// <summary>
        /// Recursive upload of local files
        /// </summary>
        /// <param name="client"></param>
        /// <param name="localPath"></param>
        /// <param name="remotePath"></param>
        void UploadDirectory(ISftpClient client, string localPath, string remotePath)
        {
            if (!client.Exists(remotePath)) {
                client.CreateDirectory(remotePath);
            }

            var infos = new DirectoryInfo(localPath).EnumerateFileSystemInfos();
            var folder = FolderModel.GetFolderModel(localPath);
            if (folder == null) {
                throw new Exception();
            }

            foreach (FileSystemInfo info in infos) {
                string subPath = remotePath + "/" + info.Name;

                if (info.Attributes.HasFlag(FileAttributes.Directory)) {
                    if (!client.Exists(subPath)) {
                        client.CreateDirectory(subPath);
                    }

                    UploadDirectory(client, info.FullName, subPath);
                }
                else {
                    UploadFile(client, folder, info.FullName, subPath);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="remotePath"></param>
        /// <param name="localPath"></param>
        private void DownloadDirectory(ISftpClient client, string localPath, string remotePath)
        {
            Directory.CreateDirectory(localPath);
            IEnumerable<ISftpFile> files = client.ListDirectory(remotePath);
            Parallel.ForEach(files, file => {
                if ((file.Name == ".") || (file.Name == "..")) {
                    return;
                }
                
                string sourceFilePath = remotePath + "/" + file.Name;
                string destFilePath = Path.Combine(localPath, file.Name);

                if (file.IsDirectory) {
                    //Create our directory so it exists even if it's empty
                    if (!Directory.Exists(destFilePath)) {
                        Directory.CreateDirectory(destFilePath);
                    }

                    DownloadDirectory(client, destFilePath, sourceFilePath);
                }
                else {
                    DownloadFile(client, file, destFilePath);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="file"></param>
        /// <param name="sourcePath">Fully Qualified name for the source file</param>
        /// <param name="destinationPath">Fully Qualified name for the file that should be uploaded</param>
        internal static void UploadFile(ISftpClient client, IFileSystemObject file, string sourcePath, string destinationPath) {
            if (client == null) {
                throw new ArgumentException("Client can not be null", nameof(client));
            }
            if (file == null) {
                throw new ArgumentException("File can not be null", nameof(file));
            }
            if (string.IsNullOrEmpty(sourcePath)) {
                throw new ArgumentException("The source path can not be null or empty", nameof(sourcePath));
            }
            if (string.IsNullOrEmpty(destinationPath)) {
                throw new  ArgumentException("The destination path can not be null or empty", nameof(destinationPath));
            }
            if (!sourcePath.Contains(file.Name)) {
                throw new ArgumentException("The source path is not fully qualified (does not contain the file name)", nameof(sourcePath));
            }
            
            if (!client.IsConnected) {
                client.Connect();
            }
            
            using var fileStream = new FileStream(sourcePath, FileMode.Open);
            Debug.Assert(fileStream != null, nameof(fileStream) + " != null");
            var id = Program.IdGenerator.CreateId();
            LockList.Add(id, false);
            client.UploadFile(fileStream, destinationPath, (ulong bytesUploaded) =>
            {
                var progress= GetProgress(bytesUploaded, fileStream.Length);
                UpdateUploadProgress(progress, file, id, sourcePath, bytesUploaded).GetAwaiter().GetResult();
            });
            MarkAsCompleted(id, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="file"></param>
        /// <param name="sourcePath"></param>
        /// <param name="destinationPath"></param>
        internal static void DownloadFile(ISftpClient client, ISftpFile file, string destinationPath) {
            if (client == null) {
                throw new ArgumentException("Client can not be null", nameof(client));
            }
            if (file == null) {
                throw new ArgumentException("File can not be null", nameof(file));
            }
            if (string.IsNullOrEmpty(destinationPath)) {
                throw new  ArgumentException("The destination path can not be null or empty", nameof(destinationPath));
            }
            
            if (!client.IsConnected) {
                client.Connect();
            }

            if (!File.Exists(destinationPath)) {
                File.Create(destinationPath).Dispose();
            }
            
            using var fileStream = new FileStream(destinationPath, FileMode.Open);
            Debug.Assert(fileStream != null, nameof(fileStream) + " != null");
            var id = Program.IdGenerator.CreateId();
            LockList.Add(id, false);
            var remoteFile = client.Get(file.FullName);
            client.DownloadFile(file.FullName, fileStream, (ulong bytesDownloaded) => {
                var progress = GetProgress(bytesDownloaded, remoteFile.Attributes.Size);
                UpdateDownloadProgress(progress, file, id, destinationPath, bytesDownloaded).GetAwaiter().GetResult();
            });
            MarkAsCompleted(id, CancellationToken.None);
        }
        
        /// <summary>
        /// Gets the process for an upload or download
        /// </summary>
        /// <param name="downloaded"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private static float GetProgress(ulong downloaded, long fileLength) {
            var progress = ((float)downloaded / fileLength) *100;
            return MathF.Round(progress, 2);
        }
        
        /// <summary>
        /// Updates the upload process for a file
        /// </summary>
        /// <param name="uploaded">Amount uploaded so far</param>
        /// <param name="file">The source file we are uploading</param>
        /// <param name="id"></param>
        private static async Task UpdateUploadProgress(double uploaded, IFileSystemObject file, long id, string destination, ulong uploadedSize)
        {
            if (!LockList.TryGetValue(id, out var isLocked) || isLocked) {
                return;
            }
            
            LockList[id] = true;
            var context = new SqlLiteDbContext();
            var transfer = new FileTransferModel() {
                FileTransferId = id,
                TransferStart = DateTime.Now,
                FileName = file.Name,
                Progress = (decimal)uploaded,
                Status = IFileTransferModel.FileTransferStatusEnum.Running,
                DestinationLocation = destination,
                RemoteIP = "",
                SourceLocation = "local",
                LastUpdate = DateTime.Now,
                FileSize = uploadedSize
            };
            
            var item = context.FileTransferModels.FirstOrDefault(x => x.FileTransferId == id);
            if (item != null) {
                context.Entry(item).CurrentValues.SetValues(transfer);
            }
            else {
                context.FileTransferModels.Add(transfer);
            }

            //Making sure the state hasn't changed since entry since we will get an exception that will halt the
            //progress if we collide here. We also only unlock the object once save changes is finished.
            await context.SaveChangesAsync().ContinueWith(task => { LockList[id] = false; });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="destination"></param>
        /// <param name="downloaded"></param>
        /// <param name="file"></param>\
        /// <exception cref="NotImplementedException"></exception>
        private static async Task UpdateDownloadProgress(double downloaded, ISftpFile file, long id, string destination, ulong downloadedSize)
        {
            if (!LockList.TryGetValue(id, out var isLocked) || isLocked) {
                return;
            }
            
            LockList[id] = true;
            var context = new SqlLiteDbContext();
            var transfer = new FileTransferModel {
                FileTransferId = id,
                TransferStart = DateTime.Now,
                FileName = file.Name,
                Progress = (decimal)downloaded,
                Status = IFileTransferModel.FileTransferStatusEnum.Running,
                DestinationLocation = destination,
                RemoteIP = "",
                SourceLocation = "remote",
                LastUpdate = DateTime.Now,
                FileSize = downloadedSize
            };

            var item = context.FileTransferModels.FirstOrDefault(x => x.FileTransferId == id);
            if (item != null) {
                context.Entry(item).CurrentValues.SetValues(transfer);
            }
            else {
                context.FileTransferModels.Add(transfer);
            }

            //Making sure the state hasn't changed since entry since we will get an exception that will halt the
            //progress if we collide here. We also only unlock the object once save changes is finished.
            await context.SaveChangesAsync().ContinueWith(task => { LockList[id] = false; });
        }

        /// <summary>
        /// Marks a download / upload as completed via their ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        private static void MarkAsCompleted(long id, CancellationToken token)
        {
            var dbContext = new SqlLiteDbContext();
            do
            {
                Task.Delay(100, token).GetAwaiter().GetResult();
            } while (LockList.ContainsKey(id) && LockList[id]);
            var data = dbContext.FileTransferModels.FirstOrDefault(x => x.FileTransferId == id);
            if (data == null) {
                return;
            }
            Debug.Assert(data != null, nameof(data) + " != null");
            data.Status = IFileTransferModel.FileTransferStatusEnum.Success;
            data.TransferEnd = DateTime.Now;
            data.Progress = 100;
            dbContext.SaveChangesAsync(token).GetAwaiter().GetResult();
            LockList.Remove(id);
        }

        public static string GetRemoteTempPath(SshClient client, string fileExt, bool isWindows = false) {
            if (isWindows) {
                throw new NotSupportedException("Windows is not supported yet");
            }
            
            client.ConnectSafe();
            var result = client.CreateCommand("mktemp").Execute().Replace("\n", string.Empty);
            client.CreateCommand($"rm {result}").Execute();
            
            //Create a file with the extension we desire and use that instead.
            client.CreateCommand($"touch {result}{fileExt}").Execute();
            client.CreateCommand($"chmod +x {result}{fileExt}").Execute();
            return result+fileExt;
        }

        /// <summary>
        /// Specifies the type of file Transfer, This it to make clear which direction we're transferring
        /// in and slightly overkill :)
        /// </summary>
        [Flags]
        public enum FileTransferType
        {
            RemoteToHost = 1,
            HostToRemote = 2
        }
    }
}
