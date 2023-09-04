using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Subtitle GUI controls for UnityDialogueUI.
    /// </summary>
    [System.Serializable]
    public class UnitySubtitleControls : AbstractUISubtitleControls
    {

        /// <summary>
        /// The alert panel. A panel is optional, but you may want one
        /// so you can include a background image, panel-wide effects, etc.
        /// </summary>
        public GUIControl panel;

        /// <summary>
        /// The label that will show the text of the subtitle.
        /// </summary>
        public GUILabel line;

        /// <summary>
        /// The label that will show the portrait image.
        /// </summary>
        public GUILabel portraitImage;

        /// <summary>
        /// The label that will show the name of the speaker.
        /// </summary>
        public GUILabel portraitName;

        /// <summary>
        /// The continue button. This control is only required if DisplaySettings.waitForContinueButton 
        /// is <c>true</c> -- in which case this button should send "OnContinue" to the UI when clicked.
        /// </summary>
        public GUIButton continueButton;

        /// <summary>
        /// Gets a value indicating whether text has been assigned to the subtitle controls.
        /// </summary>
        /// <value><c>true</c> if this instance has text; otherwise, <c>false</c>.</value>
        public override bool hasText
        {
            get { return (line != null) && !string.IsNullOrEmpty(line.text); }
        }

        /// <summary>
        /// Sets the controls active/inactive.
        /// </summary>
        /// <param name="value">If set to <c>true</c> value.</param>
        public override void SetActive(bool value)
        {
            UnityDialogueUIControls.SetControlActive(line, value);
            UnityDialogueUIControls.SetControlActive(portraitImage, value);
            UnityDialogueUIControls.SetControlActive(portraitName, value);
            UnityDialogueUIControls.SetControlActive(continueButton, value);
            UnityDialogueUIControls.SetControlActive(panel, value);
        }

        /// <summary>
        /// Sets the subtitle controls' contents.
        /// </summary>
        /// <param name="subtitle">Subtitle.</param>
        public override void SetSubtitle(Subtitle subtitle)
        {
            if (portraitImage != null)
            {
                var sprite = subtitle.GetSpeakerPortrait();
                portraitImage.image = (sprite != null) ? sprite.texture : null;
            }
            if (portraitName != null) portraitName.text = subtitle.speakerInfo.Name;
            if (line != null) line.SetFormattedText(subtitle.formattedText);
        }

        /// <summary>
        /// Clears the subtitle controls' contents.
        /// </summary>
        public override void ClearSubtitle()
        {
            if (portraitImage != null) portraitImage.image = null;
            if (portraitName != null) portraitName.text = null;
            if (line != null) line.SetUnformattedText(string.Empty);
        }

        /// <summary>
        /// Shows the continue button.
        /// </summary>
        public override void ShowContinueButton()
        {
            UnityDialogueUIControls.SetControlActive(continueButton, true);
        }

        /// <summary>
        /// Hides the continue button.
        /// </summary>
        public override void HideContinueButton()
        {
            UnityDialogueUIControls.SetControlActive(continueButton, false);
        }

        /// <summary>
        /// Sets the portrait sprite to use in the subtitle if the named actor is the speaker.
        /// This is used to immediately update the GUI control if the SetPortrait() sequencer 
        /// command changes the portrait sprite.
        /// </summary>
        /// <param name="actorName">Actor name in database.</param>
        /// <param name="portraitSprite">Portrait sprite.</param>
        public override void SetActorPortraitSprite(string actorName, Sprite portraitSprite)
        {
            if ((currentSubtitle != null) && string.Equals(currentSubtitle.speakerInfo.nameInDatabase, actorName))
            {
                if (portraitImage != null)
                {
                    portraitImage.image = UITools.GetTexture2D(AbstractDialogueUI.GetValidPortraitSprite(actorName, portraitSprite));
                }
            }
        }

    }

}
