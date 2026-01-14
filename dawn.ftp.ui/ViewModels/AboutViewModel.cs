using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace dawn.ftp.ui.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    [ObservableProperty]
    private string _email = "support@pamaxie.com";

    [ObservableProperty]
    private string _company = "Pamaxie, LLC";

    [ObservableProperty]
    private string _companyImageUrl = "../Assets/512.png"; // Default placeholder

    public List<OpenSourceProject> OpenSourceProjects { get; } = new()
    {
        new("Avalonia", "11.3.2", "https://github.com/AvaloniaUI/Avalonia"),
        new("CommunityToolkit.Mvvm", "8.4.0", "https://github.com/CommunityToolkit/dotnet"),
        new("Material.Avalonia", "3.13.0", "https://github.com/AvaloniaCommunity/Material.Avalonia"),
        new("Semi.Avalonia", "11.2.1.10", "https://github.com/irihire/Semi.Avalonia"),
        new("SSH.NET", "2025.0.0", "https://github.com/sshnet/SSH.NET"),
        new("Entity Framework Core", "9.0.11", "https://github.com/dotnet/efcore"),
        new("DialogHost.Avalonia", "0.9.3", "https://github.com/AvaloniaUtils/DialogHost.Avalonia"),
        new("IdGen", "3.0.7", "https://github.com/RobThree/IdGen"),
        new("KeySharp", "1.0.5", "https://github.com/pvginkel/KeySharp"),
        new("LoadingIndicators.Avalonia", "11.0.11.1", "https://github.com/AvaloniaUtils/LoadingIndicators.Avalonia"),
        new("Projektanker.Icons.Avalonia", "9.6.2", "https://github.com/Projektanker/Icons.Avalonia"),
        new("Renci.SshNet.Async", "1.4.0", "https://github.com/nico-vandenende/Renci.SshNet.Async"),
        new("Verify.CommunityToolkit.Mvvm", "1.1.0", "https://github.com/VerifyTests/Verify.CommunityToolkit.Mvvm"),
        new("Classic.Avalonia.Theme", "11.3.0.3", "https://github.com/AvaloniaCommunity/Classic.Avalonia.Theme"),
    };
}

public record OpenSourceProject(string Name, string Version, string Url);
