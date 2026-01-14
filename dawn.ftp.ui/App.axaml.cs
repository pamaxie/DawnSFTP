using System.Collections.Generic;
using System.IO;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using dawn.ftp.ui.BusinessLogic;
using dawn.ftp.ui.ViewModels;
using dawn.ftp.ui.Views;
using Sentry;

namespace dawn.ftp.ui;

public partial class App : Application
{
    public override void Initialize()
    {
        if (ConfigurationHandler.Current.CollectData) {
            // Transaction can be started by providing, at minimum, the name and the operation
            var transaction = SentrySdk.StartTransaction(
                "Dawn.Ftp",
                "Application Startup Time"
            );
            SoftwareInitialization.InitializeDb();
            SoftwareInitialization.CreateDirectoryStructure();
            AvaloniaXamlLoader.Load(this);
            ConfigurationHandler.Refresh();
            transaction.Finish();
        }
        else {
            AvaloniaXamlLoader.Load(this);
            ConfigurationHandler.Refresh();
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
            
            if (Program.IsCrashReport)
            {
                if (Program.Args.Length < 3) return;
                var category = Program.Args[1];
                var exDetails = Program.Args[2];
                var collectData = ConfigurationHandler.Current.CollectData;

                var bugDescription = $"Apologies, {About.Software} has encountered an unexpected error and has crashed.\n\n" +
                                     $"Why it crashed:\n{exDetails}\n\n" +
                                     (collectData ? "The crash has been logged and our team has been notified." : "The crash has not been logged. You can manually submit a report using the button below.");

                var system = string.Empty;
                try
                {
                    system = AboutWindow.GetEssentialInformationStatic();
                }
                catch
                {
                    // ignored
                }

                using var reader = new StringReader(exDetails);
                MessageWindow window = null!;
                
                var actions = new List<MessageWindow.Action>();
                if (!collectData)
                {
                    actions.Add(MessageWindow.CreateLinkButtonAction("Submit Report", "fa-solid fa-bug", 
                        $"https://github.com/dawnruby/dawn-ftp/issues/new?title={HttpUtility.UrlEncode($"[Crash] {reader.ReadLine()}")}&body={HttpUtility.UrlEncode($"```\n{bugDescription}\n\nSystem Info:\n{system}\n```")}", 
                        () => window?.Clipboard?.SetTextAsync($"```\n{bugDescription}\n```")));
                }
                actions.Add(MessageWindow.CreateButtonAction("Restart", "fa-solid fa-redo-alt", () => SystemAware.StartThisApplication()));
                actions.Add(MessageWindow.CreateCloseButton("fa-solid fa-sign-out-alt"));

                window = new MessageWindow($"{About.SoftwareWithVersion} - Crash Report",
                    "fa-regular fa-frown",
                    "An unexpected error occurred.",
                    bugDescription,
                    TextWrapping.Wrap,
                    actions.ToArray())
                {
                    AboutButtonIsVisible = true
                };

                try
                {
                    window.Clipboard?.SetTextAsync(bugDescription);
                }
                catch
                {
                    // ignored
                }

                desktop.MainWindow = (Window)window;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}