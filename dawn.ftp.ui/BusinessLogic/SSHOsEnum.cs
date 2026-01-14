using System;

namespace dawn.ftp.ui.BusinessLogic;
[Flags]
public enum SSHOs {
    Unknown = -1,
    Windows = 0,
    Linux = 1,
    BSD = 2,
    MacOS = 4,
}