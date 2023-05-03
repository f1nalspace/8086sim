using DevExpress.Mvvm.Native;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Final.CPU8086
{
    public class HexCellValueConverter : MarkupExtension, IValueConverter, IMultiValueConverter
    {
        public string HexPrefix { get; set; } = string.Empty;

        private static object DoConvert(object value, bool asHex, string hexPrefix, int s32Offset = 0)
        {
            if (value is byte u8Value)
            {
                if (asHex)
                    return $"{hexPrefix}{u8Value:X2}";
                else
                    return u8Value.ToString("D");
            }
            else if (value is sbyte s8Value)
            {
                if (asHex)
                    return $"{hexPrefix}{s8Value:X2}";
                else
                    return s8Value.ToString("D");
            }
            else if (value is ushort u16Value)
            {
                if (asHex)
                    return $"{hexPrefix}{u16Value:X4}";
                else
                    return u16Value.ToString("D");
            }
            else if (value is short s16Value)
            {
                if (asHex)
                    return $"{hexPrefix}{s16Value:X4}";
                else
                    return s16Value.ToString("D");
            }
            else if (value is uint u32Value)
            {
                if (asHex)
                    return $"{hexPrefix}{u32Value:X8}";
                else
                    return u32Value.ToString("D");
            }
            else if (value is int s32Value)
            {
                if (asHex)
                    return $"{hexPrefix}{(s32Offset + s32Value):X8}";
                else
                    return (s32Offset + s32Value).ToString("D");
            }
            else if (value is StreamByte sb)
            {
                if (asHex)
                    return $"{hexPrefix}{sb.Value:X2}";
                else
                    return sb.Value.ToString("D");
            }
            return DependencyProperty.UnsetValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => DoConvert(value, true, HexPrefix);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                bool asHex = values[1] is bool b && b;
                if (values.Length >= 3 && values[2] is int offset)
                    return DoConvert(values[0], asHex, HexPrefix, offset);
                else
                    return DoConvert(values[0], asHex, HexPrefix);
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        

        public override object ProvideValue(IServiceProvider serviceProvider)
            => this;
    }
}
