// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    public class VariablePicker
    {

        public DialogueDatabase database = null;
        public string currentVariable = string.Empty;
        public bool usePicker = false;
        public bool showReferenceDatabase = true;

        private string[] titles = null;
        private string[] popupTitles = null;
        private int currentIndex = -1;

        public VariablePicker(DialogueDatabase database, string currentVariable, bool usePicker)
        {
            this.database = database ?? EditorTools.FindInitialDatabase();
            this.currentVariable = currentVariable;
            this.usePicker = usePicker;
            UpdateTitles();
            bool currentVariableIsInDatabase = (database != null) || (currentIndex >= 0);
            if (usePicker && !string.IsNullOrEmpty(currentVariable) && !currentVariableIsInDatabase)
            {
                this.usePicker = false;
            }
        }

        public void UpdateTitles()
        {
            currentIndex = -1;
            if (database == null || database.variables == null)
            {
                titles = new string[0];
            }
            else
            {
                var list = new List<string>();
                var popupList = new List<string>();
                foreach (var variable in database.variables)
                {
                    var variableName = variable.Name;
                    list.Add(variableName);
                    popupList.Add(variableName.Replace(".", "/"));
                }
                titles = list.ToArray();
                popupTitles = popupList.ToArray();
                for (int i = 0; i < titles.Length; i++)
                {
                    if (string.Equals(currentVariable, titles[i]))
                    {
                        currentIndex = i;
                    }
                }
            }
        }

        public void Draw()
        {
            if (showReferenceDatabase)
            {

                // Show with reference database field:
                EditorGUILayout.BeginHorizontal();
                if (usePicker)
                {
                    var newDatabase = EditorGUILayout.ObjectField("Reference Database", database, typeof(DialogueDatabase), false) as DialogueDatabase;
                    if (newDatabase != database)
                    {
                        database = newDatabase;
                        UpdateTitles();
                    }
                }
                else
                {
                    currentVariable = EditorGUILayout.TextField("Variable", currentVariable);
                }
                DrawToggle();
                EditorGUILayout.EndHorizontal();

                if (usePicker)
                {
                    currentIndex = EditorGUILayout.Popup("Variable", currentIndex, popupTitles);
                    if (0 <= currentIndex && currentIndex < titles.Length) currentVariable = titles[currentIndex];
                    if (!showReferenceDatabase)
                    {
                        DrawToggle();
                    }
                }
            }
            else
            {

                // Show without reference database field:
                EditorGUILayout.BeginHorizontal();
                if (usePicker)
                {
                    currentIndex = EditorGUILayout.Popup("Variable", currentIndex, popupTitles);
                    if (0 <= currentIndex && currentIndex < titles.Length) currentVariable = titles[currentIndex];
                }
                else
                {
                    currentVariable = EditorGUILayout.TextField("Variable", currentVariable);
                }
                DrawToggle();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawToggle()
        {
            var newToggleValue = EditorGUILayout.Toggle(usePicker, EditorStyles.radioButton, GUILayout.Width(20));
            if (newToggleValue != usePicker)
            {
                usePicker = newToggleValue;
                if (usePicker && (database == null)) database = EditorTools.FindInitialDatabase();
                UpdateTitles();
            }
        }

        /// <summary>
        /// Draw the picker using the specified position (for EditorGUI instead of EditorGUILayout).
        /// </summary>
        /// <param name="position">Position.</param>
        public void Draw(Rect position)
        {
            int originalIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Draw popup:
            var rect = new Rect(position.x, position.y, position.width - 22, EditorGUIUtility.singleLineHeight);
            if (usePicker)
            {
                currentIndex = EditorGUI.Popup(rect, currentIndex, popupTitles);
                if (0 <= currentIndex && currentIndex < titles.Length) currentVariable = titles[currentIndex];
            }
            else
            {
                currentVariable = EditorGUI.TextField(rect, currentVariable);
            }

            // Draw toggle:
            rect = new Rect(position.x + position.width - 20, position.y, 20, EditorGUIUtility.singleLineHeight);
            var newToggleValue = EditorGUI.Toggle(rect, usePicker, EditorStyles.radioButton);
            if (newToggleValue != usePicker)
            {
                usePicker = newToggleValue;
                if (usePicker && (database == null)) database = EditorTools.FindInitialDatabase();
                UpdateTitles();
            }

            EditorGUI.indentLevel = originalIndentLevel;
        }

    }

}
