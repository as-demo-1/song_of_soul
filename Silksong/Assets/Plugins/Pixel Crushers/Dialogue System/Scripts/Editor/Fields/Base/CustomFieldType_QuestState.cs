using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem
{

    [CustomFieldTypeService.Name("Quest State")]
    public class CustomFieldType_QuestState : CustomFieldType
    {
        private static readonly string[] questStateStrings = { "(None)", "unassigned", "active", "success", "failure", "done", "abandoned", "grantable" };

        public override string Draw(string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUILayout.Popup(GetCurrentIndex(currentValue), questStateStrings);
            return questStateStrings[index];
        }

        public override string Draw(GUIContent label, string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUILayout.Popup(label.text, GetCurrentIndex(currentValue), questStateStrings);
            return questStateStrings[index];
        }

        public override string Draw(Rect rect, string currentValue, DialogueDatabase dataBase)
        {
            int index = EditorGUI.Popup(rect, GetCurrentIndex(currentValue), questStateStrings);
            return questStateStrings[index];
        }

        private int GetCurrentIndex(string currentValue)
        {
            if (string.IsNullOrEmpty(currentValue)) currentValue = questStateStrings[0];
            for (int i = 0; i < questStateStrings.Length; i++)
            {
                var item = questStateStrings[i];
                if (item == currentValue)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}