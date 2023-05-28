// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(ConditionObserver), true)]
    public class ConditionObserverEditor : Editor
    {

        private const string InspectorEditorPrefsKey = "PixelCrushers.DialogueSystem.ConditionObserverPrefs";

        [Serializable]
        public class Foldouts
        {
            public bool conditionFoldout = true;
            public bool actionFoldout = true;
            public bool sendMessageFoldout = false;
        }

        private Foldouts foldouts = null;
        private LuaScriptWizard luaScriptWizard = null;
        private QuestPicker questPicker = null;
        private Rect sequenceRect;
        private SequenceSyntaxState sequenceSyntaxState = SequenceSyntaxState.Unchecked;
        private ReorderableList sendMessageList = null;

        public void OnEnable()
        {
            var trigger = target as ConditionObserver;
            if (trigger == null) return;
            if (EditorTools.selectedDatabase == null) EditorTools.selectedDatabase = EditorTools.FindInitialDatabase();
            luaScriptWizard = new LuaScriptWizard(EditorTools.selectedDatabase);
            questPicker = new QuestPicker(EditorTools.selectedDatabase, trigger.questName, trigger.useQuestNamePicker);
            questPicker.showReferenceDatabase = false;
            sendMessageList = new ReorderableList(serializedObject, serializedObject.FindProperty("sendMessages"), true, true, true, true);
            sendMessageList.drawHeaderCallback = OnDrawSendMessageListHeader;
            sendMessageList.drawElementCallback = OnDrawSendMessageListElement;
            foldouts = EditorPrefs.HasKey(InspectorEditorPrefsKey) ? JsonUtility.FromJson<Foldouts>(EditorPrefs.GetString(InspectorEditorPrefsKey)) : new Foldouts();
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(InspectorEditorPrefsKey, JsonUtility.ToJson(foldouts));
        }

        public override void OnInspectorGUI()
        {
            var trigger = target as ConditionObserver;
            if (trigger == null) return;

            serializedObject.Update();

            // Reference database:
            EditorTools.selectedDatabase = EditorGUILayout.ObjectField(new GUIContent("Reference Database", "Database to use for pop-up menus. Assumes this database will be in memory at runtime."), EditorTools.selectedDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;

            // Frequency, once, observeGameObject:
            EditorGUILayout.PropertyField(serializedObject.FindProperty("frequency"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("once"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("observeGameObject"), true);

            // Condition:
            foldouts.conditionFoldout = EditorWindowTools.EditorGUILayoutFoldout("Conditions", "Conditions that must be true for trigger to fire.", foldouts.conditionFoldout);
            if (foldouts.conditionFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    ConditionPropertyDrawer.hideMainFoldout = true;
                    var conditionProperty = serializedObject.FindProperty("condition");
                    conditionProperty.isExpanded = true;
                    EditorGUILayout.PropertyField(conditionProperty, true);
                    ConditionPropertyDrawer.hideMainFoldout = false;
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }

            // Action:
            foldouts.actionFoldout = EditorWindowTools.EditorGUILayoutFoldout("Actions", "Actions to run when the observed condition becomes true.", foldouts.actionFoldout);
            if (foldouts.actionFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    serializedObject.ApplyModifiedProperties();

                    // Lua code / wizard:
                    if (EditorTools.selectedDatabase != luaScriptWizard.database)
                    {
                        luaScriptWizard.database = EditorTools.selectedDatabase;
                        luaScriptWizard.RefreshWizardResources();
                    }

                    EditorGUI.BeginChangeCheck();

                    var newLuaCode = luaScriptWizard.Draw(new GUIContent("Lua Code", "The Lua code to run when the condition is true"), trigger.luaCode);

                    // Sequence:
                    var newSequence = SequenceEditorTools.DrawLayout(new GUIContent("Sequence"), trigger.sequence, ref sequenceRect, ref sequenceSyntaxState);

                    // Quest:
                    if (EditorTools.selectedDatabase != questPicker.database)
                    {
                        questPicker.database = EditorTools.selectedDatabase;
                        questPicker.UpdateTitles();
                    }
                    questPicker.Draw();
                    var newQuestName = questPicker.currentQuest;
                    var newUseQuestNamePicker = questPicker.usePicker;

                    var changed = EditorGUI.EndChangeCheck();

                    serializedObject.Update();

                    if (changed)
                    {
                        serializedObject.FindProperty("luaCode").stringValue = newLuaCode;
                        serializedObject.FindProperty("sequence").stringValue = newSequence;
                        serializedObject.FindProperty("questName").stringValue = newQuestName;
                        serializedObject.FindProperty("useQuestNamePicker").boolValue = newUseQuestNamePicker;
                    }

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("questState"), true);

                    // Alert message:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("alertMessage"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("textTable"), true);

                    // Send Messages list:
                    DrawSendMessageAction();
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSendMessageAction()
        {
            foldouts.sendMessageFoldout = EditorWindowTools.EditorGUILayoutFoldout("Send Messages", "Use SendMessage to call methods on one or more GameObjects.", foldouts.sendMessageFoldout, false);
            if (foldouts.sendMessageFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    sendMessageList.DoLayoutList();
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        private void OnDrawSendMessageListHeader(Rect rect)
        {
            var hd = 14f;
            var fw = (rect.width - hd) / 3;
            EditorGUI.LabelField(new Rect(rect.x + hd, rect.y, fw, rect.height), new GUIContent("Recipient", "GameObject to send message to (i.e, call script method)."));
            EditorGUI.LabelField(new Rect(rect.x + hd + fw, rect.y, fw, rect.height), new GUIContent("Message", "Method name in a script on recipient."));
            EditorGUI.LabelField(new Rect(rect.x + hd + 2 * fw, rect.y, fw, rect.height), new GUIContent("Parameter", "Optional string parameter to pass to method."));
        }

        private void OnDrawSendMessageListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < sendMessageList.count)) return;
            var element = sendMessageList.serializedProperty.GetArrayElementAtIndex(index);
            var fw = rect.width / 3;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, fw, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("gameObject"), GUIContent.none, true);
            EditorGUI.PropertyField(new Rect(rect.x + fw, rect.y, fw - 2, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("message"), GUIContent.none, true);
            EditorGUI.PropertyField(new Rect(rect.x + 2 * fw, rect.y, fw, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("parameter"), GUIContent.none, true);
        }

    }

}
