using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Parameters for using GUI.DrawTexture[WithTexCoords].
    /// </summary>
    [System.Serializable]
    public class GUIImageParams
    {

        /// <summary>
        /// The pixel offset and size of the image. If the width and height are zero, it uses the
        /// control's scaled rect size.
        /// </summary>
        public Rect pixelRect;

        /// <summary>
        /// The image to display.
        /// </summary>
        public Texture2D texture;

        /// <summary>
        /// If <c>true</c>, uses texCoords; otherwise scales according to scaleMode and imageAspect.
        /// </summary>
        public bool useTexCoords = false;

        /// <summary>
        /// The tex coords (in the range 0..1) of the source image. Used if useTexCoords is true.
        /// </summary>
        public Rect texCoords = new Rect(0, 0, 1, 1);

        /// <summary>
        /// The scale mode (if not using tex coords).
        /// </summary>
        public ScaleMode scaleMode = ScaleMode.ScaleToFit;

        /// <summary>
        /// If <c>true</c>, alpha blends the image.
        /// </summary>
        public bool alphaBlend = true;

        /// <summary>
        /// The color to tint the image.
        /// </summary>
        public Color color = Color.white;

        /// <summary>
        /// The aspect ratio for the source image.
        /// </summary>
        public float aspect = 0;

        /// <summary>
        /// Draws the image in the specified rect using full alpha.
        /// </summary>
        /// <param name="rect">Rect.</param>
        public void Draw(Rect rect)
        {
            Draw(rect, false, 1f);
        }

        /// <summary>
        /// Draw the specified rect, taking alpha into account.
        /// </summary>
        /// <param name="rect">Rect.</param>
        /// <param name="hasAlpha">If set to <c>true</c> has alpha.</param>
        /// <param name="alpha">Alpha value of color.</param>
        public void Draw(Rect rect, bool hasAlpha, float alpha)
        {
            if (texture != null)
            {
                Color originalGuiColor = GUI.color;
                GUI.color = color;
                if (hasAlpha) GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
                Rect imageRect = new Rect(rect.x + pixelRect.x, rect.y + pixelRect.y, Tools.ApproximatelyZero(pixelRect.width) ? rect.width : pixelRect.width, Tools.ApproximatelyZero(pixelRect.width) ? rect.height : pixelRect.height);
                if (useTexCoords)
                {
                    GUI.DrawTextureWithTexCoords(imageRect, texture, texCoords, alphaBlend);
                }
                else
                {
                    GUI.DrawTexture(imageRect, texture, scaleMode, alphaBlend, aspect);
                }
                //if (hasAlpha) 
                GUI.color = originalGuiColor;
            }
        }

    }

}
