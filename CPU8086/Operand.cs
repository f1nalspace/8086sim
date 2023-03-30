using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Final.CPU8086
{
    // https://en.wikibooks.org/wiki/X86_Assembly/X86_Architecture
    public enum OperandKind : int
    {
        Unknown = 0,

        Value,

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

        KeywordFar,
        KeywordPointer,
        KeywordNearPointer,
        KeywordFarPointer,

        TypeDoubleWord,
        TypeShort,
        TypeInt,

        SourceRegister,

        ShortLabel,
        LongLabel,

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

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
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

        public static bool TryParse(string value, out Operand operand)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                operand = default;
                return false;
            }

            Match m;
            if ((m = _rexMNumber.Match(value)).Success)
            {
                int number = int.Parse(m.Groups["num"].Value);
                operand = new Operand(OperandKind.M_Number, number);
                return true;
            }

            if (int.TryParse(value, out int intValue))
            {
                operand = new Operand(OperandKind.Value, intValue);
                return true;
            }

            OperandKind type = StringToType(value);
            operand = new Operand(type);
            return type != OperandKind.Unknown;
        }

        public static Operand Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
            if (!TryParse(value, out Operand operand))
                throw new NotImplementedException($"The operand '{value}' is not implemented!");
            return operand;
        }

        public static OperandKind StringToType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return OperandKind.Unknown;
            return value.ToLower() switch
            {
                "mb" => OperandKind.MemoryByte,
                "mw" => OperandKind.MemoryWord,
                "md" => OperandKind.MemoryDoubleWord,
                "mq" => OperandKind.MemoryQuadWord,

                "mwr" => OperandKind.MemoryWordReal,
                "mdr" => OperandKind.MemoryDoubleWordReal,
                "mqr" => OperandKind.MemoryQuadWordReal,
                "mtr" => OperandKind.MemoryTenByteReal,

                "rb" => OperandKind.RegisterByte,
                "rw" => OperandKind.RegisterWord,
                "rd" => OperandKind.RegisterDoubleWord,
                "rmb" => OperandKind.RegisterOrMemoryByte,
                "rmw" => OperandKind.RegisterOrMemoryWord,
                "rmd" => OperandKind.RegisterOrMemoryDoubleWord,
                "rmq" => OperandKind.RegisterOrMemoryQuadWord,

                "ib" => OperandKind.ImmediateByte,
                "iw" => OperandKind.ImmediateWord,
                "id" => OperandKind.ImmediateDoubleWord,

                "far" => OperandKind.KeywordFar,
                "ptr" => OperandKind.KeywordPointer,
                "np" => OperandKind.KeywordNearPointer,
                "fp" => OperandKind.KeywordFarPointer,

                "dword" => OperandKind.TypeDoubleWord,
                "short" => OperandKind.TypeShort,
                "int" => OperandKind.TypeInt,

                "sr" => OperandKind.SourceRegister,

                "sl" => OperandKind.ShortLabel,
                "ll" => OperandKind.LongLabel,

                "st" => OperandKind.ST,
                "st(i)" => OperandKind.ST_I,
                "m" => OperandKind.M,

                "eax" => OperandKind.EAX,
                "rax" => OperandKind.RAX,
                "ax" => OperandKind.AX,
                "al" => OperandKind.AL,
                "ah" => OperandKind.AH,

                "rbx" => OperandKind.RBX,
                "ebx" => OperandKind.EBX,
                "bx" => OperandKind.BX,
                "bl" => OperandKind.BL,
                "bh" => OperandKind.BH,

                "rcx" => OperandKind.RCX,
                "ecx" => OperandKind.ECX,
                "cx" => OperandKind.CX,
                "cl" => OperandKind.CL,
                "ch" => OperandKind.CH,

                "rdx" => OperandKind.RDX,
                "edx" => OperandKind.EDX,
                "dx" => OperandKind.DX,
                "dl" => OperandKind.DL,
                "dh" => OperandKind.DH,

                "rsp" => OperandKind.RSP,
                "esp" => OperandKind.ESP,
                "sp" => OperandKind.SP,

                "rbp" => OperandKind.RBP,
                "ebp" => OperandKind.EBP,
                "bp" => OperandKind.BP,

                "rsi" => OperandKind.RSI,
                "esi" => OperandKind.ESI,
                "si" => OperandKind.SI,

                "rdi" => OperandKind.RDI,
                "edi" => OperandKind.EDI,
                "di" => OperandKind.DI,

                "cs" => OperandKind.CS,
                "ds" => OperandKind.DS,
                "ss" => OperandKind.SS,
                "es" => OperandKind.ES,

                "cr" => OperandKind.CR,
                "dr" => OperandKind.DR,
                "tr" => OperandKind.TR,

                "fs" => OperandKind.FS,
                "gs" => OperandKind.GS,

                _ => OperandKind.Unknown
            };
        }

        public static string TypeToString(OperandKind type)
        {
            return type switch
            {
                OperandKind.Value => nameof(OperandKind.Value),

                OperandKind.MemoryByte => nameof(OperandKind.MemoryByte),
                OperandKind.MemoryWord => nameof(OperandKind.MemoryWord),
                OperandKind.MemoryDoubleWord => nameof(OperandKind.MemoryDoubleWord),
                OperandKind.MemoryQuadWord => nameof(OperandKind.MemoryQuadWord),
                OperandKind.MemoryWordReal => nameof(OperandKind.MemoryWordReal),
                OperandKind.MemoryDoubleWordReal => nameof(OperandKind.MemoryDoubleWordReal),
                OperandKind.MemoryQuadWordReal => nameof(OperandKind.MemoryQuadWordReal),
                OperandKind.MemoryTenByteReal => nameof(OperandKind.MemoryTenByteReal),

                OperandKind.RegisterByte => nameof(OperandKind.RegisterByte),
                OperandKind.RegisterWord => nameof(OperandKind.RegisterWord),
                OperandKind.RegisterDoubleWord => nameof(OperandKind.RegisterDoubleWord),

                OperandKind.RegisterOrMemoryByte => nameof(OperandKind.RegisterOrMemoryByte),
                OperandKind.RegisterOrMemoryWord => nameof(OperandKind.RegisterOrMemoryWord),
                OperandKind.RegisterOrMemoryDoubleWord => nameof(OperandKind.RegisterOrMemoryDoubleWord),
                OperandKind.RegisterOrMemoryQuadWord => nameof(OperandKind.RegisterOrMemoryQuadWord),

                OperandKind.ImmediateByte => nameof(OperandKind.ImmediateByte),
                OperandKind.ImmediateWord => nameof(OperandKind.ImmediateWord),
                OperandKind.ImmediateDoubleWord => nameof(OperandKind.ImmediateDoubleWord),

                OperandKind.KeywordFar => nameof(OperandKind.KeywordFar),
                OperandKind.KeywordPointer => nameof(OperandKind.KeywordPointer),
                OperandKind.KeywordNearPointer => nameof(OperandKind.KeywordNearPointer),
                OperandKind.KeywordFarPointer => nameof(OperandKind.KeywordFarPointer),

                OperandKind.TypeDoubleWord => nameof(OperandKind.TypeDoubleWord),
                OperandKind.TypeShort => nameof(OperandKind.TypeShort),
                OperandKind.TypeInt => nameof(OperandKind.TypeInt),

                OperandKind.SourceRegister => nameof(OperandKind.SourceRegister),
                
                OperandKind.ShortLabel => nameof(OperandKind.ShortLabel),
                OperandKind.LongLabel => nameof(OperandKind.LongLabel),

                OperandKind.ST => nameof(OperandKind.ST),
                OperandKind.ST_I => nameof(OperandKind.ST_I),
                OperandKind.M => nameof(OperandKind.M),
                OperandKind.M_Number => nameof(OperandKind.M_Number),

                OperandKind.RAX => nameof(OperandKind.RAX),
                OperandKind.EAX => nameof(OperandKind.EAX),
                OperandKind.AX => nameof(OperandKind.AX),
                OperandKind.AL => nameof(OperandKind.AL),
                OperandKind.AH => nameof(OperandKind.AH),

                OperandKind.RBX => nameof(OperandKind.RBX),
                OperandKind.EBX => nameof(OperandKind.EBX),
                OperandKind.BX => nameof(OperandKind.BX),
                OperandKind.BL => nameof(OperandKind.BL),
                OperandKind.BH => nameof(OperandKind.BH),

                OperandKind.RCX => nameof(OperandKind.RCX),
                OperandKind.ECX => nameof(OperandKind.ECX),
                OperandKind.CX =>  nameof(OperandKind.CX),
                OperandKind.CL =>  nameof(OperandKind.CL),
                OperandKind.CH =>  nameof(OperandKind.CH),

                OperandKind.RDX => nameof(OperandKind.RDX),
                OperandKind.EDX => nameof(OperandKind.EDX),
                OperandKind.DX =>  nameof(OperandKind.DX),
                OperandKind.DL =>  nameof(OperandKind.DL),
                OperandKind.DH =>  nameof(OperandKind.DH),

                OperandKind.RSP => nameof(OperandKind.RSP),
                OperandKind.ESP => nameof(OperandKind.ESP),
                OperandKind.SP =>  nameof(OperandKind.SP),

                OperandKind.RBP => nameof(OperandKind.RBP),
                OperandKind.EBP => nameof(OperandKind.EBP),
                OperandKind.BP =>  nameof(OperandKind.BP),

                OperandKind.RSI => nameof(OperandKind.RSI),
                OperandKind.ESI => nameof(OperandKind.ESI),
                OperandKind.SI =>  nameof(OperandKind.SI),

                OperandKind.RDI => nameof(OperandKind.RDI),
                OperandKind.EDI => nameof(OperandKind.EDI),
                OperandKind.DI =>  nameof(OperandKind.DI),

                OperandKind.CS =>  nameof(OperandKind.CS),
                OperandKind.DS =>  nameof(OperandKind.DS),
                OperandKind.SS =>  nameof(OperandKind.SS),
                OperandKind.ES =>  nameof(OperandKind.ES),

                OperandKind.CR =>  nameof(OperandKind.CR),
                OperandKind.DR =>  nameof(OperandKind.DR),
                OperandKind.TR =>  nameof(OperandKind.TR),

                OperandKind.FS =>  nameof(OperandKind.FS),
                OperandKind.GS => nameof(OperandKind.GS),

                _ => nameof(OperandKind.Unknown),
            };
        }

        public override string ToString()
        {
            if (Type == OperandKind.Value)
                return Value.ToString("D");
            else
                return TypeToString(Type);
        }
    }
}
