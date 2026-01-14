namespace dawn.ftp.ui.Models.Configuration;

public interface IConfiguration {
    public bool EnableAdvanced { get; set; }
    public ulong RefreshInterval { get; set; }
    public bool ZipRemoteFiles { get; set; }
    public bool ShowIdleRemoteTasks { get; set; }
    public bool IndexLocalFileSize { get; set; }
    public bool IndexRemoteFileSize { get; set; }
    public bool IndexLocalDirectories { get; set; }
    public bool IndexRemoteDirectories { get; set; }
    public bool HideTransferStatus { get; set; }
    public bool HideSuccessfulTransfers { get; set; }
    public bool HideFailedTransfers { get; set; }
    public bool HideActiveTransfers { get; set; }
    public bool HideRemoteManagementView { get; set; }
    public bool HideRemotePerformanceMonitoring { get; set; }
    public bool HideRemoteScriptExecution { get; set; }
    public bool UseDarkMode { get; set; }
    public bool ZipLocalFiles { get; set; }
    public bool DisableAutomaticRefresh { get; set; }
    public bool DisableAutomaticFileRefresh { get; set; }
    public bool HideRemoteShell { get; set; }
    public ThemeVariant ThemeVariant { get; set; }
    public bool CollectData { get; set; }
}

public enum ThemeVariant {
    Fluent,
    Classic,
    Macos,
    Windows,
    Linux
}