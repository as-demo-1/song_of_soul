using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// A GUI control that implements GUI.Label to display text and/or a texture.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class GUILabel : GUIVisibleControl
    {

        /// <summary>
        /// The text style for the text.
        /// </summary>
        public TextStyle textStyle = TextStyle.None;

        /// <summary>
        /// The color of the text style's outline or shadow.
        /// </summary>
        public Color textStyleColor = Color.black;

        /// <summary>
        /// The color to tint the image.
        /// </summary>
        public Color imageColor = Color.white;

        /// <summary>
        /// An image to display (may be unassigned).
        /// </summary>
        public Texture2D image;

        /// <summary>
        /// The image animation settings.
        /// </summary>
        public ImageAnimation imageAnimation = new ImageAnimation();

        /// <summary>
        /// The rich text closure tags to apply. This is used by the typewriter effect
        /// to close rich text tags correctly before the whole line is done. This list
        /// acts as a stack.
        /// </summary>
        private List<string> closureTags = new List<string>();

        /// <summary>
        /// Gets or sets the length of the current amount of text to display. This is a support 
        /// property for effects like TypewriterEffect.
        /// </summary>
        /// <value>
        /// The length of the current substring.
        /// </value>
        public int currentLength
        {
            get
            {
                return substringLength;
            }
            set
            {
                substringLength = value;
                useSubstring = !string.IsNullOrEmpty(text) && (substringLength < text.Length);
            }
        }

        /// <summary>
        /// If <c>true</c> use only a substring of the text.
        /// </summary>
        private bool useSubstring = false;

        /// <summary>
        /// The length of the substring.
        /// </summary>
        private int substringLength = 0;

        /// <summary>
        /// The substring length the last time GetRichTextClosedString() was called.
        /// We keep track of this so we don't need to build the rich text-closed string
        /// every OnGUI update.
        /// </summary>
        private int substringLengthLastGetRichTextClosedString = 0;

        /// <summary>
        /// The rich text-closed string.
        /// </summary>
        private string richTextClosedString = string.Empty;

        public override void Awake()
        {
            base.Awake();
            ResetClosureTags();
        }

        /// <summary>
        /// Resets the stack of rich text tags to close.
        /// </summary>
        public void ResetClosureTags()
        {
            closureTags.Clear();
        }

        /// <summary>
        /// Adds a closure tag to the stack.
        /// </summary>
        /// <param name="tag">Tag.</param>
        public void PushClosureTag(string tag)
        {
            closureTags.Add(tag);
        }

        /// <summary>
        /// Pops the top closure tag from the stack.
        /// </summary>
        /// <param name="tag">Tag.</param>
        public void PopClosureTag()
        {
            if (closureTags.Count > 0) closureTags.RemoveAt(closureTags.Count - 1);
        }

        /// <summary>
        /// Sets the control's text and formatting. Also resets the rich text tag count.
        /// </summary>
        /// <param name="formattedText">Formatted text.</param>
        public override void SetFormattedText(FormattedText formattedText)
        {
            base.SetFormattedText(formattedText);
            ResetClosureTags();
        }

        /// <summary>
        /// Draws the control, but not its children.
        /// </summary>
        /// <param name="relativeMousePosition">Relative mouse position within the window containing this control.</param>
        public override void DrawSelf(Vector2 relativeMousePosition)
        {
            ApplyAlphaToGUIColor();
            if (image != null)
            {
                DrawImage();
            }
            if (!string.IsNullOrEmpty(text))
            {
                if (useSubstring)
                {
                    DrawSubstring();
                }
                else
                {
                    UnityGUITools.DrawText(rect, text, GuiStyle, textStyle, textStyleColor);
                }
            }
            RestoreGUIColor();
        }

        private void DrawSubstring()
        {
            //--- Was: (Used Unity's left align, but caused ugly word backtrace at end of lines)
            if (IsLeftAligned(GuiStyle.alignment))
            {
                //	UnityGUITools.DrawText(rect, GetRichTextClosedText(text.Substring(0, substringLength)), GuiStyle, textStyle, textStyleColor);
                DrawSubstringLeftAligned();
            }
            else
            {
                DrawSubstringNotLeftAligned();
            }
        }

        private bool IsLeftAligned(TextAnchor textAnchor)
        {
            return (textAnchor == TextAnchor.LowerLeft) ||
                (textAnchor == TextAnchor.MiddleLeft) ||
                    (textAnchor == TextAnchor.UpperLeft);
        }

        private bool IsCenterAligned(TextAnchor textAnchor)
        {
            return (textAnchor == TextAnchor.LowerCenter) ||
                (textAnchor == TextAnchor.MiddleCenter) ||
                    (textAnchor == TextAnchor.UpperCenter);
        }

        private TextAnchor GetLeftAlignedVersion(TextAnchor textAnchor)
        {
            switch (textAnchor)
            {
                case TextAnchor.LowerCenter:
                case TextAnchor.LowerRight:
                    return TextAnchor.LowerLeft;
                case TextAnchor.MiddleCenter:
                case TextAnchor.MiddleRight:
                    return TextAnchor.MiddleLeft;
                case TextAnchor.UpperCenter:
                case TextAnchor.UpperRight:
                    return TextAnchor.UpperLeft;
                default:
                    return textAnchor;
            }
        }

        private void DrawSubstringLeftAligned()
        {
            var currText = string.Empty;
            int start = 0;
            int charsLeft = substringLength;
            while (charsLeft > 0)
            {
                string line = GetNextLine(text, start);
                string subLine = line.Substring(0, Mathf.Min(line.Length, charsLeft));
                start += line.Length;
                charsLeft -= line.Length;
                if (string.IsNullOrEmpty(currText))
                {
                    currText = subLine;
                }
                else
                {
                    currText += "\n" + subLine;
                }
            }
            UnityGUITools.DrawText(rect, GetRichTextClosedText(currText), GuiStyle, textStyle, textStyleColor);
        }

        private void DrawSubstringNotLeftAligned()
        {
            TextAnchor originalAlignment = GuiStyle.alignment;
            bool originalWordWrap = GuiStyle.wordWrap;
            GuiStyle.alignment = GetLeftAlignedVersion(GuiStyle.alignment);
            GuiStyle.wordWrap = false;
            float lineHeight = GuiStyle.CalcSize(new GUIContent(text)).y;
            float y = rect.y;
            int start = 0;
            int charsLeft = substringLength;
            while (charsLeft > 0)
            {
                string line = GetNextLine(text, start);
                string subLine = line.Substring(0, Mathf.Min(line.Length, charsLeft));
                start += line.Length;
                charsLeft -= line.Length;
                float lineWidth = GuiStyle.CalcSize(new GUIContent(line.Trim())).x;
                float x = IsCenterAligned(originalAlignment)
                    ? Mathf.Ceil(rect.x + (0.5f * rect.width) - (0.5f * lineWidth)) + 0.5f
                        : rect.x + rect.width - lineWidth;
                UnityGUITools.DrawText(new Rect(x, y, rect.width, lineHeight), GetRichTextClosedText(subLine.Trim()), GuiStyle, textStyle, textStyleColor);
                y += GuiStyle.lineHeight;
            }
            GuiStyle.alignment = originalAlignment;
            GuiStyle.wordWrap = originalWordWrap;

        }

        private string GetNextLine(string text, int start)
        {
            string remainder = text.Substring(start);
            int lastSpace = 0;
            if (GuiStyle.CalcSize(new GUIContent(remainder.Trim())).x > rect.width)
            {
                int length = 1;
                while (start + length < text.Length)
                {
                    string candidate = text.Substring(start, length + 1);
                    if (text[start + length] == ' ') lastSpace = length;
                    float width = GuiStyle.CalcSize(new GUIContent(candidate.Trim())).x;
                    if (width < rect.width)
                    {
                        length++;
                    }
                    else
                    {
                        int maxLength = remainder.Length;
                        if (lastSpace > 0)
                        {
                            return text.Substring(start, Mathf.Max(1, Mathf.Min(lastSpace, maxLength)));
                        }
                        else
                        {
                            return text.Substring(start, Mathf.Max(1, Mathf.Min(length - 1, maxLength)));
                        }
                    }
                }
            }
            return remainder;
        }

        /// <summary>
        /// Adds rich text closure tags if necessary.
        /// </summary>
        /// <returns>The rich text closed text.</returns>
        /// <param name="s">The source string.</param>
        private string GetRichTextClosedText(string s)
        {
            if (closureTags.Count == 0)
            {
                return s;
            }
            else
            {
                if (substringLength != substringLengthLastGetRichTextClosedString)
                {
                    substringLengthLastGetRichTextClosedString = substringLength;
                    StringBuilder sb = new StringBuilder(s);
                    for (int i = closureTags.Count - 1; i >= 0; i--)
                    {
                        sb.Append(closureTags[i]);
                    }
                    richTextClosedString = sb.ToString();
                }
                return richTextClosedString;
            }
        }

        private void DrawImage()
        {
            Color originalColor = GUI.color;
            GUI.color = imageColor;
            if (imageAnimation.animate)
            {
                imageAnimation.DrawAnimation(rect, image);
            }
            else
            {
                GUI.Label(rect, image, GuiStyle);
            }
            GUI.color = originalColor;
        }

        public override void Refresh()
        {
            base.Refresh();
            if (imageAnimation.animate) imageAnimation.RefreshAnimation(image);
        }

    }

}
