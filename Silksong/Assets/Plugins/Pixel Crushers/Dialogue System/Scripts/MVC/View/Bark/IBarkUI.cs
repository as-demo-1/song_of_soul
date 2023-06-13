// Copyright (c) Pixel Crushers. All rights reserved.

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Interface for bark UI components. A bark is a one-off line of dialogue, typically spoken
    /// by an NPC for atmosphere (e.g., "Nice weather today"). Barks can also be used to give the 
    /// player an idea of an NPC's internal state (e.g., "I'm reloading. Cover me!").
    /// 
    /// To display barks text, the dialogue system uses a component that implements the IBarkUI 
    /// interface. See UnityBarkUI for a reference implementation that uses Unity GUI to display 
    /// text above the NPC's head.
    /// 
    /// Typically the IBarkUI implementation is attached to the character that needs to bark.
    /// You can disable subtitles by disabling the bark UI.
    /// </summary>
    /// <remarks>
    /// If a bark dialogue entry has a sequence, the BarkController also plays the sequence in
    /// addition to using IBarkUI to show the text.
    /// </remarks>
    public interface IBarkUI
    {

        /// <summary>
        /// Barks the specified subtitle. Your implementation should do something with the text
        /// in the FormattedText field.
        /// </summary>
        /// <param name='subtitle'>The subtitle to bark.</param>
        void Bark(Subtitle subtitle);

        /// <summary>
        /// Hides the currently-playing bark.
        /// </summary>
        void Hide();

        /// <summary>
        /// Indicates or sets whether a bark is playing.
        /// </summary>
        bool isPlaying { get; }

    }

}
