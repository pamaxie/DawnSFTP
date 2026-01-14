using System;
using System.IO;

namespace dawn.ftp.ui.Models.FileModels.Interfaces
{
    /// <summary>
    /// Basic interface that implements a common ground for file system objects (files and folders)
    /// </summary>
    public interface IFileSystemObject
    {
        /// <summary>
        /// <see cref="IFileSystemObject"/> representing the parent object of the file or folder
        /// </summary>
        public DirectoryInfo? Parent { get; set; }

        /// <summary>
        /// String representation of the file or folder name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Image that the file has
        /// </summary>
        public string FAIcon { get; set; }

        /// <summary>
        /// <see cref="DateTime"/> that specifies when the file was created
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// <see cref="DateTime"/> that specifies when the file was last modified / changed
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// The Path to the <see cref="IFileSystemObject"/>
        /// </summary>
        public string Path { get; set; }
        
        /// <summary>
        /// The size of the file system object
        /// </summary>
        public long Size { get; set; }
    }
}
