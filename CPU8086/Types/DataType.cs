using System;

namespace Final.CPU8086.Types
{
    [Flags]
    public enum DataType : byte
    {
        None = 0,
        Byte = 1 << 0,
        Word = 1 << 1,
        Short = 1 << 2,
        DoubleWord = 1 << 3,
        Int = 1 << 4,
        QuadWord = 1 << 5,
        Pointer = 1 << 6,
    }
}
