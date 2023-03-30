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

            OperandKind type = value.ToLower() switch
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

                "rax" => OperandKind.RAX,
                "eax" => OperandKind.EAX,
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

        public static string TypeToString(OperandKind type)
        {
            return type switch
            {
                OperandKind.MemoryByte => "mb",
                OperandKind.MemoryWord => "mw",
                OperandKind.MemoryDoubleWord => "md",
                OperandKind.MemoryQuadWord => "mq",

                OperandKind.MemoryWordReal => "mwr",
                OperandKind.MemoryDoubleWordReal => "mdr",
                OperandKind.MemoryQuadWordReal => "mqr",
                OperandKind.MemoryTenByteReal => "mtr",

                OperandKind.RegisterByte => "rb",
                OperandKind.RegisterWord => "rw",
                OperandKind.RegisterDoubleWord => "rd",

                OperandKind.RegisterOrMemoryByte => "rmb",
                OperandKind.RegisterOrMemoryWord => "rmw",
                OperandKind.RegisterOrMemoryDoubleWord => "rmd",
                OperandKind.RegisterOrMemoryQuadWord => "rmq",

                OperandKind.ImmediateByte => "ib",
                OperandKind.ImmediateWord => "iw",
                OperandKind.ImmediateDoubleWord => "id",

                OperandKind.KeywordFar => "far",
                OperandKind.KeywordPointer => "ptr",
                OperandKind.KeywordNearPointer => "np",
                OperandKind.KeywordFarPointer => "fp",

                OperandKind.TypeDoubleWord => "dword",
                OperandKind.TypeShort => "short",
                OperandKind.TypeInt => "int",

                OperandKind.SourceRegister => "sr",
                
                OperandKind.ShortLabel => "sl",
                OperandKind.LongLabel => "ll",

                OperandKind.ST => "st",
                OperandKind.ST_I => "st(i)",
                OperandKind.M => "m",

                OperandKind.RAX => "rax",
                OperandKind.EAX => "eax",
                OperandKind.AX => "ax",
                OperandKind.AL => "al",
                OperandKind.AH => "ah",

                OperandKind.RBX => "rbx",
                OperandKind.EBX => "ebx",
                OperandKind.BX => "bx",
                OperandKind.BL => "bl",
                OperandKind.BH => "bh",

                OperandKind.RCX => "rcx",
                OperandKind.ECX => "ecx",
                OperandKind.CX =>  "cx",
                OperandKind.CL =>  "cl",
                OperandKind.CH =>  "ch",

                OperandKind.RDX => "rdx",
                OperandKind.EDX => "edx",
                OperandKind.DX =>  "dx",
                OperandKind.DL =>  "dl",
                OperandKind.DH =>  "dh",

                OperandKind.RSP => "rsp",
                OperandKind.ESP => "esp",
                OperandKind.SP =>  "sp",

                OperandKind.RBP => "rbp",
                OperandKind.EBP => "ebp",
                OperandKind.BP =>  "bp",

                OperandKind.RSI => "rsi",
                OperandKind.ESI => "esi",
                OperandKind.SI =>  "si",

                OperandKind.RDI => "rdi",
                OperandKind.EDI => "edi",
                OperandKind.DI =>  "di",

                OperandKind.CS =>  "cs",
                OperandKind.DS =>  "ds",
                OperandKind.SS =>  "ss",
                OperandKind.ES =>  "es",

                OperandKind.CR =>  "cr",
                OperandKind.DR =>  "dr",
                OperandKind.TR =>  "tr",

                OperandKind.FS =>  "fs",
                OperandKind.GS => "gs",

                _ => string.Empty,
            };
        }

        public static implicit operator Operand(string value) => Parse(value);
        public static explicit operator string(Operand op) => op.ToString();

        public override string ToString()
        {
            if (Type == OperandKind.Value)
                return Value.ToString("D");
            else if (Type == OperandKind.M_Number)
                return $"M{Value:D}";
            else
                return TypeToString(Type);
        }
    }
}
