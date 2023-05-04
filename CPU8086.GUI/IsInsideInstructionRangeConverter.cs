using System;
using System.Globalization;
using System.Windows.Data;
using Final.CPU8086.Instructions;
using Final.CPU8086.Types;

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
                    if (values.Length >= 3 && values[2] is AssemblyLine line)
                    {
                        if (line.Type == AssemblyLineType.SourceLabel)
                            return false;
                    }
                    return testIndex == streamIndex;
                }
                else
                {
                    Instruction instruction = null;
                    AssemblyLine line = null;
                    if (values.Length >= 3)
                    {
                        if (values[2] is Instruction valueInstruction)
                            instruction = valueInstruction;
                        else if (values[2] is AssemblyLine valueLine)
                            line = valueLine;
                    }

                    uint len = 0;
                    if (values.Length >= 4 && values[3] is uint maxLen)
                        len = maxLen;

                    if (line != null)
                    {
                        if (line.Type == AssemblyLineType.SourceLabel)
                            return false;
                    }

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
