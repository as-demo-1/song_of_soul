using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// A GUI control that implements GUI.DrawTexture[WithTexCoords] to display a texture.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class GUIImage : GUIVisibleControl
    {

        /// <summary>
        /// The image to draw.
        /// </summary>
        public GUIImageParams image = new GUIImageParams();

        /// <summary>
        /// The image animation settings.
        /// </summary>
        public ImageAnimation imageAnimation = new ImageAnimation();

        public override void DrawSelf(Vector2 relativeMousePosition)
        {
            if (image != null)
            {
                if (imageAnimation.animate)
                {
                    imageAnimation.DrawAnimation(rect, image.texture);
                }
                else
                {
                    image.Draw(rect, HasAlpha, Alpha);
                }
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            if (imageAnimation.animate) imageAnimation.RefreshAnimation(image.texture);
        }

    }

}
