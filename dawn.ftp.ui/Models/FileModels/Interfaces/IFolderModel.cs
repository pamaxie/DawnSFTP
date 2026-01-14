using System.Collections.ObjectModel;

namespace dawn.ftp.ui.Models.FileModels.Interfaces
{
    internal interface IFolderModel : IFileSystemObject
    {
        /// <summary>
        /// Stores a list of children that are represented as <see cref="IFileSystemObject"/> for the folder
        /// </summary>
        public ObservableCollection<IFileSystemObject> Children { get; set; }
    }
}
