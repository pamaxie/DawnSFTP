using System;
using System.Threading;
using System.Threading.Tasks;
using dawn.ftp.ui.Models;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace dawn.ftp.ui.Extensions;

/// <summary>
/// Provides a set of extension methods for creating and safely managing SFTP and SSH client connections.
/// </summary>
internal static class SftpExtensions
{
    /// <summary>
    /// Creates an <see cref="SftpClient"/> instance using the given <see cref="SftpConnectionProperty"/>.
    /// </summary>
    /// <param name="credentials">An instance of <see cref="SftpConnectionProperty"/> containing the connection credentials and configuration.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Tuple{T1, T2, T3}"/> where T1 is an <see cref="SftpClient"/> instance, T2 is a boolean indicating success or failure, and T3 is a string containing error details if applicable.</returns>
    /// <remarks>The <see cref="SftpClient"/> returned by this method is NOT connected.</remarks>
    internal static async Task<Tuple<SftpClient?, bool, string>> CreateSftpClient(
        this SftpConnectionProperty credentials, CancellationToken token) {
        if (credentials.Credential == null) {
            return new Tuple<SftpClient?, bool, string>(null, false, "Software received an invalid input while trying to create a connection");
        }
        
        var sftpClient = new SftpClient(credentials.Credential.Domain, credentials.Credential.UserName, credentials.Credential.Password);
        if (credentials.UseKeyAuth) {
            if (credentials.PrivateKey == null) {
                return new Tuple<SftpClient?, bool, string>(null, false, "Software received an invalid input while trying to process your private key. Please report this as this is a bug");
            }
            
            var connectionInfo = new ConnectionInfo(credentials.Credential.Domain, credentials.Credential.UserName, 
                new PrivateKeyAuthenticationMethod(credentials.Credential.UserName, credentials.PrivateKey.PrivateKey));
            sftpClient = new SftpClient(connectionInfo);   
        }

        try
        {
            await sftpClient.ConnectAsync(token);
            sftpClient.Disconnect();
            return new Tuple<SftpClient?, bool, string>(sftpClient, true, string.Empty);
        }
        catch (SshAuthenticationException exception) {
            return new Tuple<SftpClient?, bool, string>(null, false, exception.Message);
        }
        catch (SshConnectionException exception) {
            return new Tuple<SftpClient?, bool, string>(null, false, exception.Message);
        }
        catch (SshException exception) {
            return new Tuple<SftpClient?, bool, string>(null, false, exception.Message);
        }
        catch (Exception exception) {
            return new Tuple<SftpClient?, bool, string>(null, false, exception.Message);
        }
    }

    /// <summary>
    /// Creates an <see cref="SshClient"/> instance using the given <see cref="SftpConnectionProperty"/>.
    /// </summary>
    /// <param name="credentials">An instance of <see cref="SftpConnectionProperty"/> containing the connection credentials and configuration.</param>
    /// <param name="token">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Tuple{T1, T2, T3}"/> where T1 is an <see cref="SshClient"/> instance, T2 is a boolean indicating success or failure, and T3 is a string containing error information if applicable.</returns>
    /// <remarks>The <see cref="SshClient"/> returned by this method is NOT connected.</remarks>
    internal static async Task<Tuple<SshClient?, bool, string>> CreateSshClient(this SftpConnectionProperty credentials,
        CancellationToken token) {
        if (credentials.Credential == null) {
            return new Tuple<SshClient?, bool, string>(null, false, "Software received an invalid input while trying to create a connection");
        }
        
        var sshClient = new SshClient(credentials.Credential.Domain, credentials.Credential.UserName, credentials.Credential.Password);
        if (credentials.UseKeyAuth) {
            if (credentials.PrivateKey == null) {
                return new Tuple<SshClient?, bool, string>(null, false, "Software received an invalid input while trying to process your private key. Please report this as this is a bug");
            }
            
            var connectionInfo = new ConnectionInfo(credentials.Credential.Domain, credentials.Credential.UserName, 
                new PrivateKeyAuthenticationMethod(credentials.Credential.UserName, credentials.PrivateKey.PrivateKey));
            sshClient = new SshClient(connectionInfo);
        }

        
        try
        {
            await sshClient.ConnectAsync(token);
            sshClient.Disconnect();
            return new Tuple<SshClient?, bool, string>(sshClient, true, string.Empty);
        }
        catch (SshAuthenticationException exception) {
            return new Tuple<SshClient?, bool, string>(null, false, exception.Message);
        }
        catch (SshConnectionException exception) {
            return new Tuple<SshClient?, bool, string>(null, false, exception.Message);
        }
        catch (SshException exception) {
            return new Tuple<SshClient?, bool, string>(null, false, exception.Message);
        }
        catch (Exception exception) {
            return new Tuple<SshClient?, bool, string>(null, false, exception.Message);
        }
    }

    /// <summary>
    /// Ensures that the <see cref="SftpClient"/> is connected. If it is not already connected, it will establish the connection.
    /// </summary>
    /// <param name="sftpClient">The <see cref="SftpClient"/> instance to check and connect.</param>
    internal static void ConnectSafe(this SftpClient sftpClient) {
        if (!sftpClient.IsConnected) {
            sftpClient.Connect();
        }
    }

    /// <summary>
    /// Ensures that the provided <see cref="SshClient"/> instance is not connected before attempting to establish a connection,
    /// preventing connection-related exceptions.
    /// </summary>
    /// <param name="sshClient">The <see cref="SshClient"/> instance to connect safely.</param>
    internal static void ConnectSafe(this SshClient sshClient) {
        if (!sshClient.IsConnected) {
            sshClient.Connect();
        }
    }

    /// <summary>
    /// Safely disconnects an <see cref="SshClient"/> if it is currently connected.
    /// </summary>
    /// <param name="sshClient">The <see cref="SshClient"/> instance to be disconnected.</param>
    internal static void DisconnectSafe(this SshClient sshClient) {
        if (!sshClient.IsConnected) {
            sshClient.Disconnect();
        }
    }

    /// <summary>
    /// This is an extension to the sftp client that ensures it's connected before disconnecting so no exception is thrown.
    /// </summary>
    /// <param name="sftpClient"></param>
    internal static void DisconnectSafe(this SftpClient sftpClient) {
        if (sftpClient.IsConnected) {
            sftpClient.Disconnect();
        }
    }
}