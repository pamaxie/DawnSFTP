using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.ReactiveUI;
using dawn.ftp.ui.BusinessLogic;
using IdGen;
using Projektanker.Icons.Avalonia;
using Projektanker.Icons.Avalonia.FontAwesome;
using Sentry;
using Sentry.Profiling;

namespace dawn.ftp.ui;

sealed class Program
{
    public static readonly IdGenerator IdGenerator = new IdGenerator(0);
    public static bool IsCrashReport;
    public static string[] Args = Array.Empty<string>();
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) {
        Args = args;
        if (args.Length > 0 && args[0] == "--crash-report") {
            IsCrashReport = true;
        }

        try {
            ConfigurationHandler.Refresh();
            if (ConfigurationHandler.Current.CollectData) {
                SentrySdk.Init(options => {
                    // A Sentry Data Source Name (DSN) is required.
                    // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
                    // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
                    options.Dsn =
                        "https://a578751e5818f587ee0a86986ac30c2a@o4510705187815424.ingest.us.sentry.io/4510711151919104";

                    // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
                    // This might be helpful, or might interfere with the normal operation of your application.
                    // We enable it here for demonstration purposes when first trying Sentry.
                    // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
                    options.Debug = true;

                    // This option is recommended. It enables Sentry's "Release Health" feature.
                    options.AutoSessionTracking = true;

                    // Set TracesSampleRate to 1.0 to capture 100%
                    // of transactions for tracing.
                    // We recommend adjusting this value in production.
                    options.TracesSampleRate = 1.0;

                    // Sample rate for profiling, applied on top of othe TracesSampleRate,
                    // e.g. 0.2 means we want to profile 20 % of the captured transactions.
                    // We recommend adjusting this value in production.
                    options.ProfilesSampleRate = 1.0;
                    // Requires NuGet package: Sentry.Profiling
                    // Note: By default, the profiler is initialized asynchronously. This can
                    // be tuned by passing a desired initialization timeout to the constructor.
                    options.AddIntegration(new ProfilingIntegration(
                        // During startup, wait up to 500ms to profile the app startup code.
                        // This could make launching the app a bit slower so comment it out if you
                        // prefer profiling to start asynchronously
                        TimeSpan.FromMilliseconds(500)
                    ));
                });
            }
            
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex) {
            if (ConfigurationHandler.Current.CollectData) {
                SentrySdk.CaptureException(ex);
            }

            Debugger.Log(0, "Error", ex.Message);
            HandleUnhandledException("App", ex);
        }
    }
    
    public static void HandleUnhandledException(string category, Exception ex)
    {

        if (category == "Task")
        {
            if (ex.Message.Contains("org.freedesktop.DBus.Error.ServiceUnknown")
                || ex.Message.Contains("org.freedesktop.DBus.Error.UnknownMethod")
               ) return;
        }

        if (!IsCrashReport)
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var module = process.MainModule;
                if (module?.FileName != null)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = module.FileName,
                        Arguments = $"--crash-report \"{category}\" \"{ex}\"",
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                Console.WriteLine(exception);
            }
        }

        Environment.Exit(-1);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() {
        IconProvider.Current.Register<FontAwesomeIconProvider>();
        ConfigurationHandler.Refresh();
        var builder = AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
        return builder;
    }
}