// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(StandardUIMenuPanel), true)]
    public class StandardUIMenuPanelEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("UI Elements", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("panel"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pcName"), new GUIContent("PC Name", "(Optional) Text element to show PC name during response menu."), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pcImage"), new GUIContent("PC Image", "(Optional) Image to show PC portrait during response menu."), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("usePortraitNativeSize"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("timerSlider"), true);

            EditorGUILayout.LabelField("Design-Time Buttons", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buttons"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonAlignment"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showUnusedButtons"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("warnOnEmptyResponseText"), true);

            EditorGUILayout.LabelField("Auto-Created Buttons", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonTemplate"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonTemplateHolder"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonTemplateScrollbar"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scrollbarEnabler"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonTemplateScrollbarResetValue"), true);

            EditorGUILayout.LabelField(new GUIContent("Navigation", "Joystick/keyboard navigation settings."), EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("firstSelected"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("focusCheckFrequency"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("refreshSelectablesFrequency"), true);
            var selectPreviousOnDisableProperty = serializedObject.FindProperty("selectPreviousOnDisable");
            if (selectPreviousOnDisableProperty != null) EditorGUILayout.PropertyField(selectPreviousOnDisableProperty); // Not present in older versions of UIPanel.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explicitNavigationForTemplateButtons"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loopExplicitNavigation"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("autonumber"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("blockInputDuration"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showSelectionWhileInputBlocked"), true);

            EditorGUILayout.LabelField("Visibility", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("startState"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showAnimationTrigger"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("hideAnimationTrigger"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("focusAnimationTrigger"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("unfocusAnimationTrigger"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_hasFocus"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("waitForShowAnimationToSetOpen"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_deactivateOnHidden"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("waitForClose"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onOpen"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onClose"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onClosed"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onFocus"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onUnfocus"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onBackButtonDown"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onContentChanged"), true);

            serializedObject.ApplyModifiedProperties();
        }

    }

}
