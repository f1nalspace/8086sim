using System;

namespace CPU8086
{
    public static class ValueTypeExtensions
    {
        public static string ToBinary(this byte value) => Convert.ToString(value, 2).PadLeft(8, '0');
    }
}
