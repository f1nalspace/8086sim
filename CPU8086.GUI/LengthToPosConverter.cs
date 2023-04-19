using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Final.CPU8086
{
    public class LengthToPosConverter : MarkupExtension, IValueConverter
    {
        public int Columns { get; set; } = 8;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int len && len > 0)
            {
                int rowCount = (len / Columns) + 1;
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

        public override object ProvideValue(IServiceProvider serviceProvider)
            => this;
    }
}
