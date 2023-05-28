// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add this to the Dialogue Manager to support animated portraits in 
    /// Unity UI Dialogue UIs. Participants' GameObjects must have
    /// AnimatedPortrait components.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UseAnimatedPortraits : MonoBehaviour
    {

        private UnityUIDialogueUI dialogueUI = null;
        private Animator npcPortraitAnimator = null;
        private Animator npcReminderPortraitAnimator = null;
        private Animator pcPortraitAnimator = null;
        private Animator otherAnimator = null;
        private Dictionary<Transform, AnimatedPortrait> animatedPortraits = new Dictionary<Transform, AnimatedPortrait>();
        private Transform lastSpeaker = null;

        /// <summary>
        /// When speaking a line, update the appropriate portrait image with the
        /// speaker's animated portrait animator controller.
        /// </summary>
        /// <param name="subtitle">Subtitle.</param>
        public void OnConversationLine(Subtitle subtitle)
        {
            if (!FindDialogueUI()) return;
            StartCoroutine(SetAnimatorAtEndOfFrame(subtitle));
        }

        private IEnumerator SetAnimatorAtEndOfFrame(Subtitle subtitle)
        {
            yield return new WaitForEndOfFrame();
            var overrideControls = dialogueUI.FindActorOverride(subtitle.speakerInfo.transform);
            if (overrideControls != null)
            {
                otherAnimator = null;
                SetAnimatorController(overrideControls.subtitle.portraitImage, subtitle.speakerInfo.transform, ref otherAnimator);
            }
            else if (subtitle.speakerInfo.characterType == CharacterType.NPC)
            {
                SetAnimatorController(dialogueUI.dialogue.npcSubtitle.portraitImage, subtitle.speakerInfo.transform, ref npcPortraitAnimator);
            }
            else
            {
                SetAnimatorController(dialogueUI.dialogue.pcSubtitle.portraitImage, subtitle.speakerInfo.transform, ref pcPortraitAnimator);
            }
            lastSpeaker = subtitle.speakerInfo.transform;
        }

        /// <summary>
        /// When showing the response menu, update the subtitle reminder image with
        /// the last speaker's animated portrait animator controller.
        /// </summary>
        /// <param name="responses">Responses.</param>
        public void OnConversationResponseMenu(Response[] responses)
        {
            if (!FindDialogueUI()) return;
            SetAnimatorController(dialogueUI.dialogue.responseMenu.subtitleReminder.portraitImage, lastSpeaker, ref npcReminderPortraitAnimator);
        }

        /// <summary>
        /// When ending the conversation, clear the cache of animated portraits.
        /// </summary>
        /// <param name="actor">Actor.</param>
        public void OnConversationEnd(Transform actor)
        {
            animatedPortraits.Clear();
        }

        private bool FindDialogueUI()
        {
            if (dialogueUI == null && DialogueManager.displaySettings.dialogueUI)
            {
                dialogueUI = DialogueManager.displaySettings.dialogueUI.GetComponent<UnityUIDialogueUI>();
            }
            return (dialogueUI != null);
        }

        private void SetAnimatorController(UnityEngine.UI.Image image, Transform speaker, ref Animator animator)
        {
            if (speaker == null || image == null) return;
            if (animator == null) animator = image.GetComponent<Animator>();
            if (animator == null) animator = image.gameObject.AddComponent<Animator>();
            if (!animatedPortraits.ContainsKey(speaker))
            {
                var animatedPortrait = (speaker != null) ? speaker.GetComponentInChildren<AnimatedPortrait>() : null;
                animatedPortraits.Add(speaker, animatedPortrait);
            }
            if (animatedPortraits[speaker] != null)
            {
                var animatorController = animatedPortraits[speaker].animatorController;
                if (animator.runtimeAnimatorController != animatorController)
                {
                    animator.runtimeAnimatorController = animatorController;
                }
            }

        }
    }

}