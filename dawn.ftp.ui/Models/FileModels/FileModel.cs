using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using dawn.ftp.ui.BusinessLogic;
using dawn.ftp.ui.Models.FileModels.Interfaces;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace dawn.ftp.ui.Models.FileModels
{
    /// <summary>
    /// <inheritdoc cref="IFileModel"/>
    /// </summary>
    public class FileModel : FileSystemObject, IFileModel
    {
        private string? _extension;
        private string? _permissionModifiers;

        /// <summary>
        /// <inheritdoc cref="IFileModel.Extension"/>
        /// </summary>
        [DisplayName("File Type")]
        public string? Extension
        {
            get => _extension;
            set
            {
                if (value == _extension) return;
                _extension = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileModel.PermissionModifiers"/>
        /// </summary>
        [DisplayName("File Permissions")]
        public string? PermissionModifiers {
            get => _permissionModifiers;
            set
            {
                if (value == _permissionModifiers) return;
                _permissionModifiers = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the full path of the file, derived by its directory path and name.
        /// </summary>
        public string? FullName => Path != null ? System.IO.Path.Combine(Path, Name) : null;


        public static FileModel? MapFile(string path) {
            FileAttributes attributes = File.GetAttributes(path);
            if (attributes.HasFlag(FileAttributes.Directory)) {
                throw new InvalidOperationException("You tried to create a file model with a folder");
            }

            if (attributes.HasFlag(FileAttributes.NotContentIndexed) || attributes.HasFlag(FileAttributes.System)) {
                return null;
            }

            var fileInfo = new FileInfo(path);
            var pathName = System.IO.Path.GetDirectoryName(path);
            var fileModel = new FileModel() {
                Extension = fileInfo.Extension,
                Name = fileInfo.Name,
                CreationDate = fileInfo.CreationTimeUtc,
                LastModified = fileInfo.LastWriteTimeUtc,
                Path = pathName,
                FAIcon = "fa-solid fa-file",
            };
            if (ConfigurationHandler.Current.IndexLocalFileSize) {
                fileModel.Size = fileInfo.Length;
            }
            
            switch (fileInfo.Extension.ToLower())
            {
                case "pdf":
                    fileModel.FAIcon += "-pdf";
                    break;
                case "docx" or "doc":
                    fileModel.FAIcon += "-word";
                    break;
                case ".mp4" or ".mov" or ".avi" or ".wmv" or "mkv" or ".webm":
                    fileModel.FAIcon += "-video";
                    break;
                case ".exe" or ".appimage" or ".app":
                    fileModel.FAIcon = "fa-solid window-maximize";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(pathName))
            {
                fileModel.Parent = new DirectoryInfo(pathName);
            }

            try
            {
                var unixMode = File.GetUnixFileMode(path);
                switch (unixMode)
                {
                    case UnixFileMode.GroupExecute or
                        UnixFileMode.UserExecute or
                        UnixFileMode.OtherExecute:
                        fileModel.PermissionModifiers = "wrx";
                        break;
                    case UnixFileMode.GroupWrite or
                        UnixFileMode.UserWrite or
                        UnixFileMode.OtherWrite:
                        fileModel.PermissionModifiers = "wr";
                        break;
                    case UnixFileMode.OtherRead or
                        UnixFileMode.UserRead or
                        UnixFileMode.GroupRead:
                        fileModel.PermissionModifiers = "r";
                        break;
                    default:
                        break;
                }
            }
            catch (PlatformNotSupportedException)
            {
                Console.WriteLine("Ignoring Permission Mode since platform is not Unix");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Ignoring file as it can't be found");
            }
            

            return fileModel;
        }

        public static IFileSystemObject MapFile(ISftpFile file, ISftpClient client)
        {
            if (!client.IsConnected)
            {
                client.Connect();
            }

            if (file.IsDirectory)
            {
                throw new InvalidOperationException("You tried to create a file model with a folder");
            }

            var extension = file.FullName.Split('.').LastOrDefault();
    

            var fileModel = new FileModel()
            {
                Extension = extension,
                Name = file.Name,
                LastModified = file.LastWriteTimeUtc,
                Path = file.FullName.Split(file.Name).FirstOrDefault(),
                FAIcon = "fa-solid fa-file",
            };

            if (ConfigurationHandler.Current.IndexRemoteFileSize) {
                fileModel.Size = file.Length;
            }

            switch (extension)
            {
                case "pdf":
                    fileModel.FAIcon += "-pdf";
                    break;
                case "docx" or "doc":
                    fileModel.FAIcon += "-word";
                    break;
                case ".mp4" or ".mov" or ".avi" or ".wmv" or "mkv" or ".webm":
                    fileModel.FAIcon += "-video";
                    break;
                case ".exe" or ".appimage" or ".app":
                    fileModel.FAIcon = "fa-solid window-maximize";
                    break;
            }

            return fileModel;
        }
    }
}
