using System;
using System.Diagnostics;
using Renci.SshNet;

namespace dawn.ftp.ui.BusinessLogic;

public interface ISshClient : IDisposable {
    bool IsConnected { get; }
    void Connect();
    ISshCommand RunCommand(string commandText);
}

public interface ISshCommand {
    string Execute();
}

internal class SshClientWrapper(SshClient client) : ISshClient {
    public bool IsConnected => client.IsConnected;
    public void Connect() => client.Connect();
    public ISshCommand RunCommand(string commandText) => new SshCommandWrapper(client.RunCommand(commandText));
    public void Dispose() => client.Dispose();
}

internal class SshCommandWrapper(SshCommand command) : ISshCommand {
    public string Execute() => command.Execute();
}

/// <summary>
/// This class handles OS Probe tasks like figuring out operating systems based on certain SSH commands
/// </summary>
public class OsProbe
{
    /// <summary>
    /// Retrieves the OS Icon via an SftpConnection
    /// </summary>
    /// <returns></returns>
    public static Tuple<string, SSHOs> GetRemoteOs(SshClient client) => GetRemoteOs(new SshClientWrapper(client));

    internal static Tuple<string, SSHOs> GetRemoteOs(ISshClient client)
    {
        if (!client.IsConnected) {
            try { client.Connect();}
            catch (Exception ex) {
                Debugger.Log(0, "Error", ex.Message);
                return new Tuple<string, SSHOs>(string.Empty, SSHOs.Unknown);
            }
            
        }
        
        var result = client.RunCommand("uname").Execute().ToLower().TrimEnd('\n', '\r');
        switch (result) {
            case "darwin":
                return new Tuple<string, SSHOs>("<i class=\"fa-brands fa-apple\"></i>", SSHOs.MacOS);
            case "linux":
                return new Tuple<string, SSHOs>("<i class=\"fa-brands fa-linux\"></i>", SSHOs.Linux);
            case "bsd":
                return new Tuple<string, SSHOs>("<i class=\"fa-brands fa-freebsd\"></i>", SSHOs.BSD);
        }

        if (result.Contains("command not found")) {
            result = client.RunCommand("ver").Execute();
            if (!result.Contains("error")) {
                return  new Tuple<string, SSHOs>("<i class=\"fa-brands fa-windows\"></i>", SSHOs.Windows);
            }
        }
        
        return new Tuple<string, SSHOs>("<i class=\"fa-regular fa-circle-question\"></i>", SSHOs.Unknown);
    }
}