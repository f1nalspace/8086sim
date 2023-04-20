using System;

namespace Final.CPU8086.Types
{
    [Flags]
    public enum DataType : byte
    {
        None = 0,
        Byte = 1 << 0,
        Word = 1 << 1,
        DoubleWord = 1 << 2,
        Int = 1 << 3,
        QuadWord = 1 << 4,
        Pointer = 1 << 5,
    }
}
