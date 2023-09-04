// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This component allows actors to override Unity UI dialogue controls. It's 
    /// particularly useful to assign world space UIs such as speech bubbles above
    /// actors' heads.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class OverrideUnityUIDialogueControls : MonoBehaviour
    {

        [Tooltip("Use these controls when playing subtitles through this actor")]
        public UnityUISubtitleControls subtitle;

        [Tooltip("Use these controls when showing subtitle reminders for actor")]
        public UnityUISubtitleControls subtitleReminder;

        [Tooltip("Use these controls when showing a response menu involving this actor")]
        public UnityUIResponseMenuControls responseMenu;

        private bool checkedContinueButton = false;

        private void Awake()
        {
            Tools.DeprecationWarning(this, "Use StandardDialogueUI and DialogueActor, which make this script unnecessary.");
        }

        public virtual void Start()
        {
            if (subtitle != null) subtitle.SetActive(false);
            if (subtitleReminder != null) subtitleReminder.SetActive(false);
            if (responseMenu != null) responseMenu.SetActive(false);
        }

        public virtual void ApplyToDialogueUI(UnityUIDialogueUI ui)
        {
            if (checkedContinueButton) return;
            if (subtitle != null && subtitle.continueButton != null)
            {
                // Make sure continue button is connected:
                if (subtitle.continueButton.onClick.GetPersistentEventCount() == 0 || // OnClick() unassigned or
                subtitle.continueButton.onClick.GetPersistentTarget(0) == null) // OnClick() target is null:
                {
                    subtitle.continueButton.onClick.AddListener(ui.OnContinue);
                }
                // If continue button has fast forward, make sure its UI reference is connected:
                var ffwd = subtitle.continueButton.GetComponent<UnityUIContinueButtonFastForward>();
                if (ffwd != null && ffwd.dialogueUI == null)
                {
                    ffwd.dialogueUI = ui;
                }
            }
            checkedContinueButton = true;
        }

    }

}
