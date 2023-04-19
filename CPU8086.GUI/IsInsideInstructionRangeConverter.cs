using System;
using System.Globalization;
using System.Windows.Data;

namespace Final.CPU8086
{
    public class IsInsideInstructionRangeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is uint index && values[1] is Instruction instruction)
            {
                uint len = 0;
                if (values.Length >= 3 && values[2] is uint maxLen)
                    len = maxLen;

                if (index >= instruction.Position && index < (instruction.Position + instruction.Length))
                    return true;
                if (instruction.Position >= index && instruction.Position < (index + len))
                    return true;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
