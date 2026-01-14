using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Xaml.Interactions.DragAndDrop;
using CommunityToolkit.Mvvm.Input;
using dawn.ftp.ui.Database;
using dawn.ftp.ui.UserControls.ViewModels;

namespace dawn.ftp.ui.UserControls;

public partial class FileView : UserControl
{
    private IDropHandler _dndDropHandler = null!;

    public static readonly DirectProperty<FileView, IDropHandler> DndDropHandlerProperty =
      AvaloniaProperty.RegisterDirect<FileView, IDropHandler>(
        nameof(DndDropHandler), o => o.DndDropHandler, (o, v) => o.DndDropHandler = v);

    public static readonly DirectProperty<FileView, NetworkCredential> CredentialProperty =
        AvaloniaProperty.RegisterDirect<FileView, NetworkCredential>(
            nameof(Credential), o => o.Credential, (o, v) => o.Credential = v);

    public IDropHandler DndDropHandler
    {
        get => _dndDropHandler;
        set => SetAndRaise(DndDropHandlerProperty, ref _dndDropHandler, value);
    }

    public NetworkCredential Credential {
        get => _networkCredential;
        set => _networkCredential = value;
    }


    private DataTemplate _addViewTemplate = null!;
    private NetworkCredential _networkCredential;
    public static readonly DirectProperty<FileView, DataTemplate> AddViewTemplateProperty =
      AvaloniaProperty.RegisterDirect<FileView, DataTemplate>(
        nameof(AddViewTemplate), o => o.AddViewTemplate, (o, v) => o.AddViewTemplate = v);

    public DataTemplate AddViewTemplate
    {
        get => _addViewTemplate;
        set => SetAndRaise(AddViewTemplateProperty, ref _addViewTemplate, value);
    }

    public FileView()
    {
        InitializeComponent();

        DataContextChanged += (sender, args) =>
        {
            if (DataContext is not FileViewModel fileView) return;
            if (!string.IsNullOrWhiteSpace(fileView.ConnectionErrors)) {
                ShowConnectionError(fileView, fileView.ConnectionErrors);
            }

            fileView.KeyChange += (o, e) => {
                ShowKeyChange(fileView, e);
            };
            
            fileView.PropertyChanged += (o, eventArgs) => {
                var obj = o;
                if (obj is not FileViewModel fileViewModel || eventArgs?.PropertyName != "ConnectionErrors") {
                    return;
                }

                if (string.IsNullOrWhiteSpace(fileView.ConnectionErrors)) {
                    return;
                }
                
                
                ShowConnectionError(fileViewModel, fileViewModel.ConnectionErrors);
            };
        };
        
    }

    private void InputElement_OnDoubleTapped(object? sender, TappedEventArgs e) {
        //Sadly avalonia does not allow us to map input bindings for this event so we have to use a call like this.
        //If in the future they add it please fix this.
        if (DataContext is FileViewModel model) {
            model.DoubleClicked(false);
        }
    }

    private void InputElement_RemoteDoubleTapped(object? sender, TappedEventArgs e)
    {
        //Sadly avalonia does not allow us to map input bindings for this event so we have to use a call like this.
        //If in the future they add it please fix this.
        if (DataContext is FileViewModel model)
        {
            model.DoubleClicked(true);
        }
    }

    private void ShowKeyChange(FileViewModel model, string key) {
        if (model.ConnectionProperty is null) {
            ShowConnectionError(model, "Connection property is null, this should normally not happen. Please report this as a bug.");
        }
        
        Dispatcher.UIThread.InvokeAsync(() => {
            //TODO: This is horrible practice but I really don't see a better way of doing this without changing the entire
            // connection logic, if you have a quick fix please fix this.
            FileGrid.IsVisible = false;
            FileGrid.IsEnabled = false;
            
            var stackPanel = new StackPanel(){Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center};
            var subStackPanel = new StackPanel(){Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right};
            var errorMessage = "The key to the connecting host " + model.ConnectionProperty?.Name + " has changed. " +
                               "Please confirm the new key to continue. " +
                               "Please be aware that normally the hosts key " +
                               "should never change except in rare circumstances, like system updates. " +
                               "If you are not sure if it's safe to proceed, " +
                               $"contact your system administrator. \nThe new keys SHA256 representation is:\n {key}";
            if (model.ConnectionProperty is not null && string.IsNullOrEmpty(model.ConnectionProperty.KeyHash)) {
                errorMessage =
                    $"The host you're trying to connect to sent us a key you haven't trusted yet, " +
                    $"please check the SHA256 representation to ensure this key is correct, the new key is:\n {key}";
            }
            
            stackPanel.Children.Add(new TextBlock { Text =  errorMessage, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(10)});
            
            ICommand? close = new RelayCommand(() => CloseWindow(model)); 
            subStackPanel.Children.Add(new Button(){Content = "Close Connection", Command = close, Margin = new Thickness(10)});
        
            ICommand trustKey = new RelayCommand(() => TrustKey(model) );
            subStackPanel.Children.Add(new Button(){Content = "Trust key once", Command = trustKey, Margin = new Thickness(10)});

            bool rememberEnabled = model.ConnectionProperty is not null && model.ConnectionProperty.Remember;
            
            ICommand trustKeyAndRemember = new RelayCommand(() => TrustKeyAndRemember(model) );
            subStackPanel.Children.Add(new Button()
            {Content = "Trust key forever", 
                IsEnabled = rememberEnabled, 
                Command = trustKeyAndRemember, 
                Margin = new Thickness(10)});
            
        
            stackPanel.Children.Add(subStackPanel);
            DialogHost.MinHeight = 300;
            DialogHost.DialogContent = stackPanel;
            DialogHost.IsOpen = true;
        }, DispatcherPriority.Background);
    }

    private void TrustKeyAndRemember(FileViewModel model) {
        Debug.Assert(model.ConnectionProperty != null, "model.ConnectionProperty != null");
        var context = new SqlLiteDbContext();
        var item = context.SftpConnectionProperties.FirstOrDefault(x => x.Name == model.ConnectionProperty.Name
                                                             && x.HostOsIcon == model.ConnectionProperty.HostOsIcon);
        if (item is not null) {
            item.KeyHash = model.ConnectionProperty.KeyHash;
            context.SaveChanges();
        }
        
        TrustKey(model);
    }

    /// <summary>
    /// This just essentially re-connects us
    /// </summary>
    /// <param name="model"></param>
    private void TrustKey(FileViewModel model) {
        Debug.Assert(model.ConnectionProperty != null, "model.ConnectionProperty == null");
        DialogHost.IsOpen = false;
        FileGrid.IsVisible = true;
        model.ConnectionErrors = string.Empty;
        model.IsKeyTrusted = true;
        
        RetryConnection(model);
    }

    private void ShowConnectionError(FileViewModel model, string error)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            //TODO: This is horrible practice but I really don't see a better way of doing this without changing the entire
            // connection logic, if you have a quick fix please fix this.
            FileGrid.IsVisible = false;
            var stackPanel = new StackPanel(){Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center};
            var subStackPanel = new StackPanel(){Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right};
            stackPanel.Children.Add(new TextBlock { Text = "Error: " + error });
        
            //Creates 2 buttons, one to close the connection,
            //the other to retry which causes a callback to re-open the connection dialog with all info pre-filled
            ICommand? close = new RelayCommand(() => CloseWindow(model)); 
            subStackPanel.Children.Add(new Button(){Content = "Close Connection", Command = close, Margin = new Thickness(10)});

            //We ensure that the connection property is not null to not throw an exception in case it is.
            if (model.ConnectionProperty is not null) {
                ICommand retry = new RelayCommand(() => RetryConnection(model) );
                subStackPanel.Children.Add(new Button(){Content = "Retry", Command = retry, Margin = new Thickness(10)});
            }
        
            ICommand changeConnection = new RelayCommand(() => ChangeConnection(model) );
            subStackPanel.Children.Add(new Button(){Content = "Change Connection", Command = changeConnection, Margin = new Thickness(10)});
        
            stackPanel.Children.Add(subStackPanel);
            DialogHost.MinHeight = 300;
            DialogHost.DialogContent = stackPanel;
            DialogHost.IsOpen = true;
        }, DispatcherPriority.Background);
    }

    private void ChangeConnection(FileViewModel model) {
        model.Owner?.Connections.Remove(model);
        model.Owner?.QuickConnectPressed(model);
    }

    private void RetryConnection(FileViewModel model) {
        Debug.Assert(model.ConnectionProperty != null, "model.ConnectionProperty != null");
        DialogHost.IsOpen = false;
        model.ConnectionErrors = string.Empty;

        Task.Run(async () => {
            var result = await model.ConfigureConnectionAsync(model.ConnectionProperty);
            if (!result && model.ConnectionErrors != null) {
                ShowConnectionError(model, model.ConnectionErrors);
            }
        });
    }

    private void CloseWindow(FileViewModel model) {
        model.Owner?.Connections.Remove(model);
    }
}