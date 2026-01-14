using System;
using System.Runtime.InteropServices;
using System.Security;

namespace dawn.ftp.ui.Extensions;

public static class SecureStringExtensions
{
    /// <summary>
    /// Converts a plain string to a SecureString instance.
    /// </summary>
    /// <param name="plainStr">The plain text string to be converted.</param>
    /// <returns>A SecureString representation of the input string.</returns>
    public static SecureString ToSecureString(this string plainStr) {
        var secStr = new SecureString(); secStr.Clear();
        foreach (char c in plainStr.ToCharArray()) {
            secStr.AppendChar(c);
        }
        
        return secStr;
    }

    /// <summary>
    /// Converts a SecureString instance to a plain text string.
    /// </summary>
    /// <param name="value">The SecureString object to be converted to plain text.</param>
    /// <returns>A plain text string representation of the input SecureString.</returns>
    public static String SecureStringToString(this SecureString value) {
        IntPtr bstr = IntPtr.Zero;
        try {
            bstr = Marshal.SecureStringToBSTR(value);
            return Marshal.PtrToStringBSTR(bstr);
        }
        finally {
            if (bstr != IntPtr.Zero) {
                Marshal.ZeroFreeBSTR(bstr);
            }
        }
    }

}