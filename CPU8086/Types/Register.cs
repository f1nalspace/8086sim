﻿namespace Final.CPU8086.Types
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
                RegisterType.RAX => 8,
                RegisterType.EAX => 4,
                RegisterType.AX => 2,
                RegisterType.AL => 1,
                RegisterType.AH => 1,

                RegisterType.RBX => 8,
                RegisterType.EBX => 4,
                RegisterType.BX => 2,
                RegisterType.BL => 1,
                RegisterType.BH => 1,

                RegisterType.RCX => 8,
                RegisterType.ECX => 4,
                RegisterType.CX => 2,
                RegisterType.CL => 1,
                RegisterType.CH => 1,

                RegisterType.RDX => 8,
                RegisterType.EDX => 4,
                RegisterType.DX => 2,
                RegisterType.DL => 1,
                RegisterType.DH => 1,

                RegisterType.RSP => 8,
                RegisterType.ESP => 4,
                RegisterType.SP => 2,

                RegisterType.RBP => 8,
                RegisterType.EBP => 4,
                RegisterType.BP => 2,

                RegisterType.RSI => 8,
                RegisterType.ESI => 4,
                RegisterType.SI => 2,

                RegisterType.RDI => 8,
                RegisterType.EDI => 4,
                RegisterType.DI => 2,

                RegisterType.CS => 2,
                RegisterType.DS => 2,
                RegisterType.SS => 2,
                RegisterType.ES => 2,

                RegisterType.CR => 2,
                RegisterType.DR => 2,
                RegisterType.TR => 2,
                RegisterType.FS => 2,
                RegisterType.GS => 2,

                _ => 0
            };
        }

        public static string GetName(RegisterType name)
        {
            return name switch
            {
                RegisterType.RAX => nameof(RegisterType.RAX),
                RegisterType.EAX => nameof(RegisterType.EAX),
                RegisterType.AX => nameof(RegisterType.AX),
                RegisterType.AL => nameof(RegisterType.AL),
                RegisterType.AH => nameof(RegisterType.AH),

                RegisterType.RBX => nameof(RegisterType.RBX),
                RegisterType.EBX => nameof(RegisterType.EBX),
                RegisterType.BX => nameof(RegisterType.BX),
                RegisterType.BL => nameof(RegisterType.BL),
                RegisterType.BH => nameof(RegisterType.BH),

                RegisterType.RCX => nameof(RegisterType.RCX),
                RegisterType.ECX => nameof(RegisterType.ECX),
                RegisterType.CX => nameof(RegisterType.CX),
                RegisterType.CL => nameof(RegisterType.CL),
                RegisterType.CH => nameof(RegisterType.CH),

                RegisterType.RDX => nameof(RegisterType.RDX),
                RegisterType.EDX => nameof(RegisterType.EDX),
                RegisterType.DX => nameof(RegisterType.DX),
                RegisterType.DL => nameof(RegisterType.DL),
                RegisterType.DH => nameof(RegisterType.DH),

                RegisterType.RSP => nameof(RegisterType.RSP),
                RegisterType.ESP => nameof(RegisterType.ESP),
                RegisterType.SP => nameof(RegisterType.SP),

                RegisterType.RBP => nameof(RegisterType.RBP),
                RegisterType.EBP => nameof(RegisterType.EBP),
                RegisterType.BP => nameof(RegisterType.BP),

                RegisterType.RSI => nameof(RegisterType.RSI),
                RegisterType.ESI => nameof(RegisterType.ESI),
                RegisterType.SI => nameof(RegisterType.SI),

                RegisterType.RDI => nameof(RegisterType.RDI),
                RegisterType.EDI => nameof(RegisterType.EDI),
                RegisterType.DI => nameof(RegisterType.DI),

                RegisterType.CS => nameof(RegisterType.CS),
                RegisterType.DS => nameof(RegisterType.DS),
                RegisterType.SS => nameof(RegisterType.SS),
                RegisterType.ES => nameof(RegisterType.ES),

                RegisterType.CR => nameof(RegisterType.CR),
                RegisterType.DR => nameof(RegisterType.DR),
                RegisterType.TR => nameof(RegisterType.TR),
                RegisterType.FS => nameof(RegisterType.FS),
                RegisterType.GS => nameof(RegisterType.GS),

                _ => null
            };
        }

        public override string ToString() => GetName(Type);
    }
}