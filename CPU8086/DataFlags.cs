﻿using System;

namespace Final.CPU8086
{
    [Flags]
    public enum DataFlags : int
    {
        None = 0,
        SignExtendedImm8 = 1 << 0,
    }
}