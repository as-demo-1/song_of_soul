// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This custom property drawer for the QuestState enum shows the states
    /// in lowercase so they match Lua.
    /// </summary>
    [CustomPropertyDrawer(typeof(QuestStateAttribute))]
    public class QuestStateDrawer : PropertyDrawer
    {

        public static string[] stateNames = {
            "unassigned",
            "active",
            "success",
            "failure",
            "abandoned",
            "grantable",
            "returnToNPC"
        };

        public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, prop);
            if (label != GUIContent.none)
            {
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            }

            var newIndex = EditorGUI.Popup(position, prop.enumValueIndex, stateNames);
            if (newIndex != prop.enumValueIndex)
            {
                prop.enumValueIndex = newIndex;
            }

            EditorGUI.EndProperty();
        }

        public static int QuestStateToDrawerIndex(QuestState questState)
        {
            switch (questState)
            {
                default:
                case QuestState.Unassigned: return 0;
                case QuestState.Active: return 1;
                case QuestState.Success: return 2;
                case QuestState.Failure: return 3;
                case QuestState.Abandoned: return 4;
                case QuestState.Grantable: return 5;
                case QuestState.ReturnToNPC: return 6;
            }
        }

        public static QuestState DrawerIndexToQuestState(int index)
        {
            switch (index)
            {
                default:
                case 0: return QuestState.Unassigned;
                case 1: return QuestState.Active;
                case 2: return QuestState.Success;
                case 3: return QuestState.Failure;
                case 4: return QuestState.Abandoned;
                case 5: return QuestState.Grantable;
                case 6: return QuestState.ReturnToNPC;
            }
        }

        public static QuestState RectQuestStatePopup(Rect position, QuestState questState)
        {
            return DrawerIndexToQuestState(EditorGUI.Popup(position, QuestStateToDrawerIndex(questState), stateNames));
        }

        public static QuestState LayoutQuestStatePopup(QuestState questState, float width)
        {
            return DrawerIndexToQuestState(EditorGUILayout.Popup(QuestStateToDrawerIndex(questState), stateNames, GUILayout.Width(96)));
        }

    }

}
