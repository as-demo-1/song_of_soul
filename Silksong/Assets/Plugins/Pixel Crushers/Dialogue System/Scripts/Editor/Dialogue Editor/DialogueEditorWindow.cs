// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEditor;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// Dialogue database editor window. This part adds the Dialogue Editor menu items to Unity.
    /// The Dialogue Editor is a custom editor window. Its functionality is split into
    /// separate files that constitute partial class definitions. Each file handles one
    /// aspect of the Dialogue Editor, such as the Actors tab or the Items tab.
    /// </summary>
    public partial class DialogueEditorWindow : EditorWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Dialogue Editor", false, -1)]
        public static DialogueEditorWindow OpenDialogueEditorWindow()
        {
            var window = GetWindow<DialogueEditorWindow>("Dialogue");
            window.OnSelectionChange();
            return window;
        }

        /// <summary>
        /// Opens the dialogue editor window to a specific dialogue entry.
        /// </summary>
        /// <param name="database">The database to open.</param>
        /// <param name="conversationID">The conversation ID.</param>
        /// <param name="dialogueEntryID">The dialogue entry ID in the conversation.</param>
        public static void OpenDialogueEntry(DialogueDatabase database, int conversationID, int dialogueEntryID)
        {
            var window = OpenDialogueEditorWindow();
            if (database != null) window.SelectObject(database);
            window.SelectDialogueEntry(conversationID, dialogueEntryID);
        }

    }

}