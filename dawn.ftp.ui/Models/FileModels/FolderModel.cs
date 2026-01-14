using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dawn.ftp.ui.BusinessLogic;
using dawn.ftp.ui.Models.FileModels.Interfaces;
using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace dawn.ftp.ui.Models.FileModels
{
    /// <summary>
    /// <inheritdoc cref="IFolderModel"/>
    /// </summary>
    internal class FolderModel: FileSystemObject, IFolderModel
    {
        private ObservableCollection<IFileSystemObject> _children;

        /// <summary>
        /// <inheritdoc cref="IFolderModel.Children"/>
        /// </summary>
        [Display(AutoGenerateField = false)]
        public ObservableCollection<IFileSystemObject> Children
        {
            get => _children;
            set
            {
                if (Equals(value, _children)) return;
                _children = value;
                OnPropertyChanged();
            }
        }


        public static FolderModel? GetFolderModel(ISftpFile file, ISftpClient client, bool maxDepth = false)
        {
            if (!file.IsDirectory)
            {
                throw new InvalidOperationException("You tried to create a folder model with a file");
            }

            //Filter out some unix FTP files since we don't need them
            if (file.Name == "." || file.Name == "..")
            {
                return null;
            }

            var folderModel = new FolderModel()
            {
                LastModified = file.LastWriteTimeUtc,
                Name = file.Name,
                Path = file.FullName.Split(file.Name).FirstOrDefault(),
                FAIcon = "fa-solid fa-folder"
            };

            if (!maxDepth)
            {
                return folderModel;
            }

            var children = new List<IFileSystemObject>();
            var indexedChildren = client.ListDirectory(file.FullName);
            foreach (var child in indexedChildren)
            {
                if (child == null) {
                    continue;
                }

                if (child.IsDirectory)
                {
                    children.Add(GetFolderModel(child, client, true));
                }

                children.Add(FileModel.MapFile(child, client));
            }

            folderModel.Children = new ObservableCollection<IFileSystemObject>(children);
            
            if (ConfigurationHandler.Current.IndexRemoteDirectories) {
                folderModel.Size = folderModel.Children.Sum(c => c.Size);
            }
            
            return folderModel;
        }




        public static FolderModel? GetFolderModel(string path, bool maxDepth = false)
        {
            FileAttributes attributes = File.GetAttributes(path);
            if (!attributes.HasFlag(FileAttributes.Directory))
            {
                throw new InvalidOperationException("You tried to create a folder model with a file");
            }

            if (attributes.HasFlag(FileAttributes.NotContentIndexed) || attributes.HasFlag(FileAttributes.System) && path.Length != 3) {
                return null;
            }

            var dirInfo = new DirectoryInfo(path);

            var folderModel = new FolderModel()
            {
                CreationDate = dirInfo.CreationTimeUtc,
                LastModified = dirInfo.LastWriteTimeUtc,
                Name = dirInfo.Name,
                Path = dirInfo.FullName.Split(dirInfo.Name).FirstOrDefault(),
                FAIcon = "fa-solid fa-folder"
            };

            //This prevents stackoverflow calls by calling this over and over again,
            //we will request a new Folder model with children as we browse :)
            if (!maxDepth) {
                return folderModel;
            }

            var children = new List<IFileSystemObject>();
            var childrenFiles = Directory.GetFiles(path);
            var childrenFolders = Directory.GetDirectories(path);
            foreach (var item in childrenFiles) {
                children.Add(FileModel.MapFile(item));
            }

            foreach (var item in childrenFolders) {
                children.Add(GetFolderModel(item, true));
            }

            folderModel.Children = new ObservableCollection<IFileSystemObject>(children);

            if (ConfigurationHandler.Current.IndexLocalDirectories) {
                folderModel.Size = DirSize(dirInfo);
            }
            
            return folderModel;
        }

        /// <summary>
        /// Calculates the total size of a directory, including all files and subdirectories.
        /// </summary>
        /// <param name="d">The directory for which the size is to be calculated.</param>
        /// <returns>The total size of the directory in bytes.</returns>
        private static long DirSize(DirectoryInfo d) {
            long size = 0;
            // Add file sizes.
            var fis = d.GetFiles();
            Parallel.ForEach(fis, fi => size += fi.Length);
            
            // Add subdirectory sizes.
            var dis = d.GetDirectories();
            Parallel.ForEach(dis, di => size += DirSize(di));
            return size;  
        }
    }
}
