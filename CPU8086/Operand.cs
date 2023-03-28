using System;
using System.Text.RegularExpressions;

namespace Final.CPU8086
{
    // https://en.wikibooks.org/wiki/X86_Assembly/X86_Architecture
    public enum OperandKind
    {
        Unknown = 0,

        MemoryByte,
        MemoryWord,
        MemoryDoubleWord,
        MemoryQuadWord,

        MemoryWordReal,
        MemoryDoubleWordReal,
        MemoryQuadWordReal,
        MemoryTenByteReal,

        RegisterByte,
        RegisterWord,
        RegisterDoubleWord,

        RegisterOrMemoryByte,
        RegisterOrMemoryWord,
        RegisterOrMemoryDoubleWord,
        RegisterOrMemoryQuadWord,

        ImmediateByte,
        ImmediateWord,
        ImmediateDoubleWord,

        Pointer,
        NearPointer,
        FarPointer,

        TypeDoubleWord,
        TypeShort,
        TypeInt,

        PrefixFar,

        SourceRegister,

        ShortLabel,
        LongLabel,

        Value,
        ST,
        ST_I,
        M,
        M_Number,

        RAX,
        EAX,
        AX,
        AL,
        AH,

        RBX,
        EBX,
        BX,
        BL,
        BH,

        RCX,
        ECX,
        CX,
        CL,
        CH,

        RDX,
        EDX,
        DX,
        DL,
        DH,

        RSP,
        ESP,
        SP,

        RBP,
        EBP,
        BP,

        RSI,
        ESI,
        SI,

        RDI,
        EDI,
        DI,

        CS,
        DS,
        SS,
        ES,

        CR,
        DR,
        TR,

        FS,
        GS,
    }

    public readonly struct Operand
    {
        private static readonly Regex _rexMNumber = new Regex("(?<prefix>[m])(?<num>[0-9]{1,2})", RegexOptions.Compiled);

        public OperandKind Type { get; }
        public int Value { get; }

        public Operand(OperandKind type, int value = 0)
        {
            Type = type;
            Value = value;
        }

        public Operand(int value) : this(OperandKind.Value, value) { }

        public static Operand Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));

            Match m;
            if ((m = _rexMNumber.Match(value)).Success)
            {
                int number = int.Parse(m.Groups["num"].Value);
                return new Operand(OperandKind.M_Number, number);
            }

            if (int.TryParse(value, out int intValue))
                return new Operand(OperandKind.Value, intValue);

            return value.ToLower() switch
            {
                "mb" => new Operand(OperandKind.MemoryByte),
                "mw" => new Operand(OperandKind.MemoryWord),
                "md" => new Operand(OperandKind.MemoryDoubleWord),
                "mq" => new Operand(OperandKind.MemoryQuadWord),

                "mwr" => new Operand(OperandKind.MemoryWordReal),
                "mdr" => new Operand(OperandKind.MemoryDoubleWordReal),
                "mqr" => new Operand(OperandKind.MemoryQuadWordReal),
                "mtr" => new Operand(OperandKind.MemoryTenByteReal),

                "rb" => new Operand(OperandKind.RegisterByte),
                "rw" => new Operand(OperandKind.RegisterWord),
                "rd" => new Operand(OperandKind.RegisterDoubleWord),
                "rmb" => new Operand(OperandKind.RegisterOrMemoryByte),
                "rmw" => new Operand(OperandKind.RegisterOrMemoryWord),
                "rmd" => new Operand(OperandKind.RegisterOrMemoryDoubleWord),
                "rmq" => new Operand(OperandKind.RegisterOrMemoryQuadWord),

                "ib" => new Operand(OperandKind.ImmediateByte),
                "iw" => new Operand(OperandKind.ImmediateWord),
                "id" => new Operand(OperandKind.ImmediateDoubleWord),

                "ptr" => new Operand(OperandKind.Pointer),
                "np" => new Operand(OperandKind.NearPointer),
                "fp" => new Operand(OperandKind.FarPointer),

                "dword" => new Operand(OperandKind.TypeDoubleWord),
                "short" => new Operand(OperandKind.TypeShort),
                "int" => new Operand(OperandKind.TypeInt),

                "far" => new Operand(OperandKind.PrefixFar),
                "sr" => new Operand(OperandKind.SourceRegister),

                "sl" => new Operand(OperandKind.ShortLabel),
                "ll" => new Operand(OperandKind.LongLabel),

                "st" => new Operand(OperandKind.ST),
                "st(i)" => new Operand(OperandKind.ST_I),

                "m" => new Operand(OperandKind.M),

                "eax" => new Operand(OperandKind.EAX),
                "rax" => new Operand(OperandKind.RAX),
                "ax" => new Operand(OperandKind.AX),
                "al" => new Operand(OperandKind.AL),
                "ah" => new Operand(OperandKind.AH),

                "rbx" => new Operand(OperandKind.RBX),
                "ebx" => new Operand(OperandKind.EBX),
                "bx" => new Operand(OperandKind.BX),
                "bl" => new Operand(OperandKind.BL),
                "bh" => new Operand(OperandKind.BH),

                "rcx" => new Operand(OperandKind.RCX),
                "ecx" => new Operand(OperandKind.ECX),
                "cx" => new Operand(OperandKind.CX),
                "cl" => new Operand(OperandKind.CL),
                "ch" => new Operand(OperandKind.CH),

                "rdx" => new Operand(OperandKind.RDX),
                "edx" => new Operand(OperandKind.EDX),
                "dx" => new Operand(OperandKind.DX),
                "dl" => new Operand(OperandKind.DL),
                "dh" => new Operand(OperandKind.DH),

                "rsp" => new Operand(OperandKind.RSP),
                "esp" => new Operand(OperandKind.ESP),
                "sp" => new Operand(OperandKind.SP),

                "rbp" => new Operand(OperandKind.RBP),
                "ebp" => new Operand(OperandKind.EBP),
                "bp" => new Operand(OperandKind.BP),

                "rsi" => new Operand(OperandKind.RSI),
                "esi" => new Operand(OperandKind.ESI),
                "si" => new Operand(OperandKind.SI),

                "rdi" => new Operand(OperandKind.RDI),
                "edi" => new Operand(OperandKind.EDI),
                "di" => new Operand(OperandKind.DI),

                "cs" => new Operand(OperandKind.CS),
                "ds" => new Operand(OperandKind.DS),
                "ss" => new Operand(OperandKind.SS),
                "es" => new Operand(OperandKind.ES),

                "cr" => new Operand(OperandKind.CR),
                "dr" => new Operand(OperandKind.DR),
                "tr" => new Operand(OperandKind.TR),

                "fs" => new Operand(OperandKind.FS),
                "gs" => new Operand(OperandKind.GS),

                _ => throw new NotImplementedException($"The operand '{value}' is not implemented!")
            };
        }

        public override string ToString()
        {
            if (Type != OperandKind.Unknown)
                return Type.ToString();
            else
                return string.Empty;
        }
    }
}
