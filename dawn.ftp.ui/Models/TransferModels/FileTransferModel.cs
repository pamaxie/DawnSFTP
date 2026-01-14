using dawn.ftp.ui.Models.TransferModels.Interfaces;

namespace dawn.ftp.ui.Models.TransferModels
{
    /// <summary>
    /// Property holding the model for failed file transfers
    /// </summary>
    public class FileTransferModel : FileTransferModelBase
    {
        internal FileTransferModel(string failureReason = "")
        {
            Status = IFileTransferModel.FileTransferStatusEnum.Faulted;
            _failureReason = failureReason;
        }
        
        private string? _failureReason;
        
        /// <summary>
        /// Reason why the transfer failed
        /// </summary>
        public string? FailureReason
        {
            get => _failureReason;
            set
            {
                if (value == _failureReason) return;
                _failureReason = value;
                OnPropertyChanged();
            }
        }
    }
}
