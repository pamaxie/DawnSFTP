using System.Diagnostics;
using Avalonia;
using dawn.ftp.ui.BusinessLogic;
using dawn.ftp.ui.Models.Configuration;

namespace dawn.ftp.ui.ViewModels;

/// <summary>
/// Represents the options view model used to manage and display
/// file extension relationships within the application.
/// This class serves as a bridge between the data layer and
/// UI components, providing observable properties to manage
/// the application's options.
/// </summary>
public class OptionsViewModel : ViewModelBase, IConfiguration
{
    private bool _enableAdvanced;
    private ulong _refreshInterval;
    private bool _zipRemoteFiles;
    private bool _showIdleRemoteTasks;
    private bool _indexLocalFileSize;
    private bool _indexRemoteFileSize;
    private bool _indexLocalDirectories;
    private bool _indexRemoteDirectories;
    private bool _hideTransferStatus;
    private bool _hideSuccessfulTransfers;
    private bool _hideFailedTransfers;
    private bool _hideActiveTransfers;
    private bool _hideRemoteManagementView;
    private bool _hideRemotePerformanceMonitoring;
    private bool _hideRemoteScriptExecution;
    private bool _useDarkMode;
    private bool _zipLocalFiles;
    private bool _disableAutomaticRefresh;
    private bool _disableAutomaticFileRefresh;
    private bool _hideRemoteShell;
    private ThemeVariant _themeVariant;
    private bool _collectData;

    /// <summary>
    /// Creates a new instance of <see cref="OptionsViewModel"/>
    /// </summary>
    public OptionsViewModel() {
        ConfigurationHandler.Refresh();
        var config = ConfigurationHandler.Current;
        this.EnableAdvanced = config.EnableAdvanced;
        this.RefreshInterval = config.RefreshInterval;
        this.ZipRemoteFiles = config.ZipRemoteFiles;
        this.ShowIdleRemoteTasks = config.ShowIdleRemoteTasks;
        this.IndexLocalFileSize = config.IndexLocalFileSize;
        this.IndexRemoteFileSize = config.IndexRemoteFileSize;
        this.IndexLocalDirectories = config.IndexLocalDirectories;
        this.IndexRemoteDirectories = config.IndexRemoteDirectories;
        this.HideTransferStatus = config.HideTransferStatus;
        this.HideSuccessfulTransfers = config.HideSuccessfulTransfers;
        this.HideFailedTransfers = config.HideFailedTransfers;
        this.HideActiveTransfers = config.HideActiveTransfers;
        this.HideRemoteManagementView = config.HideRemoteManagementView;
        this.HideRemotePerformanceMonitoring = config.HideRemotePerformanceMonitoring;
        this.HideRemoteScriptExecution = config.HideRemoteScriptExecution;
        this.UseDarkMode = config.UseDarkMode;
        this.ZipLocalFiles = config.ZipLocalFiles;
        this.DisableAutomaticRefresh = config.DisableAutomaticRefresh;
        this.DisableAutomaticFileRefresh = config.DisableAutomaticFileRefresh;
        this.HideRemoteShell = config.HideRemoteShell;
        this.ThemeVariant = config.ThemeVariant;
        this.CollectData = config.CollectData;
    }



    /// <summary>
    /// Saves the current view model state back to the global configuration.
    /// </summary>
    public void Save() {
        Apply();
        this.ExecuteClose(true);
    }

    public void Apply() {
        IConfiguration config = this;
        ConfigurationHandler.UpdateValues(config);
        ConfigurationHandler.Refresh();
        if (ConfigurationHandler.Current.UseDarkMode) {
            if (Application.Current != null) {
                Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
            }
        }
        else {
            if (Application.Current != null) {
                Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
            }
        }
    }
    
    public void Cancel() {
        this.ExecuteClose(false);
    }

    public bool EnableAdvanced {
        get => _enableAdvanced;
        set => SetProperty(ref _enableAdvanced, value);
    }

    public ulong RefreshInterval {
        get => _refreshInterval;
        set => SetProperty(ref _refreshInterval, value);
    }

    public bool ZipRemoteFiles {
        get => _zipRemoteFiles;
        set => SetProperty(ref _zipRemoteFiles, value);
    }

    public bool ShowIdleRemoteTasks {
        get => _showIdleRemoteTasks;
        set => SetProperty(ref _showIdleRemoteTasks, value);
    }

    public bool IndexLocalFileSize {
        get => _indexLocalFileSize;
        set => SetProperty(ref _indexLocalFileSize, value);
    }

    public bool IndexRemoteFileSize {
        get => _indexRemoteFileSize;
        set => SetProperty(ref _indexRemoteFileSize, value);
    }

    public bool IndexLocalDirectories {
        get => _indexLocalDirectories;
        set => SetProperty(ref _indexLocalDirectories, value);
    }

    public bool IndexRemoteDirectories {
        get => _indexRemoteDirectories;
        set => SetProperty(ref _indexRemoteDirectories, value);
    }

    public bool HideTransferStatus {
        get => _hideTransferStatus;
        set => SetProperty(ref _hideTransferStatus, value);
    }

    public bool HideSuccessfulTransfers {
        get => _hideSuccessfulTransfers;
        set => SetProperty(ref _hideSuccessfulTransfers, value);
    }

    public bool HideFailedTransfers {
        get => _hideFailedTransfers;
        set => SetProperty(ref _hideFailedTransfers, value);
    }

    public bool HideActiveTransfers {
        get => _hideActiveTransfers;
        set => SetProperty(ref _hideActiveTransfers, value);
    }

    public bool HideRemoteManagementView {
        get => _hideRemoteManagementView;
        set => SetProperty(ref _hideRemoteManagementView, value);
    }

    public bool HideRemotePerformanceMonitoring {
        get => _hideRemotePerformanceMonitoring;
        set => SetProperty(ref _hideRemotePerformanceMonitoring, value);
    }

    public bool HideRemoteScriptExecution {
        get => _hideRemoteScriptExecution;
        set => SetProperty(ref _hideRemoteScriptExecution, value);
    }

    public bool UseDarkMode {
        get => _useDarkMode;
        set => SetProperty(ref _useDarkMode, value);
    }

    public bool ZipLocalFiles {
        get => _zipLocalFiles;
        set => SetProperty(ref _zipLocalFiles, value);
    }

    public bool DisableAutomaticRefresh {
        get => _disableAutomaticRefresh;
        set => SetProperty(ref _disableAutomaticRefresh, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether automatic file refresh functionality is disabled in the application.
    /// </summary>
    /// <remarks>
    /// The <see cref="DisableAutomaticFileRefresh"/> property controls whether the system automatically refreshes
    /// file data or not. Disabling this feature suspends any automatic updates, potentially reducing resource
    /// consumption when real-time file tracking is unnecessary. Users may enable or disable this option based on
    /// their workflow or performance preferences.
    /// </remarks>
    public bool DisableAutomaticFileRefresh {
        get => _disableAutomaticFileRefresh;
        set => SetProperty(ref _disableAutomaticFileRefresh, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the remote shell should be hidden in the application interface.
    /// </summary>
    /// <remarks>
    /// The <see cref="HideRemoteShell"/> property determines the visibility of the remote shell feature
    /// within the user interface. Disabling this feature may simplify the interface for users who do not
    /// require remote shell functionality. Its availability may depend on the state of related settings,
    /// such as the visibility of the remote management view.
    /// </remarks>
    public bool HideRemoteShell {
        get => _hideRemoteShell;
        set => SetProperty(ref _hideRemoteShell, value);
    }

    /// <summary>
    /// Gets or sets the theme variant to be used in the user interface.
    /// </summary>
    /// <remarks>
    /// The <see cref="ThemeVariant"/> property defines the visual theme applied to the application.
    /// Available variants include Fluent, Classic, MacOs, Windows, and Linux, catering to different
    /// platform-specific aesthetics and user preferences.
    /// </remarks>
    public ThemeVariant ThemeVariant {
        get => _themeVariant;
        set => SetProperty(ref _themeVariant, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether data collection functionality is enabled in the application.
    /// </summary>
    /// <remarks>
    /// The <see cref="CollectData"/> property determines whether the application will actively collect
    /// and store relevant data during its operation. Enabling this functionality may be necessary for
    /// tracking system usage, performance metrics, or other analytical purposes. Users can toggle this
    /// setting based on their data collection requirements or privacy preferences.
    /// </remarks>
    public bool CollectData {
        get => _collectData;
        set => SetProperty(ref _collectData, value);
    }

    public void OpenPrivacyPolicy() {
        var process = new Process() {
            StartInfo = new ProcessStartInfo("https://sentry.io/privacy") {
                UseShellExecute = true
            }
        }.Start();
    }
}