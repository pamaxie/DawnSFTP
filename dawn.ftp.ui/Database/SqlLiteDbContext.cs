using System;
using System.IO;
using System.Linq;
using dawn.ftp.ui.BusinessLogic;
using dawn.ftp.ui.Models;
using dawn.ftp.ui.Models.TransferModels;
using Microsoft.EntityFrameworkCore;

namespace dawn.ftp.ui.Database;

public class SqlLiteDbContext : DbContext
{
    private const string DbName = "dawn-ftp.db";

    /// <summary>
    /// Returns a data version that increments specific to a data connection. Please be aware that different
    /// data connections will likely have different data versions
    /// </summary>
    /// <returns><see cref="long"/> that represents the data changes / version for the current connection.</returns>
    public long GetDataVersion()
    {
        return Database.SqlQuery<long>($"PRAGMA data_version;").AsEnumerable().FirstOrDefault();
    }

    /// <summary>
    /// Represents the database table for storing SFTP connection properties, allowing the application to
    /// query, add, or update details related to SFTP server connections, including authentication details,
    /// host information, and other configuration properties.
    /// </summary>
    public DbSet<SftpConnectionProperty> SftpConnectionProperties { get; set; }

    /// <summary>
    /// Represents the database table containing file transfer models, providing an interface to query and update
    /// file transfer-related data within the application.
    /// </summary>
    public DbSet<FileTransferModel> FileTransferModels { get; set; }
    
    public bool ExportSettings(string exportDirectory, bool canOverride = false) {
        //We can't find a db file or the db path isn't set.
        if (string.IsNullOrWhiteSpace(_dbPath) || !File.Exists(_dbPath)) {
            return false;
        }

        try {
            var destination = Path.Combine(exportDirectory, Path.GetFileName(_dbPath));
            File.Copy(_dbPath, destination, canOverride);
            return true;
        }
        catch (Exception e) {
            return false;
        }
    }

    private bool Exists() {
        if (string.IsNullOrWhiteSpace(_dbPath)) {
            throw new InvalidOperationException();
        }
        
        return File.Exists(_dbPath);
    }


    public void CreateOrUpdate() {
        Database.Migrate();
    }
    
    public SqlLiteDbContext() {
        var dir = SoftwareInitialization.SettingsFolder;
        if (!Directory.Exists(dir)) {
            Directory.CreateDirectory(dir);
        }
        
        _dbPath = Path.Combine(dir, DbName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (string.IsNullOrEmpty(_dbPath)) {
            throw new InvalidOperationException("The database path cannot be null or empty.");
        }

        if (!optionsBuilder.IsConfigured) {
            optionsBuilder.UseSqlite($"Data Source=\"{_dbPath}\";");
        }
    }
    
    private readonly string _dbPath;
}