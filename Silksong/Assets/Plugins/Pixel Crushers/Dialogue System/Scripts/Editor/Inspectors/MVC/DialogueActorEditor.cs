// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    [CustomEditor(typeof(DialogueActor), true)]
    [CanEditMultipleObjects]
    public class DialogueActorEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("This optional component specifies which actor in your dialogue database is associated with this GameObject. You can also specify other subtitle and bark settings.", MessageType.None);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("actor"), true);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("persistentDataName"), true);
            if (GUILayout.Button(new GUIContent("Unique", "Assign a unique persistent data name."), GUILayout.Width(60)))
            {
                AssignUniquePersistentDataName();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("spritePortrait"), new GUIContent("Portrait (Sprite)"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("portrait"), new GUIContent("Portrait (Texture2D)"), true);

            var barkUISettingsProperty = serializedObject.FindProperty("barkUISettings");
            barkUISettingsProperty.isExpanded = EditorGUILayout.Foldout(barkUISettingsProperty.isExpanded, "Bark UI Settings");
            if (barkUISettingsProperty.isExpanded)
            {
                var barkUIProperty = barkUISettingsProperty.FindPropertyRelative("barkUI");
                EditorGUILayout.PropertyField(barkUIProperty, true);
                if (barkUIProperty.objectReferenceValue != null)
                {
                    EditorGUILayout.PropertyField(barkUISettingsProperty.FindPropertyRelative("barkUIOffset"), true);
                }
            }

            var standardUISettingsProperty = serializedObject.FindProperty("standardDialogueUISettings");
            standardUISettingsProperty.isExpanded = EditorGUILayout.Foldout(standardUISettingsProperty.isExpanded, "Dialogue UI Settings");
            if (standardUISettingsProperty.isExpanded)
            {
                var subtitlePanelNumberProperty = standardUISettingsProperty.FindPropertyRelative("subtitlePanelNumber");
                EditorGUILayout.PropertyField(subtitlePanelNumberProperty, true);
                if (subtitlePanelNumberProperty.enumValueIndex == (int)SubtitlePanelNumber.Custom)
                {
                    EditorGUILayout.PropertyField(standardUISettingsProperty.FindPropertyRelative("customSubtitlePanel"), true);
                    EditorGUILayout.PropertyField(standardUISettingsProperty.FindPropertyRelative("customSubtitlePanelOffset"), true);
                }
                var menuPanelNumberProperty = standardUISettingsProperty.FindPropertyRelative("menuPanelNumber");
                EditorGUILayout.PropertyField(menuPanelNumberProperty, true);
                if (menuPanelNumberProperty.enumValueIndex == (int)MenuPanelNumber.Custom)
                {
                    EditorGUILayout.PropertyField(standardUISettingsProperty.FindPropertyRelative("customMenuPanel"), true);
                    EditorGUILayout.PropertyField(standardUISettingsProperty.FindPropertyRelative("customMenuPanelOffset"), true);
                }
                if (menuPanelNumberProperty.enumValueIndex != (int)MenuPanelNumber.Default)
                {
                    EditorGUILayout.PropertyField(standardUISettingsProperty.FindPropertyRelative("useMenuPanelFor"), true);
                }
                EditorGUILayout.PropertyField(standardUISettingsProperty.FindPropertyRelative("portraitAnimatorController"));
                var setSubtitleColorProperty = standardUISettingsProperty.FindPropertyRelative("setSubtitleColor");
                EditorGUILayout.PropertyField(setSubtitleColorProperty, true);
                if (setSubtitleColorProperty.boolValue)
                {
                    var applyColorToPrependedNameProperty = standardUISettingsProperty.FindPropertyRelative("applyColorToPrependedName");
                    EditorGUILayout.PropertyField(applyColorToPrependedNameProperty, true);
                    if (applyColorToPrependedNameProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(standardUISettingsProperty.FindPropertyRelative("prependActorNameSeparator"), true);
                        EditorGUILayout.PropertyField(standardUISettingsProperty.FindPropertyRelative("prependActorNameFormat"), true);
                    }
                    EditorGUILayout.PropertyField(standardUISettingsProperty.FindPropertyRelative("subtitleColor"), true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void AssignUniquePersistentDataName()
        {
            serializedObject.ApplyModifiedProperties();
            foreach (var t in targets)
            {
                var dialogueActor = t as DialogueActor;
                if (dialogueActor == null) continue;
                Undo.RecordObject(dialogueActor, "Unique ID");
                dialogueActor.persistentDataName = DialogueLua.StringToTableIndex(DialogueActor.GetActorName(dialogueActor.transform) + "_" + dialogueActor.GetInstanceID());
                EditorUtility.SetDirty(dialogueActor);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(dialogueActor.gameObject.scene);
            }
            serializedObject.Update();
        }

    }

}
