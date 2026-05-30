using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Final.CPU8086.Converters;

// Avalonia-Ersatz fuer die WPF-DataTrigger-Hervorhebung. Avalonia kennt keine
// DataTrigger; stattdessen liefert dieser MultiBinding-Converter direkt den Pinsel:
// liegt der Index in [start, start+length) (bzw. start in [index, index+maxLen)),
// wird InsideBrush zurueckgegeben, sonst OutsideBrush. Gleiche Range-Logik wie
// IsInsideRangeConverter.
//   values[0] = index (uint) oder StreamByte
//   values[1] = rangeStart (uint)
//   values[2] = rangeLength (uint)
//   values[3] = optional maxLen (uint)
public class RangeHighlightConverter : IMultiValueConverter
{
    public IBrush InsideBrush { get; set; }
    public IBrush OutsideBrush { get; set; }

    // Optionales maxLen (Selektion, die in eine Zeile hineinragt, hebt die ganze
    // Zeile hervor). Greift, wenn die Bindung kein viertes Element liefert.
    public uint MaxLen { get; set; } = 0;

    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        => IsInside(values) ? InsideBrush : OutsideBrush;

    private bool IsInside(IList<object> values)
    {
        if (values.Count >= 3 &&
            values[1] is uint rangeStart &&
            values[2] is uint rangeLength)
        {
            if (rangeLength == 0)
                return false;

            uint index;
            if (values[0] is uint index32)
                index = index32;
            else if (values[0] is StreamByte sb)
                index = sb.Index;
            else
                return false;

            uint len = MaxLen;
            if (values.Count >= 4 && values[3] is uint maxLen)
                len = maxLen;

            if (index >= rangeStart && index < rangeStart + rangeLength)
                return true;
            if (rangeStart >= index && rangeStart < index + len)
                return true;
        }
        return false;
    }
}