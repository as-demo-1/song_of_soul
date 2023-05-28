using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// A GUI control that implements GUI.ScrollView. This control measures the size of the 
    /// children contained in the scroll view and resizes the content area accordingly.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class GUIScrollView : GUIControl
    {

        /// <summary>
        /// Show or hide the vertical scrollbar.
        /// </summary>
        public bool showVerticalScrollbar = true;

        /// <summary>
        /// Show or hide the horizontal scrollbar.
        /// </summary>
        public bool showHorizontalScrollbar = false;

        /// <summary>
        /// The pixel padding inside the scroll view.
        /// </summary>
        public int padding = 2;

        /// <summary>
        /// The handler(s) to call when the scroll view needs to measure its children.
        /// </summary>
        public event Action MeasureContentHandler = null;

        /// <summary>
        /// The handler(s) to call when the scroll view needs to draw its children.
        /// </summary>
        public event Action DrawContentHandler = null;

        /// <summary>
        /// The width of the content as reported by the MeasureContentHandler.
        /// </summary>
        /// <value>
        /// The width of the content.
        /// </value>
        public float contentWidth { get; set; }

        /// <summary>
        /// The height of the content as reported by the MeasureContentHandler.
        /// </summary>
        /// <value>
        /// The height of the content.
        /// </value>
        public float contentHeight { get; set; }

        /// <summary>
        /// Resets the scroll position.
        /// </summary>
        public void ResetScrollPosition()
        {
            scrollViewVector = Vector2.zero;
        }

        /// <summary>
        /// The current scroll position in the scroll area.
        /// </summary>
        private Vector2 scrollViewVector = Vector2.zero;

        /// <summary>
        /// Draws the children, taking into account key/controller navigation if enabled.
        /// </summary>
        /// <param name="relativeMousePosition">Relative mouse position.</param>
        public override void DrawChildren(Vector2 relativeMousePosition)
        {
            clipChildren = false;
            Rect scrollContentRect = GetScrollContentRect();
            GUIStyle horizontalScrollbar = showHorizontalScrollbar ? GUI.skin.horizontalScrollbar : GUIStyle.none;
            GUIStyle verticalScrollbar = showVerticalScrollbar ? GUI.skin.verticalScrollbar : GUIStyle.none;
            scrollViewVector = GUI.BeginScrollView(rect, scrollViewVector, scrollContentRect, horizontalScrollbar, verticalScrollbar);
            try
            {
                if (DrawContentHandler != null) DrawContentHandler();
                base.DrawChildren(relativeMousePosition);
            }
            finally
            {
                GUI.EndScrollView();
            }
        }

        private Rect GetScrollContentRect()
        {
            float sliderWidth = (GUI.skin.verticalSlider.normal.background != null) ? GUI.skin.verticalSlider.normal.background.width : 16f;
            contentWidth = rect.width - sliderWidth;
            MeasureChildrenAsContent();
            if (MeasureContentHandler != null) MeasureContentHandler();
            return new Rect(0, 0, contentWidth, contentHeight);
        }

        private void MeasureChildrenAsContent()
        {
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    contentWidth = Mathf.Max(contentWidth, GetChildXMax(child));
                    contentHeight = Mathf.Max(contentHeight, GetChildYMax(child));
                }
            }
        }

        private float GetChildXMax(GUIControl child)
        {
            return child.rect.xMax;
        }

        private float GetChildYMax(GUIControl child)
        {
            if (child is GUILabel)
            {
                GUILabel guiLabel = child as GUILabel;
                if ((guiLabel.autoSize != null) && (guiLabel.autoSize.autoSizeHeight))
                {
                    guiLabel.Refresh(new Vector2(rect.width, rect.height));
                    guiLabel.UpdateLayout();
                }
            }
            return child.rect.yMax;
        }

    }

}
