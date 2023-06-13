using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Applies a slide effect to the GUI control, making it slide onscreen from a direction.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class SlideEffect : GUIEffect
    {

        /// <summary>
        /// Directions that the effect can slide from.
        /// </summary>
        public enum SlideDirection
        {
            FromBottom,
            FromTop,
            FromLeft,
            FromRight
        }

        /// <summary>
        /// The direction to slide from.
        /// </summary>
        public SlideDirection direction;

        /// <summary>
        /// The duration over which to animate the slide.
        /// </summary>
        public float duration = 0.3f;

        private GUIControl control = null;

        /// <summary>
        /// Play the slide effect.
        /// </summary>
        public override IEnumerator Play()
        {
            control = GetComponent<GUIControl>();
            if (control == null) yield break;
            control.visible = false;
            Rect rect = control.scaledRect.GetPixelRect();
            float startTime = DialogueTime.time;
            float endTime = startTime + duration;
            while (DialogueTime.time < endTime)
            {
                float elapsed = DialogueTime.time - startTime;
                float progress = Mathf.Clamp(elapsed / duration, 0, 1);
                switch (direction)
                {
                    case SlideDirection.FromBottom:
                        control.Offset = new Vector2(0, (1 - progress) * (Screen.height - rect.y));
                        break;
                    case SlideDirection.FromTop:
                        control.Offset = new Vector2(0, -(1 - progress) * (rect.y + rect.height));
                        break;
                    case SlideDirection.FromLeft:
                        control.Offset = new Vector2(-(1 - progress) * (rect.x + rect.width), 0);
                        break;
                    case SlideDirection.FromRight:
                        control.Offset = new Vector2((1 - progress) * (Screen.width - rect.x), 0);
                        break;
                }
                control.visible = true;
                control.Refresh();
                yield return null;
            }
            control.Offset = Vector2.zero;
            control.visible = true;
            control.Refresh();
        }

    }

}
