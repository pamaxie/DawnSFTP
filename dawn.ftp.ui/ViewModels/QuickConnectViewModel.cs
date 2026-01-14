using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security;
using dawn.ftp.ui.BusinessLogic;
using dawn.ftp.ui.Extensions;
using dawn.ftp.ui.Models;
using Renci.SshNet;

namespace dawn.ftp.ui.ViewModels;

/// <summary>
/// 
/// </summary>
internal class QuickConnectViewModel : ViewModelBase
{
    private int _port = 22;
    private int _passLength = 0;
    private string _ipAddress;
    private string _userName;
    private string _password;
    internal SecureString SecurePassword = new SecureString();
    private bool _useSshKey;
    private bool _showPassword;
    private SshKey? _selectedKey;
    private ObservableCollection<SshKey> _sshKeys;
    private bool _hasPasswordManager;
    private bool _rememberMe;

    /// <summary>
    /// Verify basic thins that we need for the window to work, and that only need to generally run once
    /// (Key Ring management, etc)
    /// </summary>
    internal QuickConnectViewModel() {
        _sshKeys = GetSSHKeys();
        _hasPasswordManager = KeyringManager.HasValidKeyring();
    }

    /// <summary>
    /// IP Address that we want to connect to
    /// </summary>
    [Required(ErrorMessage = "This field is required")]
    [HostName(ErrorMessage = "The hostname, IPv4 or IPv6 Address you entered is not valid.")]
    internal string IpAddress {
        get => _ipAddress;
        set => SetProperty(ref _ipAddress, value);
    }

    /// <summary>
    /// Port for the host that we want to connect to (defaults to 22)
    /// </summary>
    [Range(1, 65535, ErrorMessage = "The value set is not a valid port")]
    [Required(ErrorMessage = "This field is required")]
    [DefaultValue(22)]
    internal int Port { 
        get => _port; 
        set => SetProperty(ref _port, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether SSH key authentication is used for connections.
    /// </summary>
    [DefaultValue(false)]
    internal bool UseSshKey {
        get => _useSshKey;
        set => SetProperty(ref _useSshKey, value);
    }

    /// <summary>
    /// Indicates if the password in <see cref="Password"/> should be shown in plaintext.
    /// </summary>
    [DefaultValue(false)]
    internal bool ShowPassword
    {
        get => _showPassword;
        set => SetProperty(ref _showPassword, value);
    }

    /// <summary>
    /// Username for the host we want to connect to
    /// </summary>
    [Required(ErrorMessage = "This field is required")]
    [Base64String]
    internal string UserName { 
        get => _userName; 
        set => SetProperty(ref _userName, value);
    }

    /// <summary>
    /// Password for the host we want to connect to
    /// </summary>
    [Required(ErrorMessage = "This field is required")]
    internal String Password {
        get => SecurePassword.SecureStringToString();
        set {
            SetProperty(ref _password, "*");
            SecurePassword = value.ToSecureString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal ObservableCollection<SshKey> SSHKeys => _sshKeys;

    /// <summary>
    /// Returns a collection of SSH Keys, shouldn't be called to often since it causes a lot of I/O
    /// </summary>
    private ObservableCollection<SshKey> GetSSHKeys() {
        if (_sshKeys?.Count == 0) {
            return _sshKeys;
        }

        var folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        //User folder does not exist which likely means we have none, just return nothing
        if (string.IsNullOrEmpty(folder)) {
            return new ObservableCollection<SshKey>();
        }

        var collection = new ObservableCollection<SshKey>();
        var newPath = Path.Combine(folder, ".ssh");
        var files = Directory.GetFiles(newPath);

        foreach (var file in files) {
            //Doing some checks to weed out public key files usually ending with .pub or .key :)
            if (!file.EndsWith(".pub") || !file.EndsWith(".key") || !File.Exists(file) || file.Contains("known_hosts"))
            {
                continue;
            }

            var key = new PrivateKeyFile(file);
            var keyData = new SshKey(key, file);
            collection.Add(keyData);
        }
        
        //TODO: Handle no keys being available
        _sshKeys = collection;
        return collection;
    }

    /// <summary>
    /// A single SSH Key that's been selected
    /// </summary>
    internal SshKey? SelectedKey {
        get => _selectedKey;
        set => SetProperty(ref _selectedKey, value);
    }

    /// <summary>
    /// 
    /// </summary>
    internal bool HasPasswordManager => _hasPasswordManager;

    [DefaultValue(false)]
    internal bool SuccessfulExit { get; set; }

    [DefaultValue(false)]
    public bool RememberMe {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    /// <summary>
    /// 
    /// </summary>
    internal void CancelPressed() {
        ExecuteClose(false);
    }

    /// <summary>
    /// 
    /// </summary>
    internal void ConnectPressed() {
        if (HasErrors) {
            return;
        }
        
        //These fields are required, if connect is pressed and they are not filled in ignore
        //TODO: Add user reminder which field is not filled.
        if (string.IsNullOrEmpty(_ipAddress) ||
            string.IsNullOrEmpty(_userName)) {
            return;
        }

        //Check if we have an SSH Key or password.
        if ((UseSshKey && SelectedKey == null) || (!UseSshKey && SecurePassword.Length == 0)) {
            return;
        }

        SuccessfulExit = true;
        ExecuteClose(true);
    }
}
