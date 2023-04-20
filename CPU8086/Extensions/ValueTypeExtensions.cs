using System;

namespace Final.CPU8086.Extensions
{
    public static class ValueTypeExtensions
    {
        public static string ToBinary(this byte value) => Convert.ToString(value, 2).PadLeft(8, '0');
    }
}
