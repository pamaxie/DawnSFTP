using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Renci.SshNet;

namespace dawn.ftp.ui.UserControls.ViewModels;

public class ShellItemViewModel : ObservableObject
{
    
    private string _ttyBuffer;
    private string _commandLine;
    private bool _pointerOver;
    private ShellStream _shellStream;

    /// <summary>
    /// Represents the header text associated with a shell session or terminal.
    /// The Header property is initialized via the constructor and reflects a unique
    /// identifier or descriptive label for the shell instance in the user interface.
    /// This property is read-only and is primarily used to display the shell session
    /// title in tabs or related UI components.
    /// </summary>
    public string Header { get; }

    public static int MaxLineCount { get; set; } = 80;

    public FileViewModel Owner { get; }

    /// <summary>
    /// Represents the textual data from the terminal (TTY) buffer.
    /// This property is used for displaying the ongoing terminal session output
    /// and is updated asynchronously as new data becomes available from the ShellStream.
    /// The value of this property is read-only and can only be modified internally
    /// using the private setter.
    /// The <see cref="TtyBuffer"/> property is bound to the UI and used to render
    /// real-time terminal content within a read-only text box in the user interface.
    /// </summary>
    public string TtyBuffer {
        get => _ttyBuffer;
        private set => SetProperty(ref _ttyBuffer, value);
    }

    /// <summary>
    /// Represents the total number of lines stored within the terminal buffer.
    /// This property is computed dynamically by splitting the content of the `TtyBuffer`
    /// string on newline characters and returning the count as a string.
    /// </summary>
    public int TTyBufferLineCount => TtyBuffer.Split('\n').Length;

    /// <summary>
    /// Represents the command line input for a shell session.
    /// The CommandLine property allows for getting and setting the current user input
    /// before it is processed and sent to the shell stream.
    /// When the user presses Enter, it is cleared and its content is appended to the TtyBuffer.
    /// This property is primarily used for capturing and displaying ongoing user input
    /// within a terminal-like interface.
    /// </summary>
    public string CommandLine {
        get => _commandLine;
        set => SetProperty(ref _commandLine, value);
    }

    /// <summary>
    /// Indicates whether the pointer is currently hovering over the associated user interface element.
    /// The PointerOver property is a bindable property used to reflect real-time pointer hover state,
    /// which may be relevant for triggering visual or behavioral changes in the UI.
    /// </summary>
    public bool PointerOver {
        get => _pointerOver;
        set => SetProperty(ref _pointerOver, value);
    }

    public void MouseEntered() {
        
    }

    public ShellStream ShellStream {
        get => _shellStream;
        internal set => SetProperty(ref _shellStream, value);
    }


    public ShellItemViewModel(string header, ShellStream shellStream, FileViewModel owner)
    {
        Header = header;
        Owner = owner;
        this._shellStream = shellStream;
        Task t = new Task(() => UpdateShell().ConfigureAwait(false));
        t.Start();
        _commandLine = string.Empty;
        _ttyBuffer = string.Empty;
    }

    public void PressedEnter() {
        if (string.IsNullOrEmpty(CommandLine)) {
            return;
        }
        
        TtyBuffer += CommandLine + Environment.NewLine;
        _shellStream.WriteLine(CommandLine);
        _shellStream.Flush();
        CommandLine = "";
    }

    //TODO This is really nessecary for certain tasks.
    public void AbortPressed() {
        // To send the interrupt:
        // Write the ASCII End of Text (ETX) control character to the stream
        _shellStream.Write("\x03"); // Sends Ctrl+C
        _shellStream.Flush();
    }

    public string CleanShellOutput(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        // ANSI Escape Codes: \x1B\[[0-9;]*[a-zA-Z]
        // This covers colors, cursor movements, etc.
        string pattern = @"\x1B\[[0-9;]*[a-zA-Z]";
        string cleaned = System.Text.RegularExpressions.Regex.Replace(input, pattern, string.Empty);

        // Remove other control characters except for NewLine and Tab
        var sb = new StringBuilder();
        foreach (char c in cleaned)
        {
            if (!char.IsControl(c) || c == '\n' || c == '\r' || c == '\t')
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    private async Task UpdateShell() {
        while (true) {
            try {
                if (_shellStream.DataAvailable) {
                    var newBuffer =  _shellStream.Read();
                    
                    //This filters out our user line (aka user@xyz ~)
                    if (newBuffer.Contains("~$") || newBuffer.Contains("~#") || newBuffer.Contains("~ %")) {
                        var tempBuffer = newBuffer.Split("\n").Where(x => !x.Contains("~$") && !x.Contains("~#") && !x.Contains("~ %"));
                        newBuffer = string.Join(Environment.NewLine, tempBuffer.ToArray());
                    }
                    
                    if ((TTyBufferLineCount + 1) > MaxLineCount) {
                        var lines = Regex.Split(TtyBuffer, "\r\n|\r|\n").Skip(MaxLineCount);
                        TtyBuffer = string.Join(Environment.NewLine, lines.ToArray());
                    }
                    
                    TtyBuffer += CleanShellOutput(newBuffer) +"\n";
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
            
            Task.Delay(200).Wait();
        }
    }
}