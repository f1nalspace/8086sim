//#define EXPORT_TO_CSV
#define GENERATE_CS

//#define GENERATE_INSTRUCTION_CLASSES

#if EXPORT_TO_CSV
using CsvHelper;
using CsvHelper.Configuration;
#endif

using Final.CPU8086;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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
    enum SignBit : int
    {
        None = 0,
        SignExtendedImm8,
        Non,
    }

    static class AdditionalInstructions
    {
        public static readonly InstructionEntry[] PrefixInstructions = new InstructionEntry[] {
            // Lock
            new InstructionEntry(0xF0, new Mnemonic("LOCK"), DataWidthType.None, DataFlags.Prefix, InstructionFlags.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<Field>(), Array.Empty<Operand>()),

            // String manipulation
            new InstructionEntry(0xF3, new Mnemonic("REP"), DataWidthType.None, DataFlags.Prefix, InstructionFlags.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<Field>(), Array.Empty<Operand>()),
            new InstructionEntry(0xF2, new Mnemonic("REPNE"), DataWidthType.None, DataFlags.Prefix, InstructionFlags.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<Field>(), Array.Empty<Operand>()),

            // Segment override
            new InstructionEntry(0x2E, new Mnemonic("CS"), DataWidthType.None, DataFlags.Prefix, InstructionFlags.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<Field>(), new[]{new Operand(OperandKind.CS, DataType.None)}),
            new InstructionEntry(0x36, new Mnemonic("SS"), DataWidthType.None, DataFlags.Prefix, InstructionFlags.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<Field>(), new[]{new Operand(OperandKind.SS, DataType.None)}),
            new InstructionEntry(0x3E, new Mnemonic("DS"), DataWidthType.None, DataFlags.Prefix, InstructionFlags.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<Field>(), new[]{new Operand(OperandKind.DS, DataType.None)}),
            new InstructionEntry(0x26, new Mnemonic("ES"), DataWidthType.None, DataFlags.Prefix, InstructionFlags.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<Field>(), new[]{new Operand(OperandKind.ES, DataType.None)}),
            new InstructionEntry(0x64, new Mnemonic("FS"), DataWidthType.None, DataFlags.Prefix, InstructionFlags.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<Field>(), new[]{new Operand(OperandKind.FS, DataType.None)}),
            new InstructionEntry(0x65, new Mnemonic("GS"), DataWidthType.None, DataFlags.Prefix, InstructionFlags.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<Field>(), new[]{new Operand(OperandKind.GS, DataType.None)}),

            // Operand override (Changes size of data expected by default mode of the instruction e.g. 8-bit to 16-bit and vice versa.)
            new InstructionEntry(0x66, new Mnemonic(), DataWidthType.None, DataFlags.Prefix | DataFlags.Override, InstructionFlags.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<Field>(), Array.Empty<Operand>()),

            // Address override (Changes size of address expected by the instruction. 16-bit address could switch to 8-bit and vice versa.)
            new InstructionEntry(0x67, new Mnemonic(), DataWidthType.None, DataFlags.Prefix | DataFlags.Override, InstructionFlags.Empty, new Platform(PlatformType._8086), 1, 1, Array.Empty<Field>(), Array.Empty<Operand>()),
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
            List<InstructionEntry> allInstructions = new List<InstructionEntry>();

            Dictionary<InstructionFamily, List<string>> familyOpTypeListMap = new Dictionary<InstructionFamily, List<string>>();
            List<InstructionFamily> orderedFamilies = new List<InstructionFamily>();
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

                // Contains the group/family name with an additional description or just the description
                // E.g.
                // AAA - Ascii Adjust for Addition
                // ARPL - Adjusted Requested Privilege Level of Selector (286+ protected mode)
                // A description of the floating point instructions is not available at yet.
                string fullTitle = header.InnerText;

                string title;
                string group;
                if (fullTitle.IndexOf("-") > -1)
                {
                    group = fullTitle.Substring(0, fullTitle.IndexOf("-")).Trim();
                    title = fullTitle.Substring(fullTitle.IndexOf("-") + 1).Trim();
                }
                else
                {
                    group = string.Empty;
                    title = fullTitle;
                }

                // Table additions
                Platform globalPlatform = new Platform();
                if ("A description of the floating point instructions is not available at yet.".Equals(title, StringComparison.InvariantCultureIgnoreCase))
                {
                    globalPlatform = new Platform(PlatformType._8087);
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

                    // Parse platform from the mnemonic line using a captured regex
                    // There are not that much platforms to cover, so we could simple check for:
                    // [186] or [286] or [386] or [486] or [P5] or [32bit]
                    string platformText = string.Empty;
                    Match platformMatch = _rexPlatform.Match(mnemonics);
                    if (platformMatch.Success)
                        platformText = platformMatch.Groups["platform"].Value;

                    Platform platform = Platform.Parse(platformText);
                    if (platform < globalPlatform)
                        platform = globalPlatform;

                    // Split fields that defines each byte in the instruction stream
                    // 
                    // Each field is separated by a space, so if we split by space, we get all the fields.
                    // The very first field contains the full op-code byte always, so we parse that right away
                    //
                    // Issues:
                    // Sometimes there is a | character, so we need to remove that
                    string[] opSplit = opAndFields
                        .Replace("|", "")
                        .Split(new[] { ' ' });

                    // Parse op and get a span for the fields separately
                    byte op = 0;
                    Span<string> fieldsSplitted = Span<string>.Empty;
                    if (opSplit.Length > 0)
                    {
                        op = byte.Parse(opSplit[0], NumberStyles.HexNumber);
                        fieldsSplitted = opSplit.AsSpan(1);
                    }

                    // Parse min/max instruction length
                    // We either have a fixed length or minimum and maximum length
                    // 2
                    // 3~4
                    // 1+1
                    Match lenMatch = _rexLength.Match(lenText);
                    if (!lenMatch.Success)
                        throw new FormatException($"Unsupported length string '{lenText}' in row '{rowIndex}' for '{fullTitle}'!");
                    int.TryParse(lenMatch.Groups["min"].Value ?? string.Empty, out int minLen);
                    int.TryParse(lenMatch.Groups["max"].Value ?? string.Empty, out int maxLen);
                    if (maxLen == 0)
                        maxLen = minLen;

                    // Check for entries with an incorrect length
                    int fieldsLen = 0;
                    foreach (string single in opSplit)
                    {
                        if ("i0~i3".Equals(single))
                            fieldsLen += 4;
                        else
                            ++fieldsLen;
                    }
                    if (minLen == maxLen)
                    {
                        if (minLen > fieldsLen)
                            throw new InvalidDataException($"The min/max length of '{minLen}' for op '{mnemonics}' does not match fields length of '{fieldsLen}'");
                        //minLen = maxLen = fieldsLen;
                    }

                    // Parse Signed or Word flag (SW)
                    swText = swText.PadRight(2, '*');
                    swText = Regex.Replace(swText, "\\s", "*");
                    Debug.Assert(swText.Length == 2);

                    DataWidthType dataWidthType = DataWidthType.None;
                    if (swText[1] == 'B')
                        dataWidthType |= DataWidthType.Byte;
                    else if (swText[1] == 'W')
                        dataWidthType |= DataWidthType.Word;
                    else if (swText[1] == 'D')
                        dataWidthType |= DataWidthType.DoubleWord;
                    else if (swText[1] == 'Q')
                        dataWidthType |= DataWidthType.QuadWord;
                    else if (swText[1] == 'T')
                        dataWidthType |= DataWidthType.TenBytes; // @TODO(final): Is TenBytes correct?
                    else if (swText[1] != '*')
                        throw new NotImplementedException($"The w flag '{swText[1]}' is not implemented");

                    SignBit signBit = SignBit.None;
                    if (swText[0] == 'E')
                        signBit = SignBit.SignExtendedImm8;
                    else if (swText[0] == 'N')
                        signBit = SignBit.Non;
                    else if (swText[0] != '*')
                        throw new NotImplementedException($"The s flag '{swText[0]}' is not implemented");

                    // Parse flags
                    flagsText = Regex.Replace(flagsText, "-", "*");
                    Debug.Assert(flagsText.Length == 8);
                    InstructionFlags flags = new InstructionFlags(flagsText.AsSpan());



#if GENERATE_CS
                    string originalMemonics = mnemonics;

                    // If we have parsed a platform string, we remove it from the mnemonic line because it ends with that
                    if (!string.IsNullOrEmpty(platformText))
                        mnemonics = originalMemonics.Substring(0, originalMemonics.Length - (platformText.Length + 2));

                    // Now we split the full mnemonic line by spaces, but fist we replace all whitespaces and [ ] characters with tab charcters, to make our life more easy
                    // Also we ensure that empty entries removed, are fully removed so we end up with the just the mnemonic and the operands of it
                    string tabbedReplaced = Regex.Replace(mnemonics, @"[\s,\[\]]", "\t");
                    string[] splittedMnemonics = tabbedReplaced.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    if (splittedMnemonics.Length == 0)
                        throw new NotSupportedException($"Mnemonic '{mnemonics}' is invalid!");

                    string opName = splittedMnemonics[0];

                    // Parse fields without "Op"
                    Field[] fields = new Field[fieldsSplitted.Length];
                    for (int i = 0; i < fieldsSplitted.Length; i++)
                        fields[i] = Field.Parse(fieldsSplitted[i]);

                    // Get family, so we can group the instructions into a family of instructions
                    // For example: MOV is a family, but contains dozens of instructions with varieties
                    string familyText;
                    if (!string.IsNullOrWhiteSpace(group))
                        familyText = group;
                    else
                        familyText = opName; // @NOTE(final): The family string could be just the description, so we assume the operand is the family itself
                    if (string.IsNullOrWhiteSpace(familyText))
                        throw new NotSupportedException($"Empty family for name '{opName}'");

                    // The family can contain multiple names, but we are only interested in the very first one
                    string[] splittedFamily = familyText.Split('/', StringSplitOptions.RemoveEmptyEntries);

                    InstructionFamily family = new InstructionFamily(opName, title, platform);

                    if (!familyOpTypeListMap.TryGetValue(family, out List<string> opNames))
                    {
                        opNames = new List<string>();
                        familyOpTypeListMap.Add(family, opNames);
                        orderedFamilies.Add(family);
                    }

                    opNames.Add(opName);

#if !GENERATE_INSTRUCTION_CLASSES
                    // Parse mnemonic operands
                    List<Operand> operands = new List<Operand>(8);
                    for (int i = 1; i < splittedMnemonics.Length; i++)
                        operands.Add(Operand.Parse(splittedMnemonics[i]));

                    // Convert family into instruction type and create the instruction entry to the table
                    if (Enum.TryParse<InstructionType>(family.Name, out InstructionType type))
                    {
                        DataFlags dataFlags = DataFlags.None;
                        if (signBit == SignBit.SignExtendedImm8)
                            dataFlags |= DataFlags.SignExtendedImm8;

                        // We dont want to create any Operand´s for Keywords or Type-Casts, so we convert them into data-types
                        DataType opDataType = DataType.None;
                        List<Operand> leftOperands = new List<Operand>(2);
                        foreach (Operand operand in operands)
                        {
                            switch (operand.Kind)
                            {
                                case OperandKind.KeywordFar:
                                    opDataType |= DataType.Far;
                                    break;
                                case OperandKind.KeywordPointer:
                                    opDataType |= DataType.Pointer;
                                    break;
                                case OperandKind.TypeDoubleWord:
                                    opDataType |= DataType.DoubleWord;
                                    break;
                                case OperandKind.TypeShort:
                                    opDataType |= DataType.Word;
                                    break;
                                case OperandKind.TypeInt:
                                    opDataType |= DataType.Int;
                                    break;
                                default:
                                    leftOperands.Add(operand.WithDataType(opDataType));
                                    opDataType = DataType.None;
                                    break;
                            }
                        }

                        InstructionEntry instruction = new InstructionEntry(op, type, dataWidthType, dataFlags, flags, platform, minLen, maxLen, fields, leftOperands.ToArray());
                        allInstructions.Add(instruction);
                    }
                    //else
                    //    Debug.WriteLine($"WARNING: Family '{family}' has no enum value for '{nameof(InstructionType)}'");
#endif // !GENERATE_INSTRUCTION_CLASSES

#endif // GENERATE_CS

#if EXPORT_TO_CSV
                    csv.WriteField(mnemonics, true);
                    csv.WriteField(op);
                    csv.WriteField(op.ToString("X2"));
                    csv.WriteField(op.ToBinary());
                    csv.WriteField(string.Join(' ', fieldsSplitted.ToArray()), true);
                    csv.WriteField(swText, true);
                    csv.WriteField(minLen);
                    csv.WriteField(maxLen);
                    csv.WriteField(flagsText, true);
                    csv.WriteField(family.Name, true);
                    csv.WriteField(title, true);
                    csv.WriteField(platform.Type);
                    csv.WriteField(string.Empty);
                    csv.WriteField(string.Empty);
                    csv.WriteField(string.Empty);
                    csv.WriteField(lenText);
                    csv.WriteField(opAndFields);
                    csv.NextRecord();
#endif // EXPORT_TO_CSV
                }

                HtmlNode next = div.SelectSingleNode("following-sibling::h4");

                cur = next;
            }

#if EXPORT_TO_CSV
            writer.Flush();
            csvStream.Flush();
#endif // EXPORT_TO_CSV

#if GENERATE_CS

#if GENERATE_INSTRUCTION_CLASSES
            // Generate instruction types enum
            // Generate instruction names ToString() and TryParse() method
            StringBuilder instructionTypesText = new StringBuilder();
            instructionTypesText.AppendLine("enum InstructionType {");
            instructionTypesText.AppendLine("\t/// <summary>");
            instructionTypesText.AppendLine($"\t/// None");
            instructionTypesText.AppendLine("\t/// </summary>");
            instructionTypesText.AppendLine("\tNone = 0,");

            StringBuilder parseNamesMethodText = new StringBuilder();
            parseNamesMethodText.AppendLine($"public static bool TryParse(string name, out {nameof(Mnemonic)} result) {{");
            parseNamesMethodText.AppendLine($"\t{nameof(InstructionType)} type = (name ?? string.Empty) switch {{");

            StringBuilder nameToStringMethodText = new StringBuilder();
            nameToStringMethodText.AppendLine($"public override string {nameof(object.ToString)}()");
            nameToStringMethodText.AppendLine("{");
            nameToStringMethodText.AppendLine($"\treturn {nameof(Mnemonic.Type)} switch {{");

            foreach (InstructionFamily family in orderedFamilies)
            {
                if (family.Platform.Type != PlatformType._8086)
                    continue;
                string iname = family.Name;

                instructionTypesText.AppendLine("\t/// <summary>");
                instructionTypesText.AppendLine($"\t/// {family.Description}");
                instructionTypesText.AppendLine("\t/// </summary>");
                instructionTypesText.AppendLine($"\t{iname},");

                parseNamesMethodText.AppendLine($"\t\t\"{iname}\" => {nameof(InstructionType)}.{iname},");

                nameToStringMethodText.AppendLine($"\t\t{nameof(InstructionType)}.{iname} => \"{iname}\",");
            }
            instructionTypesText.AppendLine("}");

            parseNamesMethodText.AppendLine($"\t\t_ => {nameof(InstructionType)}.{nameof(InstructionType.None)},");
            parseNamesMethodText.AppendLine($"\t}};");
            parseNamesMethodText.AppendLine($"\tresult = new {nameof(Mnemonic)}(type);");
            parseNamesMethodText.AppendLine($"\treturn result.{nameof(Mnemonic.Type)} != {nameof(InstructionType)}.{nameof(InstructionType.None)};");
            parseNamesMethodText.AppendLine("}");

            nameToStringMethodText.AppendLine("\t\t_ => string.Empty,");
            nameToStringMethodText.AppendLine("\t};");
            nameToStringMethodText.AppendLine("}");

            Debug.WriteLine(instructionTypesText.ToString());
            Debug.WriteLine(string.Empty);
            Debug.WriteLine(parseNamesMethodText.ToString());
            Debug.WriteLine(string.Empty);
            Debug.WriteLine(nameToStringMethodText.ToString());
            Debug.WriteLine(string.Empty);
#endif

            // Fill instruction table, but skip all non 8086 platforms
            InstructionTable newTable = new InstructionTable();
            InstructionEntry[] sortedInstructions = allInstructions.OrderBy(i => i.Op).ToArray();
            foreach (InstructionEntry instruction in sortedInstructions)
            {
                if (instruction.Platform.Type != PlatformType._8086)
                    continue;
                byte op = instruction.Op;
                InstructionList list = newTable.GetOrCreate(op);
                list.Add(instruction);
            }

            // Generate instructions table class
            string entryName = "IE";
            string listName = "IL";
            string dataWidthName = "DW";
            string dataFlagsName = "DF";
            string tableName = nameof(InstructionTable);
            string varName = "_opToList";

            StringBuilder instructionsTableText = new StringBuilder();
            instructionsTableText.AppendLine($"using {listName} = {typeof(InstructionList).FullName};");
            instructionsTableText.AppendLine($"using {entryName} = {typeof(InstructionEntry).FullName};");
            instructionsTableText.AppendLine($"using {dataWidthName} = {typeof(DataWidth).FullName};");
            instructionsTableText.AppendLine($"using {dataFlagsName} = {typeof(DataFlags).FullName};");
            instructionsTableText.AppendLine();
            instructionsTableText.AppendLine($"public class {tableName}");
            instructionsTableText.AppendLine("{");
            instructionsTableText.AppendLine($"\tprivate readonly IL[] {varName} = new IL[256];");
            instructionsTableText.AppendLine();
            instructionsTableText.AppendLine($"\tpublic {tableName}()");
            instructionsTableText.AppendLine("\t{");

            foreach (InstructionList list in newTable)
            {
                if (list != null)
                {
                    string opBinary = list.Op.ToBinary();

                    string listOpHex = list.Op.ToString("X2");

                    StringBuilder entriesText = new StringBuilder();
                    foreach (InstructionEntry entry in list)
                    {
                        string entryOpHex = entry.Op.ToString("X2");

                        if (entriesText.Length > 0)
                            entriesText.AppendLine(",");

                        StringBuilder entryText = new StringBuilder();
                        entryText.Append("\t\t\t");

                        entryText.Append("new ");
                        entryText.Append(entryName);
                        entryText.Append('(');

                        // Op-Code
                        entryText.Append("0x");
                        entryText.Append(entryOpHex);

                        // Name
                        entryText.Append(", ");
                        entryText.Append('"');
                        entryText.Append(entry.Mnemonic.ToString());
                        entryText.Append('"');

                        // DataWidth
                        entryText.Append(", ");
                        entryText.Append('"');
                        entryText.Append(entry.DataWidth.ToString());
                        entryText.Append('"');

                        // DataFlags
                        entryText.Append(", ");
                        if (entry.DataFlags != DataFlags.None)
                        {
                            if (entry.DataFlags.HasFlag(DataFlags.SignExtendedImm8))
                            {
                                entryText.Append(dataFlagsName);
                                entryText.Append('.');
                                entryText.Append(nameof(DataFlags.SignExtendedImm8));
                            }
                        }
                        else
                        {
                            entryText.Append(dataFlagsName);
                            entryText.Append('.');
                            entryText.Append(nameof(DataFlags.None));
                        }

                        // Flags
                        entryText.Append(", ");
                        entryText.Append('"');
                        entryText.Append(entry.Flags.ToString());
                        entryText.Append('"');

                        // Platform
                        entryText.Append(", ");
                        entryText.Append('"');
                        entryText.Append(entry.Platform.ToString());
                        entryText.Append('"');

                        // Min/Max Length
                        entryText.Append(", ");
                        entryText.Append(entry.MinLength.ToString());
                        entryText.Append(", ");
                        entryText.Append(entry.MaxLength.ToString());

                        // Fields
                        entryText.Append(", ");
                        if (entry.Fields.Length > 0)
                        {
                            entryText.Append($"new {nameof(Field)}[] {{");
                            int fieldIndex = 0;
                            foreach (Field field in entry.Fields)
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
                            entryText.Append($"{nameof(Field)}");
                            entryText.Append(">()");
                        }

                        // Operands
                        entryText.Append(", ");
                        if (entry.Operands.Length > 0)
                        {
                            entryText.Append($"new {nameof(Operand)}[] {{");
                            int operandIndex = 0;
                            foreach (Operand operand in entry.Operands)
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
                            entryText.Append($"{nameof(Operand)}");
                            entryText.Append(">()");
                        }

                        entryText.Append(')');

                        entriesText.Append(entryText);
                    }

                    instructionsTableText.AppendLine($"\t\t{varName}[{list.Op:D}] = new {listName}(0b{opBinary},");
                    instructionsTableText.AppendLine(entriesText.ToString());
                    instructionsTableText.AppendLine($"\t\t);");
                }
            }

            instructionsTableText.AppendLine("\t}");
            instructionsTableText.AppendLine("}");

            Debug.WriteLine(instructionsTableText.ToString());

#endif // GENERATE_CS

            Console.WriteLine();
            Console.WriteLine("Done, press any key to exit");
            Console.ReadKey();
        }
    }
}