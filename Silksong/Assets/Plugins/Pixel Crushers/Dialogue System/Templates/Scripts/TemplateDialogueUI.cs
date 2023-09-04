/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this template, make a copy and remove the lines that start
 * [REMOVE THIS LINE] with "[REMOVE THIS LINE]". Then add your code where the comments indicate.
 * [REMOVE THIS LINE]
 * [REMOVE THIS LINE] If your code references scripts or assets that are outside of the Plugins
 * [REMOVE THIS LINE] folder, move this script outside of the Plugins folder, too.
 * [REMOVE THIS LINE]



using UnityEngine;
using System;
using System.Collections;
using PixelCrushers.DialogueSystem;

public class TemplateDialogueUI : MonoBehaviour, IDialogueUI
{

    public event EventHandler<SelectedResponseEventArgs> SelectedResponseHandler;

    public void Open()
    {
        // Add your code here to do any setup at the beginning of a conversation -- for example,
        // activating or initializing GUI controls.
    }

    public void Close()
    {
        // Add your code here to do any cleanup at the end of a conversation -- for example,
        // deactivating GUI controls.
    }

    public void ShowSubtitle(Subtitle subtitle)
    {
        // Add your code here to show a subtitle. Subtitles contain information such as the player 
        // type (PC or NPC), portrait texture, and the formatted text of the line.
    }

    public void HideSubtitle(Subtitle subtitle)
    {
        // Add your code here to hide a subtitle.
    }

    public void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
    {
        // Add your code here to show the player response menu. Populate the menu with the contents
        // of the responses array. When the player selects a response, call:
        //    SelectedResponseHandler(this, new SelectedResponseEventArgs(response));
        // where the argument "response" is the response that the player selected.
        // If (timeout > 0), auto-select a response when timeout seconds have passed.
    }

    public void HideResponses()
    {
        // Add your code here to hide the player response menu.
    }

    public void ShowQTEIndicator(int index)
    {
        /// Add your code here to show the Quick Time Event (QTE) indicator specified by the given 
        /// index. If your project doesn't use QTEs, you can leave this method empty.
    }

    public void HideQTEIndicator(int index)
    {
        /// Add your code here to hide the Quick Time Event (QTE) indicator specified by the given 
        /// index. If your project doesn't use QTEs, you can leave this method empty.
    }

    public void ShowAlert(string message, float duration)
    {
        // Add your code here to show an alert message. Note that the dialogue system will not
        // call Open() or Close() for alerts, since alerts are meant to be shown outside of
        // conversations.
    }

    public void HideAlert()
    {
        // Add your code here to hide the alert message if it's showing.
    }

}



/**/
