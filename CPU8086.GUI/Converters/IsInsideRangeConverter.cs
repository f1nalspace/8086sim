using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Final.CPU8086.Converters
{
    public class IsInsideRangeConverter : MarkupExtension, IMultiValueConverter
    {
        public bool IsDirectPosition { get; set; } = false;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 3 &&
                values[1] is uint selectionStart &&
                values[2] is uint selectionLength)
            {
                if (selectionLength == 0)
                    return false;

                uint index;
                if (values[0] is uint index32)
                    index = index32;
                else if (values[0] is StreamByte sb)
                    index = sb.Index;
                else
                    throw new NotSupportedException();

                uint len = 0;
                if (values.Length >= 4 && values[3] is uint maxLen)
                    len = maxLen;

                if (index >= selectionStart && index < selectionStart + selectionLength)
                    return true;
                if (selectionStart >= index && selectionStart < index + len)
                    return true;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
