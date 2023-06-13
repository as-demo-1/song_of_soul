// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Info about a line of dialogue. Subtitles are created by the ConversationModel and passed by 
    /// the ConversationView to a dialogue UI to be displayed and to the Sequencer to play any 
    /// associated sequence.
    /// </summary>
    public class Subtitle
    {

        /// <summary>
        /// Info about the speaker of this line.
        /// </summary>
        public CharacterInfo speakerInfo;

        /// <summary>
        /// Info about the listener to whom the line is being spoken.
        /// </summary>
        public CharacterInfo listenerInfo;

        /// <summary>
        /// The formatted text of the line. The IDialogueUI displays this text.
        /// </summary>
        public FormattedText formattedText;

        /// <summary>
        /// The sequence associated with the line. The Sequencer plays this sequence. See
        /// @ref sequencer.
        /// </summary>
        public string sequence;

        /// <summary>
        /// The response menu sequence associated with the line. If not blank, this sequence
        /// will play while the response menu is being shown.
        /// </summary>
        public string responseMenuSequence;

        /// <summary>
        /// The dialogue entry that the subtitle information came from.
        /// </summary>
        public DialogueEntry dialogueEntry;

        /// <summary>
        /// The entrytag associated with the dialogue entry.
        /// </summary>
        public string entrytag = string.Empty;

        /// <summary>
        /// The active conversation record associated with this subtitle; useful if
        /// multiple simultaneous conversations are running and you need to distinguish them.
        /// </summary>
        public ActiveConversationRecord activeConversationRecord = null;

        /// <summary>
        /// Initializes a new Subtitle.
        /// </summary>
        /// <param name="speakerInfo">Speaker info.</param>
        /// <param name="listenerInfo">Listener info.</param>
        /// <param name="formattedText">Formatted text.</param>
        /// <param name="sequence">Sequence.</param>
        /// <param name="responseMenuSequence">Response menu sequence.</param>
        /// <param name="dialogueEntry">Dialogue entry.</param>
        public Subtitle(CharacterInfo speakerInfo, CharacterInfo listenerInfo, FormattedText formattedText,
                        string sequence, string responseMenuSequence, DialogueEntry dialogueEntry)
        {
            this.speakerInfo = speakerInfo;
            this.listenerInfo = listenerInfo;
            this.formattedText = formattedText;
            this.sequence = sequence;
            this.responseMenuSequence = responseMenuSequence;
            this.dialogueEntry = dialogueEntry;
            this.entrytag = null;
            CheckVariableInputPrompt();
        }

        /// <summary>
        /// Initializes a new Subtitle.
        /// </summary>
        /// <param name="speakerInfo">Speaker info.</param>
        /// <param name="listenerInfo">Listener info.</param>
        /// <param name="formattedText">Formatted text.</param>
        /// <param name="sequence">Sequence.</param>
        /// <param name="responseMenuSequence">Response menu sequence.</param>
        /// <param name="dialogueEntry">Dialogue entry.</param>
        /// <param name="entrytag">Entrytag.</param>
        public Subtitle(CharacterInfo speakerInfo, CharacterInfo listenerInfo, FormattedText formattedText,
                        string sequence, string responseMenuSequence, DialogueEntry dialogueEntry, string entrytag)
        {
            this.speakerInfo = speakerInfo;
            this.listenerInfo = listenerInfo;
            this.formattedText = formattedText;
            this.sequence = sequence;
            this.responseMenuSequence = responseMenuSequence;
            this.dialogueEntry = dialogueEntry;
            this.entrytag = entrytag;
            CheckVariableInputPrompt();
        }

        /// <summary>
        /// If the formatted text has a variable input prompt, add a 
        /// TextInput() sequencer command.
        /// </summary>
        private void CheckVariableInputPrompt()
        {
            if ((formattedText != null) && formattedText.hasVariableInputPrompt)
            {
                sequence = string.Format("{0}{1}TextInput(Text Field UI,{2},{2})",
                                         sequence,
                                         (string.IsNullOrEmpty(sequence) ? string.Empty : "; "),
                                         formattedText.variableInputPrompt);
            }
        }

        /// <summary>
        /// Gets the speaker portrait for this line. This is normally speakerInfo.portrait, but
        /// it could be overrided by [pic*] tags in the formatted text.
        /// </summary>
        /// <returns>The speaker portrait.</returns>
        public Sprite GetSpeakerPortrait()
        {
            if (speakerInfo == null) return null;
            if (formattedText == null) return speakerInfo.portrait;
            if (formattedText.pic != FormattedText.NoPicOverride) return speakerInfo.GetPicOverride(formattedText.pic);
            if (formattedText.picActor != FormattedText.NoPicOverride) return speakerInfo.GetPicOverride(formattedText.picActor);
            if ((formattedText.picConversant != FormattedText.NoPicOverride) && (listenerInfo != null)) return listenerInfo.GetPicOverride(formattedText.picConversant);
            return speakerInfo.portrait;
        }

    }

}
