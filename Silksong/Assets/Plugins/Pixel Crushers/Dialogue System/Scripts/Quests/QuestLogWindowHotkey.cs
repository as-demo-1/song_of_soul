// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Allows toggling of the quest log window using a key or button.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class QuestLogWindowHotkey : MonoBehaviour
    {

        [Tooltip("Toggle the quest log window when this key is pressed.")]
        public KeyCode key = KeyCode.J;

        [Tooltip("Toggle the quest log window when this input button is presed.")]
        public string buttonName = string.Empty;

        [Tooltip("(Optional) Use this quest log window. If unassigned, will automatically find quest log window in scene. If you assign a window, assign a scene instance, not an uninstantiated prefab.")]
        public QuestLogWindow questLogWindow;

        public QuestLogWindow runtimeQuestLogWindow
        {
            get
            {
                if (questLogWindow == null) questLogWindow = GameObjectUtility.FindFirstObjectByType<QuestLogWindow>();
                return questLogWindow;
            }
        }

        void Awake()
        {
            if (questLogWindow == null) questLogWindow = GameObjectUtility.FindFirstObjectByType<QuestLogWindow>();
        }

        void Update()
        {
            if (runtimeQuestLogWindow == null) return;
            if (DialogueManager.IsDialogueSystemInputDisabled()) return;
            if (InputDeviceManager.IsKeyDown(key) || (!string.IsNullOrEmpty(buttonName) && DialogueManager.getInputButtonDown(buttonName)))
            {
                if (runtimeQuestLogWindow.isOpen) runtimeQuestLogWindow.Close(); else runtimeQuestLogWindow.Open();
            }
        }

    }

}
