using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Final.CPU8086.Converters
{
    // Markiert das Element, dessen Position der aktuellen Ausfuehrungsposition entspricht.
    //   values[0] = Position (uint)
    //   values[1] = CurrentStreamPosition (uint, uint.MaxValue = keine)
    public class PositionMarkerConverter : IMultiValueConverter
    {
        public IBrush InsideBrush { get; set; }
        public IBrush OutsideBrush { get; set; }

        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count >= 2 && values[0] is uint position && values[1] is uint current
                && current != uint.MaxValue && position == current)
                return InsideBrush;
            return OutsideBrush;
        }
    }
}
