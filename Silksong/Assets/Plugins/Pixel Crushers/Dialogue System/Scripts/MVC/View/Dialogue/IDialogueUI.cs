// Copyright (c) Pixel Crushers. All rights reserved.

using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Interface for dialogue UI components. A dialogue UI is responsible for displaying 
    /// subtitles, presenting the player response menu, and showing alerts.
    /// 
    /// See UnityDialogueUI for a reference implementation that uses Unity GUI.
    /// </summary>
    public interface IDialogueUI
    {

        /// <summary>
        /// Your implementation must define this event and make it public. Example:
        /// 
        /// <code>public event EventHandler<SelectedResponseEventArgs> SelectedResponseHandler;</code>
        /// </summary>
        event EventHandler<SelectedResponseEventArgs> SelectedResponseHandler;

        /// <summary>
        /// Called when a conversation starts, use Open() to do any setup -- for example, to show
        /// the conversation GUI controls.
        /// </summary>
        void Open();

        /// <summary>
        /// Called when a conversation ends, use Close() to clean up -- for example, to hide the
        /// conversation GUI controls.
        /// </summary>
        void Close();

        /// <summary>
        /// Shows a subtitle.
        /// </summary>
        /// <param name='subtitle'>
        /// The subtitle to show. Subtitles contain information such as the player type (PC or 
        /// NPC), portrait texture, and the formatted text of the line.
        /// </param>
        void ShowSubtitle(Subtitle subtitle);

        /// <summary>
        /// Hides a subtitle.
        /// </summary>
        /// <param name='subtitle'>
        /// The subtitle to hide.
        /// </param>
        void HideSubtitle(Subtitle subtitle);

        /// <summary>
        /// Shows the player response menu. When the player selects a response, your 
        /// implementation must call the SelectedResponseHandler event and pass along a
        /// SelectedResponseEventArgs object containing the selected response.
        /// 
        /// Example:
        /// <code>
        /// private void OnResponseClicked(Response response) {
        ///     SelectedResponseHandler(this, new SelectedResponseEventArgs(response));
        /// }
        /// </code>
        /// </summary>
        /// <param name='subtitle'>
        /// The most recently displayed subtitle. Your implementation might show this subtitle to
        /// remind the player what he or she is responding to.
        /// </param>
        /// <param name='responses'>
        /// The responses.
        /// </param>
        /// <param name='timeout'>
        /// If not <c>0</c>, the duration in seconds that the player has to choose a response; 
        /// otherwise the currently-focused response is auto-selected. If no response is
        /// focused (e.g., hovered over), the first response is auto-selected. If <c>0</c>,
        /// there is no timeout; the player can take as long as desired to choose a response.
        /// </param>
        void ShowResponses(Subtitle subtitle, Response[] responses, float timeout);

        /// <summary>
        /// Hides the player response menu.
        /// </summary>
        void HideResponses();

        /// <summary>
        /// Shows a QTE (Quick Time Event) indicator.
        /// </summary>
        /// <param name='index'>
        /// Index number of the QTE indicator.
        /// </param>
        /// <remarks>
        /// If your project doesn't use QTEs, you can just create an empty method.
        /// </remarks>
        void ShowQTEIndicator(int index);

        /// <summary>
        /// Hides a QTE (Quick Time Event) indicator.
        /// </summary>
        /// <param name='index'>
        /// Index number of the QTE indicator.
        /// </param>
        /// <remarks>
        /// If your project doesn't use QTEs, you can just create an empty method.
        /// </remarks>
        void HideQTEIndicator(int index);

        /// <summary>
        /// Shows an alert message. The dialogue system will *not* call Open() before ShowAlert(),
        /// nor Close() afterward, since alert messages are intended to be shown outside of 
        /// conversations.
        /// </summary>
        /// <param name='message'>
        /// The message to show.
        /// </param>
        /// <param name='duration'>
        /// The duration in seconds to show the message.
        /// </param>
        void ShowAlert(string message, float duration);

        /// <summary>
        /// Hides the alert message if it's showing.
        /// </summary>
        void HideAlert();

    }

}
