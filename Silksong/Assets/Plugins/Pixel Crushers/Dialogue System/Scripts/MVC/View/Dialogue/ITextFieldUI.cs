// Copyright (c) Pixel Crushers. All rights reserved.

using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This delegate is called when the player accepts the input.
    /// </summary>
    public delegate void AcceptedTextDelegate(string text);

    /// <summary>
    /// Interface for text field UI components. Components implementing this interface
    /// should provide a GUI control for entering text.
    /// 
    /// See UnityTextFieldUI for a reference implementation that uses Unity GUI.
    /// </summary>
    public interface ITextFieldUI
    {

        /// <summary>
        /// Called to display the text field and start handling input. When the user accepts the input
        /// (e.g., by pressing the Return key), call the acceptedText delegate.
        /// </summary>
        void StartTextInput(string labelText, string text, int maxLength, AcceptedTextDelegate acceptedText);

        /// <summary>
        /// Called to force the text field to cancel.
        /// </summary>
        void CancelTextInput();

    }

}
