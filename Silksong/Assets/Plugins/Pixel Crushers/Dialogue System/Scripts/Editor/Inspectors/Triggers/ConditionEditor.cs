// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This custom editor for Condition uses LuaConditionWizard.
    /// </summary>
    [System.Serializable]
    public class ConditionEditor
    {

        public bool foldout = false;
        public bool drawReferenceDatabase = true;

        public LuaConditionWizard luaConditionWizard = new LuaConditionWizard(null);
        public string currentLuaWizardContent = string.Empty;

        public ConditionEditor(DialogueDatabase database, bool drawReferenceDatabase)
        {
            EditorTools.selectedDatabase = database ?? EditorTools.FindInitialDatabase();
            this.drawReferenceDatabase = drawReferenceDatabase;
        }

        public void Draw(Condition condition, SerializedObject serializedObject)
        {
            foldout = EditorGUILayout.Foldout(foldout, "Condition Editor");
            if (!foldout || (serializedObject == null)) return;

            serializedObject.Update();

            if (drawReferenceDatabase)
            {
                EditorTools.selectedDatabase = EditorGUILayout.ObjectField(new GUIContent("Reference Database", "Database to use for Lua and Quest conditions"), EditorTools.selectedDatabase, typeof(DialogueDatabase), true) as DialogueDatabase;
            }

            if (serializedObject.FindProperty("condition").FindPropertyRelative("luaConditions").isExpanded)
            {
                luaConditionWizard.database = EditorTools.selectedDatabase;
                if (luaConditionWizard.database != null)
                {
                    if (!luaConditionWizard.IsOpen)
                    {
                        luaConditionWizard.OpenWizard(string.Empty);
                    }
                    currentLuaWizardContent = luaConditionWizard.Draw(new GUIContent("Lua Condition Wizard", "Use to add Lua conditions below"), currentLuaWizardContent, false);
                    if (!luaConditionWizard.IsOpen && !string.IsNullOrEmpty(currentLuaWizardContent))
                    {
                        List<string> luaList = new List<string>(condition.luaConditions);
                        luaList.Add(currentLuaWizardContent);
                        condition.luaConditions = luaList.ToArray();
                        currentLuaWizardContent = string.Empty;
                        luaConditionWizard.OpenWizard(string.Empty);
                    }
                }
            }

            EditorWindowTools.StartIndentedSection();

            SerializedProperty conditions = serializedObject.FindProperty("condition");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(conditions, true);
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();

            EditorWindowTools.EndIndentedSection();
        }

    }

}
