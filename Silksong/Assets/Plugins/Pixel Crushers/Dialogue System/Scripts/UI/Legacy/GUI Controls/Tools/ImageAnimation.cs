using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Specifies image animation settings.
    /// </summary>
    [System.Serializable]
    public class ImageAnimation
    {

        /// <summary>
        /// If <c>true</c>, animate the image.
        /// </summary>
        public bool animate = false;

        /// <summary>
        /// The width of the frame (one cel of animation).
        /// </summary>
        public int frameWidth = 64;

        /// <summary>
        /// Speed to show frames.
        /// </summary>
        public float framesPerSecond = 1f;

        private int numFrames = 1;

        private float frameNormalWidth = 1;

        private int currentFrame = 0;

        private float nextFrameTime = 0;

        private Rect texCoords;

        private float lastDialogueTime = 0;

        public void RefreshAnimation(Texture2D image)
        {
            if (image == null) return;
            if (!Application.isPlaying) return;
            if (image != null)
            {
                numFrames = image.width / Mathf.Max(frameWidth, 1);
                frameNormalWidth = 1 / (float)Mathf.Max(numFrames, 1);
                nextFrameTime = DialogueTime.time + (1 / Mathf.Max(framesPerSecond, 0.05f));
                lastDialogueTime = DialogueTime.time;
            }
            else
            {
                nextFrameTime = Mathf.Infinity;
            }
        }

        public void DrawAnimation(Rect rect, Texture2D image)
        {
            if (Application.isPlaying)
            {
                if ((DialogueTime.time >= nextFrameTime) || (DialogueTime.time < lastDialogueTime))
                {
                    if (numFrames == 0 || frameNormalWidth == 0)
                    {
                        numFrames = image.width / Mathf.Max(frameWidth, 1);
                        frameNormalWidth = 1 / (float)Mathf.Max(numFrames, 1);
                    }
                    currentFrame = (currentFrame + 1) % Mathf.Max(numFrames, 1);
                    texCoords = new Rect((float)currentFrame * frameNormalWidth, 0, frameNormalWidth, 1);
                    nextFrameTime = DialogueTime.time + (1 / Mathf.Max(framesPerSecond, 0.05f));
                }
                lastDialogueTime = DialogueTime.time;
            }
            else
            {
                texCoords = new Rect(0, 0, (float)frameWidth / (float)Mathf.Max(image.width, 1), 1);
            }
            if (texCoords.width > 0)
            {
                GUI.DrawTextureWithTexCoords(rect, image, texCoords);
            }
        }
    }

}
