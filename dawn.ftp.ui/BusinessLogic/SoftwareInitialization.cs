using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Avalonia.Platform.Storage;
using dawn.ftp.ui.Database;

namespace dawn.ftp.ui.BusinessLogic;

public static class SoftwareInitialization
{
    private static KeyValuePair<string, IStorageFile> _scripts;
    private const string SettingsFolderName = "dawn-ftp";

    private static Mutex scriptsMutex = new ();

    public static KeyValuePair<string, IStorageFile> Scripts {
        get {
            return _scripts;
        }
        set {
            if (Equals(_scripts, value)) {
                return;
            }
            
            if (scriptsMutex.WaitOne(TimeSpan.FromMilliseconds(200))) {
                _scripts = value;
            }
            
        }
    }
    
    internal static readonly string SettingsFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), SettingsFolderName);

    internal static readonly string SSHFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh");
    
    internal static readonly string ScriptFolder = Path.Combine(SettingsFolder, "scripts");
    
    public static SqlLiteDbContext InitializeDb() {
        var context = new SqlLiteDbContext();
        context.CreateOrUpdate();
        return context;
    }

    
    public static void CreateDirectoryStructure() {
        var dir = SettingsFolder;
        if (!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }

        if (!Path.Exists(SSHFolder)) {
            Directory.CreateDirectory(SSHFolder);
        }
        
        var scriptFolder = Path.Combine(dir, "scripts");
        if (!Directory.Exists(scriptFolder)) {
            Directory.CreateDirectory(scriptFolder);
        }
    }
}