using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace dawn.ftp.ui.BusinessLogic.Converters;

public class TimeConverter : IValueConverter {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not TimeSpan timeSpan) {
            return value;
        }

        if (timeSpan.TotalHours >= 1) {
            return $"{timeSpan.TotalHours:F0} hours";
        }
        
        if (timeSpan.TotalMinutes >= 1) {
            return $"{timeSpan.TotalMinutes:F0} minutes";
        }
        
        if (timeSpan.TotalSeconds >= 1) {
            return $"{timeSpan.TotalSeconds:F0} seconds";
        }

        return $"{timeSpan.TotalMilliseconds:F0} milliseconds";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        return "";
    }
}