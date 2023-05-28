// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    public delegate void DrawAssetInspectorDelegate(DialogueDatabase database, Asset asset);
    public delegate void DrawDialogueEntryInspectorDelegate(DialogueDatabase database, DialogueEntry entry);
    public delegate void DrawDialogueEntryNodeDelegate(DialogueDatabase database, DialogueEntry entry, Rect boxRect);
    public delegate void SetupGenericDialogueEditorMenuDelegate(DialogueDatabase database, GenericMenu menu);

    /// <summary>
    /// This part of the Dialogue Editor window handles the main code for 
    /// the conversation node editor.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        /// <summary>
        /// Assign handler(s) to perform extra drawing in Actor, Item, and Location inspector views.
        /// </summary>
        public static event DrawAssetInspectorDelegate customDrawAssetInspector = null;

        /// <summary>
        /// Assign handler(s) to perform extra drawing in the dialogue entry inspector view.
        /// </summary>
        public static event DrawDialogueEntryInspectorDelegate customDrawDialogueEntryInspector = null;

        /// <summary>
        /// Assign handler(s) to perform extra drawing on nodes in the node editor.
        /// </summary>
        public static event DrawDialogueEntryNodeDelegate customDrawDialogueEntryNode = null;

        /// <summary>
        /// Assign handler(s) to add extra menu items to the node editor menu.
        /// </summary>
        public static event SetupGenericDialogueEditorMenuDelegate customNodeMenuSetup = null;

    }
}
