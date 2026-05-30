using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Final.CPU8086.Converters
{
    public class BytePositionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is uint bytePos)
            {
                if (bytePos != uint.MaxValue)
                    return bytePos.ToString();
            }
            return "-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
