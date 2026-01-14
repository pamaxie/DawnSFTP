using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using dawn.ftp.ui.Database;
using dawn.ftp.ui.UserControls.ViewModels;
using dawn.ftp.ui.ViewModels;

namespace dawn.ftp.ui.Models {
    public class SftpConnectionProperty : ObservableObject {
        private string _name;
        private string? _hostOsIcon;
        private SshKey? _privateKey;
        private NetworkCredential? _credential;
        private bool _useKeyAuth;
        private ulong _connectionId;
        private int _port;
        private CancellationTokenSource _cancellationTokenSource;
        private string _keyHash;
        private bool _remember;

        internal SftpConnectionProperty() {
            Name = Credential == null ? "Unknown..." : $"{Credential?.UserName} @ {Credential?.Domain}";
        }

        [Key]
        public ulong ConnectionId
        {
            get => _connectionId;
            set => SetProperty(ref _connectionId, value);
        }
        
        /// <summary>
        /// Gets or sets the name for the remote connection that's displayed on the Tab
        /// </summary>
        [MaxLength(128)]
        public string Name { 
            get => _name;
            set => SetProperty(ref _name, value); 
        }

        /// <summary>
        /// Gets or sets the icon representing the host operating system.
        /// </summary>
        [MaxLength(128)]
        public string? HostOsIcon {
            get => _hostOsIcon; 
            set => SetProperty(ref _hostOsIcon, value); 
        }
        
        /// <summary>
        /// Gets or sets the cryptographic hash representation of the hosts key.
        /// </summary>
        [MaxLength(256)]
        public string KeyHash {
            get => _keyHash;
            set => SetProperty(ref _keyHash, value);
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether key-based authentication is enabled for requests.
        /// </summary>
        public bool UseKeyAuth {
            get => _useKeyAuth;
            set => SetProperty(ref _useKeyAuth, value);
        }

        /// <summary>
        /// Port for the connection
        /// </summary>
        public int Port {
            get => _port;
            set => SetProperty(ref _port, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the SFTP connection details should be remembered
        /// for future use during the application session or later operations.
        /// </summary>
        [NotMapped]
        public bool Remember {
            get => _remember;
            set => SetProperty(ref _remember, value);
        }

        /// <summary>
        /// Gets or sets the network credentials used for authentication with the SSH service.
        /// </summary>
        [NotMapped]
        public NetworkCredential? Credential {
            get => _credential;
            set => SetProperty(ref _credential, value);
        }

        /// <summary>
        /// Gets or sets a value indicating the path used for Key-Based Authentication
        /// </summary>
        [NotMapped]
        public SshKey? PrivateKey {
            get => _privateKey;
            set => SetProperty(ref _privateKey, value);
        }

        /// <summary>
        /// Holds the cancellation token for our connection which allows us to terminate the connection at any time.
        /// </summary>
        [NotMapped]
        public CancellationTokenSource CancellationTokenSource {
            get => _cancellationTokenSource;
            set => SetProperty(ref _cancellationTokenSource, value);
        }
        
        public FileViewModel Connect(MainWindowViewModel mainWindowViewModel, bool remember = false) {
            if (remember) {
                try {
                    var context = new SqlLiteDbContext();
                    var exists = context.SftpConnectionProperties.Any(x => x.Name == Name && x.HostOsIcon == HostOsIcon);
                    if (!exists) {
                        context.SftpConnectionProperties.Add(this);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex) {
                    Debugger.Log(0, "Error", ex.ToString());
                }    
            }

            var vm = new FileViewModel(this, mainWindowViewModel, new CancellationTokenSource());
            mainWindowViewModel.Connections.Add(vm);
            return vm;
        }
    }
}
