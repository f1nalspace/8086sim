using CsvHelper;
using CsvHelper.Configuration;
using Final.CPU8086;
using Final.CPU8086.Instructions;
using Final.CPU8086.Types;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Final.ITP
{
    enum SignBit : int
    {
        None = 0,
        SignExtendedImm8,
        Non,
    }

    static class AdditionalInstructions
    {
        public static readonly InstructionDefinition[] PrefixInstructions = new InstructionDefinition[] {
            new InstructionDefinition(0xF0, new Mnemonic("LOCK"), DataWidthType.None, InstructionFlags.Prefix, DataType.None, "--------", new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), Array.Empty<OperandDefinition>()) { Description = "Lock Prefix" },
            new InstructionDefinition(0xF2, new Mnemonic("REPNE"), DataWidthType.None, InstructionFlags.Prefix, DataType.None, "-----z--", new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), Array.Empty<OperandDefinition>()) { Description = "Repeat Not Equal Prefix" },
            new InstructionDefinition(0xF3, new Mnemonic("REP"), DataWidthType.None, InstructionFlags.Prefix, DataType.None, "-----z--", new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), Array.Empty<OperandDefinition>()) { Description = "Repeat Prefix" },

            new InstructionDefinition(0x2E, new Mnemonic("CS"), DataWidthType.None, InstructionFlags.Prefix | InstructionFlags.Segment, DataType.None, FlagsDefinition.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), new[]{new OperandDefinition(OperandDefinitionKind.CS, DataType.None)}) { Description = "CS Segment Override Prefix" },
            new InstructionDefinition(0x36, new Mnemonic("SS"), DataWidthType.None, InstructionFlags.Prefix | InstructionFlags.Segment, DataType.None, FlagsDefinition.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), new[]{new OperandDefinition(OperandDefinitionKind.SS, DataType.None)}) { Description = "SS Segment Override Prefix" },
            new InstructionDefinition(0x3E, new Mnemonic("DS"), DataWidthType.None, InstructionFlags.Prefix | InstructionFlags.Segment, DataType.None, FlagsDefinition.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), new[]{new OperandDefinition(OperandDefinitionKind.DS, DataType.None)}) { Description = "DS Segment Override Prefix" },
            new InstructionDefinition(0x26, new Mnemonic("ES"), DataWidthType.None, InstructionFlags.Prefix | InstructionFlags.Segment, DataType.None, FlagsDefinition.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), new[]{new OperandDefinition(OperandDefinitionKind.ES, DataType.None)}) { Description = "ES Segment Override Prefix" },
            new InstructionDefinition(0x64, new Mnemonic("FS"), DataWidthType.None, InstructionFlags.Prefix | InstructionFlags.Segment, DataType.None, FlagsDefinition.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), new[]{new OperandDefinition(OperandDefinitionKind.FS, DataType.None)}) { Description = "FS Segment Override Prefix" },
            new InstructionDefinition(0x65, new Mnemonic("GS"), DataWidthType.None, InstructionFlags.Prefix | InstructionFlags.Segment, DataType.None, FlagsDefinition.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), new[]{new OperandDefinition(OperandDefinitionKind.GS, DataType.None)}) { Description = "GS Segment Override Prefix" },

            new InstructionDefinition(0x66, new Mnemonic("DATA8"), DataWidthType.Byte, InstructionFlags.Prefix | InstructionFlags.Override, DataType.Byte, FlagsDefinition.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), Array.Empty<OperandDefinition>()) { Description = "Data to 8-bit Override Prefix" },
            new InstructionDefinition(0x66, new Mnemonic("DATA16"), DataWidthType.Word, InstructionFlags.Prefix | InstructionFlags.Override, DataType.Word, FlagsDefinition.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), Array.Empty<OperandDefinition>()) { Description = "Data to 16-bit Override Prefix" },

            new InstructionDefinition(0x67, new Mnemonic("ADDR8"), DataWidthType.Byte, InstructionFlags.Prefix | InstructionFlags.Override, DataType.Byte, FlagsDefinition.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), Array.Empty<OperandDefinition>()) { Description = "Address to 8-bit Override Prefix" },
            new InstructionDefinition(0x67, new Mnemonic("ADDR16"), DataWidthType.Word, InstructionFlags.Prefix | InstructionFlags.Override, DataType.Word, FlagsDefinition.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<FieldDefinition>(), Array.Empty<OperandDefinition>()) { Description = "Address to 16-bit Override Prefix" },
        };
    }

    class InstructionFamily : IEquatable<InstructionFamily>
    {
        public string Name { get; }
        public string Description { get; }
        public Platform Platform { get; }

        public InstructionFamily(string name, string description, Platform platform)
        {
            Name = name;
            Description = description;
            Platform = platform;
        }

        public override int GetHashCode() => HashCode.Combine(Name, Platform);
        public bool Equals(InstructionFamily other) => Name.Equals(other.Name) && Platform.Equals(other.Platform);
        public override bool Equals(object obj) => obj is InstructionFamily other && Equals(other);

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append(Name);
            if (!string.IsNullOrWhiteSpace(Description))
            {
                s.Append(' ');
                s.Append('-');
                s.Append(' ');
                s.Append(Description);
            }
            s.Append(' ');
            s.Append('[');
            s.Append(Platform);
            s.Append(']');
            return s.ToString();
        }
    }

    sealed class ParsedInstructionRow
    {
        public InstructionDefinition Definition { get; }
        public InstructionFamily Family { get; }
        public string CleanedMnemonics { get; }
        public byte Op { get; }
        public string FieldsText { get; }
        public string NormalizedSignWidthText { get; }
        public int MinLength { get; }
        public int MaxLength { get; }
        public string NormalizedFlagsText { get; }
        public string Title { get; }
        public Platform Platform { get; }
        public string LengthText { get; }
        public string OpAndFieldsText { get; }

        public ParsedInstructionRow(InstructionDefinition definition, InstructionFamily family, string cleanedMnemonics, byte op, string fieldsText, string normalizedSignWidthText, int minLength, int maxLength, string normalizedFlagsText, string title, Platform platform, string lengthText, string opAndFieldsText)
        {
            Definition = definition;
            Family = family;
            CleanedMnemonics = cleanedMnemonics;
            Op = op;
            FieldsText = fieldsText;
            NormalizedSignWidthText = normalizedSignWidthText;
            MinLength = minLength;
            MaxLength = maxLength;
            NormalizedFlagsText = normalizedFlagsText;
            Title = title;
            Platform = platform;
            LengthText = lengthText;
            OpAndFieldsText = opAndFieldsText;
        }
    }

    sealed class ParsedInstructionReference
    {
        public List<ParsedInstructionRow> Rows { get; } = new List<ParsedInstructionRow>();
        public List<InstructionDefinition> Instructions { get; } = new List<InstructionDefinition>();
        public List<InstructionFamily> OrderedFamilies { get; } = new List<InstructionFamily>();
    }

    record InstructionTableCell(InstructionDefinition Instruction, (Color background, Color foreground) colors);

    public class Program
    {
        private static readonly Regex _rexLength = new Regex("(?<min>[0-6])(([~+])(?<max>[0-6]))?", RegexOptions.Compiled);
        private static readonly Regex _rexPlatform = new Regex("\\s+\\[(?<platform>(?:[0-9]|[bit]|[P5]){2,5})\\]\\s*$", RegexOptions.Compiled);

        private const string GenTypesSwitchName= "--types";
        private const string GenTableSwitchName = "--table";
        private const string GenCSVSwitchName= "--csv";
        private const string SaveReferenceSwitchName= "--ref";
        
        private static void PrintUsage()
        {
            string assemblyFilename = Path.GetFileName(Environment.ProcessPath);
            Console.Error.WriteLine($"Usage: {assemblyFilename} {{target-path}} [arguments]");
            Console.Error.WriteLine($"Arguments:");
            Console.Error.WriteLine($"{GenTypesSwitchName} -> Generate types and enums");
            Console.Error.WriteLine($"{GenTableSwitchName} -> Generate instruction table");
            Console.Error.WriteLine($"{GenCSVSwitchName} -> Write instruction table CSV");
            Console.Error.WriteLine($"{SaveReferenceSwitchName} -> Save original reference documents");
        }
        
        public static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Missing arguments!");
                PrintUsage();
                return 1;
            }

            string targetPath = args[0];
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                Console.Error.WriteLine("Target path is empty!");
                PrintUsage();
                return 1;
            }
            
            bool genTypes = false;
            bool genTable = false;
            bool genCSV = false;
            bool saveReference = false;
            for (int i = 1; i < args.Length; ++i)
            {
                string arg = args[i];
                if (string.Equals(GenTypesSwitchName, arg))
                    genTypes = true;
                else if (string.Equals(GenTableSwitchName, arg))
                    genTable = true;
                else if (string.Equals(GenCSVSwitchName, arg))
                    genCSV = true;
                else if (string.Equals(SaveReferenceSwitchName, arg))
                    saveReference = true;
                else
                {
                    Console.Error.WriteLine($"Unsupported argument: {arg}");
                    PrintUsage();
                    return 1;
                }
            }

            Directory.CreateDirectory(targetPath);
            
            HtmlDocument referenceDocument = LoadInstructionReferenceHtmlDocument();

            ParsedInstructionReference reference = ParseInstructionReferenceDocument(referenceDocument);

            if (genCSV)
            {
                string csvFilePath = Path.Combine(targetPath, "8086-instruction-table.csv");
                WriteParsedInstructionsToCsvFile(reference, csvFilePath);
            }

            if (genTypes)
            {
                string instructionTypeSource = GenerateInstructionTypeEnumAndNameConversionSource(reference);

                string generatedTypesFilePath = Path.Combine(targetPath, "8086-types.cs");
                
                Debug.WriteLine(instructionTypeSource);
            }

            if (genTable)
            {
                string instructionTableSource = GenerateInstructionTableConstructorSource(reference);
                
                string generatedTableFilePath = Path.Combine(targetPath, "8086-table.cs");
                
                Debug.WriteLine(instructionTableSource);
            }

            if (saveReference)
            {
                string referenceFilePath = Path.Combine(targetPath, "8086-instruction-table.html");
                WriteParsedInstructionsToHtmlFile(reference, referenceFilePath);
            }

            Console.WriteLine();
            Console.WriteLine("Done, press any key to exit");
            Console.ReadKey();

            return 0;
        }

        private static HtmlDocument LoadInstructionReferenceHtmlDocument()
        {
            Assembly assembly = typeof(Program).Assembly;
            Stream stream = assembly.GetManifestResourceStream("Final.ITP.x86asmref.htm");
            HtmlDocument document = new HtmlDocument();
            document.Load(stream);
            return document;
        }

        private static ParsedInstructionReference ParseInstructionReferenceDocument(HtmlDocument document)
        {
            ParsedInstructionReference reference = new ParsedInstructionReference();
            HashSet<InstructionFamily> seenFamilies = new HashSet<InstructionFamily>();

            HtmlNode body = document.DocumentNode.SelectSingleNode("//body");
            HtmlNode hr = body.SelectSingleNode("hr");
            HtmlNode cur = hr.SelectSingleNode("following-sibling::h4");

            while (cur != null)
            {
                if (!"h4".Equals(cur.Name, StringComparison.InvariantCultureIgnoreCase))
                    throw new FormatException("Missing h4 node!");

                string fullTitle = cur.InnerText;

                SplitInstructionTitleIntoGroupAndDescription(fullTitle, out string group, out string title);

                Platform globalPlatform = DetermineGlobalPlatformFromTitle(title);

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
                HtmlNodeCollection firstColumns = firstRow.SelectNodes("th");
                if (firstColumns.Count != 5)
                    throw new FormatException($"Expect header row to have '{5}' columns, but got '{firstColumns.Count}' for '{fullTitle}'!");

                HtmlNode div = table.SelectSingleNode("following-sibling::div");
                if (div == null)
                    throw new FormatException($"Missing div node for '{fullTitle}'!!");

                for (int rowIndex = 1; rowIndex < rows.Count; ++rowIndex)
                {
                    ParsedInstructionRow row = ParseSingleInstructionRow(rows[rowIndex], rowIndex, fullTitle, group, title, globalPlatform);

                    reference.Rows.Add(row);

                    if (row.Definition != null)
                        reference.Instructions.Add(row.Definition);

                    if (seenFamilies.Add(row.Family))
                        reference.OrderedFamilies.Add(row.Family);
                }

                cur = div.SelectSingleNode("following-sibling::h4");
            }

            return reference;
        }

        private static ParsedInstructionRow ParseSingleInstructionRow(HtmlNode row, int rowIndex, string fullTitle, string group, string title, Platform globalPlatform)
        {
            HtmlNodeCollection cols = row.SelectNodes("td");
            if (cols.Count != 5)
                throw new FormatException($"Expect content row '{rowIndex}' to have '{5}' columns, but got '{cols.Count}' for '{fullTitle}'!");

            string mnemonics = HttpUtility.HtmlDecode(cols[0].InnerText);
            string opAndFields = HttpUtility.HtmlDecode(cols[1].InnerText);
            string swText = HttpUtility.HtmlDecode(cols[2].InnerText);
            string lenText = HttpUtility.HtmlDecode(cols[3].InnerText);
            string flagsText = HttpUtility.HtmlDecode(cols[4].InnerText);

            Platform platform = ParsePlatformFromMnemonicsLine(mnemonics, globalPlatform, out string platformText);

            ParseOpcodeByteAndFieldTokens(opAndFields, out byte op, out string[] fieldTokens, out string[] allTokens);

            ParseMinimumAndMaximumInstructionLength(lenText, fullTitle, rowIndex, out int minLen, out int maxLen);
            ValidateInstructionLengthAgainstFieldTokens(allTokens, minLen, maxLen, mnemonics);

            ParseSignBitAndDataWidthType(swText, out SignBit signBit, out DataWidthType dataWidthType, out string normalizedSwText);

            FlagsDefinition usedFlags = ParseAffectedFlagsDefinition(flagsText, out string normalizedFlagsText);

            string cleanedMnemonics = RemovePlatformSuffixFromMnemonicsLine(mnemonics, platformText);
            string[] mnemonicTokens = SplitMnemonicLineIntoTokens(cleanedMnemonics);
            string opName = mnemonicTokens[0];

            FieldDefinition[] fields = ParseFieldDefinitions(fieldTokens);
            OperandDefinition[] operands = ParseOperandDefinitions(mnemonicTokens);

            InstructionFamily family = new InstructionFamily(opName, title, platform);

            InstructionDefinition definition = TryBuildInstructionDefinitionForFamily(op, family, signBit, dataWidthType, usedFlags, platform, minLen, maxLen, fields, operands);

            return new ParsedInstructionRow(definition, family, cleanedMnemonics, op, string.Join(' ', fieldTokens), normalizedSwText, minLen, maxLen, normalizedFlagsText, title, platform, lenText, opAndFields);
        }

        private static void SplitInstructionTitleIntoGroupAndDescription(string fullTitle, out string group, out string title)
        {
            int dashIndex = fullTitle.IndexOf("-");
            if (dashIndex > -1)
            {
                group = fullTitle.Substring(0, dashIndex).Trim();
                title = fullTitle.Substring(dashIndex + 1).Trim();
            }
            else
            {
                group = string.Empty;
                title = fullTitle;
            }
        }

        private static Platform DetermineGlobalPlatformFromTitle(string title)
        {
            if ("A description of the floating point instructions is not available at yet.".Equals(title, StringComparison.InvariantCultureIgnoreCase))
                return new Platform(PlatformType._8087);
            return new Platform();
        }

        private static Platform ParsePlatformFromMnemonicsLine(string mnemonics, Platform globalPlatform, out string platformText)
        {
            platformText = string.Empty;
            Match platformMatch = _rexPlatform.Match(mnemonics);
            if (platformMatch.Success)
                platformText = platformMatch.Groups["platform"].Value;

            Platform platform = Platform.Parse(platformText);
            if (platform < globalPlatform)
                platform = globalPlatform;
            return platform;
        }

        private static void ParseOpcodeByteAndFieldTokens(string opAndFields, out byte op, out string[] fieldTokens, out string[] allTokens)
        {
            allTokens = opAndFields
                .Replace("|", "")
                .Split(new[] { ' ' });

            op = 0;
            fieldTokens = Array.Empty<string>();
            if (allTokens.Length > 0)
            {
                op = byte.Parse(allTokens[0], NumberStyles.HexNumber);
                fieldTokens = allTokens.AsSpan(1).ToArray();
            }
        }

        private static void ParseMinimumAndMaximumInstructionLength(string lenText, string fullTitle, int rowIndex, out int minLen, out int maxLen)
        {
            Match lenMatch = _rexLength.Match(lenText);
            if (!lenMatch.Success)
                throw new FormatException($"Unsupported length string '{lenText}' in row '{rowIndex}' for '{fullTitle}'!");
            int.TryParse(lenMatch.Groups["min"].Value ?? string.Empty, out minLen);
            int.TryParse(lenMatch.Groups["max"].Value ?? string.Empty, out maxLen);
            if (maxLen == 0)
                maxLen = minLen;
        }

        private static void ValidateInstructionLengthAgainstFieldTokens(string[] allTokens, int minLen, int maxLen, string mnemonics)
        {
            int fieldsLen = 0;
            foreach (string single in allTokens)
            {
                if ("i0~i3".Equals(single))
                    fieldsLen += 4;
                else
                    ++fieldsLen;
            }
            if (minLen == maxLen && minLen > fieldsLen)
                throw new InvalidDataException($"The min/max length of '{minLen}' for op '{mnemonics}' does not match fields length of '{fieldsLen}'");
        }

        private static void ParseSignBitAndDataWidthType(string swText, out SignBit signBit, out DataWidthType dataWidthType, out string normalizedSwText)
        {
            normalizedSwText = swText.PadRight(2, '*');
            normalizedSwText = Regex.Replace(normalizedSwText, "\\s", "*");
            Debug.Assert(normalizedSwText.Length == 2);

            dataWidthType = DataWidthType.None;
            char widthChar = normalizedSwText[1];
            if (widthChar == 'B')
                dataWidthType |= DataWidthType.Byte;
            else if (widthChar == 'W')
                dataWidthType |= DataWidthType.Word;
            else if (widthChar == 'D')
                dataWidthType |= DataWidthType.DoubleWord;
            else if (widthChar == 'Q')
                dataWidthType |= DataWidthType.QuadWord;
            else if (widthChar == 'T')
                dataWidthType |= DataWidthType.TenBytes;
            else if (widthChar != '*')
                throw new NotImplementedException($"The w flag '{widthChar}' is not implemented");

            signBit = SignBit.None;
            char signChar = normalizedSwText[0];
            if (signChar == 'E')
                signBit = SignBit.SignExtendedImm8;
            else if (signChar == 'N')
                signBit = SignBit.Non;
            else if (signChar != '*')
                throw new NotImplementedException($"The s flag '{signChar}' is not implemented");
        }

        private static FlagsDefinition ParseAffectedFlagsDefinition(string flagsText, out string normalizedFlagsText)
        {
            normalizedFlagsText = Regex.Replace(flagsText, "-", "*");
            Debug.Assert(normalizedFlagsText.Length == 8);
            return new FlagsDefinition(normalizedFlagsText.AsSpan());
        }

        private static string RemovePlatformSuffixFromMnemonicsLine(string mnemonics, string platformText)
        {
            if (!string.IsNullOrEmpty(platformText))
                return mnemonics.Substring(0, mnemonics.Length - (platformText.Length + 2));
            return mnemonics;
        }

        private static string[] SplitMnemonicLineIntoTokens(string mnemonics)
        {
            string tabbedReplaced = Regex.Replace(mnemonics, @"[\s,\[\]]", "\t");
            string[] tokens = tabbedReplaced.Split('\t', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                throw new NotSupportedException($"Mnemonic '{mnemonics}' is invalid!");
            return tokens;
        }

        private static FieldDefinition[] ParseFieldDefinitions(string[] fieldTokens)
        {
            FieldDefinition[] fields = new FieldDefinition[fieldTokens.Length];
            for (int i = 0; i < fieldTokens.Length; i++)
                fields[i] = FieldDefinition.Parse(fieldTokens[i]);
            return fields;
        }

        private static OperandDefinition[] ParseOperandDefinitions(string[] mnemonicTokens)
        {
            List<OperandDefinition> operands = new List<OperandDefinition>(8);
            for (int i = 1; i < mnemonicTokens.Length; i++)
                operands.Add(OperandDefinition.Parse(mnemonicTokens[i]));
            return operands.ToArray();
        }

        private static InstructionDefinition TryBuildInstructionDefinitionForFamily(byte op, InstructionFamily family, SignBit signBit, DataWidthType dataWidthType, FlagsDefinition usedFlags, Platform platform, int minLen, int maxLen, FieldDefinition[] fields, OperandDefinition[] operands)
        {
            if (!Enum.TryParse<InstructionType>(family.Name, out InstructionType type))
                return null;

            InstructionFlags flags = InstructionFlags.None;
            if (signBit == SignBit.SignExtendedImm8)
                flags |= InstructionFlags.SignExtendedImm8;

            DataType dataType = DataType.None;
            foreach (OperandDefinition operand in operands)
            {
                if (operand.DataType > dataType)
                    dataType = operand.DataType;
                switch (operand.Kind)
                {
                    case OperandDefinitionKind.FarPointer:
                        flags |= InstructionFlags.Far;
                        break;

                    case OperandDefinitionKind.NearPointer:
                        flags |= InstructionFlags.Near;
                        break;

                    case OperandDefinitionKind.KeywordFar:
                        flags |= InstructionFlags.Far;
                        break;

                    default:
                        break;
                }
            }

            DataWidth dataWidth = new DataWidth(dataWidthType);
            if (dataWidth == DataWidth.None)
                dataWidth = DataWidth.DataTypeToWidth(dataType);

            return new InstructionDefinition(op, type, dataWidth, flags, dataType, usedFlags, platform, minLen, maxLen, fields, operands);
        }

        private static void WriteParsedInstructionsToCsvFile(ParsedInstructionReference reference, string csvFilePath)
        {
            using FileStream csvStream = File.Create(csvFilePath);
            using StreamWriter writer = new StreamWriter(csvStream, encoding: Encoding.UTF8, leaveOpen: true);

            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = ";",
                Encoding = Encoding.UTF8,
            };

            using CsvWriter csv = new CsvWriter(writer, config, leaveOpen: true);

            WriteCsvHeaderRow(csv);

            foreach (ParsedInstructionRow row in reference.Rows)
                WriteCsvInstructionRow(csv, row);

            writer.Flush();
            csvStream.Flush();
        }

        private static void WriteCsvHeaderRow(CsvWriter csv)
        {
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
        }

        private static void WriteCsvInstructionRow(CsvWriter csv, ParsedInstructionRow row)
        {
            csv.WriteField(row.CleanedMnemonics, true);
            csv.WriteField(row.Op);
            csv.WriteField(row.Op.ToString("X2"));
            csv.WriteField(row.Op.ToBinary());
            csv.WriteField(row.FieldsText, true);
            csv.WriteField(row.NormalizedSignWidthText, true);
            csv.WriteField(row.MinLength);
            csv.WriteField(row.MaxLength);
            csv.WriteField(row.NormalizedFlagsText, true);
            csv.WriteField(row.Family.Name, true);
            csv.WriteField(row.Title, true);
            csv.WriteField(row.Platform.Type);
            csv.WriteField(string.Empty);
            csv.WriteField(string.Empty);
            csv.WriteField(string.Empty);
            csv.WriteField(row.LengthText);
            csv.WriteField(row.OpAndFieldsText);
            csv.NextRecord();
        }

        private static List<InstructionFamily> BuildFamilyListWithPrefixInstructions(List<InstructionFamily> orderedFamilies)
        {
            List<InstructionFamily> families = new List<InstructionFamily>(orderedFamilies);

            foreach (InstructionDefinition prefixInstruction in AdditionalInstructions.PrefixInstructions)
            {
                string name = prefixInstruction.Mnemonic.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                InstructionFamily family = families.FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.InvariantCultureIgnoreCase));
                if (family != null)
                    families.Remove(family);
            }

            foreach (InstructionDefinition prefixInstruction in AdditionalInstructions.PrefixInstructions)
            {
                string name = prefixInstruction.Mnemonic.Name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                families.Add(new InstructionFamily(name, prefixInstruction.Description, prefixInstruction.Platform));
            }

            return families;
        }

        private static List<InstructionDefinition> BuildInstructionListWithPrefixInstructions(List<InstructionDefinition> instructions)
        {
            List<InstructionDefinition> result = new List<InstructionDefinition>(instructions);

            foreach (InstructionDefinition prefixInstruction in AdditionalInstructions.PrefixInstructions)
                result.RemoveAll(i => i.Op == prefixInstruction.Op);

            foreach (InstructionDefinition prefixInstruction in AdditionalInstructions.PrefixInstructions)
                result.Add(prefixInstruction);

            return result;
        }

        private static string GenerateInstructionTypeEnumAndNameConversionSource(ParsedInstructionReference reference)
        {
            List<InstructionFamily> families = BuildFamilyListWithPrefixInstructions(reference.OrderedFamilies);

            StringBuilder instructionTypesText = new StringBuilder();
            instructionTypesText.AppendLine("enum InstructionType {");
            instructionTypesText.AppendLine("\t/// <summary>");
            instructionTypesText.AppendLine($"\t/// None");
            instructionTypesText.AppendLine("\t/// </summary>");
            instructionTypesText.AppendLine("\tNone = 0,");

            StringBuilder stringToTypeMethodText = new StringBuilder();
            stringToTypeMethodText.AppendLine($"public static {nameof(InstructionType)} NameToType(string name) {{");
            stringToTypeMethodText.AppendLine($"\treturn (name ?? string.Empty) switch {{");

            StringBuilder typeToNameMethodText = new StringBuilder();
            typeToNameMethodText.AppendLine($"public static string TypeToName({nameof(InstructionType)} type) {{");
            typeToNameMethodText.AppendLine($"\treturn type switch {{");

            foreach (InstructionFamily family in families)
            {
                if (family.Platform.Type != PlatformType._8086)
                    continue;
                string iname = family.Name;

                instructionTypesText.AppendLine("\t/// <summary>");
                instructionTypesText.AppendLine($"\t/// {family.Description}");
                instructionTypesText.AppendLine("\t/// </summary>");
                instructionTypesText.AppendLine($"\t{iname},");

                stringToTypeMethodText.AppendLine($"\t\t\"{iname}\" => {nameof(InstructionType)}.{iname},");

                typeToNameMethodText.AppendLine($"\t\t{nameof(InstructionType)}.{iname} => \"{iname}\",");
            }
            instructionTypesText.AppendLine("}");

            stringToTypeMethodText.AppendLine($"\t\t_ => {nameof(InstructionType)}.{nameof(InstructionType.None)},");
            stringToTypeMethodText.AppendLine($"\t}};");
            stringToTypeMethodText.AppendLine("}");

            typeToNameMethodText.AppendLine("\t\t_ => string.Empty,");
            typeToNameMethodText.AppendLine("\t};");
            typeToNameMethodText.AppendLine("}");

            StringBuilder result = new StringBuilder();
            result.AppendLine(instructionTypesText.ToString());
            result.AppendLine();
            result.AppendLine(stringToTypeMethodText.ToString());
            result.AppendLine();
            result.AppendLine(typeToNameMethodText.ToString());
            return result.ToString();
        }

        private static string GenerateInstructionTableConstructorSource(ParsedInstructionReference reference)
        {
            List<InstructionDefinition> instructions = BuildInstructionListWithPrefixInstructions(reference.Instructions);

            InstructionTable newTable = new InstructionTable();
            InstructionDefinition[] sortedInstructions = instructions.OrderBy(i => i.Op).ToArray();
            foreach (InstructionDefinition instruction in sortedInstructions)
            {
                if (instruction.Platform.Type != PlatformType._8086)
                    continue;
                InstructionDefinitionList list = newTable.GetOrCreate(instruction.Op);
                list.Add(instruction);
            }

            string entryName = "IE";
            string mnemonicName = "MNE";
            string instructionTypeName = "IT";
            string listName = "IL";
            string dataTypeName = "DT";
            string flagsName = "IF";
            string tableName = nameof(InstructionTable);
            string varName = "_opToList";

            StringBuilder instructionsTableText = new StringBuilder();
            instructionsTableText.AppendLine($"using {listName} = {typeof(InstructionDefinitionList).FullName};");
            instructionsTableText.AppendLine($"using {entryName} = {typeof(InstructionDefinition).FullName};");
            instructionsTableText.AppendLine($"using {instructionTypeName} = {typeof(InstructionType).FullName};");
            instructionsTableText.AppendLine($"using {dataTypeName} = {typeof(DataType).FullName};");
            instructionsTableText.AppendLine($"using {flagsName} = {typeof(InstructionFlags).FullName};");
            instructionsTableText.AppendLine($"using {mnemonicName} = {typeof(Mnemonic).FullName};");
            instructionsTableText.AppendLine();
            instructionsTableText.AppendLine($"public class {tableName}");
            instructionsTableText.AppendLine("{");
            instructionsTableText.AppendLine($"\tprivate readonly IL[] {varName} = new IL[256];");
            instructionsTableText.AppendLine();
            instructionsTableText.AppendLine($"\tpublic {tableName}()");
            instructionsTableText.AppendLine("\t{");

            foreach (InstructionDefinitionList list in newTable)
            {
                if (list == null)
                    continue;

                string opBinary = list.Op.ToBinary();

                StringBuilder entriesText = new StringBuilder();
                foreach (InstructionDefinition entry in list)
                {
                    if (entriesText.Length > 0)
                        entriesText.AppendLine(",");
                    entriesText.Append(BuildInstructionDefinitionConstructorSource(entry, entryName, mnemonicName, instructionTypeName, dataTypeName, flagsName));
                }

                instructionsTableText.AppendLine($"\t\t{varName}[{list.Op:D}] = new {listName}(0b{opBinary},");
                instructionsTableText.AppendLine(entriesText.ToString());
                instructionsTableText.AppendLine($"\t\t);");
            }

            instructionsTableText.AppendLine("\t}");
            instructionsTableText.AppendLine("}");

            return instructionsTableText.ToString();
        }

        private static string BuildInstructionDefinitionConstructorSource(InstructionDefinition entry, string entryName, string mnemonicName, string instructionTypeName, string dataTypeName, string flagsName)
        {
            InstructionFlags[] allFlags = Enum.GetValues<InstructionFlags>().Where(d => d != InstructionFlags.None).ToArray();
            DataType[] allDataTypes = Enum.GetValues<DataType>().Where(d => d != DataType.None).ToArray();

            string entryOpHex = entry.Op.ToString("X2");

            StringBuilder entryText = new StringBuilder();
            entryText.Append("\t\t\t");

            entryText.Append("new ");
            entryText.Append(entryName);
            entryText.Append('(');

            entryText.Append("0x");
            entryText.Append(entryOpHex);

            entryText.Append(", ");
            entryText.Append("new ");
            entryText.Append(mnemonicName);
            entryText.Append('(');
            entryText.Append(instructionTypeName);
            entryText.Append('.');
            entryText.Append(entry.Mnemonic.Type.ToString());
            entryText.Append(',');
            entryText.Append('"');
            entryText.Append(entry.Mnemonic.Name);
            entryText.Append('"');
            entryText.Append(')');

            entryText.Append(", ");
            entryText.Append('"');
            entryText.Append(entry.DataWidth.ToString());
            entryText.Append('"');

            entryText.Append(", ");
            if (entry.Flags != InstructionFlags.None)
            {
                int flagCount = 0;
                foreach (InstructionFlags flag in allFlags)
                {
                    if (entry.Flags.HasFlag(flag))
                    {
                        if (flagCount > 0)
                            entryText.Append(" | ");
                        entryText.Append(flagsName);
                        entryText.Append('.');
                        entryText.Append(flag);
                        ++flagCount;
                    }
                }
            }
            else
            {
                entryText.Append(flagsName);
                entryText.Append('.');
                entryText.Append(nameof(InstructionFlags.None));
            }

            entryText.Append(", ");
            if (entry.DataType != DataType.None)
            {
                int dataTypeCount = 0;
                foreach (DataType dataType in allDataTypes)
                {
                    if (entry.DataType.HasFlag(dataType))
                    {
                        if (dataTypeCount > 0)
                            entryText.Append(" | ");
                        entryText.Append(dataTypeName);
                        entryText.Append('.');
                        entryText.Append(dataType.ToString());
                        ++dataTypeCount;
                    }
                }
            }
            else
            {
                entryText.Append(dataTypeName);
                entryText.Append('.');
                entryText.Append(nameof(DataType.None));
            }

            entryText.Append(", ");
            entryText.Append('"');
            entryText.Append(entry.UsedFlags.ToString());
            entryText.Append('"');

            entryText.Append(", ");
            entryText.Append('"');
            entryText.Append(entry.Platform.ToString());
            entryText.Append('"');

            entryText.Append(", ");
            entryText.Append(entry.MinLength.ToString());
            entryText.Append(", ");
            entryText.Append(entry.MaxLength.ToString());

            entryText.Append(", ");
            if (entry.Fields.Length > 0)
            {
                entryText.Append($"new {nameof(FieldDefinition)}[] {{");
                int fieldIndex = 0;
                foreach (FieldDefinition field in entry.Fields)
                {
                    if (fieldIndex > 0)
                        entryText.Append(", ");
                    entryText.Append('"');
                    entryText.Append(field.ToString());
                    entryText.Append('"');
                    ++fieldIndex;
                }
                entryText.Append("}");
            }
            else
            {
                entryText.Append(nameof(Array));
                entryText.Append('.');
                entryText.Append(nameof(Array.Empty));
                entryText.Append('<');
                entryText.Append($"{nameof(FieldDefinition)}");
                entryText.Append(">()");
            }

            entryText.Append(", ");
            if (entry.Operands.Length > 0)
            {
                entryText.Append($"new {nameof(OperandDefinition)}[] {{");
                int operandIndex = 0;
                foreach (OperandDefinition operand in entry.Operands)
                {
                    if (operandIndex > 0)
                        entryText.Append(", ");
                    entryText.Append('"');
                    entryText.Append(operand.ToString());
                    entryText.Append('"');
                    ++operandIndex;
                }
                entryText.Append("}");
            }
            else
            {
                entryText.Append(nameof(Array));
                entryText.Append('.');
                entryText.Append(nameof(Array.Empty));
                entryText.Append('<');
                entryText.Append($"{nameof(OperandDefinition)}");
                entryText.Append(">()");
            }

            entryText.Append(')');

            return entryText.ToString();
        }

        private static void WriteParsedInstructionsToHtmlFile(ParsedInstructionReference reference, string htmlFilePath)
        {
            StringBuilder htmlString = new StringBuilder();

            htmlString.AppendLine("<!DOCTYPE html>");
            htmlString.AppendLine("<html>");
            htmlString.AppendLine("<head>");
            htmlString.AppendLine("<title>Op code table</title>");
            htmlString.AppendLine("<style>");
            htmlString.AppendLine("html, body, table, th, td {");
            htmlString.AppendLine("\tfont-family: consolas; font-size: 16px;");
            htmlString.AppendLine("}");
            htmlString.AppendLine("");
            htmlString.AppendLine("table, th, td {");
            htmlString.AppendLine("\tborder: 1px solid black;");
            htmlString.AppendLine("}");
            htmlString.AppendLine("th, td {");
            htmlString.AppendLine("\twidth: 200px;");
            htmlString.AppendLine("\theight: 100px;");
            htmlString.AppendLine("\ttext-align: center;");
            htmlString.AppendLine("}");
            htmlString.AppendLine("</style>");
            htmlString.AppendLine("</head>");
            htmlString.AppendLine("<body>");
            htmlString.AppendLine("<h1>");
            htmlString.AppendLine("Intel 8086 Instruction Table");
            htmlString.AppendLine("</h1>");
            htmlString.AppendLine("<table>");

            InstructionTableCell[] cells = BuildInstructionTableCells(reference.Instructions);

            htmlString.AppendLine("<thead>");
            htmlString.AppendLine("<tr>");
            htmlString.AppendLine("<th></th>");
            for (int colIndex = 0; colIndex < 16; ++colIndex)
                htmlString.AppendLine($"<th>{colIndex:X1}</th>");
            htmlString.AppendLine("</tr>");
            htmlString.AppendLine("</thead>");

            htmlString.AppendLine("<tbody>");
            for (int rowIndex = 0; rowIndex < 16; ++rowIndex)
            {
                htmlString.AppendLine("<tr>");
                htmlString.AppendLine($"<td>{rowIndex:X1}</td>");
                for (int colIndex = 0; colIndex < 16; ++colIndex)
                {
                    InstructionTableCell cell = cells[rowIndex * 16 + colIndex];

                    Color? backgroundColor = cell?.colors.background;
                    Color? foregroundColor = cell?.colors.foreground;

                    if (backgroundColor.HasValue)
                        htmlString.AppendLine($"<td style=\"background-color: #{backgroundColor.Value.R:X2}{backgroundColor.Value.G:X2}{backgroundColor.Value.B:X2}; color: #{foregroundColor.Value.R:X2}{foregroundColor.Value.G:X2}{foregroundColor.Value.B:X2};\">");
                    else
                        htmlString.AppendLine("<td>");

                    if (cell is not null)
                    {
                        htmlString.AppendLine("<div>");
                        if (cell.Instruction.Operands.Length == 0)
                            htmlString.AppendLine(cell.Instruction.Mnemonic.Name);
                        else if (cell.Instruction.Operands.Length == 1)
                            htmlString.AppendLine($"{cell.Instruction.Mnemonic.Name} {cell.Instruction.Operands[0]}");
                        else if (cell.Instruction.Operands.Length == 2)
                            htmlString.AppendLine($"{cell.Instruction.Mnemonic.Name} {cell.Instruction.Operands[0]}, {cell.Instruction.Operands[1]}");
                        htmlString.AppendLine("</div>");

                        htmlString.AppendLine("<div>");
                        htmlString.AppendLine($"{cell.Instruction.MinLength} / {cell.Instruction.MaxLength}");
                        htmlString.AppendLine("</div>");
                        htmlString.AppendLine("<div>");
                        htmlString.AppendLine($"{cell.Instruction.DataType}");
                        htmlString.AppendLine("</div>");
                        htmlString.AppendLine("<div>");
                        htmlString.AppendLine($"{cell.Instruction.UsedFlags}");
                        htmlString.AppendLine("</div>");
                    }
                    htmlString.AppendLine("</td>");
                }
                htmlString.AppendLine("</tr>");
            }
            htmlString.AppendLine("</tbody>");

            htmlString.AppendLine("</table>");
            htmlString.AppendLine("</body>");
            htmlString.AppendLine("</html>");

            using FileStream htmlStream = File.Create(htmlFilePath);
            using (StreamWriter writer = new StreamWriter(htmlStream, encoding: Encoding.UTF8, leaveOpen: true))
                writer.Write(htmlString.ToString());
            htmlStream.Flush();
        }

        private static InstructionTableCell[] BuildInstructionTableCells(List<InstructionDefinition> instructions)
        {
            Random rnd = new Random(42);
            Span<byte> colorBytes = stackalloc byte[3];

            InstructionTableCell[] cells = new InstructionTableCell[256];
            Dictionary<InstructionType, (Color, Color)> colorsMap = new Dictionary<InstructionType, (Color, Color)>();
            foreach (InstructionDefinition instruction in instructions)
            {
                int index = instruction.Op;
                if (!colorsMap.TryGetValue(instruction.Type, out (Color backColor, Color foregroundColor) colors))
                {
                    rnd.NextBytes(colorBytes);

                    Color backColor = Color.FromArgb(colorBytes[0], colorBytes[1], colorBytes[2]);
                    float brightness = backColor.GetBrightness();
                    Color foregroundColor = brightness < 0.45f ? Color.White : Color.Black;

                    colors = (backColor, foregroundColor);
                    colorsMap.Add(instruction.Type, colors);
                }
                cells[index] = new InstructionTableCell(instruction, colors);
            }
            return cells;
        }
    }
}
