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
        Offset0,
        Offset1,
        Segment0,
        Segment1,
        RelativeLabelDisplacement0,
        RelativeLabelDisplacement1,
        ShortLabel,
        LongLabel
    }

    record Field(FieldType Type, string Value, byte Constant = 0)
    {
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
                "sl" => FieldType.ShortLabel,
                "ll" => FieldType.LongLabel,
                _ => FieldType.None,
            };
            if (type == FieldType.None)
            {
                if (byte.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte constantValue))
                    return new Field(FieldType.Constant, value, constantValue);
            }
            return new Field(type, value);
        }

        public override string ToString()
        {
            if (Type == FieldType.Constant)
                return Constant.ToString("X2");
            else if (Type != FieldType.None)
                return Type.ToString();
            else
                return Value;
        }
    }

    sealed record Family(string Name, string Description, string Platform)
    {
        public override int GetHashCode() => Name.GetHashCode();
        public bool Equals(Family other) => Name.Equals(other.Name);
    }

    record Instruction(byte Op, string Mnemonic, string Destination, string Source, int MinLength, int MaxLength, Field[] Fields, string Platform)
    {
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("0x");
            s.Append(Op.ToString("X2"));
            s.Append('|');
            s.Append(Mnemonic);
            if (!string.IsNullOrWhiteSpace(Destination))
            {
                s.Append(' ');
                s.Append(Destination);
            }
            if (!string.IsNullOrWhiteSpace(Source))
            {
                s.Append(", ");
                s.Append(Source);
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
            if (!string.IsNullOrWhiteSpace(Platform))
            {
                s.Append('|');
                s.Append(Platform);
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

                    string platform = string.Empty;
                    if (mnemonics.EndsWith("]"))
                    {
                        int p = mnemonics.IndexOf("[");
                        platform = mnemonics.Substring(p);
                    }

                    int minLen = 0;
                    int maxLen = 0;

                    Match lenMatch = _rexLength.Match(lenText);
                    if (!lenMatch.Success)
                        throw new FormatException($"Unsupported length string '{lenText}' in row '{rowIndex}' for '{fullTitle}'!");

                    int.TryParse(lenMatch.Groups["min"].Value ?? string.Empty, out minLen);
                    int.TryParse(lenMatch.Groups["max"].Value ?? string.Empty, out maxLen);

                    if (maxLen == 0)
                        maxLen = minLen;

                    swText = swText.PadLeft(2, ' ');

                    swText = Regex.Replace(swText, "\\s", "*");
                    Debug.Assert(swText.Length == 2);

                    flagsText = Regex.Replace(flagsText, "-", "*");

                    opAndFields = opAndFields.Replace("|", "");

                    string[] opSplit = opAndFields.Split(' ');
                    byte op = 0;
                    string[] fieldsRaw = opSplit[1..];
                    if (opSplit.Length > 0)
                        op = byte.Parse(opSplit[0], NumberStyles.HexNumber);

#if GENERATE_CS
                    string opName;
                    string opDest = string.Empty;
                    string opSrc = string.Empty;

                    string originalMemonics = mnemonics;
                    if (!string.IsNullOrEmpty(platform))
                        mnemonics = originalMemonics.Substring(0, originalMemonics.Length - platform.Length);

                    string tabbedReplaced = Regex.Replace(mnemonics, "[\\s,]", "\t");

                    string[] splittedMnemonics = tabbedReplaced.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                    if (splittedMnemonics.Length == 0)
                        throw new NotSupportedException($"Mnemonic '{mnemonics}' is invalid!");

                    opName = splittedMnemonics[0];
                    if (splittedMnemonics.Length == 2)
                    {
                        opDest = splittedMnemonics[1];
                    }
                    else if (splittedMnemonics.Length == 3)
                    {
                        opDest = splittedMnemonics[1];
                        opSrc = splittedMnemonics[2];
                    }

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

                    Family firstFamily = new Family(firstFamilyRaw, title, platform);

                    if (!familyOpTypeListMap.TryGetValue(firstFamily, out List<string> opNames))
                    {
                        opNames = new List<string>();
                        familyOpTypeListMap.Add(firstFamily, opNames);
                        orderedFamilies.Add(firstFamily);
                    }

                    opNames.Add(opName);

                    Instruction instruction = new Instruction(op, opName, opDest, opSrc, minLen, maxLen, fields, platform.Trim('[', ']'));
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
                if (instruction.Platform.Length != 0)
                    continue;
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