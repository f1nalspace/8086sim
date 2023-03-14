using System;

namespace Final.CPU8086
{
    [Flags]
    public enum FieldEncoding
    {
        None = 0,
        ModRemRM = 1 << 0,
    }
}
