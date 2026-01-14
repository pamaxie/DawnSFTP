using dawn.ftp.ui.UserControls.ViewModels;

namespace dawn.ftp.ui.BusinessLogic.EventArgs;

public class ConnectionErrorEventArgs : System.EventArgs
{
    public ConnectionErrorEventArgs(FileViewModel sender, string errorMessage) {
        Sender = sender;
        ErrorMessage = errorMessage;
    }
    
    public readonly FileViewModel Sender;

    public readonly string ErrorMessage;
}