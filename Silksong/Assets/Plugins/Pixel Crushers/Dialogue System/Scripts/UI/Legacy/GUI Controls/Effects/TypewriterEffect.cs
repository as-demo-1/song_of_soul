using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Applies a typewriter effect to the text of a GUI control. Characters are displayed one at a
    /// time at a rate of charactersPerSecond.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class TypewriterEffect : GUIEffect
    {

        /// <summary>
        /// How fast to "type."
        /// </summary>
        public float charactersPerSecond = 50;

        /// <summary>
        /// The audio clip to play with each character.
        /// </summary>
        public AudioClip audioClip = null;

        /// <summary>
        /// Indicates whether the effect is playing.
        /// </summary>
        /// <value><c>true</c> if this instance is playing; otherwise, <c>false</c>.</value>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// Plays the typewriter effect.
        /// </summary>
        public override IEnumerator Play()
        {
            GUILabel control = GetComponent<GUILabel>();
            if (control == null) yield break;
            IsPlaying = true;
            control.currentLength = 0;
            while ((control.currentLength + 1) < control.text.Length)
            {
                float delay = 1 / charactersPerSecond;
                if (!DialogueTime.isPaused)
                {
                    if (audioClip != null && control.currentLength > 0) control.PlaySound(audioClip);
                    AdvanceOneCharacter(control);
                    // The code below adds extra delay for punctuation, but I'm not sure I like it:
                    // char c = control.text[control.currentLength];
                    // if (c == '.' || c == '\n' || c == '!' || c == '?') delay *= 4f;
                }

                yield return StartCoroutine(DialogueTime.WaitForSeconds(delay)); // new WaitForSeconds(delay);
            }
            control.currentLength = control.text.Length;
            control.ResetClosureTags();
            IsPlaying = false;
        }

        private const string RichTextBoldOpen = "<b>";
        private const string RichTextBoldClose = "</b>";
        private const string RichTextItalicOpen = "<i>";
        private const string RichTextItalicClose = "</i>";
        private const string RichTextColorOpenPrefix = "<color=";
        private const string RichTextColorClose = "</color>";

        /// <summary>
        /// Advances the label one character or rich text code.
        /// </summary>
        /// <param name="control">GUI Label to advance.</param>
        private void AdvanceOneCharacter(GUILabel control)
        {
            if (control.text[control.currentLength] == '<')
            {
                if (string.Compare(control.text, control.currentLength, RichTextBoldOpen, 0, RichTextBoldOpen.Length) == 0)
                {
                    control.currentLength += RichTextBoldOpen.Length;
                    control.PushClosureTag(RichTextBoldClose);
                }
                else if (string.Compare(control.text, control.currentLength, RichTextBoldClose, 0, RichTextBoldClose.Length) == 0)
                {
                    control.currentLength += RichTextBoldClose.Length;
                    control.PopClosureTag();
                }
                else if (string.Compare(control.text, control.currentLength, RichTextItalicOpen, 0, RichTextItalicOpen.Length) == 0)
                {
                    control.currentLength += RichTextItalicOpen.Length;
                    control.PushClosureTag(RichTextItalicClose);
                }
                else if (string.Compare(control.text, control.currentLength, RichTextItalicClose, 0, RichTextItalicClose.Length) == 0)
                {
                    control.currentLength += RichTextItalicClose.Length;
                    control.PopClosureTag();
                }
                if (string.Compare(control.text, control.currentLength, RichTextColorOpenPrefix, 0, RichTextColorOpenPrefix.Length) == 0)
                {
                    control.currentLength += RichTextColorOpenPrefix.Length + 10; // <color=#rrggbbaa>
                    control.PushClosureTag(RichTextColorClose);
                }
                else if (string.Compare(control.text, control.currentLength, RichTextColorClose, 0, RichTextColorClose.Length) == 0)
                {
                    control.currentLength += RichTextColorClose.Length;
                    control.PopClosureTag();
                }
            }
            else
            {
                control.currentLength++;
            }
        }

        /// <summary>
        /// Stops the effect.
        /// </summary>
        public override void Stop()
        {
            base.Stop();
            IsPlaying = false;
            GUILabel control = GetComponent<GUILabel>();
            if (control != null)
            {
                control.currentLength = control.text.Length;
                control.ResetClosureTags();
            }
        }

    }

}
