// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is an implementation of IDialogueUI that uses the participants' 
    /// bark UIs to run a conversation. Since it uses bark UIs, there's no
    /// response menu. Instead, if there are multiple player responses, it
    /// automatically chooses the first one.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class BarkDialogueUI : MonoBehaviour, IDialogueUI
    {

        [Tooltip("Play sequence associated with bark. ConversationView already plays it, but tick this if bark UI needs to wait for sequence to end.")]
        public bool playSequence = false;

        public event EventHandler<SelectedResponseEventArgs> SelectedResponseHandler;

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void ShowSubtitle(Subtitle subtitle)
        {
            // Use the bark UI to show the subtitle, don't tell it to skip
            // the sequence since the ConversationView will already play it:
            StartCoroutine(BarkController.Bark(subtitle, !playSequence));
        }

        public void HideSubtitle(Subtitle subtitle)
        {
        }

        public void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
        {
            if (responses.Length > 0)
            {
                SelectedResponseHandler(this, new SelectedResponseEventArgs(responses[0]));
            }
        }

        public void HideResponses()
        {
        }

        public void ShowQTEIndicator(int index)
        {
        }

        public void HideQTEIndicator(int index)
        {
        }

        public void ShowAlert(string message, float duration)
        {
        }

        public void HideAlert()
        {
        }

    }

}
