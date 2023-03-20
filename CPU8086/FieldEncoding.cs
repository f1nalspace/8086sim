using System;

namespace Final.CPU8086
{
    [Flags]
    public enum FieldEncoding
    {
        None = 0,
        Mod = 1 << 0,
        Reg = 1 << 1,
        RM = 1 << 2,
        // @TODO(final): Fix typo, Rem vs Reg
        ModRemRM = Mod | Reg | RM
    }
}
