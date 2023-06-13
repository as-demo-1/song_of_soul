// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PixelCrushers
{

    /// <summary>
    /// Custom editor window for TextTable.
    /// </summary>
    public class TextTableEditorWindow : EditorWindow
    {

        #region Menu Item

        [MenuItem("Tools/Pixel Crushers/Common/Text Table Editor")]
        public static void ShowWindow()
        {
            GetWindow<TextTableEditorWindow>();
        }

        #endregion

        #region Variables

        public static bool isOpen { get { return instance != null; } }

        public static TextTableEditorWindow instance { get { return s_instance; } }

        private static TextTableEditorWindow s_instance = null;

        private const string WindowTitle = "Text Table";

        private static GUIContent[] ToolbarLabels = new GUIContent[]
            { new GUIContent("Languages"), new GUIContent("Fields") };

        [SerializeField]
        private int m_textTableInstanceID;

        [SerializeField]
        private Vector2 m_languageListScrollPosition;

        [SerializeField]
        private Vector2 m_fieldListScrollPosition;

        [SerializeField]
        private int m_toolbarSelection = 0;

        [SerializeField]
        private int m_selectedLanguageIndex = 0;

        [SerializeField]
        private int m_selectedLanguageID = 0;

        [SerializeField]
        private string m_csvFilename = string.Empty;

        [SerializeField]
        private bool m_isSearchPanelOpen = false;

        [SerializeField]
        private string m_searchString = string.Empty;

        [SerializeField]
        private string m_replaceString = string.Empty;

        [SerializeField]
        private bool m_matchCase = false;

        private TextTable m_textTable;

        private bool m_needRefreshLists = true;
        private ReorderableList m_languageList = null;
        private ReorderableList m_fieldList = null;
        private SerializedObject m_serializedObject = null;
        private GUIStyle textAreaStyle = null;
        private bool isTextAreaStyleInitialized = false;

        private const string EncodingTypeEditorPrefsKey = "PixelCrushers.EncodingType";
        private const string ToolbarSelectionPrefsKey = "PixelCrushers.TextTableEditor.Toolbar";
        private const string SearchBarPrefsKey = "PixelCrushers.TextTableEditor.SearchBar";
        private const double TimeBetweenUpdates = 10;

        private bool m_needToUpdateSO;
        private bool m_needToApplyBeforeUpdateSO;
        private bool m_isPickingOtherTextTable;
        private System.DateTime m_lastApply;

        [System.Serializable]
        public class SearchBarSettings
        {
            public bool open = false;
            public string searchString = string.Empty;
            public string replaceString = string.Empty;
            public bool matchCase = false;
        }

        #endregion

        #region Editor Entrypoints

        private void OnEnable()
        {
            m_needToUpdateSO = true;
            m_needToApplyBeforeUpdateSO = false;
            m_lastApply = System.DateTime.Now;
            s_instance = this;
            titleContent.text = "Text Table";
            m_needRefreshLists = true;
            Undo.undoRedoPerformed += Repaint;
            if (m_textTableInstanceID != 0) Selection.activeObject = EditorUtility.InstanceIDToObject(m_textTableInstanceID);
            m_toolbarSelection = EditorPrefs.GetInt(ToolbarSelectionPrefsKey, 0);
            if (EditorPrefs.HasKey(SearchBarPrefsKey))
            {
                var searchBarSettings = JsonUtility.FromJson<SearchBarSettings>(EditorPrefs.GetString(SearchBarPrefsKey));
                if (searchBarSettings != null)
                {
                    m_isSearchPanelOpen = searchBarSettings.open;
                    m_searchString = searchBarSettings.searchString;
                    m_replaceString = searchBarSettings.replaceString;
                    m_matchCase = searchBarSettings.matchCase;
                }
            }
            OnSelectionChange();
        }

        private void OnDisable()
        {
            if (m_serializedObject != null) m_serializedObject.ApplyModifiedProperties();
            s_instance = null;
            Undo.undoRedoPerformed -= Repaint;
            EditorPrefs.SetInt(ToolbarSelectionPrefsKey, m_toolbarSelection);
            var searchBarSettings = new SearchBarSettings();
            searchBarSettings.open = m_isSearchPanelOpen;
            searchBarSettings.searchString = m_searchString;
            searchBarSettings.replaceString = m_replaceString;
            searchBarSettings.matchCase = m_matchCase;
            EditorPrefs.SetString(SearchBarPrefsKey, JsonUtility.ToJson(searchBarSettings));
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is TextTable)
            {
                SelectTextTable(Selection.activeObject as TextTable);
                Repaint();
            }
            else if (m_textTable == null && m_textTableInstanceID != 0)
            {
                SelectTextTable(EditorUtility.InstanceIDToObject(m_textTableInstanceID) as TextTable);
                Repaint();
            }
        }

        private void SelectTextTable(TextTable newTable)
        {
            m_textTable = newTable;
            ResetLanguagesTab();
            ResetFieldsTab();
            m_needRefreshLists = true;
            m_needToUpdateSO = true;
            m_needToApplyBeforeUpdateSO = false;
            m_serializedObject = (newTable != null) ? new SerializedObject(newTable) : null;
            if (m_textTable != null && m_textTable.languages.Count == 0) m_textTable.AddLanguage("Default");
            m_textTableInstanceID = (newTable != null) ? newTable.GetInstanceID() : 0;
        }

        private void OnGUI()
        {
            if (Event.current.commandName == "ObjectSelectorClosed" || Event.current.commandName == "ObjectSelectorUpdated")
            { 
                if (m_isPickingOtherTextTable)
                { 
                    m_isPickingOtherTextTable = false;
                    AskConfirmImportOtherTextTable(EditorGUIUtility.GetObjectPickerObject() as TextTable);
                }
                return;
            }

            DrawWindowContents();
            if (m_needRefreshLists) Repaint();
        }

        private void DrawWindowContents()
        {
            DrawTextTableField();
            if (m_textTable == null || m_serializedObject == null) return;
            var now = System.DateTime.Now;
            var elapsed = (now - m_lastApply).TotalSeconds;
            if (m_needToUpdateSO)
            {
                if (m_needToApplyBeforeUpdateSO)
                {
                    m_serializedObject.ApplyModifiedProperties();
                    m_needToApplyBeforeUpdateSO = false;
                }
                m_needToUpdateSO = false;
                m_serializedObject.Update();
            }
            var newToolbarSelection = GUILayout.Toolbar(m_toolbarSelection, ToolbarLabels);
            if (newToolbarSelection != m_toolbarSelection)
            {
                m_toolbarSelection = newToolbarSelection;
                if (newToolbarSelection == 1) m_languageDropdownList = null;
            }
            if (m_toolbarSelection == 0)
            {
                DrawLanguagesTab();
            }
            else
            {
                DrawFieldsTab();
            }
            if (GUI.changed) m_needToApplyBeforeUpdateSO = true;
            if (elapsed > TimeBetweenUpdates)
            {
                m_lastApply = now;
                m_serializedObject.ApplyModifiedProperties();
                m_needToApplyBeforeUpdateSO = false;
            }
        }

        private void DrawTextTableField()
        {
            EditorGUILayout.BeginHorizontal();
            var newTable = EditorGUILayout.ObjectField(m_textTable, typeof(TextTable), false) as TextTable;
            if (newTable != m_textTable) SelectTextTable(newTable);
            DrawGearMenu();
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Language List

        private void ResetLanguagesTab()
        {
            m_languageList = null;
            m_languageListScrollPosition = Vector2.zero;
        }

        private void DrawLanguagesTab()
        {
            if (m_languageList == null)
            {
                m_languageList = new ReorderableList(m_serializedObject, m_serializedObject.FindProperty("m_languageKeys"), true, true, true, true);
                m_languageList.drawHeaderCallback = OnDrawLanguageListHeader;
                m_languageList.drawElementCallback = OnDrawLanguageListElement;
                m_languageList.onAddCallback = OnAddLanguageListElement;
                m_languageList.onCanRemoveCallback = OnCanRemoveLanguageListElement;
                m_languageList.onRemoveCallback = OnRemoveLanguageListElement;
                m_languageList.onSelectCallback = OnSelectLanguageListElement;
                m_languageList.onReorderCallback = OnReorderLanguageListElement;
            }
            m_languageListScrollPosition = GUILayout.BeginScrollView(m_languageListScrollPosition, false, false);
            try
            {
                m_languageList.DoLayoutList();
            }
            finally
            {
                GUILayout.EndScrollView();
            }
        }

        private void OnDrawLanguageListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Languages");
        }

        private void OnDrawLanguageListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var languageKeysProperty = m_serializedObject.FindProperty("m_languageKeys");
            var languageKeyProperty = languageKeysProperty.GetArrayElementAtIndex(index);
            var languageValuesProperty = m_serializedObject.FindProperty("m_languageValues");
            var languageValueProperty = languageValuesProperty.GetArrayElementAtIndex(index);
            EditorGUI.BeginDisabledGroup(languageValueProperty.intValue == 0);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + 1, rect.width, EditorGUIUtility.singleLineHeight), languageKeyProperty, GUIContent.none, false);
            EditorGUI.EndDisabledGroup();
        }

        private void OnAddLanguageListElement(ReorderableList list)
        {
            m_serializedObject.ApplyModifiedProperties();
            m_textTable.AddLanguage("Language " + m_textTable.nextLanguageID);
            m_serializedObject.Update();
            ResetFieldsTab();
        }

        private bool OnCanRemoveLanguageListElement(ReorderableList list)
        {
            var languageValuesProperty = m_serializedObject.FindProperty("m_languageValues");
            var languageValueProperty = languageValuesProperty.GetArrayElementAtIndex(list.index);
            return languageValueProperty.intValue > 0;
        }

        private void OnRemoveLanguageListElement(ReorderableList list)
        {
            var languageKeysProperty = m_serializedObject.FindProperty("m_languageKeys");
            var languageKeyProperty = languageKeysProperty.GetArrayElementAtIndex(list.index);
            var languageName = languageKeyProperty.stringValue;
            var languageValuesProperty = m_serializedObject.FindProperty("m_languageValues");
            var languageValueProperty = languageValuesProperty.GetArrayElementAtIndex(list.index);
            var languageID = languageValueProperty.intValue;
            if (!EditorUtility.DisplayDialog("Delete " + languageName, "Are you sure you want to delete the language '" + languageName +
                "' and all field values associated with it?", "OK", "Cancel")) return;
            m_serializedObject.ApplyModifiedProperties();
            m_textTable.RemoveLanguage(languageID);
            m_serializedObject.Update();
            ResetFieldsTab();
        }

        private int m_selectedLanguageListIndex = -1;

        private void OnSelectLanguageListElement(ReorderableList list)
        {
            m_selectedLanguageListIndex = list.index;
        }

        private void OnReorderLanguageListElement(ReorderableList list)
        {
            // Also reorder values:
            var languageValuesProperty = m_serializedObject.FindProperty("m_languageValues");
            var value = languageValuesProperty.GetArrayElementAtIndex(m_selectedLanguageListIndex).intValue;
            languageValuesProperty.DeleteArrayElementAtIndex(m_selectedLanguageListIndex);
            languageValuesProperty.InsertArrayElementAtIndex(list.index);
            languageValuesProperty.GetArrayElementAtIndex(list.index).intValue = value;
            ResetFieldsTab();
        }

        #endregion

        #region Field List

        private void ResetFieldsTab()
        {
            m_fieldList = null;
            m_fieldListScrollPosition = Vector2.zero;
            m_selectedLanguageIndex = 0;
            m_selectedLanguageID = 0;
        }

        private void DrawFieldsTab()
        {
            DrawGrid();
            DrawEntryBox();
            if (m_isSearchPanelOpen)
            {
                DrawSearchPanel();
            }
            //else
            //{
            //    DrawEntryBox();
            //}
        }

        private const float MinColumnWidth = 100;

        private string[] m_languageDropdownList = null;

        private class CachedFieldInfo
        {
            public SerializedProperty fieldNameProperty;
            public SerializedProperty fieldValueProperty;
            public string nameControl;
            public string valueControl;
            public CachedFieldInfo(int index, SerializedProperty fieldNameProperty, SerializedProperty fieldValueProperty)
            {
                this.fieldNameProperty = fieldNameProperty;
                this.fieldValueProperty = fieldValueProperty;
                this.nameControl = "Field" + index;
                this.valueControl = "Value" + index;
            }
        }
        private List<CachedFieldInfo> m_fieldCache = new List<CachedFieldInfo>();

        private void DrawGrid()
        {
            if (m_textTable == null) return;
            try
            {
                var entryBoxHeight = IsAnyFieldSelected() ? (6 * EditorGUIUtility.singleLineHeight) : 0;
                if (m_isSearchPanelOpen) entryBoxHeight += (4 * EditorGUIUtility.singleLineHeight);
                GUILayout.BeginArea(new Rect(0, 2 * (EditorGUIUtility.singleLineHeight + 4), position.width,
                    position.height - (2 * (EditorGUIUtility.singleLineHeight + 4) + 4) - entryBoxHeight));
                m_fieldListScrollPosition = GUILayout.BeginScrollView(m_fieldListScrollPosition, false, false);

                if (m_needRefreshLists || m_fieldList == null || m_languageDropdownList == null)
                {
                    m_needRefreshLists = false;
                    m_fieldList = new ReorderableList(m_serializedObject, m_serializedObject.FindProperty("m_fieldValues"), true, true, true, true);
                    m_fieldList.drawHeaderCallback = OnDrawFieldListHeader;
                    m_fieldList.drawElementCallback = OnDrawFieldListElement;
                    m_fieldList.onAddCallback = OnAddFieldListElement;
                    m_fieldList.onRemoveCallback = OnRemoveFieldListElement;
                    m_fieldList.onSelectCallback = OnSelectFieldListElement;
                    m_fieldList.onReorderCallback = OnReorderFieldListElement;

                    var languages = new List<string>();
                    var languageKeysProperty = m_serializedObject.FindProperty("m_languageKeys");
                    for (int i = 0; i < languageKeysProperty.arraySize; i++)
                    {
                        languages.Add(languageKeysProperty.GetArrayElementAtIndex(i).stringValue);
                    }
                    m_languageDropdownList = languages.ToArray();

                    RebuildFieldCache();
                }

                m_fieldList.DoLayoutList();

                CheckMouseEvents();
            }
            finally
            {
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }

        private void RebuildFieldCache()
        {
            m_fieldCache.Clear();

            var fieldValuesProperty = m_serializedObject.FindProperty("m_fieldValues");
            for (int index = 0; index < fieldValuesProperty.arraySize; index++)
            {
                var fieldValueProperty = fieldValuesProperty.GetArrayElementAtIndex(index);
                var fieldNameProperty = fieldValueProperty.FindPropertyRelative("m_fieldName");
                var keysProperty = fieldValueProperty.FindPropertyRelative("m_keys");
                var valuesProperty = fieldValueProperty.FindPropertyRelative("m_values");

                var valueIndex = -1;
                for (int i = 0; i < keysProperty.arraySize; i++)
                {
                    if (keysProperty.GetArrayElementAtIndex(i).intValue == m_selectedLanguageID)
                    {
                        valueIndex = i;
                        break;
                    }
                }
                if (valueIndex == -1)
                {
                    valueIndex = keysProperty.arraySize;
                    keysProperty.arraySize++;
                    keysProperty.GetArrayElementAtIndex(valueIndex).intValue = m_selectedLanguageID;
                    valuesProperty.arraySize++;
                    valuesProperty.GetArrayElementAtIndex(valueIndex).stringValue = string.Empty;
                }
                var valueProperty = valuesProperty.GetArrayElementAtIndex(valueIndex);

                m_fieldCache.Add(new CachedFieldInfo(index, fieldNameProperty, valueProperty));
            }
        }

        private void OnDrawFieldListHeader(Rect rect)
        {
            var headerWidth = rect.width - 14;
            var columnWidth = headerWidth / 2;
            EditorGUI.LabelField(new Rect(rect.x + 14, rect.y, columnWidth, rect.height), "Field");
            var popupRect = new Rect(rect.x + rect.width - columnWidth, rect.y, columnWidth, rect.height);
            var newIndex = EditorGUI.Popup(popupRect, m_selectedLanguageIndex, m_languageDropdownList);
            if (newIndex != m_selectedLanguageIndex)
            {
                m_selectedLanguageIndex = newIndex;
                var languageValuesProperty = m_serializedObject.FindProperty("m_languageValues");
                var languageValueProperty = languageValuesProperty.GetArrayElementAtIndex(newIndex);
                m_selectedLanguageID = languageValueProperty.intValue;
                RebuildFieldCache();
            }
        }

        private void OnDrawFieldListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.width <= 0) return;
            // Since lists can be very long, only draw elements within the visible window:
            if (!(0 <= index && index < m_fieldCache.Count)) return;
            var isElementVisible = rect.Overlaps(new Rect(0, m_fieldListScrollPosition.y, position.width, position.height));
            if (!isElementVisible) return;

            var columnWidth = (rect.width / 2) - 1;

            var info = m_fieldCache[index];

            GUI.SetNextControlName(info.nameControl);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + 1, columnWidth, EditorGUIUtility.singleLineHeight), info.fieldNameProperty, GUIContent.none, false);

            if (info.fieldValueProperty != null)
            {
                GUI.SetNextControlName(info.valueControl);
                EditorGUI.PropertyField(new Rect(rect.x + rect.width - columnWidth, rect.y + 1, columnWidth, EditorGUIUtility.singleLineHeight), info.fieldValueProperty, GUIContent.none, false);
                var focusedControl = GUI.GetNameOfFocusedControl();
                if (string.Equals(info.nameControl, focusedControl) || string.Equals(info.valueControl, focusedControl))
                {
                    m_selectedFieldListElement = index;
                    m_fieldList.index = index;
                }
            }
        }

        private void OnAddFieldListElement(ReorderableList list)
        {
            m_serializedObject.ApplyModifiedProperties();
            m_textTable.AddField("Field " + m_textTable.nextFieldID);
            m_serializedObject.Update();
            RebuildFieldCache();
            Repaint();
        }

        private void OnRemoveFieldListElement(ReorderableList list)
        {
            var fieldKeysProperty = m_serializedObject.FindProperty("m_fieldKeys");
            var fieldKeyProperty = fieldKeysProperty.GetArrayElementAtIndex(list.index);
            var fieldID = fieldKeyProperty.intValue;
            var fieldValuesProperty = m_serializedObject.FindProperty("m_fieldValues");
            var fieldValueProperty = fieldValuesProperty.GetArrayElementAtIndex(list.index);
            var fieldNameProperty = fieldValueProperty.FindPropertyRelative("m_fieldName");
            var fieldName = fieldNameProperty.stringValue;
            if (!EditorUtility.DisplayDialog("Delete Field", "Are you sure you want to delete the field '" + fieldName +
                "' and all values associated with it?", "OK", "Cancel")) return;
            m_serializedObject.ApplyModifiedProperties();
            m_textTable.RemoveField(fieldID);
            m_serializedObject.Update();
            RebuildFieldCache();
        }

        private int m_selectedFieldListElement;

        private void OnSelectFieldListElement(ReorderableList list)
        {
            m_selectedFieldListElement = list.index;
        }

        private void OnReorderFieldListElement(ReorderableList list)
        {
            // Also reorder keys:
            var fieldKeysProperty = m_serializedObject.FindProperty("m_fieldKeys");
            var value = fieldKeysProperty.GetArrayElementAtIndex(m_selectedFieldListElement).intValue;
            fieldKeysProperty.DeleteArrayElementAtIndex(m_selectedFieldListElement);
            fieldKeysProperty.InsertArrayElementAtIndex(list.index);
            fieldKeysProperty.GetArrayElementAtIndex(list.index).intValue = value;
        }

        private void CheckMouseEvents()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1) // Right-click
            {
                var scrolledClickPosition = Event.current.mousePosition.y - 16;
                var elementHeight = (EditorGUIUtility.singleLineHeight + 5);
                var index = Mathf.FloorToInt(scrolledClickPosition / elementHeight);
                if (0 <= index && index < m_fieldList.count)
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Insert Field"), false, InsertFieldListElement, index);
                    menu.AddItem(new GUIContent("Delete Field"), false, DeleteFieldListElement, index);
                    menu.ShowAsContext();
                }
            }
        }

        private void InsertFieldListElement(object data)
        {
            int index = (int)data;
            m_serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(m_textTable, "Insert Field");
            m_textTable.InsertField(index, "Field " + m_textTable.nextFieldID);
            EditorUtility.SetDirty(m_textTable);
            m_serializedObject.Update();
            RebuildFieldCache();
            Repaint();
        }

        private void DeleteFieldListElement(object data)
        {
            int index = (int)data;
            var info = m_fieldCache[index];
            if (EditorUtility.DisplayDialog("Delete Field", "Delete '" + info.fieldNameProperty.stringValue + "'?", "OK", "Cancel"))
            {
                m_serializedObject.ApplyModifiedProperties();
                Undo.RecordObject(m_textTable, "Delete Field");
                m_textTable.RemoveField(info.fieldNameProperty.stringValue);
                EditorUtility.SetDirty(m_textTable);
                m_serializedObject.Update();
                RebuildFieldCache();
                Repaint();
            }
        }

        private bool IsAnyFieldSelected()
        {
            return m_fieldList != null && 0 <= m_fieldList.index && m_fieldList.index < m_fieldList.serializedProperty.arraySize;
        }

        private void DrawEntryBox()
        {
            if (m_needRefreshLists || !IsAnyFieldSelected()) return;
            var rect = new Rect(2, position.height - 6 * EditorGUIUtility.singleLineHeight, position.width - 4, 6 * EditorGUIUtility.singleLineHeight);
            if (m_isSearchPanelOpen)
            {
                var searchPanelHeight = (4 * EditorGUIUtility.singleLineHeight);
                rect = new Rect(rect.x, rect.y - searchPanelHeight, rect.width, rect.height);
            }
            var fieldValuesProperty = m_serializedObject.FindProperty("m_fieldValues");
            var fieldValueProperty = fieldValuesProperty.GetArrayElementAtIndex(m_fieldList.index);
            var keysProperty = fieldValueProperty.FindPropertyRelative("m_keys");
            var valuesProperty = fieldValueProperty.FindPropertyRelative("m_values");
            var valueIndex = -1;
            for (int i = 0; i < keysProperty.arraySize; i++)
            {
                if (keysProperty.GetArrayElementAtIndex(i).intValue == m_selectedLanguageID)
                {
                    valueIndex = i;
                    break;
                }
            }
            if (valueIndex == -1)
            {
                valueIndex = keysProperty.arraySize;
                keysProperty.arraySize++;
                keysProperty.GetArrayElementAtIndex(valueIndex).intValue = m_selectedLanguageID;
                valuesProperty.arraySize++;
                valuesProperty.GetArrayElementAtIndex(valueIndex).stringValue = string.Empty;
            }
            if (textAreaStyle == null || !isTextAreaStyleInitialized)
            {
                isTextAreaStyleInitialized = true;
                textAreaStyle = new GUIStyle(EditorStyles.textField);
                textAreaStyle.wordWrap = true;
            }
            var valueProperty = valuesProperty.GetArrayElementAtIndex(valueIndex);
            valueProperty.stringValue = EditorGUI.TextArea(rect, valueProperty.stringValue, textAreaStyle);
        }

        #endregion

        #region Gear Menu

        private void DrawGearMenu()
        {
            if (MoreEditorGuiUtility.DoLayoutGearMenu())
            {
                var menu = new GenericMenu();
                if (m_textTable == null)
                {
                    menu.AddDisabledItem(new GUIContent("Search..."));
                    menu.AddDisabledItem(new GUIContent("Sort..."));
                    menu.AddDisabledItem(new GUIContent("Delete All..."));
                    menu.AddDisabledItem(new GUIContent("Export/CSV..."));
                    menu.AddDisabledItem(new GUIContent("Import/CSV..."));
                    menu.AddDisabledItem(new GUIContent("Import/Other Text Table..."));
                }
                else
                {
                    menu.AddItem(new GUIContent("Search..."), false, OpenSearchPanel);
                    menu.AddItem(new GUIContent("Sort..."), false, Sort);
                    menu.AddItem(new GUIContent("Delete All..."), false, DeleteAll);
                    menu.AddItem(new GUIContent("Export/CSV..."), false, ExportCSVDialogs);
                    menu.AddItem(new GUIContent("Import/CSV..."), false, ImportCSVDialogs);
                    menu.AddItem(new GUIContent("Import/Other Text Table..."), false, ImportOtherTextTable);
                }
                menu.AddItem(new GUIContent("Encoding/UTF8"), GetEncodingType() == EncodingType.UTF8, SetEncodingType, EncodingType.UTF8);
                menu.AddItem(new GUIContent("Encoding/Unicode"), GetEncodingType() == EncodingType.Unicode, SetEncodingType, EncodingType.Unicode);
                menu.AddItem(new GUIContent("Encoding/ISO-8859-1"), GetEncodingType() == EncodingType.ISO_8859_1, SetEncodingType, EncodingType.ISO_8859_1);
                menu.ShowAsContext();
            }
        }

        private void DeleteAll()
        {
            var answer = EditorUtility.DisplayDialogComplex("Delete All", "Delete all fields or delete everything (languages and fields)?", "Fields", "Everything", "Cancel");
            if (answer == 2) return; // Cancel.

            m_serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(m_textTable, "Delete");
            switch (answer)
            {
                case 0:
                    m_textTable.RemoveAllFields();
                    Debug.Log("Deleted all fields in " + m_textTable.name, m_textTable);
                    break;
                case 1:
                    m_textTable.RemoveAll();
                    Debug.Log("Deleted everything in " + m_textTable.name, m_textTable);
                    break;
            }
            EditorUtility.SetDirty(m_textTable);
            m_serializedObject.Update();
            RebuildFieldCache();
            Repaint();
        }

        #endregion

        #region Sort

        private void Sort()
        {
            var onLanguagesTab = (m_toolbarSelection == 0);
            var section = onLanguagesTab ? "Languages" : "Fields";
            if (!EditorUtility.DisplayDialog("Sort " + section, "Sort " + section.ToLower() + " alphabetically?", "OK", "Cancel")) return;
            m_serializedObject.ApplyModifiedProperties();
            if (onLanguagesTab)
            {
                m_textTable.SortLanguages();
            }
            else
            {
                m_textTable.SortFields();
            }
            m_serializedObject.Update();
            RebuildFieldCache();
            Repaint();
        }

        #endregion

        #region Search

        private void OpenSearchPanel()
        {
            m_isSearchPanelOpen = !m_isSearchPanelOpen;
        }

        private void DrawSearchPanel()
        {
            var rect = new Rect(2, position.height - 5 * EditorGUIUtility.singleLineHeight, position.width - 4, 5 * EditorGUIUtility.singleLineHeight);
            var searchRect = new Rect(rect.x, rect.y + rect.height - 4 * EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight);
            var replaceRect = new Rect(rect.x, rect.y + rect.height - 3 * EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight);
            var buttonRect = new Rect(rect.x, rect.y + rect.height - 2 * EditorGUIUtility.singleLineHeight + 4, rect.width, EditorGUIUtility.singleLineHeight);
            m_searchString = EditorGUI.TextField(searchRect, new GUIContent("Find", "Regular expressions allowed."), m_searchString);
            m_replaceString = EditorGUI.TextField(replaceRect, "Replace With", m_replaceString);
            var buttonWidth = 78f;
            var toggleWidth = 90f;
            m_matchCase = EditorGUI.ToggleLeft(new Rect(buttonRect.x + buttonRect.width - (4 * (2 + buttonWidth)) - toggleWidth, buttonRect.y, toggleWidth, buttonRect.height), "Match Case", m_matchCase);
            if (GUI.Button(new Rect(buttonRect.x + buttonRect.width - (4 * (2 + buttonWidth)), buttonRect.y, buttonWidth, buttonRect.height), "Find Next"))
            {
                FindNext();
            }
            EditorGUI.BeginDisabledGroup(!IsAnyFieldSelected());
            if (GUI.Button(new Rect(buttonRect.x + buttonRect.width - (3 * (2 + buttonWidth)), buttonRect.y, buttonWidth, buttonRect.height), "Replace"))
            {
                ReplaceCurrent();
            }
            EditorGUI.EndDisabledGroup();
            if (GUI.Button(new Rect(buttonRect.x + buttonRect.width - (2 * (2 + buttonWidth)), buttonRect.y, buttonWidth, buttonRect.height), "Replace All"))
            {
                ReplaceAll();
            }
            if (GUI.Button(new Rect(buttonRect.x + buttonRect.width - (1 * (2 + buttonWidth)), buttonRect.y, buttonWidth, buttonRect.height), "Cancel"))
            {
                m_isSearchPanelOpen = false;
            }
        }

        private void FindNext()
        {
            var found = false;
            int currentIndex = (m_fieldList.index + 1) % m_fieldList.count;
            var regexOptions = m_matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
            int safeguard = 0;
            while (!found && safeguard < 9999)
            {
                safeguard++;
                var info = m_fieldCache[currentIndex];
                if (Regex.IsMatch(info.fieldNameProperty.stringValue, m_searchString, regexOptions) ||
                    Regex.IsMatch(info.fieldValueProperty.stringValue, m_searchString, regexOptions))
                {
                    found = true;
                    break;
                }
                else if (currentIndex == m_fieldList.index)
                {
                    break; // Wrapped around, so stop.
                }
                else
                {
                    currentIndex = (currentIndex + 1) % m_fieldList.count;
                }
            }
            if (found)
            {
                m_fieldList.index = currentIndex;
                // Scroll to position:
                var minScrollY = m_fieldList.index * (EditorGUIUtility.singleLineHeight + 5);
                m_fieldListScrollPosition = new Vector2(m_fieldListScrollPosition.x, minScrollY);
            }
            else
            {
                EditorUtility.DisplayDialog("Search Text Table", "String '" + m_searchString + "' not found in text table.", "OK");
            }
        }

        private void ReplaceCurrent()
        {
            if (!IsAnyFieldSelected()) return;
            var regexOptions = m_matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
            m_fieldCache[m_fieldList.index].fieldNameProperty.stringValue = Regex.Replace(m_fieldCache[m_fieldList.index].fieldNameProperty.stringValue, m_searchString, m_replaceString, regexOptions);
            m_fieldCache[m_fieldList.index].fieldValueProperty.stringValue = Regex.Replace(m_fieldCache[m_fieldList.index].fieldValueProperty.stringValue, m_searchString, m_replaceString, regexOptions);
        }

        private void ReplaceAll()
        {
            if (!EditorUtility.DisplayDialog("Replace All", "Replace:\n'" + m_searchString + "'\nwith:\n'" + m_replaceString + "'\nin entire table for current language?", "OK", "Cancel")) return;
            var regexOptions = m_matchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
            EditorUtility.DisplayProgressBar("Replace All", "Processing text table.", 0);
            try
            {
                for (int i = 0; i < m_fieldCache.Count; i++)
                {
                    m_fieldCache[i].fieldNameProperty.stringValue = Regex.Replace(m_fieldCache[i].fieldNameProperty.stringValue, m_searchString, m_replaceString, regexOptions);
                    m_fieldCache[i].fieldValueProperty.stringValue = Regex.Replace(m_fieldCache[i].fieldValueProperty.stringValue, m_searchString, m_replaceString, regexOptions);

                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        #endregion

        #region CSV

        private void ExportCSVDialogs()
        {
            string newFilename = EditorUtility.SaveFilePanel("Export to CSV", GetPath(m_csvFilename), m_csvFilename, "csv");
            if (string.IsNullOrEmpty(newFilename)) return;
            m_csvFilename = newFilename;
            if (Application.platform == RuntimePlatform.WindowsEditor) m_csvFilename = m_csvFilename.Replace("/", "\\");
            switch (EditorUtility.DisplayDialogComplex("Export CSV", "Export languages as columns in one file or as separate files?", "One", "Cancel", "Separate"))
            {
                case 0:
                    ExportCSV(m_csvFilename, false);
                    break;
                case 2:
                    ExportCSV(m_csvFilename, true);
                    break;
                default:
                    return;
            }
            EditorUtility.DisplayDialog("Export Complete", "The text table was exported to CSV (comma-separated values) format. ", "OK");
        }

        private void ImportCSVDialogs()
        {
            if (!EditorUtility.DisplayDialog("Import CSV?", "Importing from CSV will overwrite any existing languages or fields with the same name in the current contents. Are you sure?", "Import", "Cancel")) return;
            string newFilename = EditorUtility.OpenFilePanel("Import from CSV", GetPath(m_csvFilename), "csv");
            if (string.IsNullOrEmpty(newFilename)) return;
            if (!File.Exists(newFilename))
            {
                EditorUtility.DisplayDialog("Import CSV", "Can't find the file " + newFilename + ".", "OK");
                return;
            }
            m_csvFilename = newFilename;
            if (Application.platform == RuntimePlatform.WindowsEditor) m_csvFilename = m_csvFilename.Replace("/", "\\");
            ImportCSV(m_csvFilename);
            OnSelectionChange();
            Repaint();
        }

        private string GetPath(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return string.Empty;
            try
            {
                return Path.GetDirectoryName(filename);
            }
            catch (System.ArgumentException)
            {
                return string.Empty;
            }
        }

        private EncodingType GetEncodingType()
        {
            return (EncodingType)EditorPrefs.GetInt(EncodingTypeEditorPrefsKey, (int)EncodingType.UTF8);
        }

        private void SetEncodingType(object data)
        {
            EditorPrefs.SetInt(EncodingTypeEditorPrefsKey, (int)((EncodingType)data));
        }

        private void ExportCSV(string csvFilename, bool separateFiles)
        {
            if (separateFiles)
            {
                foreach (var languageKvp in m_textTable.languages)
                {
                    var language = languageKvp.Key;
                    var languageID = languageKvp.Value;
                    var content = new List<List<string>>();
                    var row = new List<string>();
                    row.Add("Language");
                    row.Add(language);
                    content.Add(row);
                    foreach (var fieldKvp in m_textTable.fields)
                    {
                        var field = fieldKvp.Value;
                        row = new List<string>();
                        row.Add(field.fieldName);
                        row.Add(field.GetTextForLanguage(languageID));
                        content.Add(row);
                    }
                    var languageFilename = csvFilename.Substring(0, csvFilename.Length - 4) + "_" + language + ".csv";
                    CSVUtility.WriteCSVFile(content, languageFilename, GetEncodingType());
                }
            }
            else
            {
                // All in one file:
                var content = new List<List<string>>();
                var languageIDs = new List<int>();

                // Heading rows:
                var row = new List<string>();
                content.Add(row);
                row.Add("Field");
                foreach (var kvp in m_textTable.languages)
                {
                    var language = kvp.Key;
                    var languageID = kvp.Value;
                    languageIDs.Add(languageID);
                    row.Add(language);
                }

                // One row per field:
                foreach (var kvp in m_textTable.fields)
                {
                    var field = kvp.Value;
                    row = new List<string>();
                    content.Add(row);
                    row.Add(field.fieldName);
                    for (int i = 0; i < languageIDs.Count; i++)
                    {
                        var languageID = languageIDs[i];
                        var value = field.GetTextForLanguage(languageID);
                        row.Add(value);
                    }
                }
                CSVUtility.WriteCSVFile(content, csvFilename, GetEncodingType());
            }
        }

        private void ImportCSV(string csvFilename)
        {
            var content = CSVUtility.ReadCSVFile(csvFilename, GetEncodingType());
            if (content == null || content.Count < 1 || content[0].Count < 2) return;
            var fieldList = new List<string>();
            var firstCell = content[0][0];
            if (string.Equals(firstCell, "Language"))
            {
                // Single language file:
                var language = content[0][1];
                if (!string.IsNullOrEmpty(language))
                {
                    if (!m_textTable.HasLanguage(language)) m_textTable.AddLanguage(language);
                    for (int y = 1; y < content.Count; y++)
                    {
                        var field = content[y][0];
                        if (string.IsNullOrEmpty(field)) continue;
                        fieldList.Add(field);
                        if (!m_textTable.HasField(field)) m_textTable.AddField(field);
                        for (int x = 1; x < content[y].Count; x++)
                        {
                            m_textTable.SetFieldTextForLanguage(field, language, content[y][x]);
                        }
                    }
                }
            }
            else
            {
                // All-in-one file:
                for (int x = 1; x < content[0].Count; x++)
                {
                    var language = content[0][x];
                    if (string.IsNullOrEmpty(language)) continue;
                    if (!m_textTable.HasLanguage(language)) m_textTable.AddLanguage(language);
                    for (int y = 1; y < content.Count; y++)
                    {
                        var field = content[y][0];
                        if (string.IsNullOrEmpty(field)) continue;
                        if (x == 1) fieldList.Add(field);
                        if (!m_textTable.HasField(field)) m_textTable.AddField(field);
                        if ((0 <= y && y < content.Count) && (0 <= x && x < content[y].Count))
                        {
                            m_textTable.SetFieldTextForLanguage(field, language, content[y][x]);
                        }
                    }
                }
            }
            m_textTable.ReorderFields(fieldList);
            m_textTable.OnBeforeSerialize();
            m_serializedObject.Update();
            RebuildFieldCache();
            EditorUtility.SetDirty(m_textTable);
        }

        #endregion

        #region Import Other Text Table

        private void ImportOtherTextTable()
        {
            m_isPickingOtherTextTable = true;
            EditorGUIUtility.ShowObjectPicker<TextTable>(null, false, "t:TextTable", 0);
        }

        private void AskConfirmImportOtherTextTable(TextTable other)
        {
            if (other == null || m_textTable == null) return;
            if (!EditorUtility.DisplayDialog("Import Text Table?", "Import the contents of " + other.name + " into this text table? This operation may take some time depending on the sizes of the text tables.", "Import", "Cancel")) return;
            Undo.RecordObject(m_textTable, "Import");
            m_textTable.ImportOtherTextTable(other);
            m_textTable.OnBeforeSerialize();
            m_serializedObject.Update();
            RebuildFieldCache();
            EditorUtility.SetDirty(m_textTable);
            m_needRefreshLists = true;
            Repaint();
        }

        #endregion

    }
}
