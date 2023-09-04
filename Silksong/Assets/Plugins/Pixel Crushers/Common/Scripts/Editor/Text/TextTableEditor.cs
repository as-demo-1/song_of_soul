// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    [CustomEditor(typeof(TextTable), true)]
    public class TextTableEditor : Editor
    {

        private TextTable m_textTable;
        private int m_languageCount;
        private int m_fieldCount;
        private string m_languageCountText;
        private string m_fieldCountText;

        private void OnEnable()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
            m_textTable = target as TextTable;
            UpdateLabelText();
        }

        private void OnDisable()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemOnGUI;
        }

        private void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            // Check for double-clicks to open editor window:
            var doubleClicked = Event.current.type == EventType.MouseDown && Event.current.clickCount == 2 && selectionRect.Contains(Event.current.mousePosition);
            if (doubleClicked)
            {
                TextTableEditorWindow.ShowWindow();
            }
        }

        public override void OnInspectorGUI()
        {
            if (TextTableEditorWindow.isOpen)
            {
                EditorGUILayout.HelpBox("A Text Table is a database of text fields and translations into any number of languages. Edit it in the Text Table window.", MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox("A Text Table is a database of text fields and translations into any number of languages. To edit it, click on the Open Text Table Editor button below.", MessageType.None);
                if (GUILayout.Button("Open Text Table Editor"))
                {
                    TextTableEditorWindow.ShowWindow();
                }
            }
            if (m_textTable.languages.Count != m_languageCount || m_textTable.fields.Count != m_fieldCount)
            {
                UpdateLabelText();
            }
            EditorGUILayout.LabelField(m_languageCountText);
            EditorGUILayout.LabelField(m_fieldCountText);

            // Debug: DrawDefaultInspector();
        }

        private void UpdateLabelText()
        {
            m_languageCount = m_textTable.languages.Count;
            m_fieldCount = m_textTable.fields.Count;
            m_languageCountText = "Languages: " + m_languageCount;
            m_fieldCountText = "Fields: " + m_fieldCount;
        }

    }
}
