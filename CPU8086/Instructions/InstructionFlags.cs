using System;

namespace Final.CPU8086.Instructions
{
    [Flags]
    public enum InstructionFlags : uint
    {
        None = 0,
        Lock = 1 << 0,
        Rep = 1 << 1,
        Segment = 1 << 2,
        Near = 1 << 3,
        Far = 1 << 4,
        SignExtendedImm8 = 1 << 5,
        Prefix = 1 << 6,
        Override = 1 << 7,
    }
}
