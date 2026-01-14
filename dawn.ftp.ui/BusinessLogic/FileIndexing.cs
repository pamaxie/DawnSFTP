using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using dawn.ftp.ui.Extensions;
using dawn.ftp.ui.Models.FileModels;
using dawn.ftp.ui.Models.FileModels.Interfaces;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace dawn.ftp.ui.BusinessLogic
{
    internal static class FileIndexing
    {
        internal static List<IFileSystemObject>? GetFileSystemObjectsForRemoteDir(SftpClient client, string directory = "") {
            try {
                client.ConnectSafe();
                
                if (string.IsNullOrEmpty(directory)) {
                    directory = client.WorkingDirectory;
                }

                var dir = client.ListDirectory(directory);
                var fileObjects = new List<IFileSystemObject>();

                foreach (var file in dir) {
                    if (file.IsDirectory) {
                        var folderMapping = FolderModel.GetFolderModel(file, client);
                        if (folderMapping != null) {
                            fileObjects.Add(folderMapping);
                        }
                        continue;
                    }

                    var fileObject = FileModel.MapFile(file, client);
                    fileObjects.Add(fileObject);
                }
                
                return fileObjects;
            }
            catch (SftpPermissionDeniedException ) {
                return null;
            }
        }

        internal static List<IFileSystemObject> GetFileSystemObjectsForDir(string directory = "")
        {
            if (string.IsNullOrEmpty(directory)) {
                directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }

            var fileObjects = new List<IFileSystemObject>();
            var files = Directory.GetFiles(directory);
            fileObjects.AddRange(files.Select(FileModel.MapFile).OfType<FileModel>());
            var folders = Directory.GetDirectories(directory);
            fileObjects.AddRange(folders.Select(folder => FolderModel.GetFolderModel(folder)).OfType<FolderModel>());
            return fileObjects;
        } 
    }
}
