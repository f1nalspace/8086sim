using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Final.CPU8086.Converters;

// Liefert true, wenn der Index in [start, start+length) liegt (bzw. start in
// [index, index+maxLen) hineinragt). Gleiche Range-Logik wie RangeHighlightConverter, nur als
// bool – damit die Markierungsfarbe per Style-Klasse + DynamicResource (theme-faehig) gesetzt
// werden kann statt fest im Converter.
//   values[0] = index (uint) oder StreamByte
//   values[1] = rangeStart (uint)
//   values[2] = rangeLength (uint)
//   values[3] = optional maxLen (uint)
public class RangeMatchConverter : IMultiValueConverter
{
    public uint MaxLen { get; set; } = 0;

    public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
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