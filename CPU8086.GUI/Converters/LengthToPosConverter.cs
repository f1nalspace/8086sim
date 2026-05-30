using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Final.CPU8086.Converters;

public class LengthToPosConverter : IValueConverter
{
    public int Columns { get; set; } = 8;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int len && len > 0)
        {
            int rowCount = len / Columns + 1;
            uint[] result = new uint[rowCount];
            uint p = 0;
            for (int i = 0; i < rowCount; ++i)
            {
                result[i] = p;
                p += (uint)Columns;
            }
            return result;
        }
        return Array.Empty<uint>();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}