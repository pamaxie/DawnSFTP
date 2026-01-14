using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using dawn.ftp.ui.Models.TransferModels.Interfaces;

namespace dawn.ftp.ui.Models.TransferModels
{
    public class FileTransferModelBase : ObservableObject, IFileTransferModel
    {
        private ulong _fileSize;
        private string? _fileName;
        private string? _sourceLocation;
        private string? _remoteIp;
        private string? _destinationLocation;
        private decimal _progress;
        private DateTime? _transferStart;
        private DateTime? _transferEnd;
        private IFileTransferModel.FileTransferStatusEnum _status;
        private long? _fileTransferId;
        private DateTime? _lastModified;
        private ulong _uploadedBytes;

        [NotMapped]
        public CancellationTokenSource? CancellationTokenSource { get; set; }

        /// <summary>
        /// <inheritdoc cref="IFileTransferModel.FileTransferId"/>
        /// </summary>
        [DisplayName("Transfer Id")]
        [Key]
        public long? FileTransferId
        {
            get => _fileTransferId;
            set
            {
                if (value.Equals(_fileTransferId)) return;
                _fileTransferId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileTransferModel.FileSize"/>
        /// </summary>
        [DisplayName("File Size")]
        public ulong FileSize
        {
            get => _fileSize;
            set
            {
                if (value == _fileSize) return;
                _fileSize = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileTransferModel.FileName"/>
        /// </summary>
        [DisplayName("File Name")]
        public string? FileName
        {
            get => _fileName;
            set
            {
                if (value == _fileName) return;
                _fileName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileTransferModel.SourceLocation"/>
        /// </summary>
        [DisplayName("Source Location")]
        public string? SourceLocation
        {
            get => _sourceLocation;
            set
            {
                if (value == _sourceLocation) return;
                _sourceLocation = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileTransferModel.RemoteIP"/>
        /// </summary>
        [DisplayName("Remote IP")]
        public string? RemoteIP
        {
            get => _remoteIp;
            set
            {
                if (value == _remoteIp) return;
                _remoteIp = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileTransferModel.DestinationLocation"/>
        /// </summary>
        [DisplayName("Destination Location")]
        public string? DestinationLocation
        {
            get => _destinationLocation;
            set
            {
                if (value == _destinationLocation) return;
                _destinationLocation = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileTransferModel.Progress"/>
        /// </summary>
        [DisplayName("Progress")]
        public decimal Progress
        {
            get => _progress;
            set
            {
                if (value == _progress) return;
                _progress = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileTransferModel.TransferStart"/>
        /// </summary>
        [DisplayName("Transfer Start Time")]
        public DateTime? TransferStart
        {
            get => _transferStart;
            set
            {
                if (value.Equals(_transferStart)) return;
                _transferStart = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileTransferModel.TransferEnd"/>
        /// </summary>
        [DisplayName("Transfer End Time")]
        public DateTime? TransferEnd
        {
            get => _transferEnd;
            set
            {
                if (value.Equals(_transferEnd)) return;
                _transferEnd = value;
                OnPropertyChanged();
            }
        }

        public DateTime? LastUpdate
        {
            get => _lastModified;
            set {
                if (value.Equals(_lastModified)) return;
                _lastModified = value;
                OnPropertyChanged();
            }
        }
        
        /// <summary>
        /// Returns the amount of time it took to transfer the item
        /// </summary>
        [DisplayName("Transfer Duration")]
        [NotMapped]
        public string? TransferTime
        {
            get
            {
                if (TransferStart == null || TransferEnd == null) {
                    return string.Empty;
                }
                    
                var time = (TransferEnd - TransferStart).Value;
                var timeStr = $"{time.Days} Days, {time.Hours}Hours, {time.Minutes}Minutes, {time.Seconds}Seconds, {time.Milliseconds} Milliseconds";
                return timeStr;
            }
        }

        /// <summary>
        /// Returns transfer speed based on Transfer time and file size
        /// </summary>
        [NotMapped]
        public string Speed
        {
            get
            {
                if (TransferStart == null) {
                    return string.Empty;
                }

                var end = DateTime.Now;
                if (TransferEnd != null) {
                    end = TransferEnd.Value;
                }
                
                var duration = TransferStart.Value - end;
                return $" KB/s";
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileTransferModel.Status"/>
        /// </summary>
        [DisplayName("Status")]
        public IFileTransferModel.FileTransferStatusEnum Status
        {
            get => _status;
            set
            {
                if (value.Equals(_status)) return;
                _status = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// <inheritdoc cref="IFileTransferModel.UploadedBytes"/>
        /// </summary>
        [NotMapped]
        public ulong UploadedBytes {
            get => _uploadedBytes;
            set {
                if (value.Equals(_uploadedBytes)) return;
                _uploadedBytes = value;
                OnPropertyChanged();
            }
        }

        protected FileTransferModelBase(CancellationTokenSource? cancellationTokenSource = null) {
            CancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
        }
    }
}
