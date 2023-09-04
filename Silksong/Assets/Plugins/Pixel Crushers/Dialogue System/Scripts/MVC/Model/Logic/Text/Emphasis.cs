// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Specifies special text formatting for a substring. Emphases are used by FormattedText.
    /// </summary>
    public struct Emphasis
    {

        /// <summary>
        /// Gets or sets the first character where the formatting begins.
        /// </summary>
        /// <value>
        /// The start index.
        /// </value>
        public int startIndex { get; set; }

        /// <summary>
        /// Gets or sets the length of the substring that should be formatted.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int length { get; set; }

        /// <summary>
        /// Gets or sets the color to use for this substring.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public Color color { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the substring in bold.
        /// </summary>
        /// <value>
        /// <c>true</c> if bold; otherwise, <c>false</c>.
        /// </value>
        public bool bold { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the substring in italics.
        /// </summary>
        /// <value>
        /// <c>true</c> if italic; otherwise, <c>false</c>.
        /// </value>
        public bool italic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the substring underlined.
        /// </summary>
        /// <value>
        /// <c>true</c> if underlined; otherwise, <c>false</c>.
        /// </value>
        public bool underline { get; set; }

        /// <summary>
        /// Initializes a new Emphasis struct.
        /// </summary>
        /// <param name='startIndex'>
        /// Start index of the substring.
        /// </param>
        /// <param name='length'>
        /// Length of the substring.
        /// </param>
        /// <param name='color'>
        /// Color of the substring.
        /// </param>
        /// <param name='bold'>
        /// If <c>true</c>, display in bold.
        /// </param>
        /// <param name='italic'>
        /// If <c>true</c>, display in italics.
        /// </param>
        /// <param name='underline'>
        /// If <c>true</c>, display underlined.
        /// </param>
        public Emphasis(int startIndex, int length, Color color, bool bold, bool italic, bool underline) : this()
        {
            this.startIndex = startIndex;
            this.length = length;
            this.color = color;
            this.bold = bold;
            this.italic = italic;
            this.underline = underline;
        }

    }

}
