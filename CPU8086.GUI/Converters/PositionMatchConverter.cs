using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Final.CPU8086.Converters;

// Liefert true, wenn die Position dem aktuellen Ausfuehrungsstand entspricht. Wird genutzt,
// um den Forward-Marker (Pfeil) in der Assembly- und Instruktionsansicht ein-/auszublenden.
//   values[0] = Position (uint)
//   values[1] = CurrentStreamPosition (uint, uint.MaxValue = keine)
public class PositionMatchConverter : IMultiValueConverter
{
    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
    {
        return values.Count >= 2 && values[0] is uint position && values[1] is uint current
               && current != uint.MaxValue && position == current;
    }
}