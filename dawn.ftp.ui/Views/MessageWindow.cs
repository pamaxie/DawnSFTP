using Avalonia.Controls;
using Avalonia.Media;

namespace dawn.ftp.ui.Views;

public class MessageWindow : Window
{
    public MessageWindow(string title, string icon, string message, string description, TextWrapping wrapping, Action[] actions)
    {
        Title = title;
        // Basic stub implementation
    }

    public bool AboutButtonIsVisible { get; set; }

    public static Action CreateLinkButtonAction(string text, string icon, string url, System.Action onClick) => new Action();
    public static Action CreateButtonAction(string text, string icon, System.Action onClick) => new Action();
    public static Action CreateCloseButton(string icon) => new Action();

    public class Action
    {
    }
}
