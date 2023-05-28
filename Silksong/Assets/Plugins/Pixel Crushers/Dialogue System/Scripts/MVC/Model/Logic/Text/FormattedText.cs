// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// A line of text including formatting. Chat Mapper and the dialogue system support formatting
    /// codes inside lines of dialogue. This class represents a line where the formatting codes 
    /// have been extracted and stored in separate formatting variables (emphases and italic).
    /// </summary>
    public class FormattedText
    {

        /// <summary>
        /// Represents an empty line.
        /// </summary>
        public static readonly FormattedText empty = new FormattedText();

        /// <summary>
        /// Represents an empty set of emphases.
        /// </summary>
        public static readonly Emphasis[] noEmphases = new Emphasis[0];

        /// <summary>
        /// Constant: indicates that the response has no specifically-assigned position and can be
        /// placed anywhere in the player response menu.
        /// </summary>
        public const int NoAssignedPosition = -1;

        /// <summary>
        /// Constant: indicates that there is no picture override for this pic tag.
        /// </summary>
        public const int NoPicOverride = 0;

        /// <summary>
        /// Gets or sets the "clean" line of text, without formatting codes.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string text { get; set; }

        /// <summary>
        /// Gets or sets the list of emphases. An Emphasis specifies special formatting for a 
        /// substring of the text.
        /// </summary>
        /// <value>
        /// The emphases.
        /// </value>
        public Emphasis[] emphases { get; set; }

        /// <summary>
        /// Gets or sets a value indicating to display the entire line in italics.
        /// </summary>
        /// <value>
        /// <c>true</c> if italic; otherwise, <c>false</c>.
        /// </value>
        public bool italic { get; set; }

        /// <summary>
        /// Gets or sets the response button position, for response text.
        /// </summary>
        /// <value>
        /// The position, or NoAssignedPosition if there's no [position #] tag in the text.
        /// </value>
        public int position { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this response forces display of the 
        /// response menu, even if there's only one response.
        /// </summary>
        /// <value>
        /// <c>true</c> to force the response menu; otherwise, <c>false</c>.
        /// </value>
        public bool forceMenu { get; set; }

        /// <summary>
        /// If true, auto-select this entry instead of showing a response menu.
        /// </summary>
        public bool forceAuto { get; set; }

        public bool noSubtitle { get; set; }

        /// <summary>
        /// Gets or sets the [pic=#] value.
        /// </summary>
        /// <value>The pic index, where <c>0</c> means unused.</value>
        public int pic { get; set; }

        /// <summary>
        /// Gets or sets the [pica=#] value.
        /// </summary>
        /// <value>The pica index, where <c>0<c/c> means unused.</value>
        public int picActor { get; set; }

        /// <summary>
        /// Gets or sets the [picc=#] value.
        /// </summary>
        /// <value>The picc index, where <c>0</c> means unused.</value>
        public int picConversant { get; set; }

        /// <summary>
        /// Gets or sets the variable input prompt. If this is set, the value is
        /// the name of the variable to prompt for input.
        /// </summary>
        /// <value>The variable name.</value>
        public string variableInputPrompt { get; set; }

        /// <summary>
        /// Returns <c>true</c> if the formatted text includes a variable
        /// input prompt.
        /// </summary>
        /// <value><c>true</c> if has variable input prompt; otherwise, <c>false</c>.</value>
        public bool hasVariableInputPrompt { get { return !string.IsNullOrEmpty(variableInputPrompt); } }

        /// <summary>
        /// The subtitle 
        /// </summary>
        public int subtitlePanelNumber { get; set; }

        /// <summary>
        /// Initializes a new FormattedText.
        /// </summary>
        /// <param name='text'>The clean line of text, without formatting codes.</param>
        /// <param name='emphases'>Emphases.</param>
        /// <param name='italic'>Italic.</param>
        /// <param name='position'>Position.</param>
        /// <param name='forceMenu'>Force menu.</param>
        /// <param name='forceAuto'>Force automatically selecting response without presenting a menu.</param>
        /// <param name='pic'>The [pic] value, or <c>0</c> for unused.</param>
        /// <param name='picActor'>The [pica] value, or <c>0</c> for unused.</param>
        /// <param name='picConversant'>The [picc] value, or <c>0</c> for unused.</param>
        /// <param name="noSubtitle">Don't show subtitle.</param>
        public FormattedText(string text = null, Emphasis[] emphases = null, bool italic = false, int position = NoAssignedPosition, bool forceMenu = true, bool forceAuto = false,
                             int pic = NoPicOverride, int picActor = NoPicOverride, int picConversant = NoPicOverride, string variableInputPrompt = null, int subtitlePanelNumber = -1, bool noSubtitle = false)
        {
            this.text = text ?? string.Empty;
            this.emphases = emphases ?? noEmphases;
            this.italic = italic;
            this.position = position;
            this.forceMenu = forceMenu;
            this.forceAuto = forceAuto;
            this.pic = pic;
            this.picActor = picActor;
            this.picConversant = picConversant;
            this.variableInputPrompt = variableInputPrompt;
            this.subtitlePanelNumber = subtitlePanelNumber;
            this.noSubtitle = noSubtitle;
        }
        /// <summary>
        /// Parses the text from a dialogue entry, which may contain formatting codes, and returns
        /// a FormattedText. The Parse() method handles these tags:
        /// 
        /// - Pipe (|): Replaced with newlines.
        /// - [a]: Italics.
        /// - [f]: Force response menu.
        /// - [position #]: Response button position.
        /// - [em#]...[/em#] (#=1-4, colors defined in dialogue database/Chat Mapper preferences). 
        /// Currently records only one emphasis unless rich text is enabled.
        /// - [var=varName]: Replaces tag with the value of a variable. Example: 
        /// <c>"Hello, [varName=Actor]."</c>.
        /// - [lua(xxx)]: Replaces tag with the result of Lua code xxx. Example: 
        /// <c>"Hello, [lua(Variable['Actor'])]."</c>. Lua tags are processed first, so your Lua
        /// code can return other formatting codes that will then be parsed properly.
        /// </summary>
        /// <param name='rawText'>
        /// The raw text to parse.
        /// </param>
        /// <param name='emphasisSettings'>
        /// The emphasis settings to use (usually from DialogueManager.MasterDatabase) when 
        /// parsing [em#] tags.
        /// </param>
        public static FormattedText Parse(string rawText, EmphasisSetting[] emphasisSettings = null)
        {
            if (emphasisSettings == null && DialogueManager.instance != null) emphasisSettings = DialogueManager.masterDatabase.emphasisSettings;
            string text = rawText ?? string.Empty;
            ReplaceLuaTags(ref text);
            string variableInputPrompt = ExtractVariableInputPrompt(ref text);
            ReplaceVarTags(ref text);
            ReplaceAutocaseTags(ref text);
            ReplacePipes(ref text); // Was: ExtractTag("|", ref text);
            int pic = FormattedText.NoPicOverride;
            int pica = FormattedText.NoPicOverride;
            int picc = FormattedText.NoPicOverride;
            if (text.Contains("[pic"))
            {
                pic = ExtractPicTag(@"\[pic=[0-9a-zA-z_]+\]", ref text);
                pica = ExtractPicTag(@"\[pica=[0-9a-zA-z_]+\]", ref text);
                picc = ExtractPicTag(@"\[picc=[0-9a-zA-z_]+\]", ref text);
            }
            bool italic = ExtractTag("[a]", ref text);
            bool forceMenu = ExtractTag("[f]", ref text);
            bool forceAuto = ExtractTag("[auto]", ref text);
            bool noSubtitle = ExtractTag("[nosubtitle]", ref text);
            int position = ExtractPositionTag(ref text);
            int subtitlePanelNumber = ExtractPanelNumberTag(ref text);
            Emphasis[] emphases = DialogueManager.instance.displaySettings.subtitleSettings.richTextEmphases
                ? ReplaceEmphasisTagsWithRichText(ref text, emphasisSettings)
                : ExtractEmphasisTags(ref text, emphasisSettings);
            return new FormattedText(text, emphases, italic, position, forceMenu, forceAuto, pic, pica, picc, variableInputPrompt, subtitlePanelNumber, noSubtitle);
        }

        /// <summary>
        /// Replaces var and lua markup tags in text.
        /// </summary>
        /// <param name="rawText">The raw text.</param>
        /// <returns>The text with var and lua markup tags evaluated and replaced.</returns>
        public static string ParseCode(string rawText)
        {
            string text = rawText ?? string.Empty;
            if (text.Contains("["))
            {
                ReplaceLuaTags(ref text);
                ReplaceVarTags(ref text);
            }
            return text;
        }

        /// <summary>
        /// Replaces pipe characters with newline characters.
        /// </summary>
        /// <param name="text">Text.</param>
        private static void ReplacePipes(ref string text)
        {
            if (text.Contains("|")) text = text.Replace("|", "\n");
        }

        /// <summary>
        /// Replaces the [lua(xxx)] tags with the result of running xxx through Lua. For brevity,
        /// xxx can omit the return statement; this method will add it.
        /// </summary>
        /// <param name='text'>
        /// Text with lua tags replaced.
        /// </param>
        private static void ReplaceLuaTags(ref string text)
        {
            const string luaTagStart = "[lua(";
            const int maxReplacements = 100;
            if (text.Contains(luaTagStart))
            {
                Regex regex = new Regex(@"\[lua\((?!lua).*\)\]"); //---Was: new Regex(@"\[lua\(.*\)\]");
                int endPosition = text.Length - 1;
                int numReplacements = 0; // Sanity check to prevent infinite loops in case of bug.
                while ((endPosition >= 0) && (numReplacements < maxReplacements))
                {
                    numReplacements++;
                    int luaTagPosition = text.LastIndexOf(luaTagStart, endPosition, System.StringComparison.OrdinalIgnoreCase);
                    endPosition = luaTagPosition - 1;
                    if (luaTagPosition >= 0)
                    {
                        string firstPart = text.Substring(0, luaTagPosition);
                        string secondPart = text.Substring(luaTagPosition);
                        string secondPartLuaReplaced = regex.Replace(secondPart, delegate (Match match)
                        {
                            string luaCode = match.Value.Substring(5, match.Value.Length - 7).Trim(); // Remove "[lua(" and ")]"
                            if (!luaCode.StartsWith("return ")) luaCode = "return " + luaCode;
                            try
                            {
                                return Lua.Run(luaCode, DialogueDebug.logInfo).asString;
                            }
                            catch (System.Exception)
                            {
                                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Lua failed: '{1}'", new System.Object[] { DialogueDebug.Prefix, luaCode }));
                                return string.Empty;
                            }
                        });
                        text = firstPart + secondPartLuaReplaced;
                    }
                }
            }
        }

        /// <summary>
        /// Replaces the [var=varName] tags with the value of the Lua variable varName.
        /// </summary>
        /// <param name='text'>
        /// Text with var tags replaced.
        /// </param>
        private static void ReplaceVarTags(ref string text)
        {
            const string varTagStart = "[var=";
            const int maxReplacements = 100;
            if (text.Contains(varTagStart))
            {
                // Match "[var=" and then anything up to "]":
                Regex regex = new Regex(@"\[var=[^\]]*\]");
                int endPosition = text.Length - 1;
                int numReplacements = 0; // Sanity check to prevent infinite loops in case of bug.
                while ((endPosition >= 0) && (numReplacements < maxReplacements))
                {
                    numReplacements++;
                    int varTagPosition = text.LastIndexOf(varTagStart, endPosition, System.StringComparison.OrdinalIgnoreCase);
                    endPosition = varTagPosition - 1;
                    if (varTagPosition >= 0)
                    {
                        string firstPart = text.Substring(0, varTagPosition);
                        string secondPart = text.Substring(varTagPosition);
                        string secondPartVarReplaced = regex.Replace(secondPart, delegate (Match match)
                        {
                            string varName = match.Value.Substring(5, match.Value.Length - 6).Trim(); // Remove "[var=" and "]"
                            try
                            {
                                return DialogueLua.GetVariable(varName).asString;
                            }
                            catch (System.Exception)
                            {
                                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Failed to get variable: '{1}'", new System.Object[] { DialogueDebug.Prefix, varName }));
                                return string.Empty;
                            }
                        });
                        text = firstPart + secondPartVarReplaced;
                    }
                }
            }
        }

        /// <summary>
        /// Replaces the [autocase=varName] tags with the value of the Lua variable varName, 
        /// capitalized if at the beginning of the string or after end-of-sentence punctuation
        /// and optional whitespace; lowercase otherwise.
        /// </summary>
        /// <param name='text'>
        /// Text with autocase tags replaced.
        /// </param>
        private static void ReplaceAutocaseTags(ref string text)
        {
            const string autocaseTagStart = "[autocase=";
            const int maxReplacements = 100;
            if (text.Contains(autocaseTagStart))
            {
                // Match "[autocase=" and then anything up to "]":
                Regex regex = new Regex(@"\[autocase=[^\]]*\]");
                int endPosition = text.Length - 1;
                int numReplacements = 0; // Sanity check to prevent infinite loops in case of bug.
                while ((endPosition >= 0) && (numReplacements < maxReplacements))
                {
                    numReplacements++;
                    int varTagPosition = text.LastIndexOf(autocaseTagStart, endPosition, System.StringComparison.OrdinalIgnoreCase);
                    endPosition = varTagPosition - 1;
                    if (varTagPosition >= 0)
                    {
                        string firstPart = text.Substring(0, varTagPosition);
                        bool capitalize = ShouldCapitalizeNextChar(firstPart);
                        string secondPart = text.Substring(varTagPosition);
                        string secondPartVarReplaced = regex.Replace(secondPart, delegate (Match match)
                        {
                            string varName = match.Value.Substring(10, match.Value.Length - 11).Trim(); // Remove "[autocase=" and "]"
                            try
                            {
                                var variableValue = DialogueLua.GetVariable(varName).asString;
                                if (variableValue.Length > 0)
                                {
                                    variableValue = SetCapitalization(capitalize, variableValue);
                                }
                                return variableValue;
                            }
                            catch (System.Exception)
                            {
                                if (DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: Failed to get variable: '{1}'", new System.Object[] { DialogueDebug.Prefix, varName }));
                                return string.Empty;
                            }
                        });
                        text = firstPart + secondPartVarReplaced;
                    }
                }
            }
        }

        private static bool ShouldCapitalizeNextChar(string s)
        {
            if (string.IsNullOrEmpty(s)) return true;
            for (int i = s.Length - 1; i >= 0; i--)
            {
                var c = s[i];
                if (c == ' ' || c == '\n') continue;
                return (c == '.' || c == '!' || c == '?');
            }
            return true;
        }

        private static string SetCapitalization(bool capitalize, string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            else if (capitalize) return FirstLetterToUpper(s);
            else return FirstLetterToLower(s);
        }

        private static string FirstLetterToUpper(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length == 1) return s.ToUpper();
            else return char.ToUpper(s[0]) + s.Substring(1);
        }

        private static string FirstLetterToLower(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length == 1) return s.ToUpper();
            else return char.ToLower(s[0]) + s.Substring(1);
        }

        /// <summary>
        /// Extracts a [var=?varName] tag if it exists, and returns the varName.
        /// </summary>
        /// <param name='text'>
        /// Text with [var=?varName] tag extracted.
        /// </param>
        private static string ExtractVariableInputPrompt(ref string text)
        {
            const string varTagStart = "[var=?";
            const int maxReplacements = 100;
            string varName = string.Empty;
            if (text.Contains(varTagStart))
            {

                Regex regex = new Regex(@"\[var=\?.*\]");
                int endPosition = text.Length - 1;
                int numReplacements = 0; // Sanity check to prevent infinite loops in case of bug.
                while ((endPosition >= 0) && (numReplacements < maxReplacements))
                {
                    numReplacements++;
                    int varTagPosition = text.LastIndexOf(varTagStart, endPosition, System.StringComparison.OrdinalIgnoreCase);
                    endPosition = varTagPosition - 1;
                    if (varTagPosition >= 0)
                    {
                        string firstPart = text.Substring(0, varTagPosition);
                        string secondPart = text.Substring(varTagPosition);
                        string secondPartVarReplaced = regex.Replace(secondPart, delegate (Match match)
                        {
                            varName = match.Value.Substring(6, match.Value.Length - 7).Trim(); // Remove "[var=?" and "]"
                            return string.Empty;
                        });
                        text = firstPart + secondPartVarReplaced;
                    }
                }
            }
            return varName;
        }

        /// <summary>
        /// Extracts all instances of a tag from a string.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the tag was found; otherwise <c>false</c>.
        /// </returns>
        /// <param name='tag'>
        /// The tag to look for.
        /// </param>
        /// <param name='text'>
        /// The text to extract tags from.
        /// </param>
        private static bool ExtractTag(string tag, ref string text)
        {
            bool found = text.Contains(tag);
            if (found) text = text.Replace(tag, string.Empty);
            return found;
        }

        /// <summary>
        /// Extracts a [position #] or [position=#] tag from a string. Removes all position tags and returns the value of the last one.
        /// </summary>
        /// <returns>
        /// The position number.
        /// </returns>
        /// <param name='text'>
        /// The text to extract tags from.
        /// </param>
        private static int ExtractPositionTag(ref string text)
        {
            int position = FormattedText.NoAssignedPosition;
            if (text.Contains("[position "))
            {
                Regex regex = new Regex(@"\[position\s+[0-9]+\]");
                text = regex.Replace(text, delegate (Match match)
                {
                    string positionString = match.Value.Substring(10, match.Value.Length - 11); // Remove "[position " and "]"
                    int.TryParse(positionString, out position);
                    return string.Empty;
                });
            }
            if (text.Contains("[position="))
            {
                Regex regex = new Regex(@"\[position=[0-9]+\]");
                text = regex.Replace(text, delegate (Match match)
                {
                    string positionString = match.Value.Substring(10, match.Value.Length - 11); // Remove "[position=" and "]"
                    int.TryParse(positionString, out position);
                    return string.Empty;
                });
            }
            return position;
        }

        /// <summary>
        /// Extracts a [panel=#] tag from a string. Removes all panel tags and returns the value of the last one.
        /// </summary>
        /// <returns>
        /// The position number.
        /// </returns>
        /// <param name='text'>
        /// The text to extract tags from.
        /// </param>
        private static int ExtractPanelNumberTag(ref string text)
        {
            int panelNumber = -1;
            if (text.Contains("[panel="))
            {
                Regex regex = new Regex(@"\[panel=[0-9]+\]");
                text = regex.Replace(text, delegate (Match match)
                {
                    string s = match.Value.Substring(7, match.Value.Length - 8); // Remove "[panel=" and "]"
                    int.TryParse(s, out panelNumber);
                    return string.Empty;
                });
            }
            return panelNumber;
        }

        /// <summary>
        /// Extracts pic tag indices.
        /// </summary>
        /// <returns>The pic tag index, or NoPicOverride if no applicable tag is in the text.</returns>
        /// <param name="tagName">Tag regex (e.g., "[pic=".</param>
        /// <param name="text">Text.</param>
        private static int ExtractPicTag(string tagRegex, ref string text)
        {
            int index = FormattedText.NoPicOverride;
            Regex regex = new Regex(tagRegex);
            text = regex.Replace(text, delegate (Match match)
            {
                int startPos = match.Value.IndexOf('=') + 1;
                string indexString = match.Value.Substring(startPos, match.Value.Length - (startPos + 1)); // Remove "[pic[ac]=" and "]"
                if (!int.TryParse(indexString, out index))
                {
                    index = DialogueLua.GetVariable(indexString).asInt;
                }
                return string.Empty;
            });
            return index;
        }

        /// <summary>
        /// Extracts the emphasis tags.
        /// </summary>
        /// <returns>
        /// An array of emphases.
        /// </returns>
        /// <param name='text'>
        /// Text (returned without the emphasis tags).
        /// </param>
        /// <param name='emphasisSettings'>
        /// Emphasis settings to use when parsing an emphasis tag.
        /// </param>
        /// @todo Apply emphasis to specified substring. Currently returns a max of one emphasis, which covers the whole text.
        private static Emphasis[] ExtractEmphasisTags(ref string text, EmphasisSetting[] emphasisSettings)
        {
            List<Emphasis> emphases = new List<Emphasis>();
            if (text.Contains("[em"))
            {
                Regex regex = new Regex(@"\[\/?em[1-" + emphasisSettings.Length + @"]\]");
                text = regex.Replace(text, delegate (Match match)
                {
                    string numString = match.Value.Substring(match.Value.Length - 2, 1); // Get # from "[em#]"
                    int num = 1;
                    int.TryParse(numString, out num);
                    num--;
                    if ((emphasisSettings != null) && (0 <= num) && (num < emphasisSettings.Length))
                    {
                        Emphasis emphasis = new Emphasis(0, int.MaxValue, emphasisSettings[num].color, emphasisSettings[num].bold, emphasisSettings[num].italic, emphasisSettings[num].underline);
                        emphases.Clear();
                        emphases.Add(emphasis);
                    }
                    return string.Empty;
                });
            }
            return emphases.ToArray();
        }

        /// <summary>
        /// Replaces the emphasis tags with rich text, and returns an empty set of emphases.
        /// </summary>
        /// <returns>
        /// An empty set of emphases, since the rich text codes are embedded in the text itself.
        /// </returns>
        /// <param name='text'>
        /// The text, with emphasis tags replaced by rich text codes.
        /// </param>
        /// <param name='emphasisSettings'>
        /// Emphasis settings.
        /// </param>
        private static Emphasis[] ReplaceEmphasisTagsWithRichText(ref string text, EmphasisSetting[] emphasisSettings)
        {
            if (text.Contains("[em"))
            {
                for (int i = 0; i < emphasisSettings.Length; i++)
                {
                    string openTag = string.Format("[em{0}]", new System.Object[] { i + 1 });
                    string closeTag = string.Format("[/em{0}]", new System.Object[] { i + 1 });
                    if (text.Contains(openTag))
                    {
                        string openRichText = string.Format("{0}{1}{2}<color={3}>",
                            new System.Object[]
                            {
                                emphasisSettings[i].bold ? "<b>" : string.Empty,
                                emphasisSettings[i].italic ? "<i>" : string.Empty,
#if TMP_PRESENT || USE_STM
                                emphasisSettings[i].underline ? "<u>" : string.Empty,
#else
                                string.Empty,
#endif
                                Tools.ToWebColor(emphasisSettings[i].color)
                            });
                        string closeRichText = string.Format("</color>{0}{1}{2}",
                            new System.Object[]
                            {
#if TMP_PRESENT || USE_STM
                                emphasisSettings[i].underline? "</u>" : string.Empty,
#else
                                string.Empty,
#endif
                                emphasisSettings[i].italic ? "</i>" : string.Empty,
                                emphasisSettings[i].bold ? "</b>" : string.Empty
                            });
                        text = text.Replace(openTag, openRichText).Replace(closeTag, closeRichText);
                    }
                }
            }
            return new Emphasis[0];
        }

        /// <summary>
        /// Gets the Unity FonyStyle represented by an emphasis.
        /// </summary>
        /// <returns>
        /// The font style.
        /// </returns>
        /// <param name='emphasis'>
        /// Emphasis.
        /// </param>
        public static FontStyle GetFontStyle(Emphasis emphasis)
        {
            if (emphasis.bold && emphasis.italic)
            {
                return FontStyle.BoldAndItalic;
            }
            else if (emphasis.bold)
            {
                return FontStyle.Bold;
            }
            else if (emphasis.italic)
            {
                return FontStyle.Italic;
            }
            else
            {
                return FontStyle.Normal;
            }
        }

    }

}
