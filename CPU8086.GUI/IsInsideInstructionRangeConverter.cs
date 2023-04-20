using System;
using System.Globalization;
using System.Windows.Data;
using Final.CPU8086.Instructions;

namespace Final.CPU8086
{
    public class IsInsideInstructionRangeConverter : IMultiValueConverter
    {
        public bool IsDirectPosition { get; set; } = false;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is uint testIndex && values[1] is uint streamIndex)
            {
                if (IsDirectPosition)
                {
                    return testIndex == streamIndex;
                }
                else
                {
                    Instruction instruction = null;
                    if (values.Length >= 3 && values[2] is Instruction valueInstruction)
                        instruction = valueInstruction;

                    uint len = 0;
                    if (values.Length >= 4 && values[3] is uint maxLen)
                        len = maxLen;

                    if (instruction != null)
                    {
                        if (testIndex >= instruction.Position && testIndex < (instruction.Position + instruction.Length))
                            return true;
                        if (instruction.Position >= testIndex && instruction.Position < (testIndex + len))
                            return true;
                    }
                    else
                    {
                        if (streamIndex == testIndex)
                            return true;
                        if (streamIndex >= testIndex && streamIndex < (testIndex + len))
                            return true;
                    }
                }
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
