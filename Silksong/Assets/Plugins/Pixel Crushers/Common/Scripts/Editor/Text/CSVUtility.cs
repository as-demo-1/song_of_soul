// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace PixelCrushers
{

    public static class CSVUtility
    {

        /// <summary>
        /// Writes a table of strings to a CSV file.
        /// </summary>
        /// <param name="content">2D list of strings.</param>
        /// <param name="filename">Filename to write as.</param>
        /// <param name="encodingType">Encoding type to use.</param>
        public static void WriteCSVFile(List<List<string>> content, string filename, EncodingType encodingType)
        {
            using (var file = new StreamWriter(filename, false, EncodingTypeTools.GetEncoding(encodingType)))
            {
                for (int i = 0; i < content.Count; i++)
                {
                    var row = content[i];
                    StringBuilder sb = new StringBuilder();
                    var first = true;
                    for (int j = 0; j < row.Count; j++)
                    {
                        if (!first) sb.Append(",");
                        first = false;
                        var cell = row[j];
                        sb.Append(CleanField(cell));
                    }
                    file.WriteLine(sb);
                }
            }
        }

        /// <summary>
        /// Reads a CSV file into a table of strings.
        /// </summary>
        /// <param name="filename">Name of CSV file to read.</param>
        /// <param name="encodingType">Encoding type CSV file was created in.</param>
        /// <returns>Contents of CSV file as a 2D list of strings.</returns>
        public static List<List<string>> ReadCSVFile(string filename, EncodingType encodingType)
        {
            // Read the source file and combine multiline rows:
            var sourceLines = new List<string>();
            using (var file = new StreamReader(filename, EncodingTypeTools.GetEncoding(encodingType)))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    sourceLines.Add(line.TrimEnd());
                }
            }
            CombineMultilineSourceLines(sourceLines);
            if (sourceLines.Count < 1) return null;

            var content = new List<List<string>>();
            while (sourceLines.Count > 0)
            {
                var values = GetValues(sourceLines[0]);
                sourceLines.RemoveAt(0);
                if (values == null || values.Length == 0) continue;
                var row = new List<string>();
                content.Add(row);
                for (int i = 0; i < values.Length; i++)
                {
                    row.Add(values[i]);
                }
            }
            return content;
        }

        private static string CleanField(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            string s2 = s.Contains("\n") ? s.Replace("\n", "\\n") : s;
            if (s2.Contains("\r")) s2 = s2.Replace("\r", "\\r");
            if (s2.Contains(",") || s2.Contains("\""))
            {
                return "\"" + s2.Replace("\"", "\"\"") + "\"";
            }
            else
            {
                return s2;
            }
        }

        /// <summary>
        /// Returns the individual comma-separated values in a line.
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="line">Line.</param>
        private static string[] GetValues(string line)
        {
            Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)");
            List<string> values = new List<string>();
            foreach (Match match in csvSplit.Matches(line))
            {
                values.Add(UnwrapValue(match.Value.TrimStart(',')));
            }
            return values.ToArray();
        }

        /// <summary>
        /// Returns a "fixed" version of a comma-separated value where escaped newlines
        /// have been converted back into real newlines, and optional surrounding quotes 
        /// have been removed.
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="value">Value.</param>
        private static string UnwrapValue(string value)
        {
            string s = value.Replace("\\n", "\n").Replace("\\r", "\r");
            if (s.StartsWith("\"") && s.EndsWith("\""))
            {
                s = s.Substring(1, s.Length - 2).Replace("\"\"", "\"");
            }
            return s;
        }

        /// <summary>
        /// Combines lines that are actually a multiline CSV row. This also helps prevent the 
        /// CSV-splitting regex from hanging due to catastrophic backtracking on unterminated quotes.
        /// </summary>
        private static void CombineMultilineSourceLines(List<string> sourceLines)
        {
            int lineNum = 0;
            int safeguard = 0;
            int MaxIterations = 999999;
            while ((lineNum < sourceLines.Count) && (safeguard < MaxIterations))
            {
                safeguard++;
                string line = sourceLines[lineNum];
                if (line == null)
                {
                    sourceLines.RemoveAt(lineNum);
                }
                else
                {
                    bool terminated = true;
                    char previousChar = (char)0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        char currentChar = line[i];
                        bool isQuote = (currentChar == '"') && (previousChar != '\\');
                        if (isQuote) terminated = !terminated;
                        previousChar = currentChar;
                    }
                    if (terminated || (lineNum + 1) >= sourceLines.Count)
                    {
                        if (!terminated) sourceLines[lineNum] = line + '"';
                        lineNum++;
                    }
                    else
                    {
                        sourceLines[lineNum] = line + "\\n" + sourceLines[lineNum + 1];
                        sourceLines.RemoveAt(lineNum + 1);
                    }
                }
            }
        }

    }
}
