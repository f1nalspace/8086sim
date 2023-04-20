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
        Far = 1 << 3,
        SignExtendedImm8 = 1 << 4,
        Prefix = 1 << 5,
        Override = 1 << 6,
    }
}
