// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(QuestTrigger), true)]
    public class QuestTriggerEditor : Editor
    {

        private QuestPicker questPicker = null;
        private LuaScriptWizard luaScriptWizard = null;

        public void OnEnable()
        {
            var trigger = target as QuestTrigger;
            if (trigger != null)
            {
                EditorTools.SetInitialDatabaseIfNull();
                luaScriptWizard = new LuaScriptWizard(EditorTools.selectedDatabase);
                questPicker = new QuestPicker(trigger.selectedDatabase, trigger.questName, trigger.useQuestNamePicker);
            }
        }

        public override void OnInspectorGUI()
        {
            var trigger = target as QuestTrigger;
            if (trigger == null) return;

            serializedObject.Update();
            EditorWindowTools.DrawDeprecatedTriggerHelpBox();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trigger"), true);

            // Quest picker:
            if (questPicker != null)
            {
                serializedObject.ApplyModifiedProperties();
                questPicker.Draw();
                trigger.questName = questPicker.currentQuest;
                trigger.useQuestNamePicker = questPicker.usePicker;
                trigger.selectedDatabase = questPicker.database;
                serializedObject.Update();
            }

            if (trigger.selectedDatabase != null) EditorTools.selectedDatabase = trigger.selectedDatabase;
            if (EditorTools.selectedDatabase == null) EditorTools.selectedDatabase = EditorTools.FindInitialDatabase();

            // Quest state
            var setQuestStateProperty = serializedObject.FindProperty("setQuestState");
            EditorGUILayout.PropertyField(setQuestStateProperty, true);
            if (setQuestStateProperty.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("questState"), true);
            }

            // Quest entry state:
            var setQuestEntryStateProperty = serializedObject.FindProperty("setQuestEntryState");
            EditorGUILayout.PropertyField(setQuestEntryStateProperty, true);
            if (setQuestEntryStateProperty.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("questEntryNumber"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("questEntryState"), true);
            }

            // Lua code / wizard:
            if (EditorTools.selectedDatabase != luaScriptWizard.database)
            {
                luaScriptWizard.database = EditorTools.selectedDatabase;
                luaScriptWizard.RefreshWizardResources();
            }
            serializedObject.ApplyModifiedProperties();
            trigger.luaCode = luaScriptWizard.Draw(new GUIContent("Lua Code", "The Lua code to run when the condition is true"), trigger.luaCode);

            serializedObject.Update();

            // Alert:
            EditorGUILayout.PropertyField(serializedObject.FindProperty("alertMessage"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("localizedTextTable"), true);

            // Send Messages list:
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sendMessages"), true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("once"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("condition"), true);
            serializedObject.ApplyModifiedProperties();
        }

    }

}
