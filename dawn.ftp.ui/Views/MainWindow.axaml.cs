using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using dawn.ftp.ui.BusinessLogic;
using dawn.ftp.ui.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace dawn.ftp.ui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        if (Design.IsDesignMode)
            return;
        
        SoftwareInitialization.CreateDirectoryStructure();
        var db = SoftwareInitialization.InitializeDb();
        db.SavedChanges += SavedChanges;

        if (ConfigurationHandler.Current.UseDarkMode) {
            if (Application.Current != null) {
                Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Dark;
            }
        }
        else {
            if (Application.Current != null) {
                Application.Current.RequestedThemeVariant = Avalonia.Styling.ThemeVariant.Light;
            }
        }
    }

    private void SavedChanges(object? sender, SavedChangesEventArgs e) {
        Console.WriteLine(sender);
    }

    private void WindowLoaded(object? sender, RoutedEventArgs e)
    { 
        //TODO: There is easier ways to do this but right now there is no way for us to launch the application where
        // the mouse cursor is actually located on Linux and MacOs so it'll launch on the primary display instead, we should look into a fix for this.
        //Ensure the window has been initialized and screens can be accessed
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow == null) {
            return;
        }
        
        //Get the screen the window is currently on, or the primary screen if not yet shown
        var screen = Screens.ScreenFromWindow(desktop.MainWindow) ?? Screens.Primary;

        if (screen == null) {
            return;
        }
        
        //Example: Center the window on the active screen's working area
        var workingArea = screen.WorkingArea;
        var windowSize = ClientSize;

        Position = new PixelPoint(
            (int)(workingArea.X + (workingArea.Width - windowSize.Width) / 2),
            (int)(workingArea.Y + (workingArea.Height - windowSize.Height) / 2)
        );
    }
}