#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem.Articy
{

    /// <summary>
    /// This static utility class contains tools for working with Articy data.
    /// </summary>
    public static class ArticyTools
    {

        /// <summary>
        /// InitializeLuaSubtables() converts fields whose titles start with this string into subtables.
        /// </summary>
        public const string SubtableFieldPrefix = "SUBTABLE__";

        /// <summary>
        /// Convert articy markup codes to rich text codes that Unity can display.
        /// </summary>
        public static bool convertMarkupToRichText = true;

        /// <summary>
        /// Checks the first few lines of a string in articy:draft XML format for a schema identifier.
        /// </summary>
        /// <returns>
        /// <c>true</c> if it contains the schema identifier.
        /// </returns>
        /// <param name='xmlFilename'>
        /// XML data to check.
        /// </param>
        /// <param name='schemaId'>
        /// Schema identifier to check for.
        /// </param>
        public static bool DataContainsSchemaId(string xmlData, string schemaId)
        {
            StringReader xmlStream = new StringReader(xmlData);
            if (xmlStream != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    string s = xmlStream.ReadLine();
                    if (!string.IsNullOrEmpty(s) && s.Contains(schemaId)) return true;
                }
                //--- Not compatible with UWP10: xmlStream.Close();
            }
            return false;
        }

        private static string[] htmlTags = new string[] { "<html>", "<head>", "<style>", "#s0", "{text-align:left;}", "#s1",
            "{font-size:11pt;}", "</style>", "</head>", "<body>", "<p id=\"s0\">", "<span id=\"s1\">",
            "</span>", "</p>", "</body>", "</html>" };

        /// <summary>
        /// Removes HTML tags from a string.
        /// </summary>
        /// <returns>
        /// The string without HTML.
        /// </returns>
        /// <param name='s'>
        /// The HTML-filled string.
        /// </param>
        public static string RemoveHtml(string s)
        {
            // This is a rather inefficient first pass, but it gets the job done.
            // On the roadmap: Replace with http://www.codeproject.com/Articles/298519/Fast-Token-Replacement-in-Csharp
            if (!string.IsNullOrEmpty(s))
            {
                if (convertMarkupToRichText) s = ReplaceMarkup(s);
                foreach (string htmlTag in htmlTags)
                {
                    s = s.Replace(htmlTag, string.Empty);
                }
                if (s.Contains("&#")) s = ReplaceHtmlCharacterCodes(s);
                s = s.Replace("&quot;", "\"");
                s = s.Replace("&amp;", "&");
                s = s.Replace("&lt;", "<");
                s = s.Replace("&gt;", ">");
                s = s.Replace("&nbsp;", " ");
                s = s.Trim();
            }
            return s;
        }

        /// <summary>
        /// Selectively replaces HTML character codes (numeric character references) that articy uses.
        /// </summary>
        public static string ReplaceHtmlCharacterCodes(string s)
        {
            var text = s;
            Regex regex = new Regex(@"&#[0-9]+;");
            text = regex.Replace(text, delegate (Match match)
            {
                string codeString = match.Value.Substring(2, match.Value.Length - 3);
                int numericCode;
                if (!int.TryParse(codeString, out numericCode)) return match.Value;
                return char.ConvertFromUtf32(numericCode).ToString();
            });
            return text;

            //return s.Replace("&#33;", "!")
            //        .Replace("&#34;", "\"")
            //        .Replace("&#35;", "#")
            //        .Replace("&#36;", "$")
            //        .Replace("&#37;", "%")
            //        .Replace("&#38;", "&")
            //        .Replace("&#39;", "'")
            //        .Replace("&#96;", "`")
            //        .Replace("&#160;", " ")
            //        .Replace("&#162;", "¢")
            //        .Replace("&#163;", "£")
            //        .Replace("&#164;", "¤")
            //        .Replace("&#165;", "¥")
            //        .Replace("&#166;", "¦")
            //        .Replace("&#167;", "§")
            //        .Replace("&#168;", "¨")
            //        .Replace("&#169;", "©")
            //        .Replace("&#177;", "±")
            //        .Replace("&#178;", "²")
            //        .Replace("&#179;", "³")
            //        .Replace("&#180;", "´")
            //        .Replace("&#188;", "¼")
            //        .Replace("&#189;", "½")
            //        .Replace("&#190;", "¾")
            //        .Replace("&#191;", "¿");
        }

        //==================================================================
        // Code contributed by Racoon7:

        const RegexOptions Options = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase;
        static readonly Regex StylesRegex = new Regex(@"<style>(?<styles>.*?)</style>", Options); // Get the part of text dealing with styles
        static readonly Regex StyleRegex = new Regex(@"#(?<id>s[1-9]\d*) {(?<style>.*?)}", Options); // The first style "s0" is always a paragraph

        // Check a specific style for these.
        static readonly Regex BoldRegex = new Regex(@"font-weight\s*?:\s*?bold", Options);
        static readonly Regex ItalicRegex = new Regex(@"font-style\s*?:\s*?italic", Options);
        static readonly Regex ColorRegex = new Regex(@"color\s*?:\s*?(?<color>#\w{6})", Options);

        // Apply the styles to the actual text. The style tags never overlap, so they can be processed in order.
        static readonly Regex TextRegex = new Regex(@"<p id=""s0"">(?<text>.*?)</p>", Options);
        static readonly Regex PartsRegex = new Regex(@"<span id=""(?<id>s[1-9]\d*)"">(?<text>.*?)</span>", Options); // Style id : Pure text  

        static string ReplaceMarkup(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return ConvertToRichText(s);
        }

        /// <summary>Parses given text and converts the Articy markup to rich text.</summary>
        static string ConvertToRichText(string s)
        {
            s = s.Replace(@"&#39;", "'"); // Apostrophe

            // Get styles
            if (!StylesRegex.IsMatch(s)) return s; // No styles, pure text
            string stylesText = StylesRegex.Match(s).Value;
            var numberedStyles = StyleRegex.Matches(stylesText)
                                           .Cast<Match>()
                                           .Select(match => new {
                                               Id = match.Groups["id"].Value,
                                               Style = match.Groups["style"].Value
                                           });
            var styles = numberedStyles.Select(style => new {
                style.Id,
                Bold = BoldRegex.IsMatch(style.Style),
                Italic = ItalicRegex.IsMatch(style.Style),
                Color = ColorRegex.Match(style.Style).Groups["color"].Value
            });



            // Multiparagraph fix contributed by Francois Dujardin:
            var allParagraphs = TextRegex.Matches(s);

            //process each paragraph
            List<string> paragraphs = new List<string>();
            foreach (var v in allParagraphs)
            {
                var innerTexts = PartsRegex.Matches(v.ToString())
                                    .Cast<Match>()
                                    .Select(match => new {
                                        StyleId = match.Groups["id"].Value,
                                        Text = match.Groups["text"].Value
                                    });
                // Apply the styles to the texts
                var editedParts = innerTexts.Select(text => {
                    var currentStyle = styles.First(style => style.Id == text.StyleId);
                    return ApplyStyle(
                            innerText: text.Text,
                            bold: currentStyle.Bold,
                            italic: currentStyle.Italic,
                            color: currentStyle.Color
                    );
                }).ToArray();
                string tmp = string.Join(string.Empty, editedParts);
                if (!string.IsNullOrEmpty(tmp))
                    paragraphs.Add(tmp);
            }
            string editedLine = string.Join("\n", paragraphs.ToArray());

            //// Get texts (ORIGINAL CODE)
            //var fullText = TextRegex.Match(s).Value; // The dialogue text with <span> tags

            //var innerTexts = PartsRegex.Matches(fullText)
            //                           .Cast<Match>()
            //                           .Select(match => new {
            //                               StyleId = match.Groups["id"].Value,
            //                               Text = match.Groups["text"].Value
            //                           });

            //// Apply the styles to the texts
            //var editedParts = innerTexts.Select(text => {
            //    var currentStyle = styles.First(style => style.Id == text.StyleId);
            //    return ApplyStyle(
            //            innerText: text.Text,
            //            bold: currentStyle.Bold,
            //            italic: currentStyle.Italic,
            //            color: currentStyle.Color
            //    );
            //}).ToArray();
            //string editedLine = string.Join(string.Empty, editedParts);

            return editedLine;
        }

        /// <summary>Wraps a given text in rich text tags.</summary>
        static string ApplyStyle(string innerText, bool bold, bool italic, string color)
        {
            var builder = new StringBuilder(innerText);

            if (bold) WrapInTag(ref builder, "b");
            if (italic) WrapInTag(ref builder, "i");
            if (color != string.Empty) WrapInTag(ref builder, "color", color);

            return builder.ToString();
        }

        static void WrapInTag(ref StringBuilder builder, string tag, string value = "")
        {
            builder.Insert(0, (value != string.Empty) // opening tag
                    ? string.Format(@"<{0}={1}>", tag, value) // the tag has a value
                    : string.Format(@"<{0}>", tag)); // no value
            builder.Append(string.Format(@"</{0}>", tag)); // closing tag
        }

        //==================================================================

        public static bool IsQuestStateArticyPropertyName(string propertyName)
        {
            return string.Equals(propertyName, "State") ||
                Regex.Match(propertyName, @"^Entry_[0-9]+_State").Success;
        }

        public static string EnumValueToQuestState(int enumValue, string stringValue)
        {
            // In case enum is out of order, go by string value:
            if (string.Equals("unassigned", stringValue, System.StringComparison.OrdinalIgnoreCase)) return QuestLog.StateToString(QuestState.Unassigned);
            if (string.Equals("active", stringValue, System.StringComparison.OrdinalIgnoreCase)) return QuestLog.StateToString(QuestState.Active);
            if (string.Equals("success", stringValue, System.StringComparison.OrdinalIgnoreCase)) return QuestLog.StateToString(QuestState.Success);
            if (string.Equals("failure", stringValue, System.StringComparison.OrdinalIgnoreCase)) return QuestLog.StateToString(QuestState.Failure);
            if (string.Equals("abandoned", stringValue, System.StringComparison.OrdinalIgnoreCase)) return QuestLog.StateToString(QuestState.Abandoned);

            // Failing that, by enum value:
            switch (enumValue)
            {
                case 1: return QuestLog.StateToString(QuestState.Unassigned);
                case 2: return QuestLog.StateToString(QuestState.Active);
                case 3: return QuestLog.StateToString(QuestState.Success);
                case 4: return QuestLog.StateToString(QuestState.Failure);
                case 5: return QuestLog.StateToString(QuestState.Abandoned);
                default: return QuestLog.StateToString(QuestState.Unassigned);
            }
        }

        /// <summary>
        /// Articy strips are saved in a string field. This method converts them
        /// into subtables that reference the strip contents.
        /// </summary>
        public static void InitializeLuaSubtables()
        {
            if (DialogueManager.masterDatabase == null) return;
            InitializeLuaSubtablesForAsset<Actor>("Actor", DialogueManager.masterDatabase.actors);
            InitializeLuaSubtablesForAsset<Item>("Item", DialogueManager.masterDatabase.items);
        }

        private static void InitializeLuaSubtablesForAsset<T>(string tableName, List<T> assets) where T : Asset
        {
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                for (int j = 0; j < asset.fields.Count; j++)
                {
                    var field = asset.fields[j];
                    if (field.title.StartsWith(SubtableFieldPrefix))
                    {
                        var subtableTitle = field.title.Substring(SubtableFieldPrefix.Length);
                        var code = tableName + "[\"" + DialogueLua.StringToTableIndex(asset.Name) + "\"]." + DialogueLua.StringToTableIndex(subtableTitle) + " = { ";
                        if (!string.IsNullOrEmpty(field.value.Trim()))
                        {
                            var articyIds = field.value.Split(';');
                            for (int k = 0; k < articyIds.Length; k++)
                            {
                                var articyId = articyIds[k];
                                var elementItem = FindAssetByArticyId(articyId);
                                if (elementItem != null)
                                {
                                    code += ((elementItem is Actor) ? "Actor" : "Item") + "[\"" + DialogueLua.StringToTableIndex(elementItem.Name) + "\"]";
                                }
                                else
                                {
                                    code += "\"" + articyId + "\"";
                                }
                                code += ", ";
                            }
                        }
                        code += "}";
                        Lua.Run(code, DialogueDebug.logInfo);

                        // Clear original subtable field to save memory:
                        Lua.Run(tableName + "[\"" + DialogueLua.StringToTableIndex(asset.Name) + "\"]." + DialogueLua.StringToFieldName(field.title) + " = nil", true);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the item in the master database with the specified Articy Id field.
        /// </summary>
        public static Asset FindAssetByArticyId(string articyId)
        {
            if (DialogueManager.masterDatabase == null) return null;

            // Check actors:
            var asset = FindAssetInListByArticyId<Actor>(DialogueManager.masterDatabase.actors, articyId);
            if (asset != null) return asset;

            // Check items:
            asset = FindAssetInListByArticyId<Item>(DialogueManager.masterDatabase.items, articyId);
            if (asset != null) return asset;

            return null;
        }

        private static Asset FindAssetInListByArticyId<T>(List<T> assets, string articyId) where T : Asset
        {
            if (DialogueManager.masterDatabase == null) return null;
            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                if (string.Equals(articyId, asset.LookupValue("Articy Id"))) return asset;
            }
            return null;
        }

    }
}
#endif
