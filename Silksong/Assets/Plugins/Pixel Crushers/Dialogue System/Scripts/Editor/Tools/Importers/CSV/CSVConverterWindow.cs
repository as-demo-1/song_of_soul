// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is an example converter that demonstrates how to make a subclass of 
    /// AbstractConverterWindow to make your own converter for the Dialogue System.
    /// It converts CSV (comma-separated value) files in a specific format into a 
    /// dialogue database.
    /// 
    /// In the CSV file, each section is optional. The sections are:
    /// 
    /// - Database
    /// - Actors
    /// - Items (also used for quests)
    /// - Locations
    /// - Variables
    /// - Conversations (high level information about each conversation)
    /// - DialogueEntries (individual dialogue entries in the conversations)
    /// - OutgoingLinks (links between dialogue entries)
    /// 
    /// The Database section must contain:
    /// <pre>
    /// Database
    /// Name,Version,Author,Description,Emphasis1,Emphasis2,Emphasis3,Emphasis4
    /// (name),(version),(author),(description),(emphasis setting 1 in format #rrggbbaa biu),(emphasis2),(emphasis3),(emphasis4)
    /// Global User Script
    /// (luacode)
    /// </pre>
    /// 
    /// The Actors section must contain:
    /// <pre>
    /// Actors
    /// ID,Portrait,AltPortraits,SpritePortrait,AltSpritePortraits,Name,Pictures,Description,IsPlayer
    /// Number,Special,Special,Special,SpecialText,Files,Text,Boolean
    /// (id),(texturename),[(texturenames)],(name),[(picturenames)],(description),(isplayer)
    /// ...
    /// </pre>
    /// 
    /// The Items, Locations, Variables, and Conversations section must contain:
    /// <pre>
    /// (Heading) -- where this is Items, Locations, Variables, or Conversations
    /// ID,(field),(field),(field)...
    /// Number,(fieldtype),(fieldtype),(fieldtype)...
    /// (id),(fieldvalue),(fieldvalue),(fieldvalue)...
    /// </pre>
    /// The Variables section may have a final column InitialValueType that specifies the type (Text, Number, or Boolean).
    /// 
    /// The DialogueEntries section must contain:
    /// <pre>
    /// DialogueEntries
    /// entrytag,ConvID,ID,Actor,Conversant,MenuText,DialogueText,IsGroup,FalseConditionAction,ConditionPriority,Conditions,Script,Sequence,(field),(field)...,canvasRect
    /// Text,Number,Number,Number,Number,Text,Text,Boolean,Special,Special,Text,Text,Text,(fieldtype),(fieldtype),...,Text
    /// (entrytag),(ConvID),(ID),(ActorID),(ConversantID),(MenuText),(DialogueText),(IsGroup),(FalseConditionAction),(ConditionPriority),(Conditions),(Script),(Sequence),(fieldvalue),(fieldvalue)...,#;#
    /// </pre>
    /// However canvasRect (the last column) is optional.
    /// 
    /// The OutgoingLinks section must contain:
    /// <pre>
    /// OutgoingLinks
    /// OriginConvID,OriginID,DestConvID,DestID,ConditionPriority
    /// Number,Number,Number,Number,Special
    /// #,#,#,#,(priority)
    /// </pre>
    /// 
    /// Omitted values in any particular asset should be tagged with "{{omit}}".
    /// </summary>
    public class CSVConverterWindow : AbstractConverterWindow<AbstractConverterWindowPrefs>
    {

        /// <summary>
        /// Gets the source file extension (CSV).
        /// </summary>
        /// <value>The source file extension (e.g., 'xml' for XML files).</value>
        public override string sourceFileExtension { get { return "csv"; } }

        /// <summary>
        /// Gets the EditorPrefs key to save the converter window's settings under.
        /// </summary>
        /// <value>The EditorPrefs key.</value>
        public override string prefsKey { get { return "PixelCrushers.DialogueSystem.CSVConverterSettings"; } }

        /// <summary>
        /// Menu item code to create a CSVConverterWindow.
        /// </summary>
        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/CSV...", false, 1)]
        public static void Init()
        {
            EditorWindow.GetWindow(typeof(CSVConverterWindow), false, "CSV Import");
        }

        /// <summary>
        /// A list of all asset type headings.
        /// </summary>
        private static List<string> AssetTypeHeadings = new List<string>()
        { "Database", "Actors", "Items", "Locations", "Variables", "Conversations", "DialogueEntries", "OutgoingLinks" };

        /// <summary>
        /// Special values aren't actually fields in an asset's Field array, but they're still
        /// columns in the CSV that must be read and assigned to the asset's variables.
        /// </summary>
        private static List<string> DefaultSpecialValues = new List<string>()
        { "ID", "InitialValueType" };

        /// <summary>
        /// Portrait and AltPortraits are variables in the Actor class, not fields.
        /// </summary>
        private static List<string> ActorSpecialValues = new List<string>()
        { "ID", "Portrait", "AltPortraits", "SpritePortrait", "AltSpritePortraits" };

        /// <summary>
        /// The exporter manually places these columns at the front of dialogue entry rows, and the
        /// importer reads them in the order that they were exported. Some of them are special 
        /// variables of the DialogueEntry class and not actually fields. MenuText and DialogueText 
        /// are fields, but the exporter puts them with this front section to make them more accessible
        /// to people editing the CSV file.
        /// </summary>
        private static List<string> DialogueEntrySpecialValues = new List<string>()
        { "entrytag", "ConvID", "ID", "Actor", "Conversant", "Title", "MenuText", "DialogueText",
            "IsGroup", "FalseConditionAction", "ConditionPriority", "Conditions", "Script" };

        private static bool sortByID = true;

        private int numWarnings;

        /// <summary>
        /// Draws the destination section. You can override this if you want to draw
        /// more than the default controls.
        /// </summary>
        protected override void DrawDestinationSection()
        {
            base.DrawDestinationSection();
            EditorWindowTools.StartIndentedSection();
            sortByID = EditorGUILayout.Toggle("Sort By ID", sortByID);
            EditorWindowTools.EndIndentedSection();
        }

        private const int MaxIterations = 999999;

        /// <summary>
        /// Copies the source CSV file to a dialogue database. This method demonstrates 
        /// the helper methods LoadSourceFile(), IsSourceAtEnd(), PeekNextSourceLine(), 
        /// and GetNextSourceLine().
        /// </summary>
        /// <param name="database">Dialogue database to copy into.</param>
        protected override void CopySourceToDialogueDatabase(DialogueDatabase database)
        {
            Debug.Log("Dialogue System CSV Importer: Copying source to dialogue database.");
            numWarnings = 0;
            var hadError = false;
            var readConversations = false;
            var readDialogueEntries = false;
            try
            {
                try
                {
                    LoadSourceFile();
                    int numLines = sourceLines.Count;
                    int safeguard = 0;
                    bool cancel = false;
                    while (!IsSourceAtEnd() && (safeguard < MaxIterations) && !cancel)
                    {
                        safeguard++;
                        cancel = EditorUtility.DisplayCancelableProgressBar("Importing CSV", "Please wait...", (float)sourceLines.Count / (float)numLines);
                        string line = GetNextSourceLine();
                        if (string.Equals(GetFirstField(line), "Database"))
                        {
                            ReadDatabaseProperties(database);
                        }
                        else if (string.Equals(GetFirstField(line), "Actors"))
                        {
                            ReadAssets<Actor>(database.actors, true);
                        }
                        else if (string.Equals(GetFirstField(line), "Items"))
                        {
                            ReadAssets<Item>(database.items, true);
                        }
                        else if (string.Equals(GetFirstField(line), "Locations"))
                        {
                            ReadAssets<Location>(database.locations, true);
                        }
                        else if (string.Equals(GetFirstField(line), "Variables"))
                        {
                            ReadAssets<Variable>(database.variables, true);
                        }
                        else if (string.Equals(GetFirstField(line), "Conversations"))
                        {
                            readConversations = true;
                            ReadAssets<Conversation>(database.conversations, true);
                        }
                        else if (string.Equals(GetFirstField(line), "DialogueEntries"))
                        {
                            ReadDialogueEntries(database, readConversations);
                            readDialogueEntries = readConversations;
                        }
                        else if (string.Equals(GetFirstField(line), "OutgoingLinks"))
                        {
                            ReadOutgoingLinks(database, readConversations && readDialogueEntries);
                        }
                        else {
                            throw new InvalidDataException("Line not recognized: " + line);
                        }
                    }

                    // If we skipped dialogue entries, we need to re-read them now:
                    if (!readDialogueEntries)
                    {
                        Debug.Log("Dialogue System CSV Importer: Conversations section was after DialogueEntries section. Going back to read DialogueEntries now...");
                        LoadSourceFile();
                        safeguard = 0;
                        while (!IsSourceAtEnd() && (safeguard < MaxIterations) && !cancel)
                        {
                            safeguard++;
                            cancel = EditorUtility.DisplayCancelableProgressBar("Importing CSV", "Importing dialogue entries...", (float)sourceLines.Count / (float)numLines);
                            string line = GetNextSourceLine();
                            if (string.Equals(GetFirstField(line), "Database"))
                            {
                                ReadDatabaseProperties(database);
                            }
                            else if (string.Equals(GetFirstField(line), "Actors"))
                            {
                                ReadAssets<Actor>(database.actors, false);
                            }
                            else if (string.Equals(GetFirstField(line), "Items"))
                            {
                                ReadAssets<Item>(database.items, false);
                            }
                            else if (string.Equals(GetFirstField(line), "Locations"))
                            {
                                ReadAssets<Location>(database.locations, false);
                            }
                            else if (string.Equals(GetFirstField(line), "Variables"))
                            {
                                ReadAssets<Variable>(database.variables, false);
                            }
                            else if (string.Equals(GetFirstField(line), "Conversations"))
                            {
                                ReadAssets<Conversation>(database.conversations, false);
                            }
                            else if (string.Equals(GetFirstField(line), "DialogueEntries"))
                            {
                                ReadDialogueEntries(database, true);
                            }
                            else if (string.Equals(GetFirstField(line), "OutgoingLinks"))
                            {
                                ReadOutgoingLinks(database, true);
                            }
                            else {
                                throw new InvalidDataException("Line not recognized: " + line);
                            }
                        }
                    }
                    if (sortByID) SortAssetsByID(database);
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("Dialogue System CSV Importer: CSV import failed: {1}\nLine {2}: {3}", DialogueDebug.Prefix, e.Message, currentLineNumber, currentSourceLine));
                    hadError = true;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (hadError)
                {
                    var message = "Dialogue System CSV Importer: There was a fatal error that prevented import of " + prefs.sourceFilename + ".";
                    Debug.LogError(message);
                    EditorUtility.DisplayDialog("CSV Import Error", message, "OK");
                }
                else if (numWarnings > 0)
                {
                    var message = "Dialogue System CSV Importer: There were " + numWarnings + " warnings during import of " + prefs.sourceFilename + ".";
                    Debug.LogWarning(message);
                    EditorUtility.DisplayDialog("CSV Import Warnings", message, "OK");
                }
            }
        }

        /// <summary>
        /// Reads the database properties section.
        /// </summary>
        /// <param name="database">Dialogue database.</param>
        private void ReadDatabaseProperties(DialogueDatabase database)
        {
            Debug.Log("Dialogue System CSV Importer: Reading database properties");
            GetNextSourceLine(); // Field headings
            string[] values = GetValues(GetNextSourceLine());
            if (values.Length < 8) throw new IndexOutOfRangeException("Incorrect number of values in database properties line");
            database.name = values[0];
            database.version = values[1];
            database.author = values[2];
            database.description = values[3];
            database.emphasisSettings[0] = UnwrapEmField(values[4]);
            database.emphasisSettings[1] = UnwrapEmField(values[5]);
            database.emphasisSettings[2] = UnwrapEmField(values[6]);
            database.emphasisSettings[3] = UnwrapEmField(values[7]);
            GetNextSourceLine(); // Global User Script heading
            database.globalUserScript = UnwrapValue(GetNextSourceLine());
            // Some CSV exports may cause malformed global user script:
            database.globalUserScript = RemoveTrailingCommas(database.globalUserScript);
        }

        private string RemoveTrailingCommas(string s)
        {
            int safeguard = 0;
            while (!string.IsNullOrEmpty(s) && s.EndsWith(",") && safeguard < 10000)
            {
                safeguard++;
                s = s.Remove(s.Length - 1);
            }
            return s;
        }

        /// <summary>
        /// Reads a section of assets such as Actors, Items, etc.
        /// </summary>
        /// <param name="assets">List of assets to populate.</param>
        /// <typeparam name="T">The type of asset.</typeparam>
        private void ReadAssets<T>(List<T> assets, bool add) where T : Asset, new()
        {
            string typeName = typeof(T).Name;
            bool isActorSection = (typeof(T) == typeof(Actor));
            Debug.Log(string.Format("Dialogue System CSV Importer: {0} {1} section", (add ? "Reading" : "Skipping"), typeName));

            // Read field names and types:
            string[] fieldNames = GetValues(GetNextSourceLine());
            string[] fieldTypes = GetValues(GetNextSourceLine());

            // Set up ignore list for values that aren't actual fields:
            List<string> ignore = isActorSection ? ActorSpecialValues : DefaultSpecialValues;

            // Keep reading until we reach another asset type heading or end of file:
            int safeguard = 0;
            while (!(IsSourceAtEnd() || AssetTypeHeadings.Contains(GetFirstField(PeekNextSourceLine()))))
            {
                safeguard++;
                if (safeguard > MaxIterations) break;

                string[] values = GetValues(GetNextSourceLine());

                if (add)
                {
                    // Create the asset:
                    T asset = new T();
                    asset.id = Tools.StringToInt(values[0]);
                    asset.fields = new List<Field>();

                    // Preprocess a couple extra values for actors:
                    if (isActorSection) FindActorPortraits(asset as Actor, values[1], values[2], values[3], values[4]);

                    // Read the remaining values and assign them to the asset's fields:
                    ReadAssetFields(fieldNames, fieldTypes, ignore, values, asset.fields, asset);

                    // If the database already has an old asset with the same ID, delete it first:
                    assets.RemoveAll(a => a.id == asset.id);

                    // Finally, add the asset:
                    assets.Add(asset);
                }
            }
        }

        /// <summary>
        /// Reads the asset fields.
        /// </summary>
        /// <param name="fieldNames">Field names.</param>
        /// <param name="fieldTypes">Field types.</param>
        /// <param name="ignore">List of field names to not add.</param>
        /// <param name="values">CSV values.</param>
        /// <param name="fields">Fields list of populate.</param>
        private void ReadAssetFields(string[] fieldNames, string[] fieldTypes, List<string> ignore,
                                     string[] values, List<Field> fields, Asset asset)
        {
            // Look for a special field named "InitialValueType" used in Variables section:
            var isInitialValueTypeKnown = false;
            var initialValueType = FieldType.Text;
            for (int i = 0; i < fieldNames.Length; i++)
            {
                if (string.Equals(fieldNames[i], "InitialValueType"))
                {
                    initialValueType = Field.StringToFieldType(values[i]);
                    isInitialValueTypeKnown = true;
                    break;
                }
            }

            // Convert all fields:
            for (int i = 0; i < fieldNames.Length; i++)
            {
                if ((ignore != null) && ignore.Contains(fieldNames[i])) continue;
                if (string.Equals(values[i], "{{omit}}")) continue;
                if (string.IsNullOrEmpty(fieldNames[i])) continue;
                string title = fieldNames[i];
                string value = values[i];

                // Special handling for conversation overrides:
                if (asset is Conversation && title == "Overrides")
                {
                    var overrideSettings = JsonUtility.FromJson<ConversationOverrideDisplaySettings>(value);
                    if (overrideSettings != null) (asset as Conversation).overrideSettings = overrideSettings;
                }
                else
                {
                    // Special handling required for Initial Value of Variables section:
                    FieldType type = (string.Equals(title, "Initial Value") && isInitialValueTypeKnown) ? initialValueType : GuessFieldType(value, fieldTypes[i]);
                    fields.Add(new Field(title, value, type));
                }
            }
        }

        private void FindActorPortraits(Actor actor, string portraitName, string alternatePortraitNames, string spritePortraitName, string altSpritePortraitNames)
        {
            // Texture2D portraits:
            if (!string.IsNullOrEmpty(portraitName))
            {
                actor.portrait = AssetDatabase.LoadAssetAtPath(portraitName, typeof(Texture2D)) as Texture2D;
            }
            if (!(string.IsNullOrEmpty(alternatePortraitNames) || string.Equals(alternatePortraitNames, "[]")))
            {
                var inner = alternatePortraitNames.Substring(1, alternatePortraitNames.Length - 2);
                var names = inner.Split(new char[] { ';' });
                if (actor.alternatePortraits == null) actor.alternatePortraits = new List<Texture2D>();
                foreach (var altPortraitName in names)
                {
                    var texture = AssetDatabase.LoadAssetAtPath(altPortraitName, typeof(Texture2D)) as Texture2D;
                    if (texture != null)
                    {
                        actor.alternatePortraits.Add(texture);
                    }
                }
            }

            // Sprite portraits:
            if (!string.IsNullOrEmpty(spritePortraitName))
            {
                actor.spritePortrait = AssetDatabase.LoadAssetAtPath(spritePortraitName, typeof(Sprite)) as Sprite;
            }
            if (!(string.IsNullOrEmpty(altSpritePortraitNames) || string.Equals(altSpritePortraitNames, "[]")))
            {
                var inner = altSpritePortraitNames.Substring(1, altSpritePortraitNames.Length - 2);
                var names = inner.Split(new char[] { ';' });
                if (actor.spritePortraits == null) actor.spritePortraits= new List<Sprite>();
                foreach (var altSpritePortraitName in names)
                {
                    var sprite = AssetDatabase.LoadAssetAtPath(altSpritePortraitName, typeof(Sprite)) as Sprite;
                    if (sprite != null)
                    {
                        actor.spritePortraits.Add(sprite);
                    }
                }
            }
        }

        /// <summary>
        /// Reads the DialogueEntries section. DialogueEntry is not a subclass of Asset,
        /// so we can't reuse the ReadAssets() code above.
        /// </summary>
        /// <param name="database">Dialogue database.</param>
        private void ReadDialogueEntries(DialogueDatabase database, bool add)
        {
            Debug.Log("Dialogue System CSV Importer: " + (add ? "Reading" : "Skipping") + " DialogueEntries section");

            // Read field names and types:
            string[] fieldNames = GetValues(GetNextSourceLine());
            string[] fieldTypes = GetValues(GetNextSourceLine());

            // Keep reading until we reach another asset type heading or end of file:
            int safeguard = 0;
            while (!(IsSourceAtEnd() || AssetTypeHeadings.Contains(GetFirstField(PeekNextSourceLine()))))
            {
                safeguard++;
                if (safeguard > MaxIterations) break;

                string[] values = GetValues(GetNextSourceLine());

                if (add)
                {
                    // Create the dialogue entry:
                    DialogueEntry entry = new DialogueEntry();
                    entry.fields = new List<Field>();
                    // We can ignore value[0] (entrytag).
                    entry.conversationID = Tools.StringToInt(values[1]);
                    entry.id = Tools.StringToInt(values[2]);
                    entry.ActorID = Tools.StringToInt(values[3]);
                    entry.ConversantID = Tools.StringToInt(values[4]);
                    entry.Title = values[5];
                    entry.MenuText = values[6];
                    entry.DialogueText = values[7];
                    entry.isGroup = Tools.StringToBool(values[8]);
                    entry.falseConditionAction = values[9];
                    entry.conditionPriority = ConditionPriorityUtility.StringToConditionPriority(values[10]);
                    entry.conditionsString = values[11];
                    entry.userScript = values[12];

                    // Read the remaining values and assign them to the asset's fields:
                    ReadAssetFields(fieldNames, fieldTypes, DialogueEntrySpecialValues, values, entry.fields, null);

                    // Convert canvasRect field to entry position on node editor canvas:
                    entry.UseCanvasRectField();

                    // Finally, add the asset:
                    var conversation = database.GetConversation(entry.conversationID);
                    if (conversation == null) throw new InvalidDataException(string.Format("Conversation {0} referenced in entry {1} not found", entry.conversationID, entry.id));
                    conversation.dialogueEntries.Add(entry);
                }
            }
        }

        /// <summary>
        /// Reads the OutgoingLinks section. Again, Link is not a subclass of Asset,
        /// so we can't reuse the ReadAssets() method.
        /// </summary>
        /// <param name="database">Dialogue database.</param>
        private void ReadOutgoingLinks(DialogueDatabase database, bool add)
        {
            Debug.Log("Dialogue System CSV Importer: " + (add ? "Reading" : "Skipping") + " OutgoingLinks section");
            GetNextSourceLine(); // Headings
            GetNextSourceLine(); // Types

            // Keep reading until we reach another asset type heading or end of file:
            int safeguard = 0;
            while (!(IsSourceAtEnd() || AssetTypeHeadings.Contains(GetFirstField(PeekNextSourceLine()))))
            {
                safeguard++;
                if (safeguard > MaxIterations) break;
                string[] values = GetValues(GetNextSourceLine());

                if (add)
                {
                    var link = new Link(Tools.StringToInt(values[0]), Tools.StringToInt(values[1]),
                                        Tools.StringToInt(values[2]), Tools.StringToInt(values[3]));
                    link.priority = ConditionPriorityUtility.StringToConditionPriority(values[4]);
                    var entry = database.GetDialogueEntry(link.originConversationID, link.originDialogueID);
                    if (entry == null)
                    {
                        Debug.LogWarning(string.Format("Dialogue System: CSV import error: dialogue entry {0}.{1} referenced in an outgoing link was not found.", link.originConversationID, link.originDialogueID));
                        numWarnings++;
                    }
                    else
                    {
                        entry.outgoingLinks.Add(link);
                    }
                }
            }
        }

        protected override void LoadSourceFile()
        {
            try
            {
                base.LoadSourceFile();
                CombineMultilineSourceLines();
            }
            catch (System.IO.IOException e)
            {
                Debug.LogException(e);
                Debug.LogError("You may need to close the CSV file in your spreadsheet application to allow the importer to read it.");
            }
        }

        /// <summary>
        /// Combines lines that are actually a multiline CSV row. This also helps prevent the 
        /// CSV-splitting regex from hanging due to catastrophic backtracking on unterminated quotes.
        /// </summary>
        private void CombineMultilineSourceLines()
        {
            int lineNum = 0;
            int safeguard = 0;
            while ((lineNum < sourceLines.Count) && (safeguard < MaxIterations))
            {
                safeguard++;
                string line = sourceLines[lineNum];
                if (line == null)
                {
                    sourceLines.RemoveAt(lineNum);
                }
                else {
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
                    else {
                        sourceLines[lineNum] = line + "\\n" + sourceLines[lineNum + 1];
                        sourceLines.RemoveAt(lineNum + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the individual comma-separated values in a line.
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="line">Line.</param>
        private string[] GetValues(string line)
        {
            Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);
            List<string> values = new List<string>();
            foreach (Match match in csvSplit.Matches(line))
            {
                values.Add(UnwrapValue(match.Value.TrimStart(',')));
            }
            return values.ToArray();
        }

        private string GetFirstField(string line)
        {
            if (line.Contains(","))
            {
                var values = line.Split(new char[] { ',' });
                return values[0];
            }
            else {
                return line;
            }
        }

        /// <summary>
        /// Returns a "fixed" version of a comma-separated value where escaped newlines
        /// have been converted back into real newlines, and optional surrounding quotes 
        /// have been removed.
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="value">Value.</param>
        private string UnwrapValue(string value)
        {
            string s = value.Replace("\\n", "\n").Replace("\\r", "\r");
            if (s.StartsWith("\"") && s.EndsWith("\""))
            {
                s = s.Substring(1, s.Length - 2).Replace("\"\"", "\"");
            }
            return s;
        }

        /// <summary>
        /// Converts an emphasis field in the format "#rrggbbaa biu" into an EmphasisSetting object.
        /// </summary>
        /// <returns>An EmphasisSetting object.</returns>
        /// <param name="emField">Em field.</param>
        private EmphasisSetting UnwrapEmField(string emField)
        {
            return new EmphasisSetting(emField.Substring(0, 9), emField.Substring(10, 3));
        }

        /// <summary>
        /// The CSV format isn't robust enough to describe if different assets define different
        /// types for the same field name. This method checks if a "Text" field has a Boolean
        /// or Number value and returns that type instead of Text.
        /// </summary>
        /// <returns>The field type.</returns>
        /// <param name="value">Value.</param>
        /// <param name="typeSpecifier">Type specifier.</param>
        private FieldType GuessFieldType(string value, string typeSpecifier)
        {
            if (string.Equals(typeSpecifier, "Text") && !string.IsNullOrEmpty(value))
            {
                if (IsBoolean(value))
                {
                    return FieldType.Boolean;
                }
                else if (IsNumber(value))
                {
                    return FieldType.Number;
                }
            }
            if (string.Equals(typeSpecifier, "Special")) return FieldType.Text;
            return Field.StringToFieldType(typeSpecifier);
        }

        /// <summary>
        /// Determines whether a string represents a Boolean value.
        /// </summary>
        /// <returns><c>true</c> if this is a Boolean value; otherwise, <c>false</c>.</returns>
        /// <param name="value">String value.</param>
        private bool IsBoolean(string value)
        {
            return ((string.Compare(value, "True", System.StringComparison.OrdinalIgnoreCase) == 0) ||
                    (string.Compare(value, "False", System.StringComparison.OrdinalIgnoreCase) == 0));
        }

        /// <summary>
        /// Determines whether a string represents a Number value.
        /// </summary>
        /// <returns><c>true</c> if this is a number; otherwise, <c>false</c>.</returns>
        /// <param name="value">String value.</param>
        private bool IsNumber(string value)
        {
            float n;
            return float.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out n);
        }

        private void SortAssetsByID(DialogueDatabase database)
        {
            if (database == null) return;
            database.actors.Sort((x, y) => x.id.CompareTo(y.id));
            database.items.Sort((x, y) => x.id.CompareTo(y.id));
            database.locations.Sort((x, y) => x.id.CompareTo(y.id));
            database.variables.Sort((x, y) => x.id.CompareTo(y.id));
            database.conversations.Sort((x, y) => x.id.CompareTo(y.id));
            foreach (var conversation in database.conversations)
            {
                conversation.dialogueEntries.Sort((x, y) => x.id.CompareTo(y.id));
            }
        }

    }

}
