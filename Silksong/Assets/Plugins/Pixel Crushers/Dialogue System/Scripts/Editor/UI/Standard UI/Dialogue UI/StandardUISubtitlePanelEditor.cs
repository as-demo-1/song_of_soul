// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(StandardUISubtitlePanel), true)]
    public class StandardUISubtitlePanelEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("UI Elements", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("panel"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portraitImage"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portraitName"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("subtitleText"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("continueButton"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blockInputDuration"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onlyShowNPCPortraits"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useAnimatedPortraits"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("usePortraitNativeSize"), true);
            var accumulateTextProperty = serializedObject.FindProperty("accumulateText");
            EditorGUILayout.PropertyField(accumulateTextProperty, true);
            if (accumulateTextProperty.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxLines"), true); 
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("addSpeakerName"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("addSpeakerNameFormat"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("delayTypewriterUntilOpen"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clearTextOnConversationStart"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scrollbarEnabler"), true);

            EditorGUILayout.LabelField(new GUIContent("Navigation", "Joystick/keyboard navigation settings."), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("firstSelected"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("focusCheckFrequency"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("refreshSelectablesFrequency"), true);
            var selectPreviousOnDisableProperty = serializedObject.FindProperty("selectPreviousOnDisable");
            if (selectPreviousOnDisableProperty != null) EditorGUILayout.PropertyField(selectPreviousOnDisableProperty); // Not present in older versions of UIPanel.

            EditorGUILayout.LabelField("Visibility", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("visibility"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startState"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showAnimationTrigger"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hideAnimationTrigger"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("focusAnimationTrigger"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unfocusAnimationTrigger"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_hasFocus"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("waitForShowAnimationToSetOpen"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("waitForOpen"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("waitForClose"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_deactivateOnHidden"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clearTextOnClose"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onOpen"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onClose"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onClosed"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onFocus"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onUnfocus"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onBackButtonDown"), true);
            
            serializedObject.ApplyModifiedProperties();
        }

    }

}
