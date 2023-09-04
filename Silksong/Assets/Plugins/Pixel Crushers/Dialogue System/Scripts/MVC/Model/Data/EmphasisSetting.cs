// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// An emphasis setting specifies a text style. Chat Mapper projects define emphasis settings
    /// for formatting lines of dialogue. Every dialogue database stores an array of emphasis 
    /// settings.
    /// </summary>
    [System.Serializable]
    public class EmphasisSetting
    {

        /// <summary>
        /// The color of the text.
        /// </summary>
        public Color color = Color.black;

        /// <summary>
        /// If <c>true</c>, draw the text in bold.
        /// </summary>
        public bool bold = false;

        /// <summary>
        /// If <c>true</c>, draw the text in italics.
        /// </summary>
        public bool italic = false;

        /// <summary>
        /// If <c>true</c>, underline the text.
        /// </summary>
        public bool underline = false;

        /// <summary>
        /// Initializes a new EmphasisSetting.
        /// </summary>
        /// <param name='color'>
        /// Text color.
        /// </param>
        /// <param name='bold'>
        /// Bold flag.
        /// </param>
        /// <param name='italic'>
        /// Italic flag.
        /// </param>
        /// <param name='underline'>
        /// Underline flag.
        /// </param>
        public EmphasisSetting(Color color, bool bold, bool italic, bool underline)
        {
            this.color = color;
            this.bold = bold;
            this.italic = italic;
            this.underline = underline;
        }

        /// <summary>
        /// Initializes a new EmphasisSetting.
        /// </summary>
        /// <param name='colorCode'>
        /// A web RGB-format color code of the format "\#rrggbb", where rr, gg, and bb are 
        /// hexadecimal values (e.g., \#ff0000 for red).
        /// </param>
        /// <param name='styleCode'>
        /// A style code of the format "biu", where b=bold, i=italic, u=underline, and a dash turns
        /// the setting off. For example, "b--" means bold only.
        /// </param>
        public EmphasisSetting(string colorCode, string styleCode)
        {
            color = Tools.WebColor(colorCode);
            bold = !string.IsNullOrEmpty(styleCode) && (styleCode.Length > 0) && (styleCode[0] == 'b');
            italic = (!string.IsNullOrEmpty(styleCode)) && (styleCode.Length > 1) && (styleCode[1] == 'i');
            underline = (!string.IsNullOrEmpty(styleCode)) && (styleCode.Length > 2) && (styleCode[2] == 'u');
        }

        public bool IsEmpty
        {
            get { return (color == Color.black) && !bold && !italic && !underline; }
        }

    }

}