using System;
using Avalonia.Controls;
using dawn.ftp.ui.ViewModels;

namespace dawn.ftp.ui.Views;

public partial class OptionsWindow : Window
{
    public OptionsWindow()
    {
        InitializeComponent();
    }
    
    protected override void OnDataContextChanged(EventArgs e) {
        base.OnDataContextChanged(e);

        //This has to be registered here since XAML is quite lazy with updating the Data Context resulting in the
        //window closed events not firing appropriately
        if (DataContext is ViewModelBase vm) {
            vm.CloseDialogRequested += CloseWindow;
        }
    }

    private void CloseWindow(object? sender, ViewModelBase.DialogCloseRequestedEventArgs e) {
        Close(e.Result);
    }
}