using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using dawn.ftp.ui.Models.FileModels.Interfaces;

namespace dawn.ftp.ui.Models.FileModels
{
    /// <summary>
    /// <inheritdoc cref="IFileSystemObject"/>
    /// </summary>
    public class FileSystemObject : ObservableObject, IFileSystemObject
    {
        private ObservableCollection<IFileSystemObject> _children;
        private DateTime _lastModified;
        private DateTime _creationDate;
        private string _name;
        private DirectoryInfo? _parent;
        private string _faIcon;
        private string? _path;
        private long _size;
        
        public FileSystemObject() { }

        public FileSystemObject(System.IO.FileInfo file) {
            Parent = file.Directory;
            Name = file.Name;
            CreationDate = file.CreationTime;
            LastModified = file.LastWriteTime;
            Size = file.Length;
            Path = file.DirectoryName;
        }

        /// <summary>
        /// <inheritdoc cref="IFileSystemObject.Parent"/>
        /// </summary>
        [Display(AutoGenerateField = false)]
        public DirectoryInfo? Parent
        {
            get => _parent;
            set
            {
                if (Equals(value, _parent)) return;
                _parent = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileSystemObject.Name"/>
        /// </summary>
        [DisplayName("Name")]
        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileSystemObject.FileIcon"/>
        /// </summary>
        public string FAIcon
        {
            get => _faIcon;
            set {
                if (value == _faIcon) return;
                _faIcon = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileSystemObject.CreationDate"/>
        /// </summary>
        [DisplayName("Created At")]
        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                if (value.Equals(_creationDate)) return;
                _creationDate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileSystemObject.LastModified"/>
        /// </summary>
        [DisplayName("Last Modified At")]
        public DateTime LastModified
        {
            get => _lastModified;
            set
            {
                if (value.Equals(_lastModified)) return;
                _lastModified = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileSystemObject.Path"/>
        /// </summary>
        [Display(AutoGenerateField = false)]
        public string? Path
        {
            get => _path;
            set
            {
                if (value == _path) return;
                _path = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The size of the file system object
        /// </summary>
        [DisplayName("Size")]
        public long Size
        {
            get => _size;
            set
            {
                if (value == _size) return;
                _size = value;
                OnPropertyChanged();
            }
        }
    }
}
