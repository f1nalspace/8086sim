//#define EXPORT_TO_CSV
#define GENERATE_CS
//#define GENERATE_INSTRUCTION_TYPES

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

        public override int GetHashCode() => Name.GetHashCode();
        public bool Equals(InstructionFamily other) => Name.Equals(other.Name);
        public override bool Equals(object obj) => obj is InstructionFamily other && Equals(other);
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

                    // Parse Signed or Word flag (SW)
                    swText = swText.PadRight(2, '*');
                    swText = Regex.Replace(swText, "\\s", "*");
                    Debug.Assert(swText.Length == 2);

                    DataWidth dataWidth = DataWidth.None;
                    if (swText[1] == 'B')
                        dataWidth |= DataWidth.Byte;
                    else if (swText[1] == 'W')
                        dataWidth |= DataWidth.Word;
                    else if (swText[1] == 'D')
                        dataWidth |= DataWidth.DoubleWord;
                    else if (swText[1] == 'Q')
                        dataWidth |= DataWidth.QuadWord;
                    else if (swText[1] == 'T')
                        dataWidth |= DataWidth.TenBytes;
                    else if (swText[1] != '*')
                        throw new NotImplementedException($"The w flag '{swText[1]}' is not implemented");

                    SignBit signFlag = SignBit.None;
                    if (swText[0] == 'E')
                        signFlag = SignBit.SignExtendedImm8;
                    else if (swText[0] == 'N')
                        signFlag = SignBit.Non;
                    else if (swText[0] != '*')
                        throw new NotImplementedException($"The s flag '{swText[0]}' is not implemented");

                    // Parse flags
                    flagsText = Regex.Replace(flagsText, "-", "*");
                    Debug.Assert(flagsText.Length == 8);
                    InstructionFlags flags = new InstructionFlags(flagsText.AsSpan());

                    //if (flagsText[0] == 0)

                    // Parse fields, which defines each byte in the instruction stream
                    // Each field is separated by a space, so if we split by space, we get all fields
                    // The very first field contains always the full op-code byte
                    //
                    // Issues:
                    // Sometimes there is a | character, so we need to remove that

                    string[] opSplit = opAndFields
                        .Replace("|", "")
                        .Split(new[] { ' ' });

                    byte op = 0;
                    Span<string> fieldsSplitted = Span<string>.Empty;
                    if (opSplit.Length > 0)
                    {
                        op = byte.Parse(opSplit[0], NumberStyles.HexNumber);
                        fieldsSplitted = opSplit.AsSpan(1);
                    }

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

                    InstructionFamily family = new InstructionFamily(splittedFamily[0], title, platform);

                    if (!familyOpTypeListMap.TryGetValue(family, out List<string> opNames))
                    {
                        opNames = new List<string>();
                        familyOpTypeListMap.Add(family, opNames);
                        orderedFamilies.Add(family);
                    }

                    opNames.Add(opName);

                    // Parse mnemonic operands
                    Operand[] operands = new Operand[splittedMnemonics.Length - 1];
                    for (int i = 1; i < splittedMnemonics.Length; i++)
                        operands[i - 1] = Operand.Parse(splittedMnemonics[i]);

                    // Convert family into instruction type and create the instruction entry to the table
                    if (Enum.TryParse<InstructionType>(family.Name, out InstructionType type))
                    {
                        DataFlags dataFlags = DataFlags.None;
                        if (signFlag == SignBit.SignExtendedImm8)
                            dataFlags |= DataFlags.SignExtendedImm8;

                        InstructionEntry instruction = new InstructionEntry(op, type, dataWidth, dataFlags, flags, platform, minLen, maxLen, operands, fields);
                        allInstructions.Add(instruction);
                    } else
                        Debug.WriteLine($"WARNING: Family '{family}' has no enum value for '{nameof(InstructionType)}'");
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
            // Print out parsed stuff in CSharp
            InstructionEntry[] sortedInstructions = allInstructions.OrderBy(i => i.Op).ToArray();
            foreach (InstructionEntry instruction in sortedInstructions)
            {
                if (instruction.Platform.Type != PlatformType.None)
                    continue;
                Debug.WriteLine($"{instruction}");
            }

#if GENERATE_INSTRUCTION_TYPES
            Debug.WriteLine("enum InstructionType {");
            Debug.WriteLine("\t/// <summary>");
            Debug.WriteLine($"\t/// None");
            Debug.WriteLine("\t/// </summary>");
            Debug.WriteLine("\tNone = 0,");
            foreach (InstructionFamily family in orderedFamilies)
            {
                if (family.Platform.Type != PlatformType.None)
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