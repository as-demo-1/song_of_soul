// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This utility window acts like a subset of the Dialogue Editor that shows
    /// only variables. Unlike the Dialogue Editor, it also shows runtime values.
    /// </summary>
    public class VariableViewWindow : EditorWindow
    {
        [MenuItem("Tools/Pixel Crushers/Dialogue System/Tools/Variable Viewer", false, -1)]
        public static VariableViewWindow OpenVariableViewWindow()
        {
            var window = GetWindow<VariableViewWindow>("Variables");
            window.OnSelectionChange();
            return window;
        }

        public static VariableViewWindow instance = null;

        private const string DatabaseGUIDPrefsKey = "PixelCrushers.DialogueSystem.VariableViewer.DatabaseGUID";

        [SerializeField]
        private DialogueEditorVariableView variableView;
        private DialogueDatabase database;
        private Template template;
        private Vector2 scrollPosition = Vector3.zero;

        private void OnEnable()
        {
            instance = this;
            minSize = new Vector2(300, 240);
            database = DialogueEditorWindow.GetCurrentlyEditedDatabase();
            if (database == null) EditorTools.FindInitialDatabase();

            if (database == null && EditorPrefs.HasKey(DatabaseGUIDPrefsKey))
            {
                var guid = EditorPrefs.GetString(DatabaseGUIDPrefsKey);
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                database = AssetDatabase.LoadAssetAtPath<DialogueDatabase>(assetPath);
            }
            template = TemplateTools.LoadFromEditorPrefs();
            if (variableView == null) variableView = new DialogueEditorVariableView();
            variableView.Initialize(database, template, false);
        }

        private void OnDisable()
        {
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(database));
            if (database != null) EditorPrefs.SetString(DatabaseGUIDPrefsKey, guid);
            instance = null;
        }

        private void OnSelectionChange()
        {
            var selected = Selection.activeObject as DialogueDatabase;
            if (selected != null && selected != database)
            {
                database = selected;
                variableView.Initialize(database, template, false);
                Repaint();
            }
        }

        public void RefreshView()
        {
            variableView.RefreshView();
            Repaint();
        }

        private void OnGUI()
        {
            try
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                variableView.Draw();
            }
            finally 
            {
                EditorGUILayout.EndScrollView(); 
            }
        }

    }
}
