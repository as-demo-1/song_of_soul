// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    [CustomPropertyDrawer(typeof(VariablePopupAttribute))]
    public class VariablePopupDrawer : PropertyDrawer
    {

        private DialogueDatabase database = null;
        private string[] names = null;
        private string[] popupNames = null;
        private bool showReferenceDatabase = false;
        private bool usePicker = true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + (showReferenceDatabase ? EditorGUIUtility.singleLineHeight : 0);
        }

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            // Set up property drawer:
            EditorGUI.BeginProperty(position, GUIContent.none, prop);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Show database field if specified:
            showReferenceDatabase = (attribute as VariablePopupAttribute).showReferenceDatabase;
            if (EditorTools.selectedDatabase == null) EditorTools.SetInitialDatabaseIfNull();
            if (showReferenceDatabase)
            {
                var dbPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                position = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, position.height - EditorGUIUtility.singleLineHeight);
                var newDatabase = EditorGUI.ObjectField(dbPosition, EditorTools.selectedDatabase, typeof(DialogueDatabase), true) as DialogueDatabase;
                if (newDatabase != EditorTools.selectedDatabase)
                {
                    EditorTools.selectedDatabase = newDatabase;
                    database = null;
                    names = null;
                    popupNames = null;
                }
            }
            if (EditorTools.selectedDatabase == null) usePicker = false;

            // Set up titles array:
            if (names == null || database != EditorTools.selectedDatabase) UpdateNames(prop.stringValue);

            // Update current index:
            var currentIndex = GetIndex(prop.stringValue);

            // Draw popup or plain text field:
            var rect = new Rect(position.x, position.y, position.width - 48, position.height);
            if (usePicker)
            {
                var newIndex = EditorGUI.Popup(rect, currentIndex, popupNames);
                if ((newIndex != currentIndex) && (0 <= newIndex && newIndex < names.Length))
                {
                    currentIndex = newIndex;
                    prop.stringValue = names[currentIndex];
                    GUI.changed = true;
                }
                if (GUI.Button(new Rect(position.x + position.width - 45, position.y, 18, 14), "x"))
                {
                    prop.stringValue = string.Empty;
                    currentIndex = -1;
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                string value = EditorGUI.TextField(rect, prop.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    prop.stringValue = value;
                }
            }

            // Radio button toggle between popup and plain text field:
            rect = new Rect(position.x + position.width - 22, position.y, 22, position.height);
            var newToggleValue = EditorGUI.Toggle(rect, usePicker, EditorStyles.radioButton);
            if (newToggleValue != usePicker)
            {
                usePicker = newToggleValue;
                if (usePicker && (EditorTools.selectedDatabase == null)) EditorTools.selectedDatabase = EditorTools.FindInitialDatabase();
                names = null;
                popupNames = null;
            }

            EditorGUI.EndProperty();
        }

        public void UpdateNames(string currentSelection)
        {
            database = EditorTools.selectedDatabase;
            var foundCurrent = false;
            var list = new List<string>();
            var popupList = new List<string>();
            if (database != null && database.variables != null)
            {
                for (int i = 0; i < database.variables.Count; i++)
                {
                    var assetName = database.variables[i].Name;
                    list.Add(assetName);
                    popupList.Add(assetName.Replace(".", "/"));
                    if (string.Equals(currentSelection, assetName))
                    {
                        foundCurrent = true;
                    }
                }
                if (!foundCurrent && !string.IsNullOrEmpty(currentSelection))
                {
                    list.Add(currentSelection);
                    popupList.Add(currentSelection.Replace(".", "/"));
                }
            }
            names = list.ToArray();
            popupNames = popupList.ToArray();
        }

        public int GetIndex(string currentSelection)
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Equals(currentSelection, names[i])) return i;
            }
            return -1;
        }

    }

}
