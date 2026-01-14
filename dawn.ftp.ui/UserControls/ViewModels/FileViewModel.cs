using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Threading;
using dawn.ftp.ui.BusinessLogic;
using dawn.ftp.ui.Database;
using dawn.ftp.ui.Extensions;
using dawn.ftp.ui.Models;
using dawn.ftp.ui.Models.FileModels;
using dawn.ftp.ui.Models.FileModels.Interfaces;
using dawn.ftp.ui.ViewModels;
using ReactiveUI;
using Renci.SshNet;
using Renci.SshNet.Common;
using Sentry;
using Path = System.IO.Path;

namespace dawn.ftp.ui.UserControls.ViewModels;

public class FileViewModel : ViewModelBase
{
    private int _selectedItem;
    private ObservableCollection<IFileSystemObject>? _files;
    private string? _localPath;
    private ObservableCollection<IFileSystemObject>? _remoteFiles;
    private string? _remotePath;
    private int _selectedRemoteItem;
    private string? _remoteOsIcon;
    private SftpConnectionProperty? _connectionProperty;
    private SftpClient? _sftpClient;
    private SshClient? _sshClient;
    private string? _connectionErrors;
    private bool _isBusy;
    private string? _workStatus;
    private bool _isKeyTrusted;
    private ObservableCollection<RemoteProcess>? _processes;
    private readonly System.Timers.Timer _aTimer = new System.Timers.Timer();
    private string? _name;
    private ObservableCollection<ShellItemViewModel> _shellStreams = new ObservableCollection<ShellItemViewModel>();
    private SSHOs _remoteOs;

    /// <summary>
    /// Gets a value indicating whether the size of remote files should be displayed.
    /// The value is determined based on the current configuration settings, specifically
    /// the 'IndexRemoteFileSize' or 'IndexRemoteDirectories' properties, and reflects
    /// user preferences for remote file or directory indexing.
    /// </summary>
    internal bool ShowRemoteFileSize => ConfigurationHandler.Current.IndexRemoteFileSize || 
                                        ConfigurationHandler.Current.IndexRemoteDirectories;

    /// <summary>
    /// Gets a value indicating whether the size of local files should be displayed.
    /// The value is determined based on the current configuration settings, specifically
    /// the 'IndexLocalFileSize' or 'IndexLocalDirectories' properties, and reflects
    /// user preferences for local file or directory indexing.
    /// </summary>
    internal bool ShowLocalFileSize => ConfigurationHandler.Current.IndexLocalFileSize || 
                                       ConfigurationHandler.Current.IndexLocalDirectories;


    /// <summary>
    /// Occurs when there is a change in the key associated with the FileViewModel instance.
    /// Provides an event mechanism to notify subscribers of updates to key-related data, typically
    /// triggered during key verification or connection processes.
    /// </summary>
    public event EventHandler<string>? KeyChange;

    /// <summary>
    /// Represents the parent view model that owns this instance of the file view model.
    /// Provides access to shared resources and operations between connected file view models.
    /// </summary>
    public MainWindowViewModel? Owner { get; }

    /// <summary>
    /// Provides mechanisms for signaling cancellation of asynchronous operations
    /// in progress within the current instance of the file view model.
    /// Allows coordinated cancellation of tasks through a token that can be queried
    /// or passed to operations requiring cancellation awareness.
    /// </summary>
    private CancellationTokenSource CancellationTokenSource { get; }

    /// <summary>
    /// Represents a collection of file system objects used to display and interact with
    /// local files in the file view. This property holds the list of files in the current
    /// local directory context and can be refreshed or modified as needed during file
    /// operations.
    /// </summary>
    public ObservableCollection<IFileSystemObject>? Files {
        get => _files;
        set => SetProperty(ref _files, value);
    }

    /// <summary>
    /// Represents the operating system type of the remote machine.
    /// The property is used to identify and handle platform-specific functionalities,
    /// such as file system interactions or process management, when connected via SSH.
    /// </summary>
    public SSHOs RemoteOs {
        get => _remoteOs;
        set => SetProperty(ref _remoteOs, value);
    }

    /// <summary>
    /// Represents the collection of files and directories available on the remote system.
    /// This property is used to manage and display the remote file system structure, providing
    /// the ability to retrieve and update file objects from the remote directory using the SFTP connection.
    /// </summary>
    public ObservableCollection<IFileSystemObject>? RemoteFiles {
        get => _remoteFiles;
        set => SetProperty(ref _remoteFiles, value);
    }

    /// <summary>
    /// Represents a collection of ShellStream objects used to manage and interact with
    /// remote shell sessions. This property allows tracking, updating, and manipulating
    /// multiple shell streams within the context of the FileViewModel.
    /// </summary>
    public ObservableCollection<ShellItemViewModel> ShellStreams {
        get => _shellStreams;
        set => SetProperty(ref _shellStreams, value);
    }

    /// <summary>
    /// Gets or sets the SftpClient associated with the current FileViewModel instance.
    /// This property manages the SFTP client connection used for interacting with remote file systems,
    /// including operations such as transferring files, querying directories, and managing remote files.
    /// When the value of this property changes, it triggers property changed notifications
    /// to update any bindings dependent on this connection.
    /// </summary>
    public SftpClient? SftpClient {
        get => _sftpClient;
        set {
            if (Equals(value, _sftpClient)) return;
            _sftpClient = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Represents the secure shell (SSH) client used to manage and maintain secure remote connections.
    /// Provides functionality to establish, manage, and close secure sessions with a remote server.
    /// Used as the primary means of executing commands or managing the remote system within the context
    /// of the FileViewModel operations.
    /// </summary>
    public SshClient? SshClient {
        get => _sshClient;
        set {
            if (Equals(value, _sshClient)) return;
            _sshClient = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Represents the connection properties used for configuring and managing the secure FTP (SFTP) connection
    /// associated with the FileViewModel instance. This property stores details such as connection credentials,
    /// host information, and additional settings required to establish and maintain the connection.
    /// Updates to this property trigger internal property changes and may impact related connection behaviors.
    /// </summary>
    public SftpConnectionProperty? ConnectionProperty {
        get => _connectionProperty;
        set {
            OnPropertyChanged(nameof(Name));
            SetProperty(ref _connectionProperty, value);
        }
    }

    /// <summary>
    /// Represents the current file system path being navigated or managed by the FileViewModel instance.
    /// Tracks and updates the current directory path, providing a binding point for UI components
    /// and serving as a reference for file-related operations.
    /// </summary>
    public string? LocalPath {
        get => _localPath;
        set => SetProperty(ref _localPath, value);
    }

    /// <summary>
    /// Represents the current remote directory path in the file management system.
    /// Used to track and display the active directory on the remote server,
    /// typically updated during navigation or remote file interactions.
    /// </summary>
    public string? RemotePath {
        get => _remotePath;
        set {
            if (value == _remotePath || value == null) {
                return;
            }

            if (SftpClient is not { IsConnected: true }) {
                return;
            }
            
            try {
                SftpClient.ChangeDirectory(value);
            }
            catch (Exception e) {
                ShowPermissionDeniedDialog();
                return;
            }
            
            SetProperty(ref _remotePath, value);
        }
    }

    /// <summary>
    /// Gets or sets the index of the currently selected item in the collection of available files.
    /// This property reflects the user's selection in the associated UI element and is used
    /// to determine the item selected for operations such as opening or navigating.
    /// </summary>
    public int SelectedItem {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    /// <summary>
    /// Represents the index of the currently selected item in the collection of remote files.
    /// Used to track and manage user interaction with remote file listings, enabling operations
    /// such as file selection, double-click actions, and navigation.
    /// Updates to this property trigger necessary changes in the user interface or functionality.
    /// </summary>
    public int SelectedRemoteItem {
        get => _selectedItem;
        set => SetProperty(ref _selectedRemoteItem, value);
    }

    /// <summary>
    /// Represents the icon associated with the remote operating system.
    /// Typically used for UI bindings to visually indicate the operating system
    /// of a connected remote host. This property is dynamically updated based
    /// on the results of probing the remote system.
    /// </summary>
    public string? RemoteOsIcon {
        get => _remoteOsIcon;
        set => SetProperty(ref _remoteOsIcon, value);
    }

    /// <summary>
    /// Indicates whether the FileViewModel instance is currently performing a time-intensive or blocking operation.
    /// This property is typically used to enable or disable UI elements and provide feedback regarding the application's activity state.
    /// </summary>
    public bool IsBusy {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Indicates whether the key associated with a remote connection is trusted.
    /// This property can be used to determine whether the current session recognizes the
    /// cryptographic identity of the remote host as verified and trusted, ensuring a secure
    /// communication channel. Modifying this property may trigger actions such as reconnection
    /// or verification processes.
    /// </summary>
    public bool IsKeyTrusted {
        get => _isKeyTrusted;
        set => SetProperty(ref _isKeyTrusted, value);
    }

    /// <summary>
    /// Represents the error message associated with connection attempts in the FileViewModel instance.
    /// This property is typically used to store and retrieve details about any issues or failures
    /// encountered during connection operations, enabling UI components or other consumers to
    /// display appropriate error messages or take corrective action.
    /// </summary>
    public string? ConnectionErrors {
        get => _connectionErrors;
        set
        {
            if (value == _connectionErrors) return;
            _connectionErrors = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Represents the current operational status or state of the FileViewModel instance.
    /// Provides real-time feedback regarding ongoing tasks such as connection verification,
    /// error handling, or successful completion of processes.
    /// </summary>
    internal string? WorkStatus {
        get => _workStatus;
        set => SetProperty(ref _workStatus, value);
    }

    /// <summary>
    /// Represents a collection of active remote processes associated with the current file view model.
    /// Provides functionality to monitor and manage processes running on the remote system,
    /// such as retrieving information about CPU usage, memory consumption, ownership,
    /// and runtime duration for each process.
    /// </summary>
    internal ObservableCollection<RemoteProcess>? Processes {
        get => _processes;
        set => SetProperty(ref _processes, value);
    }

    /// <summary>
    /// Gets the name associated with the current SFTP connection.
    /// This property retrieves the value from the <see cref="ConnectionProperty"/> object, which represents
    /// the details of the active connection. If <see cref="ConnectionProperty"/> is null, an empty string is returned.
    /// </summary>
    internal string Name {
        get {
            if (string.IsNullOrEmpty(_name)) {
                return ConnectionProperty?.Name ?? "";
            }

            return _name;
        }
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// Represents the view model for managing file system operations,
    /// including interactions with local and remote file systems.
    /// Provides properties and methods to facilitate file browsing,
    /// connection management, and handling user interactions.
    /// </summary>
    /// <remarks>
    /// This class is responsible for the coordination and management of
    /// file system objects across both local and remote environments.
    /// It interacts with SFTP/SSH clients and offers mechanisms for updating the UI.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when mandatory dependencies such as connection property or owner view model are null.
    /// </exception>
    public FileViewModel(SftpConnectionProperty connectionProperty, MainWindowViewModel? owner,
        CancellationTokenSource cancellationTokenSource) {
        IsBusy = true;
        Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        CancellationTokenSource = cancellationTokenSource;
        WorkStatus = "Checking connection status...";
        Task.Run(() => {
            _connectionProperty =  connectionProperty ?? throw new ArgumentNullException(nameof(connectionProperty));
            ConfigureConnection(connectionProperty, cancellationTokenSource.Token); 
        }, cancellationTokenSource.Token);
        
        _aTimer.Interval = ConfigurationHandler.Current.RefreshInterval;
        _aTimer.Elapsed += RefreshRemoteData;
        _aTimer.AutoReset = true;
        _aTimer.Enabled = true;
        IsBusy = false;
    }

    private void RefreshRemoteData(object? sender, ElapsedEventArgs e) {
        if (ConfigurationHandler.Current.DisableAutomaticFileRefresh) {
            return;
        }
        
        if (SshClient is not { IsConnected: true }) {
            return;
        }

        if (SftpClient is not { IsConnected: true }) {
            return;
        }

        if (!ConfigurationHandler.Current.DisableAutomaticFileRefresh) {
            Task.Run(() => RefreshFiles(false));
        }

        _ = UpdateRemoteProcessesAsync();
    }

    /// <summary>
    /// Closes the current connection and removes this <see cref="FileViewModel"/> instance
    /// from the owner's connection list.
    /// </summary>
    /// <remarks>
    /// If the owning <see cref="MainWindowViewModel"/> is null, the operation logs an error and terminates.
    /// Ensures that the owner no longer tracks the connection upon execution.
    /// </remarks>
    public void CloseConnection() {
        if (Owner == null) {
            Debugger.Log(0, "Error", "CloseConnection: Owner is null.");
            return;
        }
        
        Owner?.Connections.Remove(this);
    }
    
        /// <summary>
    /// Handles the double-click event to navigate into directories or interact with files.
    /// Determines the action based on whether the double click occurred on a remote file system
    /// or a local file system and processes the selected item accordingly.
    /// </summary>
    /// <param name="isRemote">
    /// A boolean value indicating whether the double-click event occurred on the remote file
    /// system (true) or on the local file system (false).
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown if the selected item index is invalid or out of range for the available items.
    /// </exception>
    /// <exception cref="NotImplementedException">
    /// Thrown when attempting to perform an unsupported operation, such as file execution or
    /// editing functionality that has not yet been implemented.
    /// </exception>
    internal void DoubleClicked(bool isRemote) {
        Debug.Assert(_remoteFiles != null);
        Debug.Assert(_files != null);
        Debug.Assert(_sftpClient != null);

        if (isRemote) {
            if (_selectedRemoteItem == -1) {
                SelectedRemoteItem = 0;
            }

            if (_remoteFiles.Count < _selectedRemoteItem) {
                return;
            }

            if (_connectionProperty == null) {
                throw new ArgumentException("The connection property is not set, which means we can not establish any connections. " +
                                            "Please let us know how you generated this issue so we can resolve it in the future.");
            }
            
            var remoteDir = _remoteFiles[_selectedRemoteItem];
            switch (remoteDir)
            {
                case FolderModel _:
                {
                    if (remoteDir.Name == "...") {
                        remoteDir.Name = "";
                    }

                    var path = $"{remoteDir.Path}/{remoteDir.Name}";
                    var newDir = FileIndexing.GetFileSystemObjectsForRemoteDir(_sftpClient, path);

                    //This is caused by a permission-denied exception
                    if (newDir == null) {
                        ShowPermissionDeniedDialog();
                        return;
                    }

                    var parentPath = path.Split('/').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    var newParentPath = "";
                    for (var index = 0; index < parentPath.Count; index++) {
                        //If we only have one parent that means our parent is likely just / (root)
                        if (parentPath.Count == 1) {
                            newParentPath = "/";
                        }

                        if (index == parentPath.Count - 1) {
                            break;
                        }

                        newParentPath += $"/{parentPath[index]}";
                    }

                    if (!string.IsNullOrWhiteSpace(newParentPath)) {
                        newDir.Insert(0, new FolderModel() {
                            Children = new ObservableCollection<IFileSystemObject>(newDir),
                            Name = "...",
                            FAIcon = "",
                            Path = newParentPath
                        });
                    }

                    RemotePath = path;
                    RemoteFiles = new ObservableCollection<IFileSystemObject>(newDir);
                    break;
                }
                case FileModel remoteFile:
                {
                    var tempFile = string.Empty;
                    if (remoteFile.FullName == null) {
                        tempFile = Path.GetTempPath() + "/" + Path.GetRandomFileName() +'.' + remoteFile.Extension;
                    }
                    else {
                        tempFile = Path.GetTempPath() + remoteFile.FullName.Split('/').LastOrDefault();
                    }

                    //Make sure we are not changing our file locally only.
                    if (File.Exists(tempFile)) {
                        File.Delete(tempFile);
                    }
                    
                    var file = _sftpClient.Get(remoteFile.FullName);
                    FileTransfer.DownloadFile(_sftpClient, file, tempFile);
                    var process = new Process() {
                        StartInfo = new ProcessStartInfo(tempFile) {
                            UseShellExecute = true
                        }, EnableRaisingEvents = true
                    };
                    process.Start();
                    
                    var watcher = new FileSystemWatcher(Path.GetTempPath());
                    watcher.Filter = Path.GetFileName(tempFile);
                    watcher.EnableRaisingEvents = true;
                    watcher.IncludeSubdirectories = false;
                    watcher.NotifyFilter = NotifyFilters.LastWrite;
                    watcher.Changed += (sender, args) => {
                        try {
                            if (sender.GetType() != typeof(FileSystemWatcher)) {
                                return;
                            }
                            
                            FileSystemObject fsObject = new FileSystemObject(new FileInfo(tempFile));
                            FileTransfer.UploadFile(_sftpClient, fsObject, tempFile, remoteFile.FullName);
                        }
                        catch (Exception e) {
                            ShowError("An unknown error occured while trying to edit the remote file.", e.Message);
                        }
                    };

                    if (process.ExitCode != 0) {
                        ShowError("An error occured while trying to execute the remote file.", $"The process exited with code {process.ExitCode}.");
                    }
                    break;
                }
            }



            return;
        }
        
        if (_files.Count < SelectedItem) {
            return;
        }

        var dir = _files[SelectedItem];
        switch (dir) {
            case FolderModel _: {
                if (dir.Name == "...") {
                    dir.Name = "";
                }

                var path = string.Empty;
                var directories = new List<IFileSystemObject>();

                //We need to differentiate between macOS/linux and windows here since drive maps can get fundamentally different
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                    path = Path.Combine(dir.Path, dir.Name);
                    var newDir = FileIndexing.GetFileSystemObjectsForDir(path);
                    var d = new DirectoryInfo(path);
                
                    if (d.Parent != null) {
                        newDir.Insert(0, new FolderModel()
                        {
                            Children = new ObservableCollection<IFileSystemObject>(newDir),
                            Name = "...",
                            FAIcon = "",
                            Path = Path.GetDirectoryName(dir.Path)
                        });
                    }

                    directories = newDir;
                }
                else {
                    if (string.IsNullOrEmpty(dir.Path) && string.IsNullOrEmpty(dir.Name)) {

                        var drives = DriveInfo.GetDrives();
                        foreach (var drive in drives) {
                            if (!drive.IsReady) {
                                continue;
                            }

                            var driveFolderModel = FolderModel.GetFolderModel(drive.Name);
                            if (driveFolderModel != null) {
                                directories.Add(driveFolderModel);
                            }
                        }
                    }
                    else {
                        path = Path.Combine(dir.Path, dir.Name);
                        var newDir = FileIndexing.GetFileSystemObjectsForDir(path);
                        newDir.Insert(0, new FolderModel() {
                            Children = new ObservableCollection<IFileSystemObject>(newDir),
                            Name = "...",
                            FAIcon = "",
                            Path = Path.GetDirectoryName(dir.Path)
                        });

                        directories = newDir;
                    }

                }

                LocalPath = path;
                Files = new ObservableCollection<IFileSystemObject>(directories);
                break;
            }
            case FileModel file:
            {
                if (file.FullName == null) {
                    ShowError("Unable to edit file", "The file you selected does not have a valid path.\n" +
                                                     "This may be because the filesystem is outdated,\n" +
                                                     "please refresh your filesystem and try again.\n");
                    return;
                }

                try {
                    var process = new Process() {
                        StartInfo = new ProcessStartInfo(file.FullName) {
                            UseShellExecute = true
                        }
                    };
                    process.Start();
                    
                    if (process.ExitCode != 0) {
                        ShowError("An error occured while trying to execute the remote file.", $"The process exited with code {process.ExitCode}.");
                    }
                }
                catch (Exception e) {
                    ShowError("An unknown error occured while trying to edit the file.", e.Message);
                }
                break;
            }
            default:
            {
                ShowError("Unable to perform action", "You encountered an action that has yet to be defined / programmed.\n" +
                                                      "Please report this as a bug with the exact file type you were trying to edit and how you ended up here.\n " +
                                                      "Thanks.");
                break;
            }
        }
    }
    
    /// <summary>
    /// Cancels an ongoing operation by signaling the associated cancellation token.
    /// Ensures that no further processing occurs if a cancellation request is already active.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the associated cancellation token source has already been disposed of before calling this method.
    /// </exception>
    internal void CancelAction() {
        if (CancellationTokenSource is { IsCancellationRequested: true }) {
            return;
        }
        
        CancellationTokenSource?.Cancel();
    }
    
    internal void Disconnect() {
        CancelAction();
        SftpClient?.DisconnectSafe();
        SshClient?.DisconnectSafe();
        CloseConnection();
    }

    /// <summary>
    /// Asynchronously configures and establishes connections to both SFTP and SSH servers
    /// using the provided connection properties. Validates the connection status, handles
    /// connection errors, and updates the connection state of the ViewModel.
    /// </summary>
    /// <param name="connectionProperty">
    /// An instance of <see cref="SftpConnectionProperty"/> containing the credentials and
    /// configuration necessary to establish the SFTP and SSH connections.
    /// </param>
    /// <param name="token">
    /// A <see cref="CancellationToken"/> used to support cancellation of the connection process.
    /// Defaults to <see cref="CancellationToken.None"/> if not provided.
    /// </param>
    /// <returns>
    /// A task that resolves to a boolean indicating whether the connections to both SFTP
    /// and SSH servers were successfully established. Returns <c>true</c> if successful;
    /// otherwise, returns <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="connectionProperty"/> is null.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Thrown when the operation is canceled via the provided <paramref name="token"/>.
    /// </exception>
    internal async Task<bool> ConfigureConnectionAsync(SftpConnectionProperty connectionProperty,
        CancellationToken token = default) {
        IsBusy = true;
        WorkStatus = "Checking sftp connection status...";
        var sftpConnectionResult = await connectionProperty.CreateSftpClient(token);
        
        WorkStatus = "Checking ssh connection status...";
        var sshConnectionResult = await connectionProperty.CreateSshClient(token);
        
        if (!sftpConnectionResult.Item2) {
            ConnectionErrors += sftpConnectionResult.Item3;
        }

        if (!sshConnectionResult.Item2 && sftpConnectionResult.Item3 != ConnectionErrors) {
            ConnectionErrors += "\n\r" + sftpConnectionResult.Item3;
        }

        if (!string.IsNullOrEmpty(ConnectionErrors)) {
            IsBusy = false;
            return false;
        }
        
        _sftpClient = sftpConnectionResult.Item1;
        _sshClient = sshConnectionResult.Item1;
        Debug.Assert(_sftpClient != null);
        Debug.Assert(_sshClient != null);
        
        _sshClient.HostKeyReceived += VerifyHostKeys;
        _sshClient.ConnectSafe();
        _sftpClient.ConnectSafe();
        ShellStreams.Add(new ShellItemViewModel("Shell 1", 
            _sshClient.CreateShellStream("vt-100", 80, 60, 800, 600, 65536), this));
        
        if (_sshClient.IsConnected) {
            RefreshFiles();
        }
        
        IsBusy = false;
        return true;
    }
    
    
    /// <summary>
    /// Updates the remote processes list by executing the "top" command on the remote system.
    /// Supports macOS (Darwin) and Linux.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    internal async Task UpdateRemoteProcessesAsync()
    {
        if (SshClient == null || !SshClient.IsConnected) {
            return;
        }

        var command = string.Empty;
        if (RemoteOs.HasFlag(SSHOs.MacOS)) {
            // macOS top command: -l 1 for one iteration, -o cpu to sort by CPU, -n 20 for top 20 processes
            command = "top -l 1 -o cpu -n 20";
        }
        else if (RemoteOs.HasFlag(SSHOs.Linux)) {
            // Linux top command: -b for batch mode, -n 1 for one iteration
            command = "top -b -n 1 | head -n 30";
        }
        else if (RemoteOs.HasFlag(SSHOs.Windows)) {
            // Fallback or unsupported OS
            return;
        }else if (RemoteOs.HasFlag(SSHOs.BSD)) {
            return;
        }

        if (string.IsNullOrWhiteSpace(command)) {
            return;
        }

        await Task.Run(() =>
        {
            var result = SshClient.RunCommand(command);
            var output = result.Execute();
            var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            var newProcesses = new System.Collections.Concurrent.ConcurrentBag<RemoteProcess>();

            // Use Parallel.ForEach to parse lines as requested
            Parallel.ForEach(lines, line =>
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) {
                    return;
                }

                // Simple heuristic to find the header and skip it
                if (trimmedLine.StartsWith("PID", StringComparison.OrdinalIgnoreCase)) {
                    return;
                }

                // Check if it's a process line (starts with a digit PID)
                if (!char.IsDigit(trimmedLine[0])) {
                    return;
                }

                var parts = trimmedLine.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4) {
                    return;
                }

                try {
                    var process = new RemoteProcess();
                    if (RemoteOs.HasFlag(SSHOs.MacOS) && parts.Length > 11) {
                        // macOS: PID COMMAND %CPU TIME ...
                        process.Name = parts[1];
                        process.CpuUsage = double.Parse(parts[2].TrimEnd('%'));
                        process.Owner = parts.Length > 12 ? parts[12] : "Unknown"; // UID/User usually later
                        // Memory and Runtime parsing could be more complex, keeping it simple for now
                        if (TimeSpan.TryParse(parts[3], out var rt)) {
                            process.RunTime = rt;
                        }
                    }
                    else {
                        // Linux: PID USER PR NI VIRT RES SHR S %CPU %MEM TIME+ COMMAND
                        process.Name = parts.Last();
                        process.Owner = parts[1];
                        process.CpuUsage = double.Parse(parts[8]);
                        process.MemoryUsage = double.Parse(parts[9]);
                        // Time parsing for Linux top (e.g., 0:02.13 or 123:45.67)
                        if (parts.Length > 10) {
                            var timeStr = parts[10];
                            // top's TIME+ format is often minutes:seconds.hundredths
                            var timeParts = timeStr.Split([':', '.']);
                            if (timeParts.Length == 3) {
                                if (int.TryParse(timeParts[0], out var minutes) &&
                                    int.TryParse(timeParts[1], out var seconds) &&
                                    int.TryParse(timeParts[2], out var hundredths)) {
                                    process.RunTime = new TimeSpan(0, 0, minutes, seconds, hundredths * 10);
                                }
                            }
                            else if (timeParts.Length == 2) {
                                // Fallback for MM:SS
                                if (int.TryParse(timeParts[0], out var minutes) &&
                                    int.TryParse(timeParts[1], out var seconds)) {
                                    process.RunTime = new TimeSpan(0, 0, minutes, seconds, 0);
                                }
                            }
                        }
                    }
                    
                    
                    //Ignore processes that are sleeping or have no CPU usage
                    if ((process.Name == "sleeping" || process.CpuUsage == 0.0) && !ConfigurationHandler.Current.ShowIdleRemoteTasks) {
                        return;
                    }
                    
                    newProcesses.Add(process);
                }
                catch(Exception ex) {
                    Debugger.Log(0, "Error", $"Failed to parse top output line: {ex.Message}");
                }
            });

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Processes == null) {
                    Processes = new ObservableCollection<RemoteProcess>();
                }
                else {
                    Processes.Clear();
                }

                Processes = new ObservableCollection<RemoteProcess>(newProcesses.OrderByDescending(x => x.CpuUsage).ToList());
            }, DispatcherPriority.Background);
        });
    }
    
    /// <summary>
    /// Refreshes the file and directory listings for both the local and remote file systems.
    /// Establishes safe connections to the remote server, retrieves information about the remote
    /// operating system, updates the remote OS icon, and synchronizes the current local and
    /// remote directory structures. Performs necessary data persistence when the connection
    /// property is set to remember the configuration.
    /// </summary>
    /// <exception cref="UnhandledErrorException">
    /// Thrown if an error occurs while retrieving the remote directory structure or if the
    /// default directory structure is null.
    /// </exception>
    /// <remarks>
    /// This method sets up and validates required components, including connections through
    /// SSH and SFTP clients, and handles the configuration states appropriately. Local and
    /// remote file collections are initialized, and event handlers for collection changes are
    /// registered. The method also ensures synchronization of OS-specific icon information
    /// within the relevant data properties and database context.
    /// </remarks>
    internal void RefreshFiles(bool showUpdate = true) {
        IsBusy = showUpdate;
        WorkStatus = "Gathering info about the os...";
        Debug.Assert(SshClient != null);
        Debug.Assert(SftpClient != null);
        Debug.Assert(ConnectionProperty != null);

        SshClient.ConnectSafe();
        SftpClient.ConnectSafe();
        
        var remoteOs =  OsProbe.GetRemoteOs(SshClient);
        if (string.IsNullOrEmpty(remoteOs.Item1)) {
            ShowError("Unable to fetch remote OS Icon", 
                "Unable to fetch remote OS information, this is likely a connection error. We recommend terminating this connection and trying again.");
        }

        if ((remoteOs.Item2 & SSHOs.Unknown) == SSHOs.Unknown) {
            ShowError("Unable to fetch remote OS Icon", 
                "Unable to fetch remote OS information, this is likely a connection error. We recommend terminating this connection and trying again." +
                "This could also be caused by connecting to an unsupported operating system. If you're connecting to an operating system that's unknown to us, " +
                "please reach out to support. The Email can be found in the about section of the application.");
        }
        
        RemoteOsIcon = remoteOs.Item1;
        RemoteOs = remoteOs.Item2;
        ConnectionProperty.HostOsIcon = RemoteOsIcon;
        if (ConnectionProperty.Remember) {
            var context = new SqlLiteDbContext();
            context.SftpConnectionProperties.Update(ConnectionProperty);
            context.SaveChanges();
        }

        //Get our local files
        WorkStatus = "Getting local os directory structure...";
        if (string.IsNullOrEmpty(RemotePath)) {
            LocalPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        
        var homedir = FileIndexing.GetFileSystemObjectsForDir(LocalPath);
        homedir.Insert(0, new FolderModel() {
            Children = new ObservableCollection<IFileSystemObject>(homedir),
            Name = "...",
            FAIcon = "",
            Path = Path.GetDirectoryName(LocalPath) 
        });
        
        //Get our remote files
        WorkStatus = "Getting remote os directory structure...";
        var remoteDir = new List<IFileSystemObject>();
        
        if (RemotePath != null) {
            remoteDir = FileIndexing.GetFileSystemObjectsForRemoteDir(SftpClient, RemotePath);
        } else {
            remoteDir = FileIndexing.GetFileSystemObjectsForRemoteDir(SftpClient);
        }
        
        RemotePath = SftpClient.WorkingDirectory;
        
        if (remoteDir == null) {
            ConnectionErrors += "Failed to retrieve remote files. Please ensure you have the correct permissions to list them.\n";
            return;
        }

        var parentPath = remoteDir.FirstOrDefault()?.Path?.Split('/');
        if (parentPath != null && parentPath.Length > 0) {
            var path = string.Empty;
            parentPath = parentPath.Take(parentPath.Length - 1).ToArray();
            path = parentPath.Aggregate(path, (current, subPath) => current + $"/{subPath}");

            remoteDir.Insert(0, new FolderModel() {
                Children = new ObservableCollection<IFileSystemObject>(homedir),
                Name = "...",
                FAIcon = "",
                Path = path
            });
        }
        else {
            ConnectionErrors += "Failed to retrieve remote files. Please ensure you have the correct permissions to list them.\n";
        }
        
        
        if (Files == null) {
            //This is to minimize the amount of events we register
            homedir = homedir.OrderBy(x => x.Name).ToList();
            Files = new ObservableCollection<IFileSystemObject>(homedir);
            Files.CollectionChanged += LocalFilesChanged;
        }
        else {
            var localFilesAreEqual = !Enumerable.SequenceEqual(
                homedir.OrderBy(x => x.Name).ToList(), 
                Files.OrderBy(x => x.Name).ToList());
            var  localLengthEqual = homedir.Count == Files.Count;
            
            if (!localFilesAreEqual || !localLengthEqual) {
                Files.CollectionChanged -= LocalFilesChanged;
                Files = new ObservableCollection<IFileSystemObject>(homedir);
                Files.CollectionChanged += LocalFilesChanged;
            } 
        }

        if (RemoteFiles == null) {
            remoteDir = remoteDir.OrderBy(x => x.Name).ToList();
            RemoteFiles = new ObservableCollection<IFileSystemObject>(remoteDir);
            RemoteFiles.CollectionChanged += RemoteFilesChanged;
        }
        else {
            var localFilesAreEqual = !Enumerable.SequenceEqual(
                remoteDir.OrderBy(x => x.Name).ToList(), 
                RemoteFiles.OrderBy(x => x.Name).ToList());
            var  remoteLengthEqual = remoteDir.Count == RemoteFiles.Count;
            
            if (!localFilesAreEqual || !remoteLengthEqual) {
                RemoteFiles.CollectionChanged -= RemoteFilesChanged;
                RemoteFiles = new ObservableCollection<IFileSystemObject>(remoteDir);
                RemoteFiles.CollectionChanged += RemoteFilesChanged;
            } 
        }
        
        IsBusy = false;
    }

    /// <summary>
    /// Executes a given script file on a remote system using an established SSH and SFTP connection.
    /// Handles the process of uploading the script to a temporary remote location,
    /// navigating to the appropriate directory, and executing the script using the appropriate command based
    /// on the file extension.
    /// </summary>
    /// <param name="file">The script file to be executed on the remote system.</param>
    /// <remarks>
    /// The method requires active SSH and SFTP connections to function properly. If either connection is missing,
    /// it attempts to establish the connection before proceeding. The script is uploaded to a temporary remote
    /// location, and its directory is navigated before execution. Python scripts (with a ".py" extension)
    /// are executed using the `python3` interpreter, while other scripts are executed directly as binaries.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the file object is null or contains invalid data that prevents execution.
    /// </exception>
    /// <exception cref="IOException">
    /// Thrown when errors occur during file upload or stream operations.
    /// </exception>
    internal void RunScript(System.IO.FileInfo file) {
        if (SshClient is not { IsConnected: true }) {
            ShowError("Unable to run script", "The SSH connection is not active. Please wait for a moment while we try to establish a connection.");
            SshClient?.ConnectSafe();
            return;
        }

        if (SftpClient is not { IsConnected: true }) {
            ShowError("Unable to run script", "The SFTP connection is not active. Please wait for a moment while we try to establish a connection.");
            SftpClient?.ConnectSafe();
            return;
        }
        var guid = Guid.NewGuid(); 
        //Create our remote path for the file to be uploaded to
        var remoteFile = FileTransfer.GetRemoteTempPath(SshClient, file.Extension);
        FileTransfer.UploadFile(SftpClient, new FileSystemObject(file), file.FullName, remoteFile);
        var shellStream = SshClient.CreateShellStream("vt-100", 80, 60, 800, 600, 65536);
        if (ShellStreams.FirstOrDefault(x => x.Header == $"{file.Name}-exec") is { } shell) {
            shellStream = shell.ShellStream;
        }
        else {
            var viewModel = new ShellItemViewModel($"{file.Name}-exec", shellStream, this);
            ShellStreams.Add(viewModel);
        }
        
        var remoteDir = remoteFile.Split('/');
        remoteDir = remoteDir.Take(remoteDir.Length - 1).ToArray();
        var cdExec = remoteDir.Aggregate(string.Empty, (current, subPath) => current + $"/{subPath}");
        var exec = string.Empty;
        if (file.Extension == ".py") {
            exec = $"python3 {remoteFile.Split('/').Last()}";
        }
        else {
            exec = $"./{remoteFile.Split('/').Last()}";
        }
        
        shellStream.WriteLine($"cd {cdExec}");
        shellStream.WriteLine(exec);
        shellStream.Flush();
    }

    /// <summary>
    /// Verifies the host key during the SSH connection establishment and handles key validation
    /// logic, including updates to stored key hashes, prompting for confirmation, and disconnecting
    /// from untrusted hosts.
    /// </summary>
    /// <remarks>
    /// This method ensures the security of the SSH connection by validating the host key fingerprint.
    /// It updates the stored key hash in the database if necessary and disconnects from the server
    /// if the key is untrusted or differs from the stored value. Additionally, the method raises the
    /// <c>KeyChange</c> event to notify about changes in the host key.
    /// </remarks>
    /// <param name="sender">The sender of the event, typically the <c>SftpClient</c> that triggered the host key check.</param>
    /// <param name="hostKeyEventArgs">The event arguments containing details of the host key, including its fingerprint.</param>
    private void VerifyHostKeys(object? sender, HostKeyEventArgs hostKeyEventArgs) {
        if (ConnectionProperty == null || SshClient == null || SftpClient == null) {
            Debugger.Log(0, "Error", "VerifyHostKeys: ConnectionProperty or SshClient or SftpClient is null.");
            return;
        }
        
        var hash = SHA256.HashData(hostKeyEventArgs.FingerPrint);
        var hexHash = Convert.ToHexString(hash);
        ConnectionProperty.KeyHash = hexHash;
        var dbContext = new SqlLiteDbContext();
            
        //Check if we created a host
        var entry = dbContext.SftpConnectionProperties.FirstOrDefault(
            x => x.Name == ConnectionProperty.Name && x.HostOsIcon == ConnectionProperty.HostOsIcon);

        var keyMatches = entry != null && !string.IsNullOrEmpty(entry.KeyHash) && entry.KeyHash == hexHash;
        var autoTrustNew = entry == null && ConnectionProperty.Remember;

        //Check that we have a key hash and that it matches the one we have, or that we want to remember the connection (for new hosts), 
        //otherwise always prompt for confirmation if not already trusted in this session.
        if (IsKeyTrusted || keyMatches || autoTrustNew) {
            hostKeyEventArgs.CanTrust = true;
            return;
        }
        
        //Let's disconnect to make sure that we don't have any open connections to an untrusted party
        hostKeyEventArgs.CanTrust = false;
        SftpClient.DisconnectSafe();
        SshClient.DisconnectSafe();
            
        KeyChange?.Invoke(this, hexHash);
    }

        /// <summary>
    /// Configures the SFTP connection using the specified connection property and handles
    /// the connection process asynchronously while supporting cancellation.
    /// Ensures that essential clients are properly initialized and updates the work status
    /// after the connection attempt completes.
    /// </summary>
    /// <param name="connectionProperty">
    /// An instance of <see cref="SftpConnectionProperty"/> that contains the necessary
    /// details for establishing the connection, such as host, port, and user credentials.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that allows the asynchronous connection task to be canceled. The default
    /// value is <see cref="CancellationToken.None"/> if no token is provided.
    /// </param>
    /// <returns>
    /// A boolean value indicating whether the connection setup was successful.
    /// </returns>
    /// <exception cref="NullReferenceException">
    /// Thrown if the provided <paramref name="connectionProperty"/> is null.
    /// </exception>
    private bool ConfigureConnection(SftpConnectionProperty connectionProperty,
        CancellationToken cancellationToken = default) {
        if (connectionProperty == null) {
            throw new NullReferenceException(nameof(connectionProperty));
        }
        
        return Task.Run(async () => {
                IsBusy = true;
                await ConfigureConnectionAsync(connectionProperty, cancellationToken);
            }, 
            cancellationToken).ContinueWith((_) => {
            
                if (_sftpClient == null || _sshClient == null) {
                    IsBusy = false;
                    WorkStatus = "Connection Status failed...";
                    ConnectionErrors += "An unknown error occured while trying to establish a connection.";
                    Debug.WriteLine($"An unknown error occured while trying to establish a connection. This error occured at: \n" +
                                    $"{MethodBase.GetCurrentMethod()?.Name}\n" +
                                    $"in: " +
                                    $"\n {MethodBase.GetCurrentMethod()?.MethodHandle.Value}", "Error");
                    return false;
                }
                
                WorkStatus = "Connection successful.";
                Debug.Assert(_sshClient != null);
                Debug.Assert(_sftpClient != null);
                IsBusy = false;
                
                
                
                
                return true;
        }, cancellationToken).ConfigureAwait(true).GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Displays a permission-denied dialog to inform the user that access to a specified resource
    /// or folder is restricted. This dialog provides details about the lack of sufficient
    /// credentials and instructs the user on possible actions to resolve the issue.
    /// </summary>
    private void ShowPermissionDeniedDialog() {
        ShowError("🚫 Access Denied",
            "Sorry, you don’t have permission to access this folder.\n\n" +
            "It looks like your current credentials don’t grant you entry here.\n" +
            "If you believe this is an error, please contact your administrator.\n\n" +
            "(Or double-check you're in the right place — even FTP clients get lost sometimes.)\n\n" +
            "To exit this message just click anywhere outside this dialog.");
    }

    /// <summary>
    /// Displays an error message in a dialog with the specified title and error content.
    /// </summary>
    /// <param name="title">The title of the error message.</param>
    /// <param name="error">The detailed error message to be shown in the dialog.</param>
    private void ShowError(string title, string error) {
        if (ConfigurationHandler.Current.CollectData) {
            var sentryData = "User ran into issue:\n" + error + "\nTrace is:\n" + Environment.StackTrace;
            SentrySdk.CaptureMessage(sentryData, SentryLevel.Warning);
        }
        
        DialogHostAvalonia.DialogHost.Show(
            $"{title}\n" +
            $"{error}");
    }

    /// <summary>
    /// Handles changes to the remote files collection, initiating a file transfer when remote files are moved to the
    /// local folder.
    /// </summary>
    /// <remarks>This method responds to collection change events where remote files are moved to the local
    /// folder. When triggered, it initiates an asynchronous file transfer to ensure files are properly transferred and
    /// prevents UI sluggishness by handling the operation in the background.</remarks>
    /// <param name="sender">The source of the event, expected to be an observable collection of file system objects when files are moved.</param>
    /// <param name="e">The event data containing information about the collection change, including the items added or removed.</param>
    private void RemoteFilesChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (_connectionProperty == null) {
            return;
        }
        
        
        Debug.Assert(_sftpClient != null,  nameof(_sftpClient) + " != null");
                    
        //Ensure our remote filesystem is up to date.
        Dispatcher.UIThread.InvokeAsync(() => {
            RefreshFiles();
        }, DispatcherPriority.Background);

        //This means we're moving a Remote file to the local folder
        if (e.NewItems == null && e.Action == NotifyCollectionChangedAction.Remove) {
            return;
        }
        
        foreach (var item in e.NewItems)
        {
            if (item is not IFileSystemObject fsObject) {
                continue;
            }
            
            var desPath = RemotePath + "/" + fsObject.Name;
            if (string.IsNullOrEmpty(desPath)) {
                continue;
            }
            
            Dispatcher.UIThread.InvokeAsync(() => {
                RefreshFiles();
            }, DispatcherPriority.Background);
            Task.Run(() =>
            {
                var transfer = new FileTransfer();
                transfer.StartFileTransferAsync(fsObject, desPath,
                        FileTransfer.FileTransferType.HostToRemote, CancellationToken.None, _sftpClient)
                    .GetAwaiter()
                    .GetResult();
                this.RefreshFiles();
            });
        }
    }

    /// <summary>
    /// Handles changes to the local files collection, initiating file transfers to a remote location when files are
    /// moved.
    /// </summary>
    /// <remarks>This method responds to collection change events by transferring files from the local
    /// collection to a remote folder when items are moved. The transfer is performed asynchronously to avoid blocking
    /// the UI thread.</remarks>
    /// <param name="sender">The source of the event, typically an observable collection of file system objects representing local files.</param>
    /// <param name="e">The event data containing information about the collection change, including which items were added, removed, or
    /// moved.</param>
    private void LocalFilesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_connectionProperty == null) {
            return;
        }
        
        Debug.Assert(_sftpClient != null, nameof(_sftpClient) + " != null");
                    
        //Ensure our remote filesystem is up to date.
        Dispatcher.UIThread.InvokeAsync(() => {
            RefreshFiles();
        }, DispatcherPriority.Background);
        
        //This means we're moving a local file to a remote folder:)
        //This means we're moving a Remote file to the local folder
        if (e.NewItems == null && e.Action == NotifyCollectionChangedAction.Remove) {
            return;
        }
        
        
        foreach (var item in e.NewItems) {
            if (item is not IFileSystemObject fsObject) {
                continue;
            }
            
            var destPath = LocalPath + "/" + fsObject.Name;
            if (string.IsNullOrEmpty(destPath)) {
                continue;
            }
            
            Dispatcher.UIThread.InvokeAsync(() => {
                RefreshFiles();
            }, DispatcherPriority.Background);
            Task.Run(() => {
                var transfer = new FileTransfer();
                transfer.StartFileTransferAsync(fsObject, 
                    destPath, 
                    FileTransfer.FileTransferType.RemoteToHost, 
                    CancellationToken.None, _sftpClient).GetAwaiter().GetResult();
                this.RefreshFiles();
            });
        }
    }
}