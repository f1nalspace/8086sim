using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Final.CPU8086.Converters;

// Avalonia-Ersatz fuer DevExpress' NumericToVisibilityConverter.
// Avalonia kennt keine Visibility-Enum -> liefert bool fuer IsVisible
// (numerischer Wert != 0). Inverse kehrt das Ergebnis um.
public class NumericToBoolConverter : IValueConverter
{
    public bool Inverse { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool hasValue = value switch
        {
            int i => i != 0,
            uint u => u != 0,
            long l => l != 0,
            short s => s != 0,
            byte b => b != 0,
            _ => value != null
        };
        return Inverse ? !hasValue : hasValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}