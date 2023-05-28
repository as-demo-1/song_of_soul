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

public class TemplateTextFieldUI : MonoBehaviour, ITextFieldUI
{

    /// <summary>
    /// This is an example private field to record the delegate that must be called when the player accepts
    /// the input in the text field (e.g., by pressing the Return key).
    /// </summary>
    private AcceptedTextDelegate acceptedText = null;

    /// <summary>
    /// Starts the text input field.
    /// </summary>
    /// <param name="labelText">The label text.</param>
    /// <param name="text">The current value to use for the input field.</param>
    /// <param name="maxLength">Max length, or <c>0</c> for unlimited.</param>
    /// <param name="acceptedText">The delegate to call when accepting text.</param>
    public void StartTextInput(string labelText, string text, int maxLength, AcceptedTextDelegate acceptedText)
    {
        this.acceptedText = acceptedText;
        // Add your code here to displaying your text input controls.
        // When the player accepts the input, call the acceptedText() delegate, for example using 
        // AcceptTextInput() below.
    }

    /// <summary>
    /// Cancels the text input field.
    /// </summary>
    public void CancelTextInput()
    {
        // Add your code here to cancel/hide the text input controls.
    }

    /// <summary>
    /// Accepts the text input and calls the accept handler delegate.
    /// </summary>
    private void AcceptTextInput()
    {
        // Add your code here to accept the input and hide the controls.
        if (acceptedText != null)
        {
            // Call the delegate:
            // acceptedText(<your text here>);
            acceptedText = null;
        }
    }


}



/**/
