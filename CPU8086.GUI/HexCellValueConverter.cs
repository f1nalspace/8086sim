using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Final.CPU8086
{
    public class HexCellValueConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                bool asHex = values[1] is bool b && b;
                if (values[0] is byte u8Value)
                {
                    if (asHex)
                        return u8Value.ToString("X2");
                    else
                        return u8Value.ToString("D");
                }
                else if (values[0] is sbyte s8Value)
                {
                    if (asHex)
                        return s8Value.ToString("X2");
                    else
                        return s8Value.ToString("D");
                }
                else if (values[0] is ushort u16Value)
                {
                    if (asHex)
                        return u16Value.ToString("X4");
                    else
                        return u16Value.ToString("D");
                }
                else if (values[0] is short s16Value)
                {
                    if (asHex)
                        return s16Value.ToString("X4");
                    else
                        return s16Value.ToString("D");
                }
                else if (values[0] is StreamByte sb)
                {
                    if (asHex)
                        return sb.Value.ToString("X2");
                    else
                        return sb.Value.ToString("D");
                }
            }
            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider)
            => this;
    }
}
