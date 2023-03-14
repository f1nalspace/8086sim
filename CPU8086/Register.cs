namespace CPU8086
{
    public class Register
    {
        public byte Code { get; }
        public RegisterType Type { get; }

        public Register(byte code, RegisterType type)
        {
            Code = code;
            Type = type;
        }

        public static byte GetLength(RegisterType name)
        {
            return name switch
            {
                RegisterType.AX => 2,
                RegisterType.AL => 1,
                RegisterType.AH => 1,

                RegisterType.BX => 2,
                RegisterType.BL => 1,
                RegisterType.BH => 1,

                RegisterType.CX => 2,
                RegisterType.CL => 1,
                RegisterType.CH => 1,

                RegisterType.DX => 2,
                RegisterType.DL => 1,
                RegisterType.DH => 1,

                RegisterType.SP => 2,
                RegisterType.BP => 2,
                RegisterType.SI => 2,
                RegisterType.DI => 2,

                RegisterType.CS => 2,
                RegisterType.DS => 2,
                RegisterType.SS => 2,
                RegisterType.ES => 2,

                _ => 0
            };
        }

        public static string GetLowerName(RegisterType name)
        {
            return name switch
            {
                RegisterType.AX => "ax",
                RegisterType.AL => "al",
                RegisterType.AH => "ah",

                RegisterType.BX => "bx",
                RegisterType.BL => "bl",
                RegisterType.BH => "bh",

                RegisterType.CX => "cx",
                RegisterType.CL => "cl",
                RegisterType.CH => "ch",

                RegisterType.DX => "dx",
                RegisterType.DL => "dl",
                RegisterType.DH => "dh",

                RegisterType.SP => "sp",
                RegisterType.BP => "bp",
                RegisterType.SI => "si",
                RegisterType.DI => "di",

                RegisterType.CS => "cs",
                RegisterType.DS => "ds",
                RegisterType.SS => "ss",
                RegisterType.ES => "es",

                _ => null
            };
        }

        public static string GetName(RegisterType name)
        {
            return name switch
            {
                RegisterType.AX => nameof(RegisterType.AX),
                RegisterType.AL => nameof(RegisterType.AL),
                RegisterType.AH => nameof(RegisterType.AH),

                RegisterType.BX => nameof(RegisterType.BX),
                RegisterType.BL => nameof(RegisterType.BL),
                RegisterType.BH => nameof(RegisterType.BH),

                RegisterType.CX => nameof(RegisterType.CX),
                RegisterType.CL => nameof(RegisterType.CL),
                RegisterType.CH => nameof(RegisterType.CH),

                RegisterType.DX => nameof(RegisterType.DX),
                RegisterType.DL => nameof(RegisterType.DL),
                RegisterType.DH => nameof(RegisterType.DH),

                RegisterType.SP => nameof(RegisterType.SP),
                RegisterType.BP => nameof(RegisterType.BP),
                RegisterType.SI => nameof(RegisterType.SI),
                RegisterType.DI => nameof(RegisterType.DI),

                RegisterType.CS => nameof(RegisterType.CS),
                RegisterType.DS => nameof(RegisterType.DS),
                RegisterType.SS => nameof(RegisterType.SS),
                RegisterType.ES => nameof(RegisterType.ES),

                _ => null
            };
        }

        public override string ToString() => GetName(Type);
    }
}
