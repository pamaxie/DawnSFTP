using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using dawn.ftp.ui.ViewModels;

namespace dawn.ftp.ui.Models;

public class ScriptNode : ObservableObject {
    private FileSystemInfo _file;
    private string _pathName;
    private string _fullPath;
    private string _icon;
    private bool _canExecute;
    private ObservableCollection<ScriptNode> _children = [];
    private MainWindowViewModel _parentView;

    public MainWindowViewModel ParentView {
        get => _parentView;
        set => SetProperty(ref _parentView, value);
    }

    /// <summary>
    /// Gets the children of the script node. (Usually directories)
    /// </summary>
    public ObservableCollection<ScriptNode> Children {
        get => _children;
        set => SetProperty(ref _children, value);
    }

    /// <summary>
    /// Gets the underlying file associated with the script node.
    /// </summary>
    public FileSystemInfo File {
        get => _file;
        set => SetProperty(ref _file, value);
    }

    /// <summary>
    /// Gets the name of the script node.
    /// </summary>
    public string PathName {
        get => _pathName;
        set => SetProperty(ref _pathName, value);
    }

    /// <summary>
    /// Gets the full path of the script node.
    /// </summary>
    public string FullPath {
        get => _fullPath;
        set => SetProperty(ref _fullPath, value);
    }

    /// <summary>
    /// Gets the icon associated with the script node.
    /// </summary>
    public string Icon {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    /// <summary>
    /// Gets a value indicating whether the script node can be executed.
    /// </summary>
    public bool CanExecute {
        get => _canExecute;
        set => SetProperty(ref _canExecute, value);
    }

    public ScriptNode(string file, MainWindowViewModel parentView, bool getChildren = true) {
        if (Directory.Exists(file)) {
            var ioFile = new DirectoryInfo(file);
            _canExecute = false;
            _file = ioFile;
        }else if (System.IO.File.Exists(file)) {
            var ioFile = new FileInfo(file);
            _canExecute = true;
            _file = ioFile;
        }
        else {
            throw new InvalidOperationException("Invalid path specified");
        }

        ParentView = parentView;
        _pathName = _file.Name;
        _fullPath = _file.FullName;
        _icon = GetIcon(_file.Extension);

        if (getChildren) {
            GetChildren();
        }
    }

    private string GetIcon(string extension) {
        //Check if we have a directory
        if (!_canExecute) {
            return "<i class=\"fa-solid fa-folder\"></i>";
        }
        
        return extension.ToLower() switch {
            ".py" => "<i class=\"fa-brands fa-python\"></i>",
            ".cs" => "<i class=\"fa-solid fa-code\"></i>",
            ".js" => "<i class=\"fa-brands fa-js\"></i>",
            ".html" => "<i class=\"fa-brands fa-html5\"></i>",
            ".css" => "<i class=\"fa-brands fa-css3\"></i>",
            ".json" => "<i class=\"fa-solid fa-file-code\"></i>",
            ".txt" => "<i class=\"fa-solid fa-file-lines\"></i>",
            ".pdf" => "<i class=\"fa-solid fa-file-pdf\"></i>",
            ".sh" => "<i class=\"fa-solid fa-terminal\"></i>",
            ".zip" or ".rar" or ".7z" => "<i class=\"fa-solid fa-file-zipper\"></i>",
            _ => "<i class=\"fa-solid fa-file\"></i>"
        };
    }

    public void GetChildren() {
        if (_file is not DirectoryInfo directoryInfo) {
            return;
        }
        
        Children.Clear();
        try {
            foreach (var info in directoryInfo.GetFileSystemInfos()) {
                if (info.Attributes.HasFlag(FileAttributes.Hidden) || info.Attributes.HasFlag(FileAttributes.System)){
                    continue;
                }
                
                var node =  new ScriptNode(info.FullName, ParentView);
                Children.Add(node);
                
                if (!node.CanExecute) {
                    //This means we have a directory
                    node.GetChildren();
                }
            }
        }
        catch (Exception ex) {
            // Handle or log potential access errors
            Console.WriteLine($"Error accessing {directoryInfo.FullName}: {ex.Message}");
        }
    }

    public void RunScriptRemotely() {
        if (!CanExecute || !System.IO.File.Exists(_fullPath)) {
            return;
        }
        
        ParentView.SelectedConnection?.RunScript(new FileInfo(_fullPath));
    }

    public void EditScript() {
        if (!System.IO.File.Exists(_fullPath)) {
            return;
        }
        
        new Process()
        {
            StartInfo = new ProcessStartInfo(FullPath)
            {
                UseShellExecute = true
            }
        }.Start();
    }
}