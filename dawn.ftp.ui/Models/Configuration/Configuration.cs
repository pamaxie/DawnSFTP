using CommunityToolkit.Mvvm.ComponentModel;

namespace dawn.ftp.ui.Models.Configuration;

public class Configuration : ObservableObject, IConfiguration
{
    private bool _enableAdvanced;
    private ulong _refreshInterval = 2000;
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
    private bool _collectData = true;

    /// <summary>
    /// Gets or sets a value indicating whether advanced features are enabled within the application.
    /// </summary>
    /// <remarks>
    /// This property controls the availability of advanced functionalities or tools that are not part of
    /// the basic configuration. Enabling these features may modify the application behavior to expose
    /// additional options or detailed settings. Use caution when enabling these features as they
    /// may require a deeper understanding of the application.
    /// </remarks>
    public bool EnableAdvanced {
        get => _enableAdvanced;
        set => SetProperty(ref _enableAdvanced, value);
    }

    /// <summary>
    /// Gets or sets the interval, in milliseconds, at which the application refreshes its data or user interface.
    /// </summary>
    /// <remarks>
    /// This property defines the time period between consecutive refresh operations.
    /// Adjusting this value allows customization of the refresh frequency, balancing responsiveness
    /// and system performance. A higher interval reduces the refresh rate and conserves resources,
    /// while a lower interval increases the refresh rate for up-to-date data visibility.
    /// </remarks>
    public ulong RefreshInterval {
        get => _refreshInterval;
        set => SetProperty(ref _refreshInterval, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether files on remote systems should be compressed into a zip archive.
    /// </summary>
    /// <remarks>
    /// This property controls the ability to automatically zip files located on remote systems.
    /// Enabling this option may reduce file transfer times and storage requirements by compressing
    /// remote files before transfer or storage. Disabling it allows the files to remain in their original format.
    /// </remarks>
    public bool ZipRemoteFiles {
        get => _zipRemoteFiles;
        set => SetProperty(ref _zipRemoteFiles, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether idle remote tasks are displayed within the user interface.
    /// </summary>
    /// <remarks>
    /// This property determines whether tasks that are in an idle state on remote systems should be visible.
    /// Enabling this option provides a comprehensive view of all remote tasks, including idle ones,
    /// while disabling it can declutter the interface by hiding such tasks.
    /// </remarks>
    public bool ShowIdleRemoteTasks {
        get => _showIdleRemoteTasks;
        set => SetProperty(ref _showIdleRemoteTasks, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the sizes of local files are indexed for metadata processing.
    /// </summary>
    /// <remarks>
    /// This property controls whether the application should include local file sizes during indexing operations.
    /// Enabling this option helps in obtaining detailed storage usage and statistics for local file systems,
    /// whereas disabling it may optimize performance in scenarios where size data is not required.
    /// </remarks>
    public bool IndexLocalFileSize {
        get => _indexLocalFileSize;
        set => SetProperty(ref _indexLocalFileSize, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the sizes of remote files are indexed for metadata processing.
    /// </summary>
    /// <remarks>
    /// This property determines if the application should include remote file sizes during indexing operations.
    /// Enabling this option allows for accurate storage accounting and detailed data analysis of remote file systems,
    /// while disabling it may improve performance for scenarios where size information is not critical.
    /// </remarks>
    public bool IndexRemoteFileSize {
        get => _indexRemoteFileSize;
        set => SetProperty(ref _indexRemoteFileSize, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether local directories are indexed for metadata or content processing.
    /// </summary>
    /// <remarks>
    /// This property specifies if the application should include local directories in its indexing operations.
    /// Enabling this option allows the application to scan and organize local directory structures for efficient
    /// search and analysis. Disabling it may improve performance by focusing on other prioritized tasks.
    /// </remarks>
    public bool IndexLocalDirectories {
        get => _indexLocalDirectories;
        set => SetProperty(ref _indexLocalDirectories, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether remote directories are indexed for metadata or content processing.
    /// </summary>
    /// <remarks>
    /// This property determines if the application should include remote directories in its indexing operations.
    /// When enabled, the application will analyze and catalog remote directory structures, allowing for improved
    /// search and organization of resources that are not locally stored. Disabling it may enhance performance
    /// by focusing exclusively on local resources or other prioritized tasks.
    /// </remarks>
    public bool IndexRemoteDirectories {
        get => _indexRemoteDirectories;
        set => SetProperty(ref _indexRemoteDirectories, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the transfer status information is hidden from the user interface.
    /// </summary>
    /// <remarks>
    /// This property controls the visibility of transfer status details within the application's interface.
    /// When activated, any user interface element displaying transfer status updates will be concealed,
    /// allowing users to focus on other aspects of the application without being distracted by transfer progress details.
    /// </remarks>
    public bool HideTransferStatus {
        get => _hideTransferStatus;
        set => SetProperty(ref _hideTransferStatus, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether successful transfer processes are hidden from the user interface.
    /// </summary>
    /// <remarks>
    /// This property determines the visibility of successful transfer operations in the application's interface.
    /// When enabled, UI components or features displaying information related to successfully completed transfers
    /// will be hidden. This can help reduce visual clutter for users who prefer to focus only on ongoing or
    /// problematic transfer activities.
    /// </remarks>
    public bool HideSuccessfulTransfers {
        get => _hideSuccessfulTransfers;
        set => SetProperty(ref _hideSuccessfulTransfers, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether failed transfer processes are hidden from the user interface.
    /// </summary>
    /// <remarks>
    /// This property controls the visibility of failed transfer operations within the application's interface.
    /// When enabled, UI components or features displaying information related to transfers that have failed will be hidden.
    /// This can be useful for users who wish to focus only on ongoing or successful operations and avoid clutter in the interface.
    /// </remarks>
    public bool HideFailedTransfers {
        get => _hideFailedTransfers;
        set => SetProperty(ref _hideFailedTransfers, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether active transfer processes are hidden from the user interface.
    /// </summary>
    /// <remarks>
    /// This property controls the visibility of active transfer operations within the application's interface.
    /// When enabled, UI elements or features displaying information related to active file transfers will be hidden.
    /// This can be useful to simplify the interface or to restrict visibility to this functionality.
    /// </remarks>
    public bool HideActiveTransfers {
        get => _hideActiveTransfers;
        set => SetProperty(ref _hideActiveTransfers, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the remote management view is hidden from the user interface.
    /// </summary>
    /// <remarks>
    /// This property is used to control the visibility of the remote management functionality in the application's interface.
    /// When enabled, it removes any associated UI elements or features related to managing remote systems.
    /// This is useful for scenarios where remote management capabilities are unnecessary or should be restricted.
    /// </remarks>
    public bool HideRemoteManagementView {
        get => _hideRemoteManagementView;
        set => SetProperty(ref _hideRemoteManagementView, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the remote performance monitoring features are hidden from the user interface.
    /// </summary>
    /// <remarks>
    /// This property serves as a mechanism to control the visibility of remote performance monitoring tools in the application.
    /// Enabling this property will conceal any UI components or prompts associated with monitoring remote system performance.
    /// It is suitable for use cases where remote performance details are not required or should remain inaccessible to users.
    /// </remarks>
    public bool HideRemotePerformanceMonitoring {
        get => _hideRemotePerformanceMonitoring;
        set => SetProperty(ref _hideRemotePerformanceMonitoring, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the execution of remote scripts is hidden from the user interface.
    /// </summary>
    /// <remarks>
    /// This property can be used to control the visibility of remote script execution features in the application.
    /// When enabled, any user prompts or interface elements related to remote script execution will be concealed,
    /// which may be useful in scenarios where those features are not intended for end-user access or visibility.
    /// </remarks>
    public bool HideRemoteScriptExecution {
        get => _hideRemoteScriptExecution;
        set => SetProperty(ref _hideRemoteScriptExecution, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the application should use a dark mode theme.
    /// </summary>
    /// <remarks>
    /// When enabled, the application's user interface will switch to a darker color scheme,
    /// which can be beneficial for reducing eye strain in low-light environments or improving
    /// aesthetic appearance as per user preference.
    /// </remarks>
    public bool UseDarkMode {
        get => _useDarkMode;
        set => SetProperty(ref _useDarkMode, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether local files should be compressed into a zip archive.
    /// </summary>
    /// <remarks>
    /// When enabled, local files will be automatically zipped during the application's operations.
    /// This can be useful for reducing storage usage or preparing files for transfer.
    /// </remarks>
    public bool ZipLocalFiles {
        get => _zipLocalFiles;
        set => SetProperty(ref _zipLocalFiles, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether automatic refresh functionality is disabled.
    /// </summary>
    /// <remarks>
    /// When set to <c>true</c>, the application will not perform automatic refresh operations.
    /// This setting can help reduce resource usage in scenarios where frequent automatic updates
    /// are unnecessary or undesirable.
    /// </remarks>
    public bool DisableAutomaticRefresh {
        get => _disableAutomaticRefresh;
        set => SetProperty(ref _disableAutomaticRefresh, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the automatic refresh of file data should be disabled.
    /// </summary>
    /// <remarks>
    /// When set to <c>true</c>, the application will not automatically refresh file data.
    /// This can be useful in scenarios where automatic updates to file information
    /// are not required or may consume unnecessary resources.
    /// </remarks>
    public bool DisableAutomaticFileRefresh {
        get => _disableAutomaticFileRefresh;
        set => SetProperty(ref _disableAutomaticFileRefresh, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the remote shell interface should be hidden in the user interface.
    /// </summary>
    /// <remarks>
    /// This property controls the visibility of the remote shell functionality.
    /// When set to <c>true</c>, the remote shell interface will not be displayed to the user.
    /// </remarks>
    public bool HideRemoteShell {
        get => _hideRemoteShell;
        set => SetProperty(ref _hideRemoteShell, value);
    }

    public ThemeVariant ThemeVariant {
        get => _themeVariant;
        set => SetProperty(ref _themeVariant, value);
    }

    public bool CollectData {
        get => _collectData;
        set => SetProperty(ref _collectData, value);
    }
}
