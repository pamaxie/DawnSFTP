using System;
using Avalonia.Controls;

namespace dawn.ftp.ui.Views;

public partial class AboutWindow : Window {
    public AboutWindow() {
        InitializeComponent();
    }

    public static string GetEssentialInformationStatic() {
        return $"OS: {Environment.OSVersion}\nRuntime: {Environment.Version}";
    }
}