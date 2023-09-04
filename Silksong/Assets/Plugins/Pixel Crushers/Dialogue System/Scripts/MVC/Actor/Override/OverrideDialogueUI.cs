// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Overrides the dialogue UI for conversations involving the game object. To use this
    /// component, add it to a game object. When the game object is a conversant, the conversation
    /// will use the dialogue UI on this component instead of the UI on the DialogueManager.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class OverrideDialogueUI : OverrideUIBase
    {

        /// <summary>
        /// The dialogue UI to use for the game object this component is attached to.
        /// </summary>
        [Tooltip("Use this dialogue UI when this GameObject is involved in conversation.")]
        public GameObject ui;

        [Tooltip("If instantiating a prefab, keep it ready in memory instead of destroying it when conversation ends.")]
        public bool dontDestroyPrefabIntance = true;

        protected virtual void OnDestroy()
        {
            if (!Tools.IsPrefab(ui)) Destroy(ui);
        }

    }

}
