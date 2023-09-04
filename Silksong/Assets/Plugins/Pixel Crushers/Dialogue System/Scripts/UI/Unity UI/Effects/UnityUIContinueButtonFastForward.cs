// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This script replaces the normal continue button functionality with
    /// a two-stage process. If the typewriter effect is still playing, it
    /// simply stops the effect. Otherwise it sends OnContinue to the UI.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UnityUIContinueButtonFastForward : MonoBehaviour
    {

        [Tooltip("Dialogue UI that the continue button affects.")]
        public UnityUIDialogueUI dialogueUI;

        [Tooltip("Typewriter effect to fast forward if it's not done playing.")]
        public UnityUITypewriterEffect typewriterEffect;

        [Tooltip("Hide the continue button when continuing.")]
        public bool hideContinueButtonOnContinue = false;

        private UnityEngine.UI.Button continueButton;

        public virtual void Awake()
        {
            if (dialogueUI == null)
            {
                dialogueUI = Tools.GetComponentAnywhere<UnityUIDialogueUI>(gameObject);
            }
            if (typewriterEffect == null)
            {
                typewriterEffect = GetComponentInChildren<UnityUITypewriterEffect>();
            }
            continueButton = GetComponent<UnityEngine.UI.Button>();
            Tools.DeprecationWarning(this);
        }

        public virtual void OnFastForward()
        {
            if ((typewriterEffect != null) && typewriterEffect.IsPlaying)
            {
                typewriterEffect.Stop();
            }
            else
            {
                if (hideContinueButtonOnContinue && continueButton != null) continueButton.gameObject.SetActive(false);
                if (dialogueUI != null) dialogueUI.OnContinue();
            }
        }

    }

}
