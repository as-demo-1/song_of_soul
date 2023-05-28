// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(DialogueSystemTrigger), true)]
    public class DialogueSystemTriggerEditor : Editor
    {

        protected const string InspectorEditorPrefsKey = "PixelCrushers.DialogueSystem.DialogueSystemTriggerPrefs";

        [Serializable]
        public class Foldouts
        {
            public bool conditionFoldout = false;
            public bool actionFoldout = false;
            public bool questFoldout = false;
            public bool luaFoldout = false;
            public bool alertFoldout = false;
            public bool sendMessageFoldout = false;
            public bool barkFoldout = false;
            public bool conversationFoldout = false;
            public bool sequenceFoldout = false;
            public bool setActiveFoldout = false;
            public bool setEnabledFoldout = false;
            public bool setAnimatorStateFoldout = false;
            public bool unityEventFoldout = false;
        }

        protected Foldouts foldouts = null;
        protected ReorderableList sendMessageList = null;
        protected ReorderableList setActiveList = null;
        protected ReorderableList setEnabledList = null;
        protected ReorderableList setAnimatorStateList = null;
        protected QuestPicker questPicker = null;
        protected LuaScriptWizard luaScriptWizard = null;
        protected Rect sequenceRect;
        protected DialogueEntryPicker entryPicker = null;
        protected string[] conversationTitles = null;

        protected DialogueSystemTrigger trigger;
        protected SerializedProperty triggerProperty;

        protected bool showSetQuestStateAction;
        protected bool showRunLuaCodeAction;
        protected bool showPlaySequenceAction;
        protected bool showAlertAction;
        protected bool showSendMessagesAction;
        protected bool showBarkAction;
        protected bool showConversationAction;
        protected bool showSetActiveAction;
        protected bool showSetEnabledAction;
        protected bool showAnimatorStatesAction;
        protected bool showUnityEventAction;

        protected SequenceSyntaxState sequenceSyntaxState = SequenceSyntaxState.Unchecked;

        public virtual void OnEnable()
        {
            var trigger = target as DialogueSystemTrigger;
            if (trigger == null) return;
            if (trigger.selectedDatabase == null)
            {
                if (EditorTools.selectedDatabase == null) EditorTools.selectedDatabase = EditorTools.FindInitialDatabase();
                trigger.selectedDatabase = EditorTools.selectedDatabase;
            }
            else
            {
                EditorTools.selectedDatabase = trigger.selectedDatabase;
            }
            luaScriptWizard = new LuaScriptWizard(trigger.selectedDatabase);
            questPicker = new QuestPicker(trigger.selectedDatabase, trigger.questName, trigger.useQuestNamePicker);
            questPicker.showReferenceDatabase = false;

            showSetQuestStateAction = !string.IsNullOrEmpty(trigger.questName);
            showRunLuaCodeAction = !string.IsNullOrEmpty(trigger.luaCode);
            showPlaySequenceAction = !string.IsNullOrEmpty(trigger.sequence); // || trigger.sequenceSpeaker != null || trigger.sequenceListener != null;
            showAlertAction = !string.IsNullOrEmpty(trigger.alertMessage); // || trigger.alertDuration > 0;
            showSendMessagesAction = trigger.sendMessages.Length > 0;
            showBarkAction = !string.IsNullOrEmpty(trigger.barkConversation) || !string.IsNullOrEmpty(trigger.barkText) || !string.IsNullOrEmpty(trigger.barkTextSequence); // || trigger.barker != null;
            showConversationAction = !string.IsNullOrEmpty(trigger.conversation); // || trigger.conversationActor != null || trigger.conversationConversant != null;
            showSetActiveAction = trigger.setActiveActions.Length > 0;
            showSetEnabledAction = trigger.setEnabledActions.Length > 0;
            showAnimatorStatesAction = trigger.setAnimatorStateActions.Length > 0;
            showUnityEventAction = trigger.onExecute.GetPersistentEventCount() > 0;

            sendMessageList = new ReorderableList(serializedObject, serializedObject.FindProperty("sendMessages"), true, true, true, true);
            sendMessageList.drawHeaderCallback = OnDrawSendMessageListHeader;
            sendMessageList.drawElementCallback = OnDrawSendMessageListElement;

            setActiveList = new ReorderableList(serializedObject, serializedObject.FindProperty("setActiveActions"), true, true, true, true);
            setActiveList.drawHeaderCallback = OnDrawSetActiveListHeader;
            setActiveList.drawElementCallback = OnDrawSetActiveListElement;

            setEnabledList = new ReorderableList(serializedObject, serializedObject.FindProperty("setEnabledActions"), true, true, true, true);
            setEnabledList.drawHeaderCallback = OnDrawSetEnabledListHeader;
            setEnabledList.drawElementCallback = OnDrawSetEnabledListElement;

            setAnimatorStateList = new ReorderableList(serializedObject, serializedObject.FindProperty("setAnimatorStateActions"), true, true, true, true);
            setAnimatorStateList.drawHeaderCallback = OnDrawSetAnimatorStateListHeader;
            setAnimatorStateList.drawElementCallback = OnDrawSetAnimatorStateListElement;

            foldouts = EditorPrefs.HasKey(InspectorEditorPrefsKey) ? JsonUtility.FromJson<Foldouts>(EditorPrefs.GetString(InspectorEditorPrefsKey)) : new Foldouts();
            if (foldouts == null) foldouts = new Foldouts();

        }

        protected virtual void OnDisable()
        {
            EditorPrefs.SetString(InspectorEditorPrefsKey, JsonUtility.ToJson(foldouts));
        }

        public override void OnInspectorGUI()
        {
            trigger = target as DialogueSystemTrigger;
            if (trigger == null) return;
            serializedObject.Update();
            DrawTopInfo();
            DrawConditions();
            DrawActions();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawTopInfo()
        {
            // Trigger event:
            triggerProperty = serializedObject.FindProperty("trigger");
            EditorGUILayout.PropertyField(triggerProperty, true);

            // HelpBox for OnTrigger/Collision:
            var isPhysicsEvent =
                triggerProperty.enumValueIndex == 3 || //DialogueSystemTriggerEvent.OnTriggerEnter
                triggerProperty.enumValueIndex == 7 || //DialogueSystemTriggerEvent.OnTriggerExit
                triggerProperty.enumValueIndex == 11 || //DialogueSystemTriggerEvent.OnCollisionEnter
                triggerProperty.enumValueIndex == 12;   //DialogueSystemTriggerEvent.OnCollisionExit
            if (isPhysicsEvent)
            {
                var acceptedTags = serializedObject.FindProperty("condition").FindPropertyRelative("acceptedTags");
                if (acceptedTags.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("You may want to set Conditions > Accepted Tags to observe collisions only with GameObjects with specific tags such as 'Player'. Otherwise this trigger may fire for unintended collisions such as world geometry.", MessageType.Info);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add 'Player' Tag", EditorStyles.miniButton, EditorTools.GUILayoutButtonWidth("Add 'Player' Tag")))
                    {
                        acceptedTags.arraySize = 1;
                        acceptedTags.GetArrayElementAtIndex(0).stringValue = "Player";
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            // Reference database:
            var databaseProperty = serializedObject.FindProperty("selectedDatabase");
            var oldDatabase = databaseProperty.objectReferenceValue;
            EditorGUILayout.PropertyField(databaseProperty, new GUIContent("Reference Database", "Database to use for pop-up menus. Assumes this database will be in memory at runtime."), true);
            var newDatabase = databaseProperty.objectReferenceValue as DialogueDatabase;
            if (newDatabase != oldDatabase)
            {
                luaScriptWizard = new LuaScriptWizard(newDatabase);
                questPicker = new QuestPicker(newDatabase, trigger.questName, trigger.useQuestNamePicker);
                questPicker.showReferenceDatabase = false;
            }
            if (newDatabase != null) EditorTools.selectedDatabase = newDatabase;
        }

        protected virtual void DrawConditions()
        {
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
        }

        protected virtual void DrawActions()
        {
            foldouts.actionFoldout = EditorWindowTools.EditorGUILayoutFoldout("Actions", "Perform these actions when trigger fires.", foldouts.actionFoldout);
            if (foldouts.actionFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();

                    EditorGUILayout.BeginHorizontal();
                    if (showSetQuestStateAction || showRunLuaCodeAction || showPlaySequenceAction || showAlertAction || showSendMessagesAction || showBarkAction ||
                        showConversationAction || showSetActiveAction || showSetEnabledAction || showAnimatorStatesAction || showUnityEventAction)
                    {
                        EditorGUILayout.LabelField("Actions are performed in this order:", EditorTools.GUILayoutLabelWidth("Actions are performed in this order:"));
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Click Add Action:", EditorTools.GUILayoutLabelWidth("Click Add Action to add new action types:"));
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginDisabledGroup(showSetQuestStateAction && showRunLuaCodeAction && showPlaySequenceAction && showAlertAction && showSendMessagesAction && showBarkAction &&
                        showConversationAction && showSetActiveAction && showSetEnabledAction && showAnimatorStatesAction && showUnityEventAction);
                    if (GUILayout.Button("Add Action", EditorTools.GUILayoutButtonWidth("Add Action"))) ShowAddActionMenu();
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();

                    if (showSetQuestStateAction) DrawQuestAction();
                    if (showRunLuaCodeAction) DrawLuaAction();
                    if (showPlaySequenceAction) DrawSequenceAction();
                    if (showAlertAction) DrawAlertAction();
                    if (showSendMessagesAction) DrawSendMessageAction();
                    if (showBarkAction) DrawBarkAction();
                    if (showConversationAction) DrawConversationAction();
                    if (showSetActiveAction) DrawSetActiveAction();
                    if (showSetEnabledAction) DrawSetEnabledAction();
                    if (showAnimatorStatesAction) DrawSetAnimatorStateAction();
                    if (showUnityEventAction) DrawUnityEventAction();
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        protected virtual void ShowAddActionMenu()
        {
            var menu = new GenericMenu();
            if (showSetQuestStateAction) menu.AddDisabledItem(new GUIContent("Set Quest State")); else menu.AddItem(new GUIContent("Set Quest State"), false, AddSetQuestStateAction);
            if (showRunLuaCodeAction) menu.AddDisabledItem(new GUIContent("Run Lua Code")); else menu.AddItem(new GUIContent("Run Lua Code"), false, AddLuaCodeAction);
            if (showPlaySequenceAction) menu.AddDisabledItem(new GUIContent("Play Sequence")); else menu.AddItem(new GUIContent("Play Sequence"), false, AddPlaySequenceAction);
            if (showAlertAction) menu.AddDisabledItem(new GUIContent("Show Alert")); else menu.AddItem(new GUIContent("Show Alert"), false, AddShowAlertAction);
            if (showSendMessagesAction) menu.AddDisabledItem(new GUIContent("Send Messages")); else menu.AddItem(new GUIContent("Send Messages"), false, AddSendMessagesAction);
            if (showBarkAction) menu.AddDisabledItem(new GUIContent("Bark")); else menu.AddItem(new GUIContent("Bark"), false, AddBarkAction);
            if (showConversationAction) menu.AddDisabledItem(new GUIContent("Start Conversation")); else menu.AddItem(new GUIContent("Start Conversation"), false, AddConversationAction);
            if (showSetActiveAction) menu.AddDisabledItem(new GUIContent("Set GameObjects Active or Inactive")); else menu.AddItem(new GUIContent("Set GameObjects Active or Inactive"), false, AddSetActiveAction);
            if (showSetEnabledAction) menu.AddDisabledItem(new GUIContent("Set Components Enabled or Disabled")); else menu.AddItem(new GUIContent("Set Components Enabled or Disabled"), false, AddSetEnabledAction);
            if (showAnimatorStatesAction) menu.AddDisabledItem(new GUIContent("Set Animator States")); else menu.AddItem(new GUIContent("Set Animator States"), false, AddShowAnimatorStatesAction);
            if (showUnityEventAction) menu.AddDisabledItem(new GUIContent("OnExecute() UnityEvent")); else menu.AddItem(new GUIContent("OnExecute() UnityEvent"), false, AddUnityEventAction);
            menu.ShowAsContext();
        }

        protected virtual void AddSetQuestStateAction()
        {
            showSetQuestStateAction = true;
            foldouts.questFoldout = true;
        }

        protected virtual void AddLuaCodeAction()
        {
            showRunLuaCodeAction = true;
            foldouts.luaFoldout = true;
        }

        protected virtual void AddPlaySequenceAction()
        {
            showPlaySequenceAction = true;
            foldouts.sequenceFoldout = true;
        }

        protected virtual void AddShowAlertAction()
        {
            showAlertAction = true;
            foldouts.alertFoldout = true;
        }

        protected virtual void AddSendMessagesAction()
        {
            showSendMessagesAction = true;
            foldouts.sendMessageFoldout = true;
        }

        protected virtual void AddBarkAction()
        {
            showBarkAction = true;
            foldouts.barkFoldout = true;
        }

        protected virtual void AddConversationAction()
        {
            showConversationAction = true;
            foldouts.conversationFoldout = true;
        }

        protected virtual void AddSetActiveAction()
        {
            showSetActiveAction = true;
            foldouts.setActiveFoldout = true;
        }

        protected virtual void AddSetEnabledAction()
        {
            showSetEnabledAction = true;
            foldouts.setEnabledFoldout = true;
        }

        protected virtual void AddShowAnimatorStatesAction()
        {
            showAnimatorStatesAction = true;
            foldouts.setAnimatorStateFoldout = true;
        }

        protected virtual void AddUnityEventAction()
        {
            showUnityEventAction = true;
            foldouts.unityEventFoldout = true;
        }

        protected virtual void DrawQuestAction()
        {
            foldouts.questFoldout = EditorWindowTools.EditorGUILayoutFoldout("Set Quest State", "Set quest states.", foldouts.questFoldout, false);
            if (foldouts.questFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    // Quest picker:
                    if (questPicker != null)
                    {
                        serializedObject.ApplyModifiedProperties();
                        questPicker.Draw();
                        var hadQuestName = !string.IsNullOrEmpty(trigger.questName);
                        trigger.questName = questPicker.currentQuest;
                        trigger.useQuestNamePicker = questPicker.usePicker;
                        trigger.selectedDatabase = questPicker.database;
                        if (EditorTools.selectedDatabase == null) EditorTools.selectedDatabase = trigger.selectedDatabase;
                        if (hadQuestName && string.IsNullOrEmpty(trigger.questName)) showSetQuestStateAction = false;
                        serializedObject.Update();
                    }

                    // Quest state:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("setQuestState"), true);
                    if (serializedObject.FindProperty("setQuestState").boolValue)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("questState"), true);
                    }

                    // Quest entry state:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("setQuestEntryState"), true);
                    if (serializedObject.FindProperty("setQuestEntryState").boolValue)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("questEntryNumber"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("questEntryState"), true);

                        // Additional quest entry state:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("setAnotherQuestEntryState"), true);
                        if (serializedObject.FindProperty("setAnotherQuestEntryState").boolValue)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("anotherQuestEntryNumber"), true);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("anotherQuestEntryState"), true);
                        }
                    }
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        protected virtual void DrawLuaAction()
        {
            foldouts.luaFoldout = EditorWindowTools.EditorGUILayoutFoldout("Run Lua Code", "Run Lua code.", foldouts.luaFoldout, false);
            if (foldouts.luaFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    if (EditorTools.selectedDatabase != luaScriptWizard.database)
                    {
                        luaScriptWizard.database = EditorTools.selectedDatabase;
                        luaScriptWizard.RefreshWizardResources();
                    }
                    serializedObject.ApplyModifiedProperties();
                    EditorGUI.BeginChangeCheck();
                    var newLuaCode = luaScriptWizard.Draw(new GUIContent("Lua Code", "The Lua code to run when the condition is true."), trigger.luaCode);
                    var changed = EditorGUI.EndChangeCheck();
                    serializedObject.Update();
                    if (changed) serializedObject.FindProperty("luaCode").stringValue = newLuaCode;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(14)))
                    {
                        serializedObject.FindProperty("luaCode").stringValue = string.Empty;
                        showRunLuaCodeAction = false;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        protected virtual void DrawSequenceAction()
        {
            foldouts.sequenceFoldout = EditorWindowTools.EditorGUILayoutFoldout("Play Sequence", "Play a sequence.", foldouts.sequenceFoldout, false);
            if (foldouts.sequenceFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    EditorGUILayout.BeginHorizontal();
                    if (DialogueTriggerEventDrawer.IsEnableOrStartEnumIndex(triggerProperty.enumValueIndex))
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("waitOneFrameOnStartOrEnable"), new GUIContent("Wait 1 Frame", "Tick to wait one frame to allow other components to finish their OnStart/OnEnable"), true);

                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(14)))
                    {
                        serializedObject.FindProperty("sequence").stringValue = string.Empty;
                        showPlaySequenceAction = false;
                    }
                    EditorGUILayout.EndHorizontal();
                    serializedObject.ApplyModifiedProperties();
                    EditorGUI.BeginChangeCheck();
                    var newSequence = SequenceEditorTools.DrawLayout(new GUIContent("Sequence"), trigger.sequence, ref sequenceRect, ref sequenceSyntaxState);
                    var changed = EditorGUI.EndChangeCheck();
                    serializedObject.Update();
                    if (changed) serializedObject.FindProperty("sequence").stringValue = newSequence;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sequenceSpeaker"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("sequenceListener"), true);
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        protected virtual void DrawAlertAction()
        {
            foldouts.alertFoldout = EditorWindowTools.EditorGUILayoutFoldout("Show Alert", "Show an alert message.", foldouts.alertFoldout, false);
            if (foldouts.alertFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("alertMessage"), true);
                    if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(14)))
                    {
                        serializedObject.FindProperty("alertMessage").stringValue = string.Empty;
                        showAlertAction = false;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("textTable"), true);
                    var alertDurationProperty = serializedObject.FindProperty("alertDuration");
                    bool specifyAlertDuration = !Mathf.Approximately(0, alertDurationProperty.floatValue);
                    specifyAlertDuration = EditorGUILayout.Toggle(new GUIContent("Specify Duration", "Tick to specify an alert duration; untick to use the default"), specifyAlertDuration);
                    if (specifyAlertDuration)
                    {
                        if (Mathf.Approximately(0, alertDurationProperty.floatValue)) alertDurationProperty.floatValue = 5;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("alertDuration"), true);
                    }
                    else
                    {
                        alertDurationProperty.floatValue = 0;
                    }
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        protected virtual void DrawBarkAction()
        {
            foldouts.barkFoldout = EditorWindowTools.EditorGUILayoutFoldout("Bark", "Bark.", foldouts.barkFoldout, false);
            if (foldouts.barkFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    EditorGUILayout.BeginHorizontal();
                    var barkSourceProperty = serializedObject.FindProperty("barkSource");
                    EditorGUILayout.PropertyField(barkSourceProperty, true);
                    if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(14)))
                    {
                        serializedObject.FindProperty("barkSource").enumValueIndex = 0;
                        serializedObject.FindProperty("barkConversation").stringValue = string.Empty;
                        serializedObject.FindProperty("barkText").stringValue = string.Empty;
                        serializedObject.FindProperty("barkTextSequence").stringValue = string.Empty;
                        showBarkAction = false;
                    }
                    EditorGUILayout.EndHorizontal();
                    switch ((DialogueSystemTrigger.BarkSource)barkSourceProperty.enumValueIndex)
                    {
                        case DialogueSystemTrigger.BarkSource.Conversation:
                            var barkConversationProperty = serializedObject.FindProperty("barkConversation");
                            EditorGUILayout.PropertyField(barkConversationProperty, true);
                            if (!string.IsNullOrEmpty(barkConversationProperty.stringValue))
                            {
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("barkOrder"), true);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("barker"), true);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("barkTarget"), true);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("skipBarkIfNoValidEntries"), true);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowBarksDuringConversations"), true);
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("cacheBarkLines"), true);
                            }
                            break;
                        case DialogueSystemTrigger.BarkSource.Text:
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("barkText"), true);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("barkTextSequence"), true);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("barker"), true);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("barkTarget"), true);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("allowBarksDuringConversations"), true);
                            break;
                    }
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        protected virtual void DrawConversationAction()
        {
            foldouts.conversationFoldout = EditorWindowTools.EditorGUILayoutFoldout("Start Conversation", "Start a conversation.", foldouts.conversationFoldout, false);
            if (foldouts.conversationFoldout)
            {
                try
                {
                    var conversationProperty = serializedObject.FindProperty("conversation");
                    var hadConversation = !string.IsNullOrEmpty(conversationProperty.stringValue);
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(conversationProperty, true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        conversationTitles = null;
                    }
                    if (string.IsNullOrEmpty(conversationProperty.stringValue))
                    {
                        if (hadConversation) showConversationAction = false;
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("conversationActor"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("conversationConversant"), true);

                        var entryIDProperty = serializedObject.FindProperty("startConversationEntryID");
                        var entryTitleProperty = serializedObject.FindProperty("startConversationEntryTitle");
                        var specifyEntryID = EditorGUILayout.Toggle(new GUIContent("Specify Starting Entry", "Start conversation at a specific entry ID."), (entryIDProperty.intValue != -1));
                        if (specifyEntryID)
                        {
                            // Draw entry ID picker:
                            if (entryPicker == null)
                            {
                                entryPicker = new DialogueEntryPicker(conversationProperty.stringValue);
                            }
                            if (entryPicker.isValid)
                            {
                                entryIDProperty.intValue = Mathf.Max(0, entryPicker.DoLayout("Entry ID", entryIDProperty.intValue));
                            }
                            else
                            {
                                entryIDProperty.intValue = Mathf.Max(0, EditorGUILayout.IntField(new GUIContent("Entry ID", "Start at this entry ID."), entryIDProperty.intValue));
                            }
                            if (entryIDProperty.intValue > 0) entryTitleProperty.stringValue = string.Empty;

                            // Draw entry title picker:
                            if (conversationTitles == null)
                            {
                                conversationTitles = GetUniqueTitles(conversationProperty.stringValue);
                            }
                            var titleIndex = (entryIDProperty.intValue <= 0) ? GetTitleIndex(conversationTitles, entryTitleProperty.stringValue) : -1;
                            EditorGUI.BeginChangeCheck();
                            titleIndex = EditorGUILayout.Popup(new GUIContent("Entry Title", "Start at entry with this Title."), titleIndex, conversationTitles);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (0 <= titleIndex && titleIndex < conversationTitles.Length)
                                {
                                    entryTitleProperty.stringValue = conversationTitles[titleIndex];
                                    entryIDProperty.intValue = 0;
                                }
                            }
                        }
                        else
                        {
                            entryIDProperty.intValue = -1;
                            entryTitleProperty.stringValue = string.Empty;
                        }

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("exclusive"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("replace"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("skipIfNoValidEntries"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("preventRestartOnSameFrameEnded"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("stopConversationOnTriggerExit"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("stopConversationIfTooFar"), true);
                        if (serializedObject.FindProperty("stopConversationIfTooFar").boolValue)
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxConversationDistance"), true);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("monitorConversationDistanceFrequency"), true);
                        }
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("showCursorDuringConversation"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("pauseGameDuringConversation"), true);
                    }
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        protected string[] GetUniqueTitles(string conversationTitle)
        {
            var list = new List<string>();
            if (trigger.selectedDatabase != null)
            {
                var conversation = trigger.selectedDatabase.GetConversation(conversationTitle);
                if (conversation != null)
                {
                    foreach (var entry in conversation.dialogueEntries)
                    {
                        var title = entry.Title;
                        if (!list.Contains(title))
                        {
                            list.Add(title);
                        }
                    }
                }
            }
            return list.ToArray();
        }

        protected int GetTitleIndex(string[] titles, string currentTitle)
        {
            if (string.IsNullOrEmpty(currentTitle) || titles == null) return -1;
            for (int i = 0; i < titles.Length; i++)
            {
                if (string.Equals(currentTitle, titles[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        protected virtual void DrawUnityEventAction()
        {
            foldouts.unityEventFoldout = EditorWindowTools.EditorGUILayoutFoldout("OnExecute() UnityEvent", "Connect other events in the Inspector.", foldouts.unityEventFoldout, false);
            if (foldouts.unityEventFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("onExecute"), true);
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        protected virtual void DrawSendMessageAction()
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

        protected void OnDrawSendMessageListHeader(Rect rect)
        {
            var hd = 14f;
            var fw = (rect.width - hd) / 3;
            EditorGUI.LabelField(new Rect(rect.x + hd, rect.y, fw, rect.height), new GUIContent("Recipient", "GameObject to send message to (i.e, call script method)."));
            EditorGUI.LabelField(new Rect(rect.x + hd + fw, rect.y, fw, rect.height), new GUIContent("Message", "Method name in a script on recipient."));
            EditorGUI.LabelField(new Rect(rect.x + hd + 2 * fw, rect.y, fw, rect.height), new GUIContent("Parameter", "Optional string parameter to pass to method."));
        }

        protected void OnDrawSendMessageListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < sendMessageList.count)) return;
            var element = sendMessageList.serializedProperty.GetArrayElementAtIndex(index);
            var fw = rect.width / 3;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, fw, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("gameObject"), GUIContent.none, true);
            EditorGUI.PropertyField(new Rect(rect.x + fw, rect.y, fw - 2, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("message"), GUIContent.none, true);
            EditorGUI.PropertyField(new Rect(rect.x + 2 * fw, rect.y, fw, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("parameter"), GUIContent.none, true);
        }

        protected virtual void DrawSetActiveAction()
        {
            foldouts.setActiveFoldout = EditorWindowTools.EditorGUILayoutFoldout("Set GameObjects Active/Inactive", "Set GameObjects active or inactive.", foldouts.setActiveFoldout, false);
            if (foldouts.setActiveFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    if (setActiveList.count > 0) EditorGUILayout.LabelField("Click on a row to edit its conditions.");
                    setActiveList.DoLayoutList();
                    if (0 <= setActiveList.index && setActiveList.index < setActiveList.count)
                    {
                        EditorWindowTools.EditorGUILayoutBeginIndent();
                        EditorGUILayout.PropertyField(setActiveList.serializedProperty.GetArrayElementAtIndex(setActiveList.index).FindPropertyRelative("condition"), true);
                        EditorWindowTools.EditorGUILayoutEndIndent();
                    }
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        protected const float ToggleWidth = 64;

        protected void OnDrawSetActiveListHeader(Rect rect)
        {
            var hd = 14f;
            EditorGUI.LabelField(new Rect(rect.x + hd, rect.y, rect.width - hd - 2 - ToggleWidth, rect.height), new GUIContent("GameObject", "GameObject to set active/inactive."));
            EditorGUI.LabelField(new Rect(rect.x + rect.width - ToggleWidth, rect.y, ToggleWidth, rect.height), new GUIContent("State", "State to set the target GameObject."));
        }

        protected void OnDrawSetActiveListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (!(0 <= index && index < setActiveList.count)) return;
            var element = setActiveList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 2 - ToggleWidth, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("target"), GUIContent.none, true);
            EditorGUI.PropertyField(new Rect(rect.x + rect.width - ToggleWidth, rect.y, ToggleWidth, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("state"), GUIContent.none, true);
        }

        protected virtual void DrawSetEnabledAction()
        {
            foldouts.setEnabledFoldout = EditorWindowTools.EditorGUILayoutFoldout("Set Components Enabled/Disabled", "Set components active or inactive.", foldouts.setEnabledFoldout, false);
            if (foldouts.setEnabledFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    if (setEnabledList.count > 0) EditorGUILayout.LabelField("Click on a row to edit its conditions.");
                    setEnabledList.DoLayoutList();
                    if (0 <= setEnabledList.index && setEnabledList.index < setEnabledList.count)
                    {
                        EditorWindowTools.EditorGUILayoutBeginIndent();
                        EditorGUILayout.PropertyField(setEnabledList.serializedProperty.GetArrayElementAtIndex(setEnabledList.index).FindPropertyRelative("condition"), true);
                        EditorWindowTools.EditorGUILayoutEndIndent();
                    }
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        protected void OnDrawSetEnabledListHeader(Rect rect)
        {
            var hd = 14f;
            EditorGUI.LabelField(new Rect(rect.x + hd, rect.y, rect.width - hd - 2 - ToggleWidth, rect.height), new GUIContent("Component", "Component to set enabled/disabled."));
            EditorGUI.LabelField(new Rect(rect.x + rect.width - ToggleWidth, rect.y, ToggleWidth, rect.height), new GUIContent("State", "State to set the target component."));
        }

        protected void OnDrawSetEnabledListElement(Rect rect, int index, bool isEnabled, bool isFocused)
        {
            if (!(0 <= index && index < setEnabledList.count)) return;
            var element = setEnabledList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 2 - ToggleWidth, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("target"), GUIContent.none, true);
            EditorGUI.PropertyField(new Rect(rect.x + rect.width - ToggleWidth, rect.y, ToggleWidth, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("state"), GUIContent.none, true);
        }

        protected virtual void DrawSetAnimatorStateAction()
        {
            foldouts.setAnimatorStateFoldout = EditorWindowTools.EditorGUILayoutFoldout("Set Animator States", "Set Animator states on one or more GameObjects.", foldouts.setAnimatorStateFoldout, false);
            if (foldouts.setAnimatorStateFoldout)
            {
                try
                {
                    EditorWindowTools.EditorGUILayoutBeginGroup();
                    if (setAnimatorStateList.count > 0) EditorGUILayout.LabelField("Click on a row to edit its conditions.");
                    setAnimatorStateList.DoLayoutList();
                    if (0 <= setAnimatorStateList.index && setAnimatorStateList.index < setAnimatorStateList.count)
                    {
                        EditorWindowTools.EditorGUILayoutBeginIndent();
                        EditorGUILayout.PropertyField(setAnimatorStateList.serializedProperty.GetArrayElementAtIndex(setAnimatorStateList.index).FindPropertyRelative("condition"), true);
                        EditorWindowTools.EditorGUILayoutEndIndent();
                    }
                }
                finally
                {
                    EditorWindowTools.EditorGUILayoutEndGroup();
                }
            }
        }

        protected void OnDrawSetAnimatorStateListHeader(Rect rect)
        {
            var hd = 14f;
            var fw = (rect.width - hd - ToggleWidth - 4) / 2;
            EditorGUI.LabelField(new Rect(rect.x + hd, rect.y, fw, rect.height), new GUIContent("Animator", "GameObject whose animator to control."));
            EditorGUI.LabelField(new Rect(rect.x + hd + fw + 2, rect.y, fw, rect.height), new GUIContent("State", "Animator state name to crossfade into."));
            EditorGUI.LabelField(new Rect(rect.x + rect.width - ToggleWidth, rect.y, ToggleWidth, rect.height), new GUIContent("Fade", "Crossfade duration in seconds."));
        }

        protected void OnDrawSetAnimatorStateListElement(Rect rect, int index, bool isEnabled, bool isFocused)
        {
            if (!(0 <= index && index < setAnimatorStateList.count)) return;
            var fw = (rect.width - ToggleWidth - 4) / 2;
            var element = setAnimatorStateList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, fw, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("target"), GUIContent.none, true);
            EditorGUI.PropertyField(new Rect(rect.x + fw + 2, rect.y, fw, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("stateName"), GUIContent.none, true);
            EditorGUI.PropertyField(new Rect(rect.x + rect.width - ToggleWidth, rect.y, ToggleWidth, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("crossFadeDuration"), GUIContent.none, true);
        }

    }
}
