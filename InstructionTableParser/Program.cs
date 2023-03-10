using CsvHelper;
using CsvHelper.Configuration;
using HtmlAgilityPack;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Final.ITP
{
    public class Program
    {
        private static readonly Regex _rexTitle = new Regex("\\(([0-9]+[+]?.*)\\).*$", RegexOptions.Compiled);
        private static readonly Regex _rexLength = new Regex("(?<min>[0-6])(([~+])(?<max>[0-6]))?", RegexOptions.Compiled);

        public static void Main(string[] args)
        {
            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                NewLine = Environment.NewLine,
                Delimiter = ";",
                Encoding = Encoding.UTF8,
            };

            string outFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "8086-instruction-table.csv");

            using FileStream csvStream = File.Create(outFilePath);

            using StreamWriter writer = new StreamWriter(csvStream, encoding: Encoding.UTF8, leaveOpen: true);

            using (CsvWriter csv = new CsvWriter(writer, config, leaveOpen: true))
            {
                csv.WriteConvertedField("mnemonics", typeof(string));
                csv.WriteConvertedField("op xx xx xx xx xx", typeof(string));
                csv.WriteConvertedField("sw", typeof(string));
                csv.WriteConvertedField("minlen", typeof(int));
                csv.WriteConvertedField("maxlen", typeof(int));
                csv.WriteConvertedField("flags", typeof(string));
                csv.WriteConvertedField("family", typeof(string));
                csv.WriteConvertedField("title", typeof(string));
                csv.WriteConvertedField("platform", typeof(string));
                csv.WriteConvertedField("len", typeof(string));
                csv.NextRecord();

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
                    string family;
                    if (fullTitle.IndexOf("-") > -1)
                    {
                        family = fullTitle.Substring(0, fullTitle.IndexOf("-")).Trim();
                        title = fullTitle.Substring(fullTitle.IndexOf("-") + 1).Trim();
                    }
                    else
                    {
                        family = string.Empty;
                        title = fullTitle;
                    }

                    string platform = "any";
                    Match titleMatch = _rexTitle.Match(fullTitle);
                    if (titleMatch.Success)
                        platform = titleMatch.Groups[1].Value;

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

                        int minLen = 0;
                        int maxLen = 0;

                        Match lenMatch = _rexLength.Match(lenText);
                        if (!lenMatch.Success)
                            throw new FormatException($"Unsupported length string '{lenText}' in row '{rowIndex}' for '{fullTitle}'!");

                        int.TryParse(lenMatch.Groups["min"].Value ?? string.Empty, out minLen);
                        int.TryParse(lenMatch.Groups["max"].Value ?? string.Empty, out maxLen);

                        if (maxLen == 0)
                            maxLen = minLen;

                        csv.WriteField(mnemonics, true);
                        csv.WriteField(opAndFields, true);
                        csv.WriteField(swText, true);
                        csv.WriteField(minLen);
                        csv.WriteField(maxLen);
                        csv.WriteField(flagsText, true);
                        csv.WriteField(family, true);
                        csv.WriteField(title, true);
                        csv.WriteField(platform, true);
                        csv.WriteField(lenText);
                        csv.NextRecord();
                    }

                    HtmlNode next = div.SelectSingleNode("following-sibling::h4");

                    cur = next;
                }
            }

            writer.Flush();

            csvStream.Flush();

            //byte[] data = csvStream.GetBuffer();

            //string text = Encoding.UTF8.GetString(data);

            //Console.WriteLine(text);

            Console.WriteLine();
            Console.WriteLine("Done, press any key to exit");
            Console.ReadKey();
        }
    }
};