/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this template, make a copy and remove the lines that start
 * [REMOVE THIS LINE] with "[REMOVE THIS LINE]". Then add your code where the comments indicate.
 * [REMOVE THIS LINE]
 * [REMOVE THIS LINE] If your code references scripts or assets that are outside of the Plugins
 * [REMOVE THIS LINE] folder, move this script outside of the Plugins folder, too.
 * [REMOVE THIS LINE]



using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

public class TemplateBarkUI : MonoBehaviour, IBarkUI
{

    public void Bark(Subtitle subtitle)
    {
        // Add your code here to display the subtitle. The basic text is in 
        // subtitle.formattedText.text. Info about the speaker (such as PC or
        // NPC, the speaker's transform, etc.) is in subtitle.speakerInfo.
    }

    public void Hide()
    {
        // Optional: Add your code here to hide the bark.
    }

    public bool isPlaying
    {
        get
        {
            // Replace the line below with your code to report whether a bark is
            // currently playing or not.
            return false;
        }
    }

}



/**/
