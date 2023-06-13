using UnityEngine;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Uses a specified text color for subtitle lines spoken by the actor.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class ActorSubtitleColor : MonoBehaviour
    {

        /// <summary>
        /// The color to use for subtitle lines spoken by this actor.
        /// </summary>
        [Tooltip("Color to use for subtitle lines spoken by this actor.")]
        public Color color = Color.white;

        public enum ApplyTo { DialogueText, PrependedActorName }

        [Tooltip("Apply color to entire Dialogue Text or prepend actor name and apply color only to name.")]
        public ApplyTo applyTo = ApplyTo.DialogueText;

        [Tooltip("If prepending actor name, separate from Dialogue Text with this string.")]
        public string prependActorNameSeparator = ": ";

        public void OnConversationLine(Subtitle subtitle)
        {
            CheckSubtitle(subtitle);
        }

        public void OnBarkLine(Subtitle subtitle)
        {
            CheckSubtitle(subtitle);
        }

        private void CheckSubtitle(Subtitle subtitle)
        {
            if (subtitle != null && subtitle.speakerInfo != null && subtitle.speakerInfo.transform == this.transform)
            {
                subtitle.formattedText.text = ProcessText(subtitle);
            }
        }

        private string ProcessText(Subtitle subtitle)
        {
            switch (applyTo)
            {
                default:
                case ApplyTo.DialogueText:
                    return UITools.WrapTextInColor(subtitle.formattedText.text, color);
                case ApplyTo.PrependedActorName:
                    return UITools.WrapTextInColor(subtitle.speakerInfo.Name + prependActorNameSeparator, color) + subtitle.formattedText.text;
            }
        }

    }
}
