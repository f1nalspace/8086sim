using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Final.CPU8086.Types;

namespace Final.CPU8086.Instructions
{
    // https://en.wikibooks.org/wiki/X86_Assembly/X86_Architecture
    public enum OperandDefinitionKind : byte
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
        NearPointer,
        FarPointer,

        TypeDoubleWord,
        TypeWord,
        TypeShort,
        TypeInt,
        TypePointer,

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
    public class OperandDefinition
    {
        private static readonly Regex _rexMNumber = new Regex("(?<prefix>[m])(?<num>[0-9]{1,2})", RegexOptions.Compiled);
        private static readonly Regex _regFull = new Regex("(?<type>\\(.*\\))?\\s*(?<remaining>.*)", RegexOptions.Compiled);

        public OperandDefinitionKind Kind { get; }
        public DataType DataType { get; }
        public int Value { get; }
        public short Padding { get; }

        public OperandDefinition(OperandDefinitionKind kind, DataType dataType, int value = 0)
        {
            Kind = kind;
            DataType = dataType;
            Value = value;
            Padding = 0;
        }

        public OperandDefinition(int value, DataType dataType) : this(OperandDefinitionKind.Value, dataType, value) { }

        public static bool TryParse(string value, out OperandDefinition operand)
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
                operand = new OperandDefinition(OperandDefinitionKind.M_Number, dataType, number);
                return true;
            }

            if (int.TryParse(value, out int intValue))
            {
                operand = new OperandDefinition(OperandDefinitionKind.Value, dataType, intValue);
                return true;
            }

            OperandDefinitionKind kind = StringToKind(value);

            if (dataType == DataType.None)
                dataType = KindToDataType(kind);

            operand = new OperandDefinition(kind, dataType);
            return kind != OperandDefinitionKind.Unknown;
        }

        public static OperandDefinition Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException(nameof(value));
            if (!TryParse(value, out OperandDefinition operand))
                throw new NotImplementedException($"The operand '{value}' is not implemented!");
            return operand;
        }

        public static OperandDefinitionKind StringToKind(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return OperandDefinitionKind.Unknown;
            return value.ToLower() switch
            {
                "mb" => OperandDefinitionKind.MemoryByte,
                "mw" => OperandDefinitionKind.MemoryWord,
                "md" => OperandDefinitionKind.MemoryDoubleWord,
                "mq" => OperandDefinitionKind.MemoryQuadWord,

                "mwr" => OperandDefinitionKind.MemoryWordReal,
                "mdr" => OperandDefinitionKind.MemoryDoubleWordReal,
                "mqr" => OperandDefinitionKind.MemoryQuadWordReal,
                "mtr" => OperandDefinitionKind.MemoryTenByteReal,

                "rb" => OperandDefinitionKind.RegisterByte,
                "rw" => OperandDefinitionKind.RegisterWord,
                "rd" => OperandDefinitionKind.RegisterDoubleWord,
                "rmb" => OperandDefinitionKind.RegisterOrMemoryByte,
                "rmw" => OperandDefinitionKind.RegisterOrMemoryWord,
                "rmd" => OperandDefinitionKind.RegisterOrMemoryDoubleWord,
                "rmq" => OperandDefinitionKind.RegisterOrMemoryQuadWord,

                "ib" => OperandDefinitionKind.ImmediateByte,
                "iw" => OperandDefinitionKind.ImmediateWord,
                "id" => OperandDefinitionKind.ImmediateDoubleWord,

                "far" => OperandDefinitionKind.KeywordFar,
                "np" => OperandDefinitionKind.NearPointer,
                "fp" => OperandDefinitionKind.FarPointer,

                "dword" => OperandDefinitionKind.TypeDoubleWord,
                "short" => OperandDefinitionKind.TypeShort,
                "word" => OperandDefinitionKind.TypeWord,
                "int" => OperandDefinitionKind.TypeInt,
                "ptr" => OperandDefinitionKind.TypePointer,

                "sr" => OperandDefinitionKind.SourceRegister,

                "sl" => OperandDefinitionKind.ShortLabel,
                "ll" => OperandDefinitionKind.LongLabel,

                "st" => OperandDefinitionKind.ST,
                "st(i)" => OperandDefinitionKind.ST_I,
                "m" => OperandDefinitionKind.M,

                "rax" => OperandDefinitionKind.RAX,
                "eax" => OperandDefinitionKind.EAX,
                "ax" => OperandDefinitionKind.AX,
                "al" => OperandDefinitionKind.AL,
                "ah" => OperandDefinitionKind.AH,

                "rbx" => OperandDefinitionKind.RBX,
                "ebx" => OperandDefinitionKind.EBX,
                "bx" => OperandDefinitionKind.BX,
                "bl" => OperandDefinitionKind.BL,
                "bh" => OperandDefinitionKind.BH,

                "rcx" => OperandDefinitionKind.RCX,
                "ecx" => OperandDefinitionKind.ECX,
                "cx" => OperandDefinitionKind.CX,
                "cl" => OperandDefinitionKind.CL,
                "ch" => OperandDefinitionKind.CH,

                "rdx" => OperandDefinitionKind.RDX,
                "edx" => OperandDefinitionKind.EDX,
                "dx" => OperandDefinitionKind.DX,
                "dl" => OperandDefinitionKind.DL,
                "dh" => OperandDefinitionKind.DH,

                "rsp" => OperandDefinitionKind.RSP,
                "esp" => OperandDefinitionKind.ESP,
                "sp" => OperandDefinitionKind.SP,

                "rbp" => OperandDefinitionKind.RBP,
                "ebp" => OperandDefinitionKind.EBP,
                "bp" => OperandDefinitionKind.BP,

                "rsi" => OperandDefinitionKind.RSI,
                "esi" => OperandDefinitionKind.ESI,
                "si" => OperandDefinitionKind.SI,

                "rdi" => OperandDefinitionKind.RDI,
                "edi" => OperandDefinitionKind.EDI,
                "di" => OperandDefinitionKind.DI,

                "cs" => OperandDefinitionKind.CS,
                "ds" => OperandDefinitionKind.DS,
                "ss" => OperandDefinitionKind.SS,
                "es" => OperandDefinitionKind.ES,

                "cr" => OperandDefinitionKind.CR,
                "dr" => OperandDefinitionKind.DR,
                "tr" => OperandDefinitionKind.TR,

                "fs" => OperandDefinitionKind.FS,
                "gs" => OperandDefinitionKind.GS,

                _ => OperandDefinitionKind.Unknown
            };
        }

        public static DataType KindToDataType(OperandDefinitionKind kind)
        {
            return kind switch
            {
                OperandDefinitionKind.MemoryByte => DataType.Byte,
                OperandDefinitionKind.MemoryWord => DataType.Word,
                OperandDefinitionKind.MemoryDoubleWord => DataType.DoubleWord,
                OperandDefinitionKind.MemoryQuadWord => DataType.QuadWord,
                OperandDefinitionKind.MemoryWordReal => DataType.Word,
                OperandDefinitionKind.MemoryDoubleWordReal => DataType.DoubleWord,
                OperandDefinitionKind.MemoryQuadWordReal => DataType.QuadWord,
                OperandDefinitionKind.RegisterByte => DataType.Byte,
                OperandDefinitionKind.RegisterWord => DataType.Word,
                OperandDefinitionKind.RegisterDoubleWord => DataType.DoubleWord,
                OperandDefinitionKind.RegisterOrMemoryByte => DataType.Byte,
                OperandDefinitionKind.RegisterOrMemoryWord => DataType.Word,
                OperandDefinitionKind.RegisterOrMemoryDoubleWord => DataType.DoubleWord,
                OperandDefinitionKind.RegisterOrMemoryQuadWord => DataType.QuadWord,
                OperandDefinitionKind.ImmediateByte => DataType.Byte,
                OperandDefinitionKind.ImmediateWord => DataType.Word,
                OperandDefinitionKind.ImmediateDoubleWord => DataType.DoubleWord,
                OperandDefinitionKind.TypeDoubleWord => DataType.DoubleWord,
                OperandDefinitionKind.TypeWord => DataType.Word,
                OperandDefinitionKind.TypeShort => DataType.Short,
                OperandDefinitionKind.TypeInt => DataType.Int,
                OperandDefinitionKind.TypePointer => DataType.Pointer,
                OperandDefinitionKind.NearPointer => DataType.Pointer,
                OperandDefinitionKind.FarPointer => DataType.Pointer,
                OperandDefinitionKind.ShortLabel => DataType.Byte,
                OperandDefinitionKind.LongLabel => DataType.Word,
                OperandDefinitionKind.RAX => DataType.QuadWord,
                OperandDefinitionKind.EAX => DataType.DoubleWord,
                OperandDefinitionKind.AX => DataType.Word,
                OperandDefinitionKind.AL => DataType.Byte,
                OperandDefinitionKind.AH => DataType.Byte,
                OperandDefinitionKind.RBX => DataType.QuadWord,
                OperandDefinitionKind.EBX => DataType.DoubleWord,
                OperandDefinitionKind.BX => DataType.Word,
                OperandDefinitionKind.BL => DataType.Byte,
                OperandDefinitionKind.BH => DataType.Byte,
                OperandDefinitionKind.RCX => DataType.QuadWord,
                OperandDefinitionKind.ECX => DataType.DoubleWord,
                OperandDefinitionKind.CX => DataType.Word,
                OperandDefinitionKind.CL => DataType.Byte,
                OperandDefinitionKind.CH => DataType.Byte,
                OperandDefinitionKind.RDX => DataType.QuadWord,
                OperandDefinitionKind.EDX => DataType.DoubleWord,
                OperandDefinitionKind.DX => DataType.Word,
                OperandDefinitionKind.DL => DataType.Byte,
                OperandDefinitionKind.DH => DataType.Byte,
                OperandDefinitionKind.RSP => DataType.QuadWord,
                OperandDefinitionKind.ESP => DataType.DoubleWord,
                OperandDefinitionKind.SP => DataType.Word,
                OperandDefinitionKind.RBP => DataType.QuadWord,
                OperandDefinitionKind.EBP => DataType.DoubleWord,
                OperandDefinitionKind.BP => DataType.Word,
                OperandDefinitionKind.RSI => DataType.QuadWord,
                OperandDefinitionKind.ESI => DataType.DoubleWord,
                OperandDefinitionKind.SI => DataType.Word,
                OperandDefinitionKind.RDI => DataType.QuadWord,
                OperandDefinitionKind.EDI => DataType.DoubleWord,
                OperandDefinitionKind.DI => DataType.Word,
                OperandDefinitionKind.CS => DataType.Word,
                OperandDefinitionKind.DS => DataType.Word,
                OperandDefinitionKind.SS => DataType.Word,
                OperandDefinitionKind.ES => DataType.Word,
                OperandDefinitionKind.CR => DataType.Word,
                OperandDefinitionKind.DR => DataType.Word,
                OperandDefinitionKind.TR => DataType.Word,
                OperandDefinitionKind.FS => DataType.Word,
                OperandDefinitionKind.GS => DataType.Word,
                _ => DataType.None,
            };
        }

        public static string KindToString(OperandDefinitionKind type)
        {
            return type switch
            {
                OperandDefinitionKind.MemoryByte => "mb",
                OperandDefinitionKind.MemoryWord => "mw",
                OperandDefinitionKind.MemoryDoubleWord => "md",
                OperandDefinitionKind.MemoryQuadWord => "mq",

                OperandDefinitionKind.MemoryWordReal => "mwr",
                OperandDefinitionKind.MemoryDoubleWordReal => "mdr",
                OperandDefinitionKind.MemoryQuadWordReal => "mqr",
                OperandDefinitionKind.MemoryTenByteReal => "mtr",

                OperandDefinitionKind.RegisterByte => "rb",
                OperandDefinitionKind.RegisterWord => "rw",
                OperandDefinitionKind.RegisterDoubleWord => "rd",

                OperandDefinitionKind.RegisterOrMemoryByte => "rmb",
                OperandDefinitionKind.RegisterOrMemoryWord => "rmw",
                OperandDefinitionKind.RegisterOrMemoryDoubleWord => "rmd",
                OperandDefinitionKind.RegisterOrMemoryQuadWord => "rmq",

                OperandDefinitionKind.ImmediateByte => "ib",
                OperandDefinitionKind.ImmediateWord => "iw",
                OperandDefinitionKind.ImmediateDoubleWord => "id",

                OperandDefinitionKind.KeywordFar => "far",
                OperandDefinitionKind.NearPointer => "np",
                OperandDefinitionKind.FarPointer => "fp",

                OperandDefinitionKind.TypeDoubleWord => "dword",
                OperandDefinitionKind.TypeWord => "word",
                OperandDefinitionKind.TypeShort => "short",
                OperandDefinitionKind.TypeInt => "int",
                OperandDefinitionKind.TypePointer => "ptr",

                OperandDefinitionKind.SourceRegister => "sr",

                OperandDefinitionKind.ShortLabel => "sl",
                OperandDefinitionKind.LongLabel => "ll",

                OperandDefinitionKind.ST => "st",
                OperandDefinitionKind.ST_I => "st(i)",
                OperandDefinitionKind.M => "m",

                OperandDefinitionKind.RAX => "rax",
                OperandDefinitionKind.EAX => "eax",
                OperandDefinitionKind.AX => "ax",
                OperandDefinitionKind.AL => "al",
                OperandDefinitionKind.AH => "ah",

                OperandDefinitionKind.RBX => "rbx",
                OperandDefinitionKind.EBX => "ebx",
                OperandDefinitionKind.BX => "bx",
                OperandDefinitionKind.BL => "bl",
                OperandDefinitionKind.BH => "bh",

                OperandDefinitionKind.RCX => "rcx",
                OperandDefinitionKind.ECX => "ecx",
                OperandDefinitionKind.CX => "cx",
                OperandDefinitionKind.CL => "cl",
                OperandDefinitionKind.CH => "ch",

                OperandDefinitionKind.RDX => "rdx",
                OperandDefinitionKind.EDX => "edx",
                OperandDefinitionKind.DX => "dx",
                OperandDefinitionKind.DL => "dl",
                OperandDefinitionKind.DH => "dh",

                OperandDefinitionKind.RSP => "rsp",
                OperandDefinitionKind.ESP => "esp",
                OperandDefinitionKind.SP => "sp",

                OperandDefinitionKind.RBP => "rbp",
                OperandDefinitionKind.EBP => "ebp",
                OperandDefinitionKind.BP => "bp",

                OperandDefinitionKind.RSI => "rsi",
                OperandDefinitionKind.ESI => "esi",
                OperandDefinitionKind.SI => "si",

                OperandDefinitionKind.RDI => "rdi",
                OperandDefinitionKind.EDI => "edi",
                OperandDefinitionKind.DI => "di",

                OperandDefinitionKind.CS => "cs",
                OperandDefinitionKind.DS => "ds",
                OperandDefinitionKind.SS => "ss",
                OperandDefinitionKind.ES => "es",

                OperandDefinitionKind.CR => "cr",
                OperandDefinitionKind.DR => "dr",
                OperandDefinitionKind.TR => "tr",

                OperandDefinitionKind.FS => "fs",
                OperandDefinitionKind.GS => "gs",

                _ => string.Empty,
            };
        }

        static DataType ParseDataType(string value)
        {
            return value switch
            {
                "(byte)" => DataType.Byte,
                "(word)" => DataType.Word,
                "(short)" => DataType.Short,
                "(int)" => DataType.Int,
                "(dword)" => DataType.DoubleWord,
                "(qword)" => DataType.QuadWord,
                "(ptr)" => DataType.Pointer,
                "(byte ptr)" => DataType.Byte | DataType.Pointer,
                "(word ptr)" => DataType.Word | DataType.Pointer,
                "(short ptr)" => DataType.Short | DataType.Pointer,
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
                    return "(word)";
                else if (dataType == DataType.Short)
                    return "(short)";
                else if (dataType == DataType.Int)
                    return "(int)";
                else if (dataType == DataType.DoubleWord)
                    return "(dword)";
                else if (dataType == DataType.QuadWord)
                    return "(qword)";
                else if (dataType == DataType.Pointer)
                    return "(ptr)";
                else if (dataType.HasFlag(DataType.Byte) && dataType.HasFlag(DataType.Pointer))
                    return "(byte ptr)";
                else if (dataType.HasFlag(DataType.Word) && dataType.HasFlag(DataType.Pointer))
                    return "(word ptr)";
                else if (dataType.HasFlag(DataType.Short) && dataType.HasFlag(DataType.Pointer))
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

        public static implicit operator OperandDefinition(string value) => Parse(value);
        public static explicit operator string(OperandDefinition op) => op.ToString();

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            if (Kind == OperandDefinitionKind.Value)
                s.Append(Value.ToString("D"));
            else if (Kind == OperandDefinitionKind.M_Number)
                s.Append($"M{Value:D}");
            else
                s.Append(KindToString(Kind));
            return s.ToString();
        }

        public OperandDefinition WithDataType(DataType dataType) => new OperandDefinition(Kind, dataType, Value);
    }
}
