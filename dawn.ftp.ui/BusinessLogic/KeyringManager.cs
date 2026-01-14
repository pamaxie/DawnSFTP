using System;
using System.Security;
using System.Security.Cryptography;
using dawn.ftp.ui.Extensions;
using KeySharp;

namespace dawn.ftp.ui.BusinessLogic;

public static class KeyringManager
{
    /// <summary>
    /// 
    /// </summary>
    private const string PackageName = "dawn.ftp";

    /// <summary>
    /// We are making this static as it's not required to be dynamic
    /// </summary>
    private static bool? _hasValidKeyring = null;
    
    /// <summary>
    /// Gets a valid keyring and sets an internal static value to return the value later.
    /// </summary>
    /// <returns></returns>
    public static bool HasValidKeyring() {
        if (_hasValidKeyring.HasValue) {
            return _hasValidKeyring.Value;
        }
        
        try {
            Keyring.SetPassword(PackageName, "TestService", "user", "password");
            Keyring.GetPassword(PackageName, "TestService", "user");
            Keyring.DeletePassword(PackageName, "TestService", "user");
        }
        catch (KeyringException ex) {
            //Known bug that we have to catch as there may be access issues on Linux / MacOs when deleting stuff, the password will still be deleted.
            if (ex.Message != " (NoError)") {
                _hasValidKeyring = false;
            }
        }

        _hasValidKeyring = true;
        return true;
    }

    public static SecureString GetDbPassword() {
        if (!HasValidKeyring()) {
            return "noKeyring".ToSecureString();
        }
        
        try {
            return GetPassword("local.db", Environment.UserName);
        }
        catch (KeyringException ex) {
            //The Random number generator created here is thread safe.
            var rng = RandomNumberGenerator.Create();
            var tokenBuffer = new byte[32];
            rng.GetBytes(tokenBuffer);
            var password = Convert.ToBase64String(tokenBuffer).ToSecureString();
            SavePassword("local.db", Environment.UserName, password);
            
            return GetPassword("local.db", Environment.UserName);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="username"></param>
    /// <returns></returns>
    internal static SecureString GetPassword(string url, string username) => 
        Keyring.GetPassword(PackageName, url, username).ToSecureString();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    internal static void SavePassword(string url, string username, SecureString password) =>    
        Keyring.SetPassword(PackageName, url, username, password.SecureStringToString());
    
}