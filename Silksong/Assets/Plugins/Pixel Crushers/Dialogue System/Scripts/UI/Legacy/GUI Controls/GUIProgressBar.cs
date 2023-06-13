using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// A GUI control that implements a flexible progress bar. It can fill from any direction 
    /// (left, right, top, bottom, center).
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class GUIProgressBar : GUIVisibleControl
    {

        public enum Origin { Top, Bottom, Left, Right, HorizontalCenter, VerticalCenter };

        /// <summary>
        /// The direction from which the progress bar fills.
        /// </summary>
        public Origin origin;

        /// <summary>
        /// The empty image (e.g., empty frame of progress bar).
        /// </summary>
        public Texture2D emptyImage;

        /// <summary>
        /// The full image that gradually covers the empty image as progress increases.
        /// </summary>
        public Texture2D fullImage;

        /// <summary>
        /// The current progress of the bar.
        /// </summary>
        public float progress = 0;

        public override void DrawSelf(Vector2 relativeMousePosition)
        {
            if (emptyImage != null) GUI.DrawTexture(rect, emptyImage);
            float current = Mathf.Clamp(progress, 0, 1);
            Rect fullRect;
            Rect texRect;
            switch (origin)
            {
                case Origin.Top:
                    float heightFromTop = rect.height * current;
                    fullRect = new Rect(rect.x, rect.y, rect.width, heightFromTop);
                    texRect = new Rect(0, 1 - current, 1, current);
                    break;
                case Origin.Bottom:
                    float heightFromBottom = rect.height * current;
                    fullRect = new Rect(rect.x, rect.yMax - heightFromBottom, rect.width, heightFromBottom);
                    texRect = new Rect(0, 0, 1, current);
                    break;
                case Origin.Left:
                default:
                    fullRect = new Rect(rect.x, rect.y, rect.width * current, rect.height);
                    texRect = new Rect(0, 0, current, 1);
                    break;
                case Origin.Right:
                    float widthFromRight = rect.width * current;
                    fullRect = new Rect(rect.xMax - widthFromRight, rect.y, widthFromRight, rect.height);
                    texRect = new Rect(1 - current, 0, current, 1);
                    break;
                case Origin.HorizontalCenter:
                    float widthFromCenter = rect.width * current;
                    fullRect = new Rect(rect.x + (0.5f * (rect.width - widthFromCenter)), rect.y, widthFromCenter, rect.height);
                    texRect = new Rect(0.5f * (1 - current), 0, current, 1);
                    break;
                case Origin.VerticalCenter:
                    float heightFromCenter = rect.height * current;
                    fullRect = new Rect(rect.x, rect.y + (0.5f * (rect.height - heightFromCenter)), rect.width, heightFromCenter);
                    texRect = new Rect(0, 0.5f * (1 - current), 1, current);
                    break;
            }
            if (fullImage != null) GUI.DrawTextureWithTexCoords(fullRect, fullImage, texRect);
        }

    }

}
