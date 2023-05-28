// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Globalization;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window provides a Search bar in the 
    /// outline-style dialogue tree editor.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        [SerializeField]
        private bool isSearchBarOpen = false;
        [SerializeField]
        private string searchString = string.Empty;
        [SerializeField]
        private bool searchCaseSensitive = false;

        private bool IsSearchBarVisible { get { return isSearchBarOpen && (toolbar.Current == Toolbar.Tab.Conversations); } }

        private void ToggleDialogueTreeSearchBar()
        {
            isSearchBarOpen = !isSearchBarOpen;
        }

        private void DrawDialogueTreeSearchBar()
        {
            if (showNodeEditor)
            {
                GUILayout.BeginArea(new Rect(0, 49, position.width, 24));
            }
            else
            {
                EditorWindowTools.DrawHorizontalLine();
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Empty, GUILayout.Width(8));
            GUI.SetNextControlName("SearchTextField");
            searchString = EditorGUILayout.TextField("Search", searchString, "ToolbarSeachTextField");
            GUI.SetNextControlName("SearchClearButton");
            if (GUILayout.Button("Clear", "ToolbarSeachCancelButton"))
            {
                searchString = string.Empty;
                GUI.FocusControl("SearchClearButton"); // Need to deselect search field to clear text field's display.
            }
            searchCaseSensitive = EditorGUILayout.ToggleLeft(new GUIContent("Aa", "Case-sensitive"), searchCaseSensitive, GUILayout.Width(30));
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(searchString));
            if (GUILayout.Button("↓", EditorStyles.miniButtonLeft, GUILayout.Width(22))) SearchDialogueTree(1);
            if (GUILayout.Button("↑", EditorStyles.miniButtonMid, GUILayout.Width(22))) SearchDialogueTree(-1);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(22))) isSearchBarOpen = false;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(string.Empty, GUILayout.Height(1));
            if (showNodeEditor)
            {
                GUILayout.EndArea();
            }
        }

        private void SearchDialogueTree(int direction)
        {
            // Assumes dialogue tree has already been built. Otherwise just exits.
            DialogueEntry entry = (currentEntry != null)
                ? currentEntry
                : ((dialogueTree != null) ? dialogueTree.entry : null);
            if (entry == null) return;
            int start = GetValidSearchIndex(currentConversation.dialogueEntries.IndexOf(entry));
            int current = GetValidSearchIndex(start + direction);
            while (current != start)
            {
                if (ContainsSearchString(currentConversation.dialogueEntries[current]))
                {
                    if (showNodeEditor)
                    {
                        SetCurrentEntry(currentConversation.dialogueEntries[current]);
                        CenterOnCurrentEntry();
                    }
                    else
                    {
                        currentEntry = currentConversation.dialogueEntries[current];
                    }
                    return;
                }
                else {
                    current = GetValidSearchIndex(current + direction);
                }
            }
        }

        private int GetValidSearchIndex(int index)
        {
            if (index < 0)
            {
                return currentConversation.dialogueEntries.Count - 1;
            }
            else if (index >= currentConversation.dialogueEntries.Count)
            {
                return 0;
            }
            else {
                return index;
            }
        }

        private bool ContainsSearchString(DialogueEntry entry)
        {
            foreach (var field in entry.fields)
            {
                if (ContainsSearchStringCaseInsensitive(field.value)) return true;
            }
            if (ContainsSearchStringCaseInsensitive(entry.conditionsString)) return true;
            if (ContainsSearchStringCaseInsensitive(entry.userScript)) return true;
            if (ContainsSearchStringCaseInsensitive(entry.Title)) return true;
            return false;
        }

        private bool ContainsSearchStringCaseInsensitive(string s)
        {
            var compareOptions = searchCaseSensitive ? CompareOptions.None : CompareOptions.IgnoreCase;
            return !string.IsNullOrEmpty(s) && (CultureInfo.InvariantCulture.CompareInfo.IndexOf(s, searchString, compareOptions) >= 0);

        }

    }

}