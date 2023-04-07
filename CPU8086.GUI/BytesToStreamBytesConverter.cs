using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Data;
using System.Windows.Markup;

namespace Final.CPU8086
{
    public class BytesToStreamBytesConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<byte> bytes)
            {
                int index = 0;
                ImmutableArray<StreamByte> result = bytes.Select(b => new StreamByte(Interlocked.Increment(ref index) - 1, b)).ToImmutableArray();
                return result;
            }
            return ImmutableArray<StreamByte>.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
             => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider)
            => this;
    }
}
