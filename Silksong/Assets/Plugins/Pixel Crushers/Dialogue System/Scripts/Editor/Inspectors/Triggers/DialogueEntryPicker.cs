using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Utility class to choose a dialogue entry ID using a dropdown menu.
    /// </summary>
    public class DialogueEntryPicker
    {
        public bool isValid { get { return entryTexts != null; } }

        private string[] entryTexts = null;
        private Dictionary<int, int> idToIndex = new Dictionary<int, int>();
        private Dictionary<int, int> indexToId = new Dictionary<int, int>();

        public DialogueEntryPicker(string conversationTitle)
        {
            var db = EditorTools.FindInitialDatabase();
            if (db == null) return;
            var conversation = db.GetConversation(conversationTitle);
            if (conversation == null) return;
            entryTexts = new string[conversation.dialogueEntries.Count];
            for (int i = 0; i < conversation.dialogueEntries.Count; i++)
            {
                var entry = conversation.dialogueEntries[i];
                entryTexts[i] = "[" + entry.id + "] " + ((entry.id == 0) ? "<START>" :  entry.subtitleText).Replace("/", "\u2215");
                idToIndex.Add(entry.id, i);
                indexToId.Add(i, entry.id);
            }
        }

        private int GetID(int index)
        {
            return indexToId.ContainsKey(index) ? indexToId[index] : -1;
        }

        private int GetIndex(int id)
        {
            return idToIndex.ContainsKey(id) ? idToIndex[id] : -1;
        }

        public int Draw(Rect rect, string label, int id)
        {
            return GetID(EditorGUI.Popup(rect, label, GetIndex(id), entryTexts));
        }

        public int DoLayout(string label, int id)
        {
            return GetID(EditorGUILayout.Popup(label, GetIndex(id), entryTexts));
        }
    }
}
