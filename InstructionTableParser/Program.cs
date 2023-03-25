//#define EXPORT_TO_CSV
#define GENERATE_CS
#define GENERATE_OPTYPES

#if EXPORT_TO_CSV
using CsvHelper;
using CsvHelper.Configuration;
#endif

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Final.ITP
{
    enum FieldType : int
    {
        None = 0,
        Constant,
        ModRegRM,
        Mod000RM,
        Mod001RM,
        Mod010RM,
        Mod011RM,
        Mod100RM,
        Mod101RM,
        Mod110RM,
        Mod111RM,
        Displacement0,
        Displacement1,
        Immediate0,
        Immediate1,
        Immediate2,
        Immediate3,
        Immediate0to3,
        Offset0,
        Offset1,
        Segment0,
        Segment1,
        RelativeLabelDisplacement0,
        RelativeLabelDisplacement1,
        ShortLabelOrShortLow,
        LongLabel,
        ShortHigh,
    }

    enum PlatformType
    {
        Unknown = 0,
        _32Bit,
        _186,
        _286,
        _386,
        _387,
        _486,
        P5,
    }

    record Platform(PlatformType Type, string Original = null)
    {
        public static Platform Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            switch (value.ToLower())
            {
                case "186":
                    return new Platform(PlatformType._186);
                case "286":
                    return new Platform(PlatformType._286);
                case "386":
                    return new Platform(PlatformType._386);
                case "387":
                    return new Platform(PlatformType._387);
                case "486":
                    return new Platform(PlatformType._486);
                case "32bit":
                    return new Platform(PlatformType._32Bit);
                case "p5":
                    return new Platform(PlatformType.P5);
                default:
                    throw new NotSupportedException($"The platform '{value}' is not supported!");
            }
        }

        public override string ToString()
        {
            if (Type != PlatformType.Unknown)
                return Type.ToString();
            else
                return Original;
        }
    }

    enum Expression
    {
        None = 0,
        Plus_I
    }

    readonly struct Field
    {
        private static readonly Regex _rexConstantExpression = new Regex("(?<hex>[0-9a-fA-F]{2})(?<exp>\\+[a-z])?", RegexOptions.Compiled);

        public FieldType Type { get; }
        public string Raw { get; }
        public byte Value { get; }
        public Expression Expression { get; }

        public Field(FieldType type, string raw, byte value, Expression expression = Expression.None)
        {
            Type = type;
            Raw = raw;
            Value = value;
            Expression = expression;
        }

        public static Field Parse(string value)
        {
            FieldType type = value switch
            {
                "mr" => FieldType.ModRegRM,
                "d0" => FieldType.Displacement0,
                "d1" => FieldType.Displacement1,
                "i0" => FieldType.Immediate0,
                "i1" => FieldType.Immediate1,
                "i2" => FieldType.Immediate2,
                "i3" => FieldType.Immediate3,
                "o0" => FieldType.Offset0,
                "o1" => FieldType.Offset1,
                "s0" => FieldType.Segment0,
                "s1" => FieldType.Segment1,
                "r0" => FieldType.RelativeLabelDisplacement0,
                "r1" => FieldType.RelativeLabelDisplacement1,
                "/0" => FieldType.Mod000RM,
                "/1" => FieldType.Mod001RM,
                "/2" => FieldType.Mod010RM,
                "/3" => FieldType.Mod011RM,
                "/4" => FieldType.Mod100RM,
                "/5" => FieldType.Mod101RM,
                "/6" => FieldType.Mod110RM,
                "/7" => FieldType.Mod111RM,
                "sl" => FieldType.ShortLabelOrShortLow,
                "ll" => FieldType.LongLabel,
                "sh" => FieldType.ShortHigh,
                "i0~i3" => FieldType.Immediate0to3,
                _ => FieldType.None,
            };
            if (type == FieldType.None)
            {
                Match m = _rexConstantExpression.Match(value);
                if (m.Success)
                {
                    byte constantValue = byte.Parse(m.Groups["hex"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    string expressionRaw = m.Groups["exp"].Value?.ToLower();
                    if (string.IsNullOrWhiteSpace(expressionRaw))
                        return new Field(FieldType.Constant, value, constantValue);
                    else
                    {
                        Expression expression = expressionRaw switch
                        {
                            "+i" => Expression.Plus_I,
                            _ => throw new NotSupportedException($"The expression '{expressionRaw}' is not supported!")
                        };
                        return new Field(FieldType.Constant, value, constantValue, expression);
                    }
                } else
                    throw new NotSupportedException($"The value '{value}' is not supported!");
            }
            return new Field(type, value, 0);
        }

        public override string ToString()
        {
            if (Type == FieldType.Constant)
                return Value.ToString("X2");
            else if (Type != FieldType.None)
                return Type.ToString();
            else
                return Raw;
        }
    }

    sealed record Family(string Name, string Description, string Platform)
    {
        public override int GetHashCode() => Name.GetHashCode();
        public bool Equals(Family other) => Name.Equals(other.Name);
    }

    // https://en.wikibooks.org/wiki/X86_Assembly/X86_Architecture
    enum OperandType
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

    record Operand(OperandType Type, int Value = 0, string Original = null)
    {
        private static readonly Regex _rexMNumber = new Regex("(?<prefix>[m])(?<num>[0-9]{1,2})", RegexOptions.Compiled);

        public override string ToString()
        {
            if (Type != OperandType.Unknown)
                return Type.ToString();
            else
                return Original;
        }

        public static Operand Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            Match m;
            if ((m = _rexMNumber.Match(value)).Success)
            {
                int number = int.Parse(m.Groups["num"].Value);
                return new Operand(OperandType.M_Number, number);
            }

            if (int.TryParse(value, out int intValue))
                return new Operand(OperandType.Value, intValue);

            return value.ToLower() switch
            {
                "mb" => new Operand(OperandType.MemoryByte),
                "mw" => new Operand(OperandType.MemoryWord),
                "md" => new Operand(OperandType.MemoryDoubleWord),
                "mq" => new Operand(OperandType.MemoryQuadWord),

                "mwr" => new Operand(OperandType.MemoryWordReal),
                "mdr" => new Operand(OperandType.MemoryDoubleWordReal),
                "mqr" => new Operand(OperandType.MemoryQuadWordReal),
                "mtr" => new Operand(OperandType.MemoryTenByteReal),

                "rb" => new Operand(OperandType.RegisterByte),
                "rw" => new Operand(OperandType.RegisterWord),
                "rd" => new Operand(OperandType.RegisterDoubleWord),
                "rmb" => new Operand(OperandType.RegisterOrMemoryByte),
                "rmw" => new Operand(OperandType.RegisterOrMemoryWord),
                "rmd" => new Operand(OperandType.RegisterOrMemoryDoubleWord),
                "rmq" => new Operand(OperandType.RegisterOrMemoryQuadWord),

                "ib" => new Operand(OperandType.ImmediateByte),
                "iw" => new Operand(OperandType.ImmediateWord),
                "id" => new Operand(OperandType.ImmediateDoubleWord),

                "ptr" => new Operand(OperandType.Pointer),
                "np" => new Operand(OperandType.NearPointer),
                "fp" => new Operand(OperandType.FarPointer),

                "dword" => new Operand(OperandType.TypeDoubleWord),
                "short" => new Operand(OperandType.TypeShort),
                "int" => new Operand(OperandType.TypeInt),

                "far" => new Operand(OperandType.PrefixFar),
                "sr" => new Operand(OperandType.SourceRegister),

                "sl" => new Operand(OperandType.ShortLabel),
                "ll" => new Operand(OperandType.LongLabel),

                "st" => new Operand(OperandType.ST),
                "st(i)" => new Operand(OperandType.ST_I),

                "m" => new Operand(OperandType.M),

                "eax" => new Operand(OperandType.EAX),
                "rax" => new Operand(OperandType.RAX),
                "ax" => new Operand(OperandType.AX),
                "al" => new Operand(OperandType.AL),
                "ah" => new Operand(OperandType.AH),

                "rbx" => new Operand(OperandType.RBX),
                "ebx" => new Operand(OperandType.EBX),
                "bx" => new Operand(OperandType.BX),
                "bl" => new Operand(OperandType.BL),
                "bh" => new Operand(OperandType.BH),

                "rcx" => new Operand(OperandType.RCX),
                "ecx" => new Operand(OperandType.ECX),
                "cx" => new Operand(OperandType.CX),
                "cl" => new Operand(OperandType.CL),
                "ch" => new Operand(OperandType.CH),

                "rdx" => new Operand(OperandType.RDX),
                "edx" => new Operand(OperandType.EDX),
                "dx" => new Operand(OperandType.DX),
                "dl" => new Operand(OperandType.DL),
                "dh" => new Operand(OperandType.DH),

                "rsp" => new Operand(OperandType.RSP),
                "esp" => new Operand(OperandType.ESP),
                "sp" => new Operand(OperandType.SP),

                "rbp" => new Operand(OperandType.RBP),
                "ebp" => new Operand(OperandType.EBP),
                "bp" => new Operand(OperandType.BP),

                "rsi" => new Operand(OperandType.RSI),
                "esi" => new Operand(OperandType.ESI),
                "si" => new Operand(OperandType.SI),

                "rdi" => new Operand(OperandType.RDI),
                "edi" => new Operand(OperandType.EDI),
                "di" => new Operand(OperandType.DI),

                "cs" => new Operand(OperandType.CS),
                "ds" => new Operand(OperandType.DS),
                "ss" => new Operand(OperandType.SS),
                "es" => new Operand(OperandType.ES),

                "cr" => new Operand(OperandType.CR),
                "dr" => new Operand(OperandType.DR),
                "tr" => new Operand(OperandType.TR),

                "fs" => new Operand(OperandType.FS),
                "gs" => new Operand(OperandType.GS),

                _ => throw new NotImplementedException($"The operand '{value}' is not implemented!")
            };
        }
    }

    record Instruction(byte Op, Family Family, int MinLength, int MaxLength, Platform Platform, Operand[] Operands, Field[] Fields)
    {
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("0x");
            s.Append(Op.ToString("X2"));
            s.Append('|');
            s.Append(Family.Name);
            foreach (Operand operand in Operands)
            {
                s.Append(' ');
                s.Append(operand);
            }
            s.Append('|');
            s.Append(MinLength);
            s.Append('|');
            s.Append(MaxLength);
            if (Fields.Length > 0)
            {
                s.Append("|");
                for (int i = 0; i < Fields.Length; i++)
                {
                    if (i > 0)
                        s.Append(", ");
                    s.Append(Fields[i].ToString());
                }
            }
            if (Platform != null)
            {
                s.Append(' ');
                s.Append('[');
                s.Append(Platform);
                s.Append(']');
            }
            return s.ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        private static readonly Regex _rexTitle = new Regex("\\(([0-9]+[+]?.*)\\).*$", RegexOptions.Compiled);
        private static readonly Regex _rexLength = new Regex("(?<min>[0-6])(([~+])(?<max>[0-6]))?", RegexOptions.Compiled);
        private static readonly Regex _rexPlatform = new Regex("\\s+\\[(?<platform>(?:[0-9]|[bit]|[P5]){2,5})\\]\\s*$", RegexOptions.Compiled);

        public static void Main(string[] args)
        {
#if EXPORT_TO_CSV
            string outFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "8086-instruction-table.csv");

            using FileStream csvStream = File.Create(outFilePath);

            using StreamWriter writer = new StreamWriter(csvStream, encoding: Encoding.UTF8, leaveOpen: true);

            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = ";",
                Encoding = Encoding.UTF8,
            };

            using CsvWriter csv = new CsvWriter(writer, config, leaveOpen: true);
            csv.WriteConvertedField("mnemonics", typeof(string));
            csv.WriteConvertedField("op byte", typeof(byte));
            csv.WriteConvertedField("op hex", typeof(string));
            csv.WriteConvertedField("op bits", typeof(string));
            csv.WriteConvertedField("v1 v2 v3 v4 v5", typeof(string));
            csv.WriteConvertedField("sw", typeof(string));
            csv.WriteConvertedField("minlen", typeof(int));
            csv.WriteConvertedField("maxlen", typeof(int));
            csv.WriteConvertedField("flags", typeof(string));
            csv.WriteConvertedField("family", typeof(string));
            csv.WriteConvertedField("title", typeof(string));
            csv.WriteConvertedField("platform", typeof(string));
            csv.WriteConvertedField("", typeof(string));
            csv.WriteConvertedField("", typeof(string));
            csv.WriteConvertedField("", typeof(string));
            csv.WriteConvertedField("len", typeof(string));
            csv.WriteConvertedField("op", typeof(string));
            csv.NextRecord();
#endif

#if GENERATE_CS
            List<Instruction> instructions = new List<Instruction>();

            Dictionary<Family, List<string>> familyOpTypeListMap = new Dictionary<Family, List<string>>();
            List<Family> orderedFamilies = new List<Family>();
#endif

            Assembly asm = typeof(Program).Assembly;
            Stream stream = asm.GetManifestResourceStream("Final.ITP.x86asmref.htm");

            HtmlDocument doc = new HtmlDocument();
            doc.Load(stream);

            HtmlNode body = doc.DocumentNode.SelectSingleNode("//body");

            HtmlNode hr = body.SelectSingleNode("hr");

            HtmlNode cur = hr.SelectSingleNode("following-sibling::h4");

            while (cur != null)
            {
                if (!"h4".Equals(cur.Name, System.StringComparison.InvariantCultureIgnoreCase))
                    throw new FormatException("Missing h4 node!");

                HtmlNode header = cur;

                string fullTitle = header.InnerText;

                string title;
                string familyRaw;
                if (fullTitle.IndexOf("-") > -1)
                {
                    familyRaw = fullTitle.Substring(0, fullTitle.IndexOf("-")).Trim();
                    title = fullTitle.Substring(fullTitle.IndexOf("-") + 1).Trim();
                }
                else
                {
                    familyRaw = string.Empty;
                    title = fullTitle;
                }

                HtmlNode table = cur.SelectSingleNode("following-sibling::table");
                if (table == null)
                    throw new FormatException($"Missing table node for '{fullTitle}'!");

                HtmlNode tbody = table.SelectSingleNode("tbody");
                if (tbody == null)
                    throw new FormatException($"Missing tbody node for '{fullTitle}'!!");

                HtmlNodeCollection rows = tbody.SelectNodes("tr");
                if (rows.Count < 2)
                    throw new FormatException($"Empty table for '{fullTitle}'!");

                HtmlNode firstRow = rows[0];
                var firstColumns = firstRow.SelectNodes("th");
                if (firstColumns.Count != 5)
                    throw new FormatException($"Expect header row to have '{5}' columns, but got '{firstColumns.Count}' for '{fullTitle}'!");

                HtmlNode div = table.SelectSingleNode("following-sibling::div");
                if (div == null)
                    throw new FormatException($"Missing div node for '{fullTitle}'!!");

                for (int rowIndex = 1; rowIndex < rows.Count; ++rowIndex)
                {
                    HtmlNode row = rows[rowIndex];
                    HtmlNodeCollection cols = row.SelectNodes("td");
                    if (cols.Count != 5)
                        throw new FormatException($"Expect content row '{rowIndex}' to have '{5}' columns, but got '{cols.Count}' for '{fullTitle}'!");

                    string mnemonics = HttpUtility.HtmlDecode(cols[0].InnerText);
                    string opAndFields = HttpUtility.HtmlDecode(cols[1].InnerText);
                    string swText = HttpUtility.HtmlDecode(cols[2].InnerText);
                    string lenText = HttpUtility.HtmlDecode(cols[3].InnerText);
                    string flagsText = HttpUtility.HtmlDecode(cols[4].InnerText);

                    string platformRaw = string.Empty;
                    var platformMatch = _rexPlatform.Match(mnemonics);
                    if (platformMatch.Success)
                        platformRaw = platformMatch.Groups["platform"].Value;

                    Match lenMatch = _rexLength.Match(lenText);
                    if (!lenMatch.Success)
                        throw new FormatException($"Unsupported length string '{lenText}' in row '{rowIndex}' for '{fullTitle}'!");

                    int.TryParse(lenMatch.Groups["min"].Value ?? string.Empty, out int minLen);
                    int.TryParse(lenMatch.Groups["max"].Value ?? string.Empty, out int maxLen);

                    if (maxLen == 0)
                        maxLen = minLen;

                    swText = swText.PadLeft(2, ' ');

                    swText = Regex.Replace(swText, "\\s", "*");
                    Debug.Assert(swText.Length == 2);

                    flagsText = Regex.Replace(flagsText, "-", "*");

                    opAndFields = opAndFields.Replace("|", "");

                    string[] opSplit = opAndFields.Split(new[] { ' ' });
                    byte op = 0;
                    string[] fieldsRaw = opSplit[1..];
                    if (opSplit.Length > 0)
                        op = byte.Parse(opSplit[0], NumberStyles.HexNumber);

#if GENERATE_CS
                    string opName;

                    string originalMemonics = mnemonics;
                    if (!string.IsNullOrEmpty(platformRaw))
                        mnemonics = originalMemonics.Substring(0, originalMemonics.Length - (platformRaw.Length + 2));

                    string tabbedReplaced = Regex.Replace(mnemonics, "[\\s,\\[\\]]", "\t");

                    string[] splittedMnemonics = tabbedReplaced.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    if (splittedMnemonics.Length == 0)
                        throw new NotSupportedException($"Mnemonic '{mnemonics}' is invalid!");

                    opName = splittedMnemonics[0];

                    Field[] fields = new Field[fieldsRaw.Length];
                    for (int i = 0; i < fieldsRaw.Length; i++)
                        fields[i] = Field.Parse(fieldsRaw[i]);

                    string family;
                    if (!string.IsNullOrWhiteSpace(familyRaw))
                        family = familyRaw;
                    else
                        family = opName;

                    if (string.IsNullOrWhiteSpace(family))
                        throw new NotSupportedException($"Empty family for name '{opName}'");

                    string[] splittedFamily = family.Split('/', StringSplitOptions.RemoveEmptyEntries);

                    string firstFamilyRaw = splittedFamily[0];

                    Family firstFamily = new Family(firstFamilyRaw, title, platformRaw);

                    if (!familyOpTypeListMap.TryGetValue(firstFamily, out List<string> opNames))
                    {
                        opNames = new List<string>();
                        familyOpTypeListMap.Add(firstFamily, opNames);
                        orderedFamilies.Add(firstFamily);
                    }

                    opNames.Add(opName);

                    var operands = new Operand[splittedMnemonics.Length - 1];
                    for (int i = 1; i < splittedMnemonics.Length; i++)
                    {
                        operands[i - 1] = Operand.Parse(splittedMnemonics[i]);
                    }

                    Platform platform = Platform.Parse(platformRaw);

                    Instruction instruction = new Instruction(op, firstFamily, minLen, maxLen, platform, operands, fields);
                    instructions.Add(instruction);
#endif

#if EXPORT_TO_CSV
                    csv.WriteField(mnemonics, true);
                    csv.WriteField(op);
                    csv.WriteField(op.ToString("X2"));
                    csv.WriteField(op.ToBinary());
                    csv.WriteField(string.Join(' ', fieldsRaw), true);
                    csv.WriteField(swText, true);
                    csv.WriteField(minLen);
                    csv.WriteField(maxLen);
                    csv.WriteField(flagsText, true);
                    csv.WriteField(family, true);
                    csv.WriteField(title, true);
                    csv.WriteField(platform, true);
                    csv.WriteField(string.Empty);
                    csv.WriteField(string.Empty);
                    csv.WriteField(string.Empty);
                    csv.WriteField(lenText);
                    csv.WriteField(opAndFields);
                    csv.NextRecord();
#endif
                }

                HtmlNode next = div.SelectSingleNode("following-sibling::h4");

                cur = next;
            }

#if EXPORT_TO_CSV
            writer.Flush();
            csvStream.Flush();
#endif

#if GENERATE_CS

            Instruction[] sortedInstructions = instructions.OrderBy(i => i.Op).ToArray();
            foreach (Instruction instruction in sortedInstructions)
            {
                Debug.WriteLine($"{instruction}");
            }

#if GENERATE_OPTYPES
            Debug.WriteLine("enum InstructionType {");
            Debug.WriteLine("\t/// <summary>");
            Debug.WriteLine($"\t/// None");
            Debug.WriteLine("\t/// </summary>");
            Debug.WriteLine("\tNone = 0,");
            foreach (Family family in orderedFamilies)
            {
                if (family.Platform.Length != 0)
                    continue;
                Debug.WriteLine("\t/// <summary>");
                Debug.WriteLine($"\t/// {family.Description}");
                Debug.WriteLine("\t/// </summary>");
                Debug.WriteLine($"\t{family.Name},");
            }
            Debug.WriteLine("}");
#endif

#endif

            Console.WriteLine();
            Console.WriteLine("Done, press any key to exit");
            Console.ReadKey();
        }
    }
}