using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace dawn.ftp.ui.Models.FileModels.Interfaces
{
    /// <summary>
    /// Extension of <see cref="IFileSystemObject"/> representing files specifically
    /// </summary>
    internal interface IFileModel : IFileSystemObject
    {
        /// <summary>
        /// The extension the file goes by
        /// </summary>
        [DisplayName("File Type")]
        public string Extension { get; set; }

        /// <summary>
        /// Unix style modifiers that specify what the current users permission for the file are (rwx)
        /// </summary>
        [DisplayName("Permissions")]
        public string? PermissionModifiers { get; set; }
        
        
        /// <summary>
        /// Generates the full path for an object via their name and path.
        /// </summary>
        [NotMapped]
        public string FullPath => System.IO.Path.Combine(Path, Name);
    }
}
