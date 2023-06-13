// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is a variation of UnityUIDialogueUI that uses the speaker's bark UI for subtitles.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UnityUIBarkSubtitleDialogueUI : UnityUIDialogueUI
    {
        public override void Awake()
        {
            base.Awake();
            Tools.DeprecationWarning(this);
        }

        public override void ShowSubtitle(Subtitle subtitle)
        {
            var barkUI = subtitle.speakerInfo.transform.GetComponentInChildren<UnityUIBarkUI>();
            if (barkUI == null)
            {
                Debug.LogWarning("Dialogue System: Speaker (" + subtitle.speakerInfo.transform + ") doesn't have a bark UI: " + subtitle.formattedText.text, subtitle.speakerInfo.transform);
            }
            else
            {
                barkUI.Bark(subtitle);
            }
            HideResponses();
        }

    }

}
