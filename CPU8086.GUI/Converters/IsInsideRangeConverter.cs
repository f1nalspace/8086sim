using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Final.CPU8086.Converters
{
    public class IsInsideRangeConverter : IMultiValueConverter
    {
        public bool IsDirectPosition { get; set; } = false;

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count >= 3 &&
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
                    return false;

                uint len = 0;
                if (values.Count >= 4 && values[3] is uint maxLen)
                    len = maxLen;

                if (index >= selectionStart && index < selectionStart + selectionLength)
                    return true;
                if (selectionStart >= index && selectionStart < index + len)
                    return true;
            }
            return false;
        }
    }
}
