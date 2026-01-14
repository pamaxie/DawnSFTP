using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace dawn.ftp.ui.ViewModels;

/// <summary>
/// 
/// </summary>
public class ViewModelBase : ObservableValidator
{   
    public event EventHandler<DialogCloseRequestedEventArgs> CloseDialogRequested;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    protected void ExecuteClose(bool args) {
        CloseDialogRequested?.Invoke(this, new DialogCloseRequestedEventArgs(args));
    }

    public class DialogCloseRequestedEventArgs : EventArgs {
        public bool Result { get; }
        public DialogCloseRequestedEventArgs(bool result) => Result = result;
    }
}