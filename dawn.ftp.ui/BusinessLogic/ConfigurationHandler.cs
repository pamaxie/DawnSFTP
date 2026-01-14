using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using dawn.ftp.ui.Models.Configuration;

namespace dawn.ftp.ui.BusinessLogic;

public static class ConfigurationHandler
{
    private static string _settingsFilePath = Path.Combine(SoftwareInitialization.SettingsFolder, "settings.json");
    private static readonly ReaderWriterLockSlim Lock = new();
    private static IConfiguration _currentConfig;

    internal static string SettingsFilePath {
        get => _settingsFilePath;
        set => _settingsFilePath = value;
    }

    public static IConfiguration Current
    {
        get
        {
            Lock.EnterReadLock();
            try
            {
                return _currentConfig;
            }
            finally
            {
                Lock.ExitReadLock();
            }
        }
    }

    public static void Refresh()
    {
        Lock.EnterWriteLock();
        try
        {
            if (!File.Exists(SettingsFilePath)) {
                _currentConfig = new Configuration();
                SaveInternal();
                return;
            }

            try {
                var json = File.ReadAllText(SettingsFilePath);
                var config = JsonSerializer.Deserialize<Configuration>(json);
                if (config != null)
                {
                    _currentConfig = Migrate(config);
                }
            }
            catch (Exception ex) {
                // Fallback to default if there's an error reading/deserializing
                Console.WriteLine($"Error loading configuration: {ex.Message}");
                _currentConfig = new Configuration();
            }
        }
        finally {
            Lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Migrates the configuration if properties are missing by ensuring it's fully populated.
    /// Since we use JSON deserialization into a concrete class, missing properties in JSON 
    /// will result in default values for those types in the object. 
    /// This method can be expanded if specific migration logic (e.g. renaming keys) is needed.
    /// </summary>
    /// <param name="config">The configuration to migrate.</param>
    /// <returns>A migrated configuration.</returns>
    internal static IConfiguration Migrate(Configuration config)
    {
        // Currently, JsonSerializer.Deserialize handles missing properties by using the default 
        // values defined in the Configuration class.
        // If we want to ensure it's saved back with all properties (including new ones):
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsFilePath, json);
        return config;
    }

    public static void UpdateValues(IConfiguration newConfig)
    {
        Lock.EnterWriteLock();
        try {
            _currentConfig = newConfig;
            SaveInternal();
        }
        finally {
            Lock.ExitWriteLock();
        }
    }

    private static void SaveInternal()
    {
        try {
            SoftwareInitialization.CreateDirectoryStructure();
            var json = JsonSerializer.Serialize(_currentConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex) {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
        }
    }
}