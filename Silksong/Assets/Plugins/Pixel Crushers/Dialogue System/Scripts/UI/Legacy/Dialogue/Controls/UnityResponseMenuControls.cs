using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Response menu controls for UnityDialogueUI.
    /// </summary>
    [System.Serializable]
    public class UnityResponseMenuControls : AbstractUIResponseMenuControls
    {

        /// <summary>
        /// The alert panel. A panel is optional, but you may want one
        /// so you can include a background image, panel-wide effects, etc.
        /// </summary>
        public GUIControl panel;

        /// <summary>
        /// The PC portrait image.
        /// </summary>
        public GUILabel pcImage;

        /// <summary>
        /// The PC portrait name.
        /// </summary>
        public GUILabel pcName;

        /// <summary>
        /// The reminder of the last subtitle.
        /// </summary>
        public UnitySubtitleControls subtitleReminder;

        /// <summary>
        /// The (optional) timer.
        /// </summary>
        public GUIProgressBar timer;

        /// <summary>
        /// The response buttons.
        /// </summary>
        public GUIButton[] buttons;

        public override AbstractUISubtitleControls subtitleReminderControls
        {
            get { return subtitleReminder; }
        }

        private TimerEffect timerEffect = null;

        private Texture2D pcPortraitTexture = null;
        private string pcPortraitName = null;

        /// <summary>
        /// Sets the PC portrait name and sprite to use in the response menu.
        /// </summary>
        /// <param name="portraitSprite">Portrait sprite.</param>
        /// <param name="portraitName">Portrait name.</param>
        public override void SetPCPortrait(Sprite portraitSprite, string portraitName)
        {
            pcPortraitTexture = UITools.GetTexture2D(portraitSprite);
            pcPortraitName = portraitName;
        }

        /// <summary>
        /// Sets the portrait sprite to use in the response menu if the named actor is the player.
        /// This is used to immediately update the GUI control if the SetPortrait() sequencer 
        /// command changes the portrait sprite.
        /// </summary>
        /// <param name="actorName">Actor name in database.</param>
        /// <param name="portraitSprite">Portrait sprite.</param>
        public override void SetActorPortraitSprite(string actorName, Sprite portraitSprite)
        {
            if (string.Equals(actorName, pcPortraitName))
            {
                var actorPortraitTexture = UITools.GetTexture2D(AbstractDialogueUI.GetValidPortraitSprite(actorName, portraitSprite));
                pcPortraitTexture = actorPortraitTexture;
                if ((pcImage != null) && (DialogueManager.masterDatabase.IsPlayer(actorName)))
                {
                    pcImage.image = actorPortraitTexture;
                }
            }
        }

        /// <summary>
        /// Sets the controls active/inactive, except this method never activates the timer. If the
        /// UI's display settings specify a timeout, then the UI will call StartTimer() to manually
        /// activate the timer.
        /// </summary>
        /// <param name='value'>
        /// Value (<c>true</c> for active; otherwise inactive).
        /// </param>
        public override void SetActive(bool value)
        {
            subtitleReminder.SetActive(value && subtitleReminder.hasText);
            foreach (GUIButton button in buttons)
            {
                UnityDialogueUIControls.SetControlActive(button, value && button.visible);
            }
            UnityDialogueUIControls.SetControlActive(timer, false);
            UnityDialogueUIControls.SetControlActive(pcImage, value);
            UnityDialogueUIControls.SetControlActive(pcName, value);
            UnityDialogueUIControls.SetControlActive(panel, value);

            if (value == true)
            {
                if ((pcImage != null) && (pcPortraitTexture != null)) pcImage.image = pcPortraitTexture;
                if ((pcName != null) && (pcPortraitName != null)) pcName.text = pcPortraitName;
            }
        }

        /// <summary>
        /// Clears the response buttons.
        /// </summary>
        protected override void ClearResponseButtons()
        {
            if (buttons != null)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    SetResponseButton(buttons[i], null, null);
                    buttons[i].visible = showUnusedButtons;
                }
            }
        }

        /// <summary>
        /// Sets the response buttons.
        /// </summary>
        /// <param name='responses'>
        /// Responses.
        /// </param>
        /// <param name='target'>
        /// Target that will receive OnClick events from the buttons.
        /// </param>
        protected override void SetResponseButtons(Response[] responses, Transform target)
        {
            if ((buttons != null) && (buttons.Length > 0) && (responses != null))
            {

                // Add explicitly-positioned buttons:
                for (int i = 0; i < responses.Length; i++)
                {
                    if (responses[i].formattedText.position != FormattedText.NoAssignedPosition)
                    {
                        int position = Mathf.Clamp(responses[i].formattedText.position, 0, buttons.Length - 1);
                        SetResponseButton(buttons[position], responses[i], target);
                    }
                }

                // Auto-position remaining buttons:
                if (buttonAlignment == ResponseButtonAlignment.ToFirst)
                {

                    // Align to first, so add in order to front:
                    for (int i = 0; i < Mathf.Min(buttons.Length, responses.Length); i++)
                    {
                        if (responses[i].formattedText.position == FormattedText.NoAssignedPosition)
                        {
                            int position = Mathf.Clamp(GetNextAvailableResponseButtonPosition(0, 1), 0, buttons.Length - 1);
                            SetResponseButton(buttons[position], responses[i], target);
                        }
                    }
                }
                else
                {

                    // Align to last, so add in reverse order to back:
                    for (int i = Mathf.Min(buttons.Length, responses.Length) - 1; i >= 0; i--)
                    {
                        if (responses[i].formattedText.position == FormattedText.NoAssignedPosition)
                        {
                            int position = Mathf.Clamp(GetNextAvailableResponseButtonPosition(buttons.Length - 1, -1), 0, buttons.Length - 1);
                            SetResponseButton(buttons[position], responses[i], target);
                        }
                    }
                }
            }
        }

        private void SetResponseButton(GUIButton button, Response response, Transform target)
        {
            if (button != null)
            {
                button.visible = true;
                button.clickable = (response != null) && response.enabled;
                if (response != null)
                {
                    button.SetFormattedText(response.formattedText);
                }
                else if (showUnusedButtons)
                {
                    button.SetUnformattedText(" "); // Need a blank space to make empty button visible.
                }
                button.target = target;
                button.data = response;
            }
        }

        private int GetNextAvailableResponseButtonPosition(int start, int direction)
        {
            if (buttons != null)
            {
                int position = start;
                while ((0 <= position) && (position < buttons.Length))
                {
                    if (buttons[position].clickable)
                    {
                        position += direction;
                    }
                    else
                    {
                        return position;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        /// <param name='timeout'>
        /// Timeout duration in seconds.
        /// </param>
        public override void StartTimer(float timeout)
        {
            if (timer != null)
            {
                if (timerEffect == null)
                {
                    UnityDialogueUIControls.SetControlActive(timer, true);
                    timerEffect = timer.GetComponent<TimerEffect>();
                    UnityDialogueUIControls.SetControlActive(timer, false);
                }
                if (timerEffect != null)
                {
                    timer.progress = 1;
                    timerEffect.duration = timeout;
                    timerEffect.TimeoutHandler -= OnTimeout;
                    timerEffect.TimeoutHandler += OnTimeout;
                    UnityDialogueUIControls.SetControlActive(timer, true);
                }

            }
        }

        /// <summary>
        /// This method is called if the timer runs out. The Dialogue Manager can be configured 
        /// to take different actions on timeout.
        /// </summary>
        public void OnTimeout()
        {
            DialogueManager.instance.SendMessage("OnConversationTimeout");
        }

    }

}
