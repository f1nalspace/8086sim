using System;

namespace Final.CPU8086
{
    [Flags]
    public enum ImmediateFlag : short
    {
        None = 0,
        RelativeJumpDisplacement = 1 << 0,
    }
}
