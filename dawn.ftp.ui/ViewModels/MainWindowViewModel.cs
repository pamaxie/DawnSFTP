using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Timers;
using dawn.ftp.ui.BusinessLogic;
using dawn.ftp.ui.Database;
using dawn.ftp.ui.Extensions;
using dawn.ftp.ui.Models;
using dawn.ftp.ui.Models.TransferModels;
using dawn.ftp.ui.Models.TransferModels.Interfaces;
using dawn.ftp.ui.UserControls.ViewModels;
using dawn.ftp.ui.Views;
using Timer = System.Timers.Timer;

namespace dawn.ftp.ui.ViewModels;

public class MainWindowViewModel : ViewModelBase {
    private int _selectedTab;
    private ObservableCollection<FileViewModel> _connections;
    private ObservableCollection<FileTransferModel> _failedTransfers;
    private ObservableCollection<FileTransferModel> _transfers;
    private ObservableCollection<FileTransferModel> _completedTransfers;
    private ObservableCollection<SftpConnectionProperty> _previousConnections;
    private FileViewModel? _selectedConnection;
    private ObservableCollection<ScriptNode>? _scriptNodes;
    private bool _scriptsRefreshing;
    private static Timer aTimer;
    private static long DataVersion = 0;

    /// <summary>
    /// Creates a new instance of <see cref="MainWindowViewModel"/>
    /// </summary>
    public MainWindowViewModel() {
        _connections ??= [];
        SetTimer();
        
        var dbContext = new SqlLiteDbContext();
        var connections = dbContext.SftpConnectionProperties.ToList();
        PreviousConnections = new ObservableCollection<SftpConnectionProperty>(connections);
        
        //We need to do this to ensure that we recognized connection changes
        Connections.CollectionChanged += (sender, args) => {
            OnPropertyChanged(nameof(HasConnection));
        };
    }
    
    public ObservableCollection<ShellItemViewModel> Items { get; set; } = new() { };

    /// <summary>
    /// Gets or sets the collection of network credentials representing active connections.
    /// </summary>
    /// <remarks>Changes to the collection are automatically reflected in data-bound user interfaces due to
    /// its observable nature. Assigning a new collection will replace the existing set of connections and notify
    /// listeners of the change.</remarks>
    internal ObservableCollection<FileViewModel> Connections {
        get => _connections;
        set => SetProperty(ref _connections, value);
    }
    
    private void SetTimer()
   {
        // Create a timer with a two second interval.
        aTimer = new Timer(200);
        // Hook up the Elapsed event for the timer. 
        aTimer.Elapsed += OnTimedEvent;
        aTimer.AutoReset = true;
        aTimer.Enabled = true;
    }

    private void OnTimedEvent(object? sender, ElapsedEventArgs e)
    {
        var dbContext = new SqlLiteDbContext();
        var dataVersion = dbContext.GetDataVersion();
        if (DataVersion == dataVersion) {
            return;
        }
        
        
        var transferModels = dbContext.FileTransferModels.ToList();
        foreach (var value in transferModels)
        {
            var timeSinceLastUpdate = (DateTime.Now - value.LastUpdate);
            //We time this operation out as it's likely failed
            if (value.Status == IFileTransferModel.FileTransferStatusEnum.Success ||
                value.Status == IFileTransferModel.FileTransferStatusEnum.Faulted) {
                continue;
            }

            if (timeSinceLastUpdate is { TotalSeconds: < 2 }) {
                continue;
            }
            
            value.Status = IFileTransferModel.FileTransferStatusEnum.Faulted;
            dbContext.FileTransferModels.Update(value);
            dbContext.SaveChangesAsync();
        }
        
        Transfers = new ObservableCollection<FileTransferModel>(transferModels.Where(x => x.Status == IFileTransferModel.FileTransferStatusEnum.Running));
        FailedTransfers = new ObservableCollection<FileTransferModel>(transferModels.Where(x => x.Status == IFileTransferModel.FileTransferStatusEnum.Faulted));
        CompletedTransfers = new ObservableCollection<FileTransferModel>(transferModels.Where(x => x.Status == IFileTransferModel.FileTransferStatusEnum.Success));
        DataVersion = dbContext.GetDataVersion();
        OnPropertyChanged(nameof(Transfers));
        OnPropertyChanged(nameof(FailedTransfers));
        OnPropertyChanged(nameof(CompletedTransfers));

        if (ScriptNodes == null) {
            return;
        }
        //Update script for the script folder so we make sure it's always up to date.
        var dirInfo = new DirectoryInfo(SoftwareInitialization.ScriptFolder);
        var files = dirInfo.GetFiles();
        if (files.Length > ScriptNodes.Count) {
            RefreshScripts();
        }

        //Update each subdirectory
        foreach (var file in files) {
            if (!Directory.Exists(file.FullName)) continue;
            var scriptNode = ScriptNodes.FirstOrDefault(x => x.FullPath == file.FullName);
            if (scriptNode is null || file.Directory is null) {
                continue;
            }
            
            if (scriptNode.Children.Count != file.Directory.GetFiles().Length) {
                RefreshScripts();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal ObservableCollection<FileTransferModel> Transfers {
        get => _transfers;
        set => SetProperty(ref _transfers, value);
    }
    
    /// <summary>
    /// 
    /// </summary>
    internal ObservableCollection<FileTransferModel> FailedTransfers {
        get => _failedTransfers;
        set => SetProperty(ref _failedTransfers, value);
    }

    /// <summary>
    /// 
    /// </summary>
    internal ObservableCollection<FileTransferModel> CompletedTransfers {
        get => _completedTransfers;
        set => SetProperty(ref _completedTransfers, value);
    }

    internal ObservableCollection<SftpConnectionProperty> PreviousConnections {
        get => _previousConnections;
        set => SetProperty(ref _previousConnections, value);
    }

    /// <summary>
    /// Gets or sets the index of the currently selected tab.
    /// </summary>
    /// <remarks>The value represents the zero-based index of the selected tab. Changing this property updates
    /// the selection and may trigger related UI updates or events.</remarks>
    public int SelectedTab {
        get => _selectedTab;
        set {
            //This has to be before the equality comparer otherwise our Selected Connection will be null.
            if (value < Connections.Count && value >= 0) {
                var connection = Connections[value];
                SelectedConnection = connection;
            }
            
            if (Equals(value, _selectedTab)) {
                return;
            }
            
            SetProperty(ref _selectedTab, value);
        }
    }

    public FileViewModel? SelectedConnection {
        get => _selectedConnection;
        private set {
            if (Equals(value, _selectedConnection)) {
                return;
            }
            
            SetProperty(ref _selectedConnection, value);
        }
    }

    public ObservableCollection<ScriptNode>? ScriptNodes {
        get => _scriptNodes;
        set => SetProperty(ref _scriptNodes, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the scripts are currently being refreshed.
    /// </summary>
    /// <remarks>
    /// This property is typically used to manage the state of script updates in the application.
    /// Setting this property to true may trigger UI updates or actions related to the script refresh process,
    /// while setting it to false indicates that the refresh has completed or is not in progress.
    /// </remarks>
    public bool ScriptsRefreshing {
        get => _scriptsRefreshing;
        set => SetProperty(ref _scriptsRefreshing, value);
    }

    /// <summary>
    /// Gets a value indicating whether there are any active connections available.
    /// </summary>
    /// <remarks>
    /// This property evaluates the current collection of connections to determine if it contains any items.
    /// It is typically used to enable or disable UI elements based on the presence of active connections.
    /// </remarks>
    internal bool HasConnection => Connections.Any();

    /// <summary>
    /// Refreshes the collection of script nodes displayed in the application.
    /// This method initializes a new <see cref="ObservableCollection{T}"/> of <see cref="ScriptNode"/>
    /// and adds a root node representing the script folder defined in <see cref="SoftwareInitialization.ScriptFolder"/>.
    /// </summary>
    public void RefreshScripts() {
        ScriptNodes = [
            new ScriptNode(SoftwareInitialization.ScriptFolder, this)
        ];
        OnPropertyChanged(nameof(ScriptNodes));
    }

    /// <summary>
    /// Duplicates the currently selected tab and its associated connection
    /// by creating a new instance of <see cref="FileViewModel"/> with the
    /// same connection properties as the selected tab.
    /// </summary>
    public void CloneTab() {
        var currentConnection = SelectedConnection;
        if (currentConnection is null || currentConnection.ConnectionProperty is null) {
            return;
        }
        
        var newConnection = new FileViewModel(currentConnection.ConnectionProperty, this, new CancellationTokenSource());
        if (Connections.Any(x => x?.Name == newConnection?.Name)) {
            newConnection.Name += "_copy";
        }
        Connections.Add(newConnection);
        SelectedTab = Connections.IndexOf(newConnection);
    }

    public void Connect(SftpConnectionProperty connectionProperty) {
        var hasPass = false;
        connectionProperty.Remember = true;
        try {
            if (!KeyringManager.HasValidKeyring() || connectionProperty.Credential is null) {
                return;
            }
            
            var pass = KeyringManager.GetPassword(connectionProperty.Credential.Domain, connectionProperty.Credential.UserName);
                
            if (pass.Length == 0) {
                return;
            }
                
            connectionProperty.Credential.SecurePassword = new NetworkCredential(connectionProperty.Credential.UserName, pass).SecurePassword;
            hasPass = true;
        }
        catch (Exception e) {
            Console.WriteLine(e);
        }
        finally {
            if (!hasPass) {
                var viewModel = new PasswordEntryViewModel("We were unable to retrieve your password from the keyring. Please enter it manually:");
                var dialog = new PasswordEntry() {
                    DataContext = viewModel
                };
                
                dialog.Show();
                dialog.Closed += (s, e) => {
                    if (s is not PasswordEntry pwEntry) {
                        return;
                    }
                    if (viewModel.WasSuccessfulExit) {
                        //Something went wrong so we try to recover the creds via our Name property.
                        if (connectionProperty.Credential is null) {
                            try {
                                var usernameAndAddress = connectionProperty.Name.Split('@');
                                var username = usernameAndAddress[0];
                                var address = usernameAndAddress[1];
                                connectionProperty.Credential = 
                                    new NetworkCredential(username, viewModel.Password, address);
                            }
                            catch (Exception ex) {
                                Console.WriteLine(ex);
                                //Remove our item here since it's clearly bugged.
                                var dbContext = new SqlLiteDbContext();
                                dbContext.SftpConnectionProperties.Remove(connectionProperty);
                                dbContext.SaveChanges();
                                
                                //TODO: this should show that we had an error that's not recoverable.
                                return;
                            }
                        }
                        else {
                            connectionProperty.Credential = 
                                new NetworkCredential(connectionProperty.Credential.UserName, viewModel.Password, connectionProperty.Credential.Domain);
                        }

                        //Let's try to save our password in the keyring, it potentially couldn't be saved but can be now.
                        try {
                            KeyringManager.SavePassword(connectionProperty.Credential.Domain, connectionProperty.Credential.UserName, viewModel.Password.ToSecureString());
                        }
                        catch (Exception ex) {
                            //Logging why we couldn't save our password.
                            Console.WriteLine(ex);
                        }
                        
                        var vm = connectionProperty.Connect(this, true);
                        SelectedTab = Connections.IndexOf(vm);
                    }
                };
            }
            else {
                var vm = connectionProperty.Connect(this, true);
                SelectedTab = Connections.IndexOf(vm);
            }
        }
    }

    public void OpenOptions()  {
        var options = new OptionsViewModel();
        var dialog = new OptionsWindow() {
            DataContext = options
        };
        dialog.Show();
    }

    public void QuickConnectPressed(FileViewModel? fileViewModel = null) {
        var viewModel = new QuickConnectViewModel();
        
        if (fileViewModel is { ConnectionProperty: not null }) {
            if (fileViewModel.ConnectionProperty.Credential is not null) {
                viewModel.SecurePassword = fileViewModel.ConnectionProperty.Credential.SecurePassword;
                viewModel.IpAddress = fileViewModel.ConnectionProperty.Credential.Domain;
                viewModel.UserName = fileViewModel.ConnectionProperty.Credential.UserName;
                viewModel.Password = fileViewModel.ConnectionProperty.Credential.SecurePassword.SecureStringToString();
                viewModel.RememberMe = fileViewModel.ConnectionProperty.Remember;
            }
            
            viewModel.UseSshKey = fileViewModel.ConnectionProperty.UseKeyAuth;
            viewModel.Port = fileViewModel.ConnectionProperty.Port;
            if (viewModel.UseSshKey) {
                var key = viewModel.SSHKeys.FirstOrDefault(x =>
                    x.KeyIdentifier == fileViewModel.ConnectionProperty.PrivateKey.KeyIdentifier);

                if (key != null) {
                    viewModel.SelectedKey = key;
                }
            }
        }
        
        var dialog = new QuickConnectWindow() {
            DataContext = viewModel
        };
        
        dialog.Show();
        dialog.Closed += (s, e) => {
            //Ensure we use the right data context and window, if not just return since we don't know how to handle it.
            if (s is not QuickConnectWindow window ||
                window.DataContext is not QuickConnectViewModel model) {
                return; 
            }
            
            if (!model.SuccessfulExit) {
                return;
            }

            var sftpConnectionProperty = new SftpConnectionProperty() {
                Name = $"{model.UserName}@{model.IpAddress}",
                UseKeyAuth = model.UseSshKey,
                Credential = new NetworkCredential(model.UserName, model.SecurePassword.SecureStringToString(), model.IpAddress),
                Port = model.Port,
                CancellationTokenSource =  new CancellationTokenSource(),
                Remember = model.RememberMe
            };

            if (model is { UseSshKey: true, SelectedKey: not null }) {
                sftpConnectionProperty.PrivateKey = model.SelectedKey;
            }

            //Save our password if we have one
            if (model.RememberMe) {
                KeyringManager.SavePassword(model.IpAddress, model.UserName, model.SecurePassword);
            }
            
            var vm = sftpConnectionProperty.Connect(this, model.RememberMe);
            SelectedTab = Connections.IndexOf(vm);
        };
    }

    public void RefreshPressed() {
        if (SelectedConnection is null) {
            return;
        }
        
        SelectedConnection?.UpdateRemoteProcessesAsync();
        SelectedConnection?.RefreshFiles();
    }

    public void DisconnectPressed() {
        if (SelectedConnection is null) {
            return;
        }
        
        SelectedConnection?.Disconnect();
        SelectedConnection = null;
    }

    public void CreateTab() {
        QuickConnectPressed();
    }

    public void CloseActiveTab() {
        var connection = Connections[SelectedTab];
        connection.SftpClient?.Dispose();
        connection.SshClient?.Dispose();
        Connections.Remove(connection);
    }

    public void OpenAboutWindow() {
        var viewModel = new AboutViewModel();
        var dialog = new AboutWindow() {
            DataContext = viewModel
        };
        dialog.Show();
    }
}