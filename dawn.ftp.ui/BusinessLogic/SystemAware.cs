using System.Diagnostics;

namespace dawn.ftp.ui.BusinessLogic;

public static class SystemAware
{
    public static void StartThisApplication()
    {
        var process = Process.GetCurrentProcess();
        var module = process.MainModule;
        if (module?.FileName != null)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = module.FileName,
                UseShellExecute = true
            });
        }
    }
}
