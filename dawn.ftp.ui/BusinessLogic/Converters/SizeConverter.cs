using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace dawn.ftp.ui.BusinessLogic.Converters;

public class SizeConverter : IValueConverter
{
    public static readonly SizeConverter Instance = new();
    
    const long KB = 1024;
    const long MB = KB * 1024;
    const long GB = MB * 1024;
    const long TB = GB * 1024;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not long sourceText || 
            targetType != typeof(string))
        {
            return new BindingNotification(new InvalidCastException(),
            BindingErrorType.Error);
        }

        double size = sourceText;
        switch (sourceText)
        {
            case <= 0:
                return "";
            case >= TB:
                size = Math.Round((double)sourceText / TB, 2);
                return $"{size} TB";
            case >= GB:
                size = Math.Round((double)sourceText / GB, 2);
                return $"{size} GB";
            case >= MB:
                size = Math.Round((double)sourceText / MB, 2);
                return $"{size} MB";
            case >= KB:
                size = Math.Round((double)sourceText / KB, 2);
                return $"{size} KB";
            default:
                return $"{size} Bytes";
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => "";
}