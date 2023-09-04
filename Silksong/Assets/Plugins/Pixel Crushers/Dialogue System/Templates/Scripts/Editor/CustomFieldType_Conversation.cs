/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this field type (Conversation), remove all lines that start
 * [REMOVE THIS LINE] with "[REMOVE THIS LINE]".
 * [REMOVE THIS LINE]
 * [REMOVE THIS LINE] See CustomFieldType_TemplateType.cs for a commented starter template
 * [REMOVE THIS LINE] for your own custom field types.
 * [REMOVE THIS LINE]



using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    [CustomFieldTypeService.Name("Conversation")]
    public class CustomFieldType_Conversation : CustomFieldType
    {
        public GUIContent[] conversationTitles = null;

        public override string Draw(string currentValue, DialogueDatabase database)
        {
            InitializeConversationTitles();
            if (conversationTitles == null)
            {
                return EditorGUILayout.TextField(currentValue);
            }
            EditorGUI.BeginChangeCheck();
            var index = EditorGUILayout.Popup(GUIContent.none, GetIndex(currentValue), conversationTitles);
            if (EditorGUI.EndChangeCheck())
            {
                return (1 <= index && index < conversationTitles.Length) ? conversationTitles[index].text : string.Empty;
            }
            else
            {
                return currentValue;
            }
        }

        public override string Draw(Rect rect, string currentValue, DialogueDatabase database)
        {
            InitializeConversationTitles();
            if (conversationTitles == null)
            {
                return EditorGUILayout.TextField(currentValue);
            }
            EditorGUI.BeginChangeCheck();
            var index = EditorGUI.Popup(rect, GUIContent.none, GetIndex(currentValue), conversationTitles);
            if (EditorGUI.EndChangeCheck())
            {
                return (1 <= index && index < conversationTitles.Length) ? conversationTitles[index].text : string.Empty;
            }
            else
            {
                return currentValue;
            }
        }

        protected void InitializeConversationTitles()
        {
            if (conversationTitles != null) return; // Already initialized.
            var list = new List<GUIContent>();
            list.Add(new GUIContent("(None)"));
            var database = EditorTools.FindInitialDatabase();
            if (database != null)
            {
                foreach (var conversation in database.conversations)
                {
                    list.Add(new GUIContent(conversation.Title));
                }
            }
            conversationTitles = list.ToArray();
        }

        protected int GetIndex(string currentValue)
        {
            if (conversationTitles != null)
            {
                for (int i = 1; i < conversationTitles.Length; i++)
                {
                    if (string.Equals(currentValue, conversationTitles[i]))
                    {
                        return i;
                    }
                }
            }
            return 0;
        }
    }
}



/**/
