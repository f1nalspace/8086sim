using System;

namespace Final.ITP
{
    public static class ValueTypeExtensions
    {
        public static string ToBinary(this byte value) => Convert.ToString(value, 2).PadLeft(8, '0');
    }
}
