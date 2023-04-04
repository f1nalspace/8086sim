using System;

namespace Final.CPU8086
{
    [Flags]
    public enum DataType : byte
    {
        None = 0,
        Byte = 1 << 1,
        Word = 1 << 2,
        Int = 1 << 3,
        DoubleWord = 1 << 4,
        Pointer = 1 << 5,
        Far = 1 << 6,
    }
}
