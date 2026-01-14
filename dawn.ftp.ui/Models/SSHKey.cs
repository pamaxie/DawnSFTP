using Renci.SshNet;

namespace dawn.ftp.ui.Models;

/// <summary>
/// Stores private keys and associates them with a key identifier that makes it easier for users to identify them
/// </summary>
/// <param name="privateKey">The key that should be stored</param>
/// <param name="keyIdentifier">A name that's easily recognizable by the user</param>
public class SshKey(PrivateKeyFile privateKey, string keyIdentifier)
{
    public string KeyIdentifier { get; set; } = keyIdentifier;
    public PrivateKeyFile PrivateKey { get; set; } = privateKey;
}