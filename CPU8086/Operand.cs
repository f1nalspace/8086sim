using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Final.CPU8086
{
    // https://en.wikibooks.org/wiki/X86_Assembly/X86_Architecture
    public enum OperandKind : byte
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

        TypeDoubleWord,
        TypeShort,
        TypeInt,

        NearPointer,
        FarPointer,

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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct Operand
    {
        private static readonly Regex _rexMNumber = new Regex("(?<prefix>[m])(?<num>[0-9]{1,2})", RegexOptions.Compiled);
        private static readonly Regex _regFull = new Regex("(?<type>\\(.*\\))?\\s*(?<remaining>.*)", RegexOptions.Compiled);

        public OperandKind Kind { get; }
        public DataType DataType { get; }
        public int Value { get; }
        public short Padding { get; }

        public Operand(OperandKind kind, DataType dataType, int value = 0)
        {
            Kind = kind;
            DataType = dataType;
            Value = value;
            Padding = 0;
        }

        public Operand(int value, DataType dataType) : this(OperandKind.Value, dataType, value) { }

        public static bool TryParse(string value, out Operand operand)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                operand = default;
                return false;
            }

            DataType dataType = DataType.None;
            Match fullMatch = _regFull.Match(value);
            if (fullMatch.Success)
            {
                dataType = ParseDataType(fullMatch.Groups["type"].Value);
                value = fullMatch.Groups["remaining"].Value;
            }

            Match m;
            if ((m = _rexMNumber.Match(value)).Success)
            {
                int number = int.Parse(m.Groups["num"].Value);
                operand = new Operand(OperandKind.M_Number, dataType, number);
                return true;
            }

            if (int.TryParse(value, out int intValue))
            {
                operand = new Operand(OperandKind.Value, dataType, intValue);
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

                "dword" => OperandKind.TypeDoubleWord,
                "short" => OperandKind.TypeShort,
                "int" => OperandKind.TypeInt,

                "np" => OperandKind.NearPointer,
                "fp" => OperandKind.FarPointer,
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
            operand = new Operand(type, dataType);
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
                OperandKind.TypeDoubleWord => "dword",
                OperandKind.TypeShort => "short",
                OperandKind.TypeInt => "int",

                OperandKind.SourceRegister => "sr",
                OperandKind.NearPointer => "np",
                OperandKind.FarPointer => "fp",

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
                OperandKind.CX => "cx",
                OperandKind.CL => "cl",
                OperandKind.CH => "ch",

                OperandKind.RDX => "rdx",
                OperandKind.EDX => "edx",
                OperandKind.DX => "dx",
                OperandKind.DL => "dl",
                OperandKind.DH => "dh",

                OperandKind.RSP => "rsp",
                OperandKind.ESP => "esp",
                OperandKind.SP => "sp",

                OperandKind.RBP => "rbp",
                OperandKind.EBP => "ebp",
                OperandKind.BP => "bp",

                OperandKind.RSI => "rsi",
                OperandKind.ESI => "esi",
                OperandKind.SI => "si",

                OperandKind.RDI => "rdi",
                OperandKind.EDI => "edi",
                OperandKind.DI => "di",

                OperandKind.CS => "cs",
                OperandKind.DS => "ds",
                OperandKind.SS => "ss",
                OperandKind.ES => "es",

                OperandKind.CR => "cr",
                OperandKind.DR => "dr",
                OperandKind.TR => "tr",

                OperandKind.FS => "fs",
                OperandKind.GS => "gs",

                _ => string.Empty,
            };
        }

        static DataType ParseDataType(string value)
        {
            return value switch
            {
                "(byte)" => DataType.Byte,
                "(short)" => DataType.Word,
                "(int)" => DataType.Int,
                "(dword)" => DataType.DoubleWord,
                "(qword)" => DataType.QuadWord,
                "(ptr)" => DataType.Pointer,
                "(far)" => DataType.Far,
                "(far ptr)" => DataType.Far | DataType.Pointer,
                "(byte ptr)" => DataType.Byte | DataType.Pointer,
                "(short ptr)" => DataType.Word | DataType.Pointer,
                "(int ptr)" => DataType.Int | DataType.Pointer,
                "(dword ptr)" => DataType.DoubleWord | DataType.Pointer,
                "(qword ptr)" => DataType.QuadWord | DataType.Pointer,
                _ => DataType.None,
            };
        }

        static string DataTypeToString(DataType dataType)
        {
            if (dataType != DataType.None)
            {
                if (dataType == DataType.Byte)
                    return "(byte)";
                else if (dataType == DataType.Word)
                    return "(short)";
                else if (dataType == DataType.Int)
                    return "(int)";
                else if (dataType == DataType.DoubleWord)
                    return "(dword)";
                else if (dataType == DataType.QuadWord)
                    return "(qword)";
                else if (dataType == DataType.Pointer)
                    return "(ptr)";
                else if (dataType == DataType.Far)
                    return "(far)";
                else if (dataType.HasFlag(DataType.Far) && dataType.HasFlag(DataType.Pointer))
                    return "(far ptr)";
                else if (dataType.HasFlag(DataType.Byte) && dataType.HasFlag(DataType.Pointer))
                    return "(byte ptr)";
                else if (dataType.HasFlag(DataType.Word) && dataType.HasFlag(DataType.Pointer))
                    return "(short ptr)";
                else if (dataType.HasFlag(DataType.Int) && dataType.HasFlag(DataType.Pointer))
                    return "(int ptr)";
                else if (dataType.HasFlag(DataType.DoubleWord) && dataType.HasFlag(DataType.Pointer))
                    return "(dword ptr)";
                else if (dataType.HasFlag(DataType.QuadWord) && dataType.HasFlag(DataType.Pointer))
                    return "(qword ptr)";
            }
            return string.Empty;
        }

        public static implicit operator Operand(string value) => Parse(value);
        public static explicit operator string(Operand op) => op.ToString();

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(DataTypeToString(DataType));
            if (Kind == OperandKind.Value)
                s.Append(Value.ToString("D"));
            else if (Kind == OperandKind.M_Number)
                s.Append($"M{Value:D}");
            else
                s.Append(TypeToString(Kind));
            return s.ToString();
        }

        public Operand WithDataType(DataType dataType) => new Operand(Kind, dataType, Value);
    }
}
