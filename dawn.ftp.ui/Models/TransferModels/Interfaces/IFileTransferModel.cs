using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;

namespace dawn.ftp.ui.Models.TransferModels.Interfaces
{
    public interface IFileTransferModel
    {
        public CancellationTokenSource? CancellationTokenSource { get; set; }
        
        /// <summary>
        /// Unique Id for the file transfer
        /// </summary>
        public long? FileTransferId { get; set; }
        
        /// <summary>
        /// The Size of the file
        /// </summary>
        public ulong FileSize { get; set; }

        /// <summary>
        /// The Filename, usually includes an extension
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Source of the file on the remote machine
        /// </summary>
        public string? SourceLocation { get; set; }

        /// <summary>
        /// The IP of the remote machine
        /// </summary>
        public string? RemoteIP { get; set; }
        
        /// <summary>
        /// Destination the file is supposed to go
        /// </summary>
        public string? DestinationLocation { get; set; }

        /// <summary>
        /// The Progress of the transfer
        /// </summary>
        public decimal Progress { get; set; }

        /// <summary>
        /// When the transfer ended
        /// </summary>
        public DateTime? TransferStart { get; set; }

        /// <summary>
        /// When the transfer started
        /// </summary>
        public DateTime? TransferEnd { get; set; }
        
        /// <summary>
        /// When the data was last modified
        /// </summary>
        public DateTime? LastUpdate { get; set; }

        /// <summary>
        /// The current status of the transfer
        /// </summary>
        public FileTransferStatusEnum Status { get; set; }
        
        /// <summary>
        /// Sets the uploaded bytes, not mapped to the database
        /// </summary>
        [NotMapped]
        public ulong UploadedBytes { get; set; }

        public void CancelTransfer() {
            if (CancellationTokenSource == null || CancellationTokenSource.IsCancellationRequested) {
                return;
            }
            
            CancellationTokenSource.Cancel();
        }


        [Flags]
        public enum FileTransferStatusEnum
        {
            Running = 0,
            Faulted = 1,
            Success = 2
        }

    }
}
