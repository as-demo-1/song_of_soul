using UnityEngine;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Static utility class that provides methods for drawing Unity GUI controls.
    /// </summary>
    public static class UnityGUITools
    {

        /// <summary>
        /// Draws a text label using a specified GUI style and text style.
        /// </summary>
        /// <param name='rect'>
        /// Rect where the label will be drawn.
        /// </param>
        /// <param name='text'>
        /// Text to draw.
        /// </param>
        /// <param name='guiStyle'>
        /// GUI style.
        /// </param>
        /// <param name='textStyle'>
        /// Text style.
        /// </param>
        public static void DrawText(Rect rect, string text, GUIStyle guiStyle, TextStyle textStyle = TextStyle.None)
        {
            if ((textStyle == TextStyle.Outline) || (textStyle == TextStyle.Shadow))
            {
                Color normalColor = guiStyle.normal.textColor;
                guiStyle.normal.textColor = new Color(0, 0, 0, guiStyle.normal.textColor.a);
                GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, guiStyle);
                if (textStyle == TextStyle.Outline)
                {
                    GUI.Label(new Rect(rect.x + 1, rect.y - 1, rect.width, rect.height), text, guiStyle);
                    GUI.Label(new Rect(rect.x - 1, rect.y + 1, rect.width, rect.height), text, guiStyle);
                    GUI.Label(new Rect(rect.x - 1, rect.y - 1, rect.width, rect.height), text, guiStyle);
                }
                guiStyle.normal.textColor = normalColor;
            }
            GUI.Label(rect, text, guiStyle);
        }

        /// <summary>
        /// Draws a text label using a specified GUI style and text style.
        /// </summary>
        /// <param name='rect'>
        /// Rect where the label will be drawn.
        /// </param>
        /// <param name='text'>
        /// Text to draw.
        /// </param>
        /// <param name='guiStyle'>
        /// GUI style.
        /// </param>
        /// <param name='textStyle'>
        /// Text style.
        /// </param>
        /// <param name='textStyleColor'>
        /// Text style color used for outlines and drop shadows.
        /// </param>
        public static void DrawText(Rect rect, string text, GUIStyle guiStyle, TextStyle textStyle, Color textStyleColor)
        {
            if ((textStyle == TextStyle.Outline) || (textStyle == TextStyle.Shadow))
            {
                Color normalColor = guiStyle.normal.textColor;
                guiStyle.normal.textColor = new Color(textStyleColor.r, textStyleColor.g, textStyleColor.b, guiStyle.normal.textColor.a);
                GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, guiStyle);
                if (textStyle == TextStyle.Outline)
                {
                    GUI.Label(new Rect(rect.x + 1, rect.y - 1, rect.width, rect.height), text, guiStyle);
                    GUI.Label(new Rect(rect.x - 1, rect.y + 1, rect.width, rect.height), text, guiStyle);
                    GUI.Label(new Rect(rect.x - 1, rect.y - 1, rect.width, rect.height), text, guiStyle);
                }
                guiStyle.normal.textColor = normalColor;
            }
            GUI.Label(rect, text, guiStyle);
        }

        /// <summary>
        /// Gets the most applicable GUI skin. It first tries the specified guiSkin, then
        /// the DialogueManager's UnityDialogueUI's skin if applicable, then the default skin.
        /// </summary>
        /// <returns>
        /// A valid (non-null) GUI skin.
        /// </returns>
        /// <param name='guiSkin'>
        /// The first GUI skin to try.
        /// </param>
        public static GUISkin GetValidGUISkin(GUISkin guiSkin)
        {
            return (guiSkin != null) ? guiSkin : GetDialogueManagerGUISkin();
        }

        /// <summary>
        /// Gets the GUI skin on the DialogueManager's UnityDialogueUI.
        /// </summary>
        /// <returns>
        /// The GUI skin on the DialogueManager's UnityDialogueUI, or <c>null</c> if it doesn't 
        /// exist.
        /// </returns>
        public static GUISkin GetDialogueManagerGUISkin()
        {
            UnityDialogueUI unityDialogueUI = DialogueManager.dialogueUI as UnityDialogueUI;
            return ((unityDialogueUI != null) && (unityDialogueUI.guiRoot != null) && (unityDialogueUI.guiRoot.guiSkin != null))
                ? unityDialogueUI.guiRoot.guiSkin
                : GUI.skin;
        }

        /// <summary>
        /// Using the current skin, gets a named GUI style.
        /// </summary>
        /// <returns>
        /// A copy of the GUI style named by guiStyleName, or defaultGUIStyle if the named style
        /// doesn't exist in the current skin. Since this is a copy, you can modify it without 
        /// affecting the original.
        /// </returns>
        /// <param name='guiStyleName'>
        /// GUI style name.
        /// </param>
        /// <param name='defaultGUIStyle'>
        /// Default GUI style to use if the named style doesn't exist.
        /// </param>
        public static GUIStyle GetGUIStyle(string guiStyleName, GUIStyle defaultGUIStyle)
        {
            GUIStyle guiStyle = string.IsNullOrEmpty(guiStyleName) ? null : GUI.skin.FindStyle(guiStyleName);
            return new GUIStyle(guiStyle ?? defaultGUIStyle);
        }

        /// <summary>
        /// Sets the alpha value of a color.
        /// </summary>
        /// <returns>
        /// The specified color, with the alpha value set to the specified alpha.
        /// </returns>
        /// <param name='color'>
        /// Color.
        /// </param>
        /// <param name='alpha'>
        /// Alpha.
        /// </param>
        public static Color ColorWithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Returns a font style that has bold applied. If the font already has 
        /// italics, the returned style will have bold and italics.
        /// </summary>
        /// <returns>
        /// The font style with bold.
        /// </returns>
        /// <param name='fontStyle'>
        /// The original font style.
        /// </param>
        public static FontStyle ApplyBold(FontStyle fontStyle)
        {
            return (fontStyle == FontStyle.Italic) ? FontStyle.BoldAndItalic : FontStyle.Bold;
        }

        /// <summary>
        /// Returns a font style that has italics applied. If the font already has 
        /// bold, the returned style will have bold and italics.
        /// </summary>
        /// <returns>
        /// The font style with italics.
        /// </returns>
        /// <param name='fontStyle'>
        /// The original font style.
        /// </param>
        public static FontStyle ApplyItalic(FontStyle fontStyle)
        {
            return (fontStyle == FontStyle.Bold) ? FontStyle.BoldAndItalic : FontStyle.Italic;
        }

        /// <summary>
        /// Applies the formatting specified in a FormattedText to a GUI style. If the formatting
        /// contains any emphases, currently only applies the first emphasis.
        /// </summary>
        /// <returns>
        /// The same GUI style (not a copy) with formatting applied.
        /// </returns>
        /// <param name='formattingToApply'>
        /// Formatting to apply.
        /// </param>
        /// <param name='guiStyle'>
        /// GUI style.
        /// </param>
        public static GUIStyle ApplyFormatting(FormattedText formattingToApply, GUIStyle guiStyle)
        {
            if ((guiStyle != null) && (formattingToApply != null))
            {
                if (formattingToApply.italic) guiStyle.fontStyle = ApplyItalic(guiStyle.fontStyle);
                if ((formattingToApply.emphases != null) && (formattingToApply.emphases.Length > 0))
                {
                    guiStyle.normal.textColor = formattingToApply.emphases[0].color;
                    if (formattingToApply.emphases[0].bold) guiStyle.fontStyle = ApplyBold(guiStyle.fontStyle);
                    if (formattingToApply.emphases[0].italic) guiStyle.fontStyle = ApplyItalic(guiStyle.fontStyle);
                }
            }
            return guiStyle;
        }

    }

}
