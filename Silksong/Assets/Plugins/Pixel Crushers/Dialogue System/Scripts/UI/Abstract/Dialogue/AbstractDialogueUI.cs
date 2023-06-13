// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This abstract class forms the base for many IDialogueUI implementations. It implements 
    /// common code so that the specific implementation for each GUI system need only deal 
    /// with the data structures specific to each GUI system.
    /// </summary>
    public abstract class AbstractDialogueUI : MonoBehaviour, IDialogueUI
    {

        /// <summary>
        /// Gets the user interface root.
        /// </summary>
        /// <value>
        /// The user interface root.
        /// </value>
        public abstract AbstractUIRoot uiRootControls { get; }

        /// <summary>
        /// Gets the dialogue controls.
        /// </summary>
        /// <value>
        /// The dialogue controls.
        /// </value>
        public abstract AbstractDialogueUIControls dialogueControls { get; }

        /// <summary>
        /// Gets the QTE (Quick Time Event) indicators.
        /// </summary>
        /// <value>
        /// The QTE indicators.
        /// </value>
        public abstract AbstractUIQTEControls qteControls { get; }

        /// <summary>
        /// Gets the alert message controls.
        /// </summary>
        /// <value>
        /// The alert message controls
        /// </value>
        public abstract AbstractUIAlertControls alertControls { get; }

        /// <summary>
        /// Occurs when the player selects a response.
        /// </summary>
        public event EventHandler<SelectedResponseEventArgs> SelectedResponseHandler;

        /// <summary>
        /// Gets or sets a value indicating whether the dialogue UI (conversation interface) is open.
        /// </summary>
        /// <value>
        /// <c>true</c> if open; otherwise, <c>false</c>.
        /// </value>
        public bool isOpen { get; set; }

        private bool m_hasOpenedBefore = false;

        /// @cond FOR_V1_COMPATIBILITY
        public bool IsOpen { get { return isOpen; } set { isOpen = value; } }
        /// @endcond

        /// <summary>
        /// Sets up the component.
        /// </summary>
        public virtual void Awake()
        {
            isOpen = false;
            SelectedResponseHandler = null;
        }

        /// <summary>
        /// Starts this instance by hiding everything. The only exception is if an alert message is
        /// already showing; it keeps this message visible.
        /// </summary>
        public virtual void Start()
        {
            if ((uiRootControls == null) || (dialogueControls == null) || (qteControls == null) || (alertControls == null))
            {
                enabled = false;
            }
            else
            {
                uiRootControls.Show();
                if (!isOpen) dialogueControls.Hide();
                qteControls.Hide();
                if (!alertControls.isVisible) alertControls.Hide();
                if (isOpen) Open();
                if (!(alertControls.isVisible || isOpen)) uiRootControls.Hide();
            }
        }

        /// <summary>
        /// Opens the conversation GUI.
        /// </summary>
        public virtual void Open()
        {
            m_hasOpenedBefore = true;
            dialogueControls.ShowPanel();
            uiRootControls.Show();
            isOpen = true;
        }

        /// <summary>
        /// Closes the conversation GUI.
        /// </summary>
        public virtual void Close()
        {
            dialogueControls.Hide();
            if (!AreNonDialogueControlsVisible) uiRootControls.Hide();
            isOpen = false;
        }

        /// <summary>
        /// Gets a value indicating whether non-conversation controls (e.g., alert message or QTEs)
        /// are visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if non-conversation controls are visible; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool AreNonDialogueControlsVisible
        {
            get { return alertControls.isVisible || qteControls.areVisible; }
        }

        /// <summary>
        /// Shows an alert.
        /// </summary>
        /// <param name='message'>
        /// Message to show.
        /// </param>
        /// <param name='duration'>
        /// Duration in seconds.
        /// </param>
        public virtual void ShowAlert(string message, float duration)
        {
            if (!isOpen)
            {
                if (!m_hasOpenedBefore) dialogueControls.Hide();
                uiRootControls.Show();
            }
            alertControls.ShowMessage(message, duration);
        }

        /// <summary>
        /// Hides the alert if it's showing.
        /// </summary>
        public virtual void HideAlert()
        {
            if (alertControls.isVisible)
            {
                alertControls.Hide();
                if (!(isOpen || qteControls.areVisible)) uiRootControls.Hide();
            }
        }

        /// <summary>
        /// Updates this instance by hiding the alert message when it's done.
        /// </summary>
        public virtual void Update()
        {
            if (alertControls.isVisible && alertControls.IsDone) alertControls.Hide();
        }

        /// <summary>
        /// Shows the subtitle (NPC or PC) based on the character type.
        /// </summary>
        /// <param name='subtitle'>
        /// Subtitle to show.
        /// </param>
        public virtual void ShowSubtitle(Subtitle subtitle)
        {
            SetSubtitle(subtitle, true);
        }

        /// <summary>
        /// Hides the subtitle based on its character type (PC or NPC).
        /// </summary>
        /// <param name='subtitle'>
        /// Subtitle to hide.
        /// </param>
        public virtual void HideSubtitle(Subtitle subtitle)
        {
            SetSubtitle(subtitle, false);
        }

        /// <summary>
        /// Shows the continue button.
        /// </summary>
        /// <param name="subtitle">Subtitle.</param>
        public virtual void ShowContinueButton(Subtitle subtitle)
        {
            var subtitleControls = GetSubtitleControls(subtitle);
            if (subtitleControls != null)
            {
                subtitleControls.ShowContinueButton();
            }
        }

        /// <summary>
        /// Hides the continue button. Call this after showing the subtitle.
        /// The ConversationView uses this to hide the continue button if the 
        /// display setting for continue buttons is set to NotBeforeResponseMenu.
        /// </summary>
        /// <param name="subtitle">Subtitle.</param>
        public virtual void HideContinueButton(Subtitle subtitle)
        {
            var subtitleControls = GetSubtitleControls(subtitle);
            if (subtitleControls != null)
            {
                subtitleControls.HideContinueButton();
            }
        }

        /// <summary>
        /// Sets a subtitle's content and visibility.
        /// </summary>
        /// <param name='subtitle'>
        /// Subtitle. The speaker recorded in the subtitle determines whether the NPC or
        /// PC subtitle controls are used.
        /// </param>
        /// <param name='value'>
        /// <c>true</c> to show; <c>false</c> to hide.
        /// </param>
        protected virtual void SetSubtitle(Subtitle subtitle, bool value)
        {
            var subtitleControls = GetSubtitleControls(subtitle);
            if (subtitleControls != null)
            {
                if (value == true)
                {
                    subtitleControls.ShowSubtitle(subtitle);
                }
                else
                {
                    subtitleControls.Hide();
                }
            }
        }

        /// <summary>
        /// Gets the appropriate subtitle control (PC or NPC) for a subtitle.
        /// </summary>
        /// <returns>
        /// The subtitle controls to use for a subtitle.
        /// </returns>
        /// <param name='subtitle'>
        /// Subtitle to use; the subtitle's speakerInfo property indicates whether the subtitle is
        /// a PC or NPC line.
        /// </param>
        private AbstractUISubtitleControls GetSubtitleControls(Subtitle subtitle)
        {
            return (subtitle == null)
                ? null
                : (subtitle.speakerInfo.characterType == CharacterType.NPC)
                    ? dialogueControls.npcSubtitleControls
                    : dialogueControls.pcSubtitleControls;
        }

        /// <summary>
        /// Shows the player responses menu.
        /// </summary>
        /// <param name='subtitle'>
        /// The last subtitle, shown as a reminder.
        /// </param>
        /// <param name='responses'>
        /// Responses.
        /// </param>
        /// <param name='timeout'>
        /// If not <c>0</c>, the duration in seconds that the player has to choose a response; 
        /// otherwise the currently-focused response is auto-selected. If no response is
        /// focused (e.g., hovered over), the first response is auto-selected. If <c>0</c>,
        /// there is no timeout; the player can take as long as desired to choose a response.
        /// </param>
        public virtual void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
        {
            try
            {
                if (dialogueControls == null)
                {
                    Debug.LogError(DialogueDebug.Prefix + ": In ShowResponses(): The dialogue UI's main dialogue controls field is not set.", this);
                }
                else if (dialogueControls.responseMenuControls == null)
                {
                    Debug.LogError(DialogueDebug.Prefix + ": In ShowResponses(): The dialogue UI's response menu controls field is not set.", this);
                }
                else if (this.transform == null)
                {
                    Debug.LogError(DialogueDebug.Prefix + ": In ShowResponses(): The dialogue UI's transform is null.", this);
                }
                else
                {
                    dialogueControls.responseMenuControls.ShowResponses(subtitle, responses, this.transform);
                    if (timeout > 0) dialogueControls.responseMenuControls.StartTimer(timeout);
                }
            }
            catch (NullReferenceException e)
            {
                Debug.LogError(DialogueDebug.Prefix + ": In ShowResponses(): " + e.Message);
            }
        }

        /// <summary>
        /// Hides the player response menu.
        /// </summary>
        public virtual void HideResponses()
        {
            dialogueControls.responseMenuControls.Hide();
        }

        /// <summary>
        /// Shows a QTE indicator.
        /// </summary>
        /// <param name='index'>
        /// Index of the QTE indicator.
        /// </param>
        public virtual void ShowQTEIndicator(int index)
        {
            qteControls.ShowIndicator(index);
        }

        /// <summary>
        /// Hides a QTE indicator.
        /// </summary>
        /// <param name='index'>
        /// Index of the QTE indicator.
        /// </param>
        public virtual void HideQTEIndicator(int index)
        {
            qteControls.HideIndicator(index);
        }

        /// <summary>
        /// Handles response button clicks.
        /// </summary>
        /// <param name='data'>
        /// The Response assigned to the button. This is passed back to the handler.
        /// </param>
        public virtual void OnClick(object data)
        {
            if (SelectedResponseHandler != null) SelectedResponseHandler(this, new SelectedResponseEventArgs(data as Response));
        }

        /// <summary>
        /// Handles the continue button being clicked. This sends OnConversationContinue to the 
        /// DialogueManager object so the ConversationView knows to progress the conversation.
        /// </summary>
        public virtual void OnContinue()
        {
            OnContinueAlert();
            OnContinueConversation();
        }

        public virtual void OnContinueAlert()
        {
            if (alertControls.isVisible) HideAlert();
        }

        public virtual void OnContinueConversation()
        {
            if (isOpen) DialogueManager.instance.SendMessage(DialogueSystemMessages.OnConversationContinue, (IDialogueUI)this, SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Sets the PC portrait name and sprite.
        /// </summary>
        /// <param name="portraitSprite">Portrait sprite.</param>
        /// <param name="portraitName">Portrait name.</param>
        public virtual void SetPCPortrait(Sprite portraitSprite, string portraitName)
        {
            dialogueControls.responseMenuControls.SetPCPortrait(portraitSprite, portraitName);
        }

        [System.Obsolete("Use SetPCPortrait(Sprite,string) instead.")]
        public virtual void SetPCPortrait(Texture2D portraitTexture, string portraitName)
        {
            dialogueControls.responseMenuControls.SetPCPortrait(UITools.CreateSprite(portraitTexture), portraitName);
        }

        /// <summary>
        /// Sets the portrait sprite for an actor.
        /// This is used to immediately update the GUI control if the SetPortrait() sequencer 
        /// command changes the portrait sprite.
        /// </summary>
        /// <param name="actorName">Actor name in database.</param>
        /// <param name="portraitSprite">Portrait sprite.</param>
        public virtual void SetActorPortraitSprite(string actorName, Sprite portraitSprite)
        {
            dialogueControls.npcSubtitleControls.SetActorPortraitSprite(actorName, portraitSprite);
            dialogueControls.pcSubtitleControls.SetActorPortraitSprite(actorName, portraitSprite);
            dialogueControls.responseMenuControls.SetActorPortraitSprite(actorName, portraitSprite);
        }

        /// <summary>
        /// Gets a valid portrait sprite. If the provided portraitSprite is null, grabs the default
        /// sprites from the database.
        /// </summary>
        /// <returns>The valid portrait sprite.</returns>
        /// <param name="actorName">Actor name.</param>
        /// <param name="portraitSprite">Portrait sprite.</param>
        public static Sprite GetValidPortraitSprite(string actorName, Sprite portraitSprite)
        {
            if (portraitSprite != null)
            {
                return portraitSprite;
            }
            else
            {
                var actor = DialogueManager.masterDatabase.GetActor(actorName);
                return (actor != null) ? actor.GetPortraitSprite() : null;
            }
        }

        [System.Obsolete("Use GetValidPortraitSprite instead.")]
        public static Texture2D GetValidPortraitTexture(string actorName, Texture2D portraitTexture)
        {
            if (portraitTexture != null)
            {
                return portraitTexture;
            }
            else
            {
                var actor = DialogueManager.masterDatabase.GetActor(actorName);
                return (actor != null) ? actor.portrait : null;
            }
        }

    }

}
