// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Custom inspector editor for localized text tables.
    /// </summary>
    [CustomEditor(typeof(LocalizedTextTable), true)]
    public class LocalizedTextTableEditor : Editor
    {

        #region Variables

        /// <summary>
        /// Is the languages foldout open?
        /// </summary>
        [SerializeField]
        private bool languagesFoldout = false;

        /// <summary>
        /// Is the fields foldout open?
        /// </summary>
        [SerializeField]
        private bool fieldsFoldout = true;

        /// <summary>
        /// Tracks which individual fields are open.
        /// </summary>
        private static Dictionary<int, bool> fieldFoldouts = new Dictionary<int, bool>();

        /// <summary>
        /// The localized text table that we're currently editing.
        /// </summary>
        private LocalizedTextTable table = null;

        /// <summary>
        /// The filename to use when importing and exporting CSV.
        /// </summary>
        private static string csvFilename = string.Empty;

        /// <summary>
        /// The base filename to use when exporting language text.
        /// </summary>
        private static string languageDumpBaseFilename = string.Empty;

        private static EncodingType encodingType = EncodingType.UTF8;
        private bool addFieldsAtTop = false;

        private static bool showSearchBar = false;

        private string searchString = string.Empty;
        private bool searchCaseSensitive = false;
        private int currentSearchResultIndex = 0;
        private int currentSearchResultValueIndex = -1;
        private bool needToFocusOnSearchResult = false;

        private const string EncodingTypeEditorPrefsKey = "PixelCrushers.DialogueSystem.EncodingType";
        private const string AddFieldsAtTopPrefsKey = "PixelCrushers.DialogueSystem.LocalizedTextTable.AddAtTop";
        private const string LanguagesFoldoutPrefsKey = "PixelCrushers.DialogueSystem.LocalizedTextTable.LanguagesFoldout";
        private const string FieldsFoldoutPrefsKey = "PixelCrushers.DialogueSystem.LocalizedTextTable.FieldsFoldout";
        private const string ShowSearchBarPrefsKey = "PixelCrushers.DialogueSystem.LocalizedTextTable.ShowSearchBar";
        private const string SearchStringPrefsKey = "PixelCrushers.DialogueSystem.LocalizedTextTable.SearchString";
        private const string SearchCaseSensitivePrefsKey = "PixelCrushers.DialogueSystem.LocalizedTextTable.SearchCaseSensitive";

        #endregion

        #region Main

        public void OnEnable()
        {
            encodingType = (EncodingType)EditorPrefs.GetInt(EncodingTypeEditorPrefsKey, (int)EncodingType.UTF8);
            addFieldsAtTop = EditorPrefs.GetBool(AddFieldsAtTopPrefsKey, false);
            languagesFoldout = EditorPrefs.GetBool(LanguagesFoldoutPrefsKey, true);
            fieldsFoldout = EditorPrefs.GetBool(FieldsFoldoutPrefsKey, true);
            showSearchBar = EditorPrefs.GetBool(ShowSearchBarPrefsKey, false);
            searchCaseSensitive = EditorPrefs.GetBool(SearchCaseSensitivePrefsKey, false);
            searchString = EditorPrefs.GetString(SearchStringPrefsKey, string.Empty);
        }

        public void OnDisable()
        {
            EditorPrefs.SetBool(LanguagesFoldoutPrefsKey, languagesFoldout);
            EditorPrefs.SetBool(FieldsFoldoutPrefsKey, fieldsFoldout);
            EditorPrefs.SetBool(SearchCaseSensitivePrefsKey, searchCaseSensitive);
            EditorPrefs.SetString(SearchStringPrefsKey, searchString);
        }

        /// <summary>
        /// Draws our custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            table = target as LocalizedTextTable;
            if (table == null) return;
            EditorGUILayout.HelpBox("Localized Text Tables have been superceded by Text Tables. To save the contents of this Localized Text Table as a Text Table, click the 'Save As Text Table...' button below.", MessageType.Info);
            if (GUILayout.Button("Save As Text Table..."))
            {
                SaveAsTextTable();
            }
            else
            {
                DrawLanguages();
                EditorWindowTools.DrawHorizontalLine();
                if (showSearchBar)
                {
                    DrawSearchBar();
                    EditorWindowTools.DrawHorizontalLine();
                }
                DrawFields();
                if (GUI.changed) EditorUtility.SetDirty(table);
            }
        }

        #endregion

        #region Menu

        private void DrawMenu()
        {
            if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56)))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Sort"), false, SortLanguages);
                menu.AddItem(new GUIContent("Add Fields At Top"), addFieldsAtTop, ToggleAddFieldsAtTop);
                menu.AddItem(new GUIContent("Search Bar"), showSearchBar, ToggleSearchBar);
                menu.AddItem(new GUIContent("Import..."), false, Import);
                menu.AddItem(new GUIContent("Export..."), false, Export);
                menu.AddItem(new GUIContent("Export Separate Languages..."), false, ExportSeparateLanguages);
                menu.AddItem(new GUIContent("Encoding/UTF8"), (encodingType == EncodingType.UTF8), SetEncodingType, (int)EncodingType.UTF8);
                menu.AddItem(new GUIContent("Encoding/Unicode"), (encodingType == EncodingType.Unicode), SetEncodingType, (int)EncodingType.Unicode);
                menu.ShowAsContext();
            }
        }

        private void ToggleAddFieldsAtTop()
        {
            addFieldsAtTop = !addFieldsAtTop;
            EditorPrefs.SetBool(AddFieldsAtTopPrefsKey, addFieldsAtTop);
        }

        private void ToggleSearchBar()
        {
            showSearchBar = !showSearchBar;
            EditorPrefs.SetBool(ShowSearchBarPrefsKey, showSearchBar);
        }

        #endregion

        #region Languages

        private void DrawLanguages()
        {
            // Draw Languages foldout and menu:
            EditorGUILayout.BeginHorizontal();
            languagesFoldout = EditorGUILayout.Foldout(languagesFoldout, "Languages");
            DrawMenu();
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(22)))
            {
                languagesFoldout = true;
                table.languages.Add(string.Empty);
            }
            EditorGUILayout.EndHorizontal();

            // Draw languages:
            if (languagesFoldout)
            {
                int languageIndexToDelete = -1;
                EditorWindowTools.StartIndentedSection();
                for (int i = 0; i < table.languages.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    table.languages[i] = EditorGUILayout.TextField(table.languages[i]);
                    EditorGUI.BeginDisabledGroup(i == 0);
                    if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(22)))
                    {
                        languageIndexToDelete = i;
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }
                EditorWindowTools.EndIndentedSection();
                if (languageIndexToDelete != -1) DeleteLanguage(languageIndexToDelete);
            }
        }

        private void DeleteLanguage(int languageIndex)
        {
            if (EditorUtility.DisplayDialog("Delete language?",
                                            string.Format("Are you sure you want to delete '{0}' and related text in all fields?", table.languages[languageIndex]),
                                            "Delete", "Cancel"))
            {
                table.languages.RemoveAt(languageIndex);
                foreach (var field in table.fields)
                {
                    if (languageIndex < field.values.Count) field.values.RemoveAt(languageIndex);
                }
            }
        }

        private void SortLanguages()
        {
            table.languages.RemoveAt(0);
            table.languages.Sort();
            table.languages.Insert(0, "Default");
        }

        #endregion

        #region Fields

        private void DrawFields()
        {
            // Draw Fields foldout and "+" button:
            EditorGUILayout.BeginHorizontal();
            fieldsFoldout = EditorGUILayout.Foldout(fieldsFoldout, "Fields");
            if (GUILayout.Button("Sort", EditorStyles.miniButton, GUILayout.Width(44)))
            {
                SortFields();
                return;
            }
            if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(22)))
            {
                fieldsFoldout = true;
                var index = table.fields.Count;
                if (!fieldFoldouts.ContainsKey(index)) fieldFoldouts.Add(index, false);
                if (addFieldsAtTop)
                {
                    table.fields.Insert(0, new LocalizedTextTable.LocalizedTextField());
                    for (int i = index; i > 0; i--)
                    {
                        if (!fieldFoldouts.ContainsKey(i)) fieldFoldouts.Add(i, false);
                        if (!fieldFoldouts.ContainsKey(i - 1)) fieldFoldouts.Add(i - 1, false);
                        fieldFoldouts[i] = fieldFoldouts[i - 1];
                    }
                    fieldFoldouts[0] = true;
                }
                else
                {
                    table.fields.Add(new LocalizedTextTable.LocalizedTextField());
                    fieldFoldouts[index] = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            // Draw fields:
            if (fieldsFoldout)
            {
                int fieldIndexToDelete = -1;
                EditorWindowTools.StartIndentedSection();
                for (int i = 0; i < table.fields.Count; i++)
                {
                    LocalizedTextTable.LocalizedTextField field = table.fields[i];
                    if (!fieldFoldouts.ContainsKey(i)) fieldFoldouts.Add(i, false);
                    EditorGUILayout.BeginHorizontal();
                    fieldFoldouts[i] = EditorGUILayout.Foldout(fieldFoldouts[i], string.IsNullOrEmpty(field.name) ? string.Format("Field {0}", i) : field.name);
                    if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(22)))
                    {
                        fieldIndexToDelete = i;
                    }
                    EditorGUILayout.EndHorizontal();
                    if (fieldFoldouts[i])
                    {
                        EditorGUILayout.BeginHorizontal();

                        if (currentSearchResultIndex == i && currentSearchResultValueIndex == -1)
                        {
                            GUI.SetNextControlName("Match");
                        }

                        EditorGUILayout.LabelField("Field", GUILayout.Width(60));
                        field.name = EditorGUILayout.TextField(field.name);
                        EditorGUILayout.LabelField(string.Empty, GUILayout.Width(22));
                        EditorGUILayout.EndHorizontal();
                        EditorWindowTools.StartIndentedSection();
                        for (int j = 0; j < table.languages.Count; j++)
                        {
                            if (j >= field.values.Count) field.values.Add(string.Empty);
                            EditorGUILayout.BeginHorizontal();

                            if (currentSearchResultIndex == i && currentSearchResultValueIndex == j)
                            {
                                GUI.SetNextControlName("Match");
                            }

                            EditorGUILayout.LabelField(table.languages[j], GUILayout.Width(60));
                            //---Was: field.values[j] = EditorGUILayout.TextField(field.values[j]);
                            field.values[j] = EditorGUILayout.TextArea(field.values[j]);
                            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(22));
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorWindowTools.EndIndentedSection();
                    }

                    if (currentSearchResultIndex == i && needToFocusOnSearchResult)
                    {
                        GUI.FocusControl("Match");
                        needToFocusOnSearchResult = false;
                    }
                }
                EditorWindowTools.EndIndentedSection();
                if (fieldIndexToDelete != -1) DeleteField(fieldIndexToDelete);
            }
        }

        private void DeleteField(int fieldIndex)
        {
            if (EditorUtility.DisplayDialog("Delete field?",
                                            string.Format("Are you sure you want to delete field {0} (\"{1}\")?", fieldIndex, table.fields[fieldIndex].name),
                                            "Delete", "Cancel"))
            {
                table.fields.RemoveAt(fieldIndex);
            }
        }

        private void SortFields()
        {
            table.fields.Sort((x, y) => string.Compare(x.name, y.name, System.StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Import/Export

        private void SetEncodingType(object data)
        {
            encodingType = (EncodingType)data;
            EditorPrefs.SetInt(EncodingTypeEditorPrefsKey, (int)encodingType);
        }

        private void Import()
        {
            if (!EditorUtility.DisplayDialog("Import CSV?",
                                            "Importing from CSV will overwrite the current contents. Are you sure?",
                                            "Import", "Cancel"))
            {
                return;
            }
            string newFilename = EditorUtility.OpenFilePanel("Import from CSV", EditorWindowTools.GetDirectoryName(csvFilename), "csv");
            if (!string.IsNullOrEmpty(newFilename))
            {
                csvFilename = newFilename;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    csvFilename = csvFilename.Replace("/", "\\");
                }
                try
                {
                    // Read the source file and combine multiline rows:
                    var sourceLines = new List<string>();
                    var file = new StreamReader(csvFilename, EncodingTypeTools.GetEncoding(encodingType));
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        sourceLines.Add(line.TrimEnd());
                    }
                    file.Close();
                    CombineMultilineSourceLines(sourceLines);
                    if (sourceLines.Count < 1) throw new System.Exception("No lines read from CSV file.");

                    // Work with a temporary, new table:
                    LocalizedTextTable newTable = ScriptableObject.CreateInstance<LocalizedTextTable>();

                    // Read heading:
                    string[] values = CSVExporter.GetValues(sourceLines[0]);
                    sourceLines.RemoveAt(0);
                    newTable.languages = new List<string>(values);
                    newTable.languages.RemoveAt(0);

                    // Read fields:
                    newTable.fields.Clear();
                    while (sourceLines.Count > 0)
                    {
                        values = CSVExporter.GetValues(sourceLines[0]);
                        sourceLines.RemoveAt(0);
                        LocalizedTextTable.LocalizedTextField field = new LocalizedTextTable.LocalizedTextField();
                        field.name = values[0];
                        for (int i = 1; i < values.Length; i++)
                        {
                            field.values.Add(values[i]);
                        }
                        newTable.fields.Add(field);
                    }

                    // If we got to the end, use the new table:
                    table.languages.Clear();
                    foreach (var newLanguage in newTable.languages)
                    {
                        table.languages.Add(newLanguage);
                    }
                    table.fields.Clear();
                    foreach (var newField in newTable.fields)
                    {
                        LocalizedTextTable.LocalizedTextField field = new LocalizedTextTable.LocalizedTextField();
                        field.name = newField.name;
                        field.values = new List<string>(newField.values);
                        table.fields.Add(field);
                    }
                    DestroyImmediate(newTable);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e.Message);
                    EditorUtility.DisplayDialog("Import Failed", "There was an error importing the CSV file.", "OK");
                }
                EditorUtility.DisplayDialog("Import Complete", "The localized text table was imported from " + newFilename + ".", "OK");
            }
        }

        /// <summary>
        /// Combines lines that are actually a multiline CSV row. This also helps prevent the 
        /// CSV-splitting regex from hanging due to catastrophic backtracking on unterminated quotes.
        /// </summary>
        private void CombineMultilineSourceLines(List<string> sourceLines)
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

        private void Export()
        {
            string newFilename = EditorUtility.SaveFilePanel("Export to CSV", EditorWindowTools.GetDirectoryName(csvFilename), csvFilename, "csv");
            if (!string.IsNullOrEmpty(newFilename))
            {
                csvFilename = newFilename;
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    csvFilename = csvFilename.Replace("/", "\\");
                }
                using (StreamWriter file = new StreamWriter(csvFilename, false, EncodingTypeTools.GetEncoding(encodingType)))
                {

                    // Write heading:
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Field");
                    foreach (var language in table.languages)
                    {
                        sb.AppendFormat(",{0}", CSVExporter.CleanField(language));
                    }
                    file.WriteLine(sb);

                    // Write fields:
                    foreach (var field in table.fields)
                    {
                        sb = new StringBuilder();
                        sb.Append(CSVExporter.CleanField(field.name));
                        foreach (var value in field.values)
                        {
                            sb.AppendFormat(",{0}", CSVExporter.CleanField(value));
                        }
                        file.WriteLine(sb);
                    }
                }
                EditorUtility.DisplayDialog("Export Complete", "The localized text table was exported to CSV (comma-separated values) format. ", "OK");
            }
        }

        private void ExportSeparateLanguages()
        {
            if (table == null) return;
            string newFilename = EditorUtility.SaveFilePanel("Export Language Text", EditorWindowTools.GetDirectoryName(languageDumpBaseFilename), languageDumpBaseFilename, "txt");
            if (!string.IsNullOrEmpty(newFilename))
            {
                languageDumpBaseFilename = newFilename;
                ExportLanguage(0, languageDumpBaseFilename);
                for (int i = 1; i < table.languages.Count; i++)
                {
                    var filename = Path.GetDirectoryName(languageDumpBaseFilename) + "/" + Path.GetFileNameWithoutExtension(languageDumpBaseFilename) + "_" + table.languages[i] + ".txt";
                    ExportLanguage(i, filename);
                }
            }
        }

        private void ExportLanguage(int languageIndex, string filename)
        {
            using (StreamWriter file = new StreamWriter(filename, false, EncodingTypeTools.GetEncoding(encodingType)))
            {
                for (int i = 0; i < table.fields.Count; i++)
                {
                    file.WriteLine(table.fields[i].values[languageIndex]);
                }
            }
        }

        #endregion

        #region Search

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal();
            searchString = EditorGUILayout.TextField("Search", searchString, MoreEditorGuiUtility.ToolbarSearchTextFieldName);
            GUI.SetNextControlName("SearchClearButton");
            if (GUILayout.Button("Clear", MoreEditorGuiUtility.ToolbarSearchCancelButtonName))
            {
                searchString = string.Empty;
                GUI.FocusControl("SearchClearButton"); // Need to deselect search field to clear text field's display.
            }
            searchCaseSensitive = EditorGUILayout.ToggleLeft(new GUIContent("Aa", "Case-sensitive"), searchCaseSensitive, GUILayout.Width(30));
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(searchString));
            if (GUILayout.Button("↓", EditorStyles.miniButtonLeft, GUILayout.Width(22))) Search(1);
            if (GUILayout.Button("↑", EditorStyles.miniButtonMid, GUILayout.Width(22))) Search(-1);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(22))) showSearchBar = false;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(string.Empty, GUILayout.Height(1));
        }

        private void Search(int direction)
        {
            var start = GetValidFieldIndex(currentSearchResultIndex);
            var index = GetValidFieldIndex(currentSearchResultIndex + direction);
            int safeguard = 0;
            while (index != start && safeguard < 9999)
            {
                safeguard++;
                if (MatchSearchString(index))
                {
                    currentSearchResultIndex = index;
                    fieldFoldouts[index] = true;
                    needToFocusOnSearchResult = true;
                    return;
                }
                index = GetValidFieldIndex(index + direction);
            }
            EditorUtility.DisplayDialog("Search Results", "No match found.", "OK");
        }

        private int GetValidFieldIndex(int index)
        {
            if (index < 0) return table.fields.Count - 1;
            if (index >= table.fields.Count) return 0;
            return index;
        }

        private bool MatchSearchString(int index)
        {
            if (!(0 <= index && index < table.fields.Count)) return false;
            var field = table.fields[index];

            var substr = searchString;
            if (!searchCaseSensitive) substr = searchString.ToLower();

            if (MatchSubstr(field.name, substr))
            {
                currentSearchResultValueIndex = -1;
                return true;
            }

            for (int i = 0; i < field.values.Count; i++)
            {
                if (MatchSubstr(field.values[i], substr))
                {
                    currentSearchResultValueIndex = i;
                    return true;
                }
            }

            return false;
        }

        private bool MatchSubstr(string s, string substr)
        {
            if (!searchCaseSensitive) s = s.ToLower();
            return s.Contains(substr);
        }

        #endregion

        #region Save As Text Table

        private void SaveAsTextTable()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save As Text Table", target.name + " Text Table", "asset", "Please enter a filename to save the Text Table to");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                EditorUtility.DisplayProgressBar("Saving As Text Table", "Please wait...", 0);
                var textTable = AssetUtility.CreateAssetWithFilename<TextTable>(path, false);
                foreach (var language in table.languages)
                {
                    textTable.AddLanguage(language);
                }
                for (int i = 0; i < table.fields.Count; i++)
                {
                    var field = table.fields[i];
                    EditorUtility.DisplayProgressBar("Saving As Text Table", field.name, i / table.fields.Count);
                    textTable.AddField(field.name);
                    for (int j = 0; j < table.languages.Count; j++)
                    {
                        textTable.SetFieldTextForLanguage(field.name, table.languages[j], field.values[j]);
                    }
                }
                EditorUtility.SetDirty(textTable);
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Saved Text Table", "Saved a Text Table containing this content as " + path + ".", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        #endregion

    }
}
