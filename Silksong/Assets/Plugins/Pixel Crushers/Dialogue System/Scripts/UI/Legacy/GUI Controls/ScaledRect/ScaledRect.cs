using UnityEngine;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Scaled rects allow you to specify resolution-independent rects with a variety of 
    /// positioning options. The dialogue system uses scaled rects extensively with the Unity GUI
    /// to specify the positions of dialogue display controls without having to know the screen
    /// resolution ahead of time.
    /// </summary>
    [System.Serializable]
    public class ScaledRect
    {

        /// <summary>
        /// Represents an empty rect, functionally equivalent to <c>new Rect(0,0,0,0)</c>.
        /// </summary>
        public static readonly ScaledRect empty = new ScaledRect(ScaledRectAlignment.TopLeft, ScaledRectAlignment.TopLeft, ScaledValue.zero, ScaledValue.zero, ScaledValue.zero, ScaledValue.zero);

        /// <summary>
        /// Represents a rect that occupies the whole screen or window, functionally equivalent to
        /// <c>new Rect(0,0,Screen.width,Screen.height)</c> if the window is the whole screen.
        /// </summary>
        public static readonly ScaledRect wholeScreen = new ScaledRect(ScaledRectAlignment.TopLeft, ScaledRectAlignment.TopLeft, ScaledValue.zero, ScaledValue.zero, ScaledValue.max, ScaledValue.max);

        /// <summary>
        /// The origin point of the rect. This is the location on the screen/window where the 
        /// alignment point will align to.
        /// </summary>
        public ScaledRectAlignment origin;

        /// <summary>
        /// The alignment of the rect -- that is, which corner of the rect will be placed at the
        /// origin point.
        /// </summary>
        public ScaledRectAlignment alignment;

        /// <summary>
        /// The x offset of the alignment corner. A value of <c>0</c> means no offset.
        /// </summary>
        public ScaledValue x;

        /// <summary>
        /// The y offset of the alignment corner. A value of <c>0</c> means no offset.
        /// </summary>
        public ScaledValue y;

        /// <summary>
        /// The width of the rect.
        /// </summary>
        public ScaledValue width;

        /// <summary>
        /// The height of the rect.
        /// </summary>
        public ScaledValue height;

        /// <summary>
        /// The minimum pixel width of the rect; <c>0</c> indicates no minimum.
        /// </summary>
        public float minPixelWidth;

        /// <summary>
        /// The minimum pixel height of the rect; <c>0</c> indicates no minimum.
        /// </summary>
        public float minPixelHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelCrushers.DialogueSystem.UnityGUI.ScaledRect"/> class.
        /// </summary>
        /// <param name='origin'>
        /// Origin point.
        /// </param>
        /// <param name='alignment'>
        /// Alignment corner that's placed on the origin.
        /// </param>
        /// <param name='x'>
        /// The x offset of the alignment corner.
        /// </param>
        /// <param name='y'>
        /// The y offset of the alignment corner.
        /// </param>
        /// <param name='width'>
        /// Width of the rect.
        /// </param>
        /// <param name='height'>
        /// Height of the rect.
        /// </param>
        /// <param name='minPixelWidth'>
        /// Minimum pixel width.
        /// </param>
        /// <param name='minPixelHeight'>
        /// Minimum pixel height.
        /// </param>
        public ScaledRect(ScaledRectAlignment origin, ScaledRectAlignment alignment, ScaledValue x, ScaledValue y,
            ScaledValue width, ScaledValue height, float minPixelWidth = 0, float minPixelHeight = 0)
        {
            this.origin = origin;
            this.alignment = alignment;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.minPixelWidth = minPixelWidth;
            this.minPixelHeight = minPixelHeight;
        }

        /// <summary>
        /// Copy constructor. Initializes a new ScaledRect.
        /// </summary>
        /// <param name='source'>
        /// Source to copy.
        /// </param>
        public ScaledRect(ScaledRect source)
        {
            if (source != null)
            {
                this.origin = source.origin;
                this.alignment = source.alignment;
                this.x = new ScaledValue(source.x);
                this.y = new ScaledValue(source.y);
                this.width = new ScaledValue(source.width);
                this.height = new ScaledValue(source.height);
                this.minPixelWidth = source.minPixelWidth;
                this.minPixelHeight = source.minPixelHeight;
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ScaledRect()
        {
        }

        /// <summary>
        /// Static utility function to build a new ScaledRect at an origin point and with the 
        /// specified size.
        /// </summary>
        /// <returns>
        /// The ScaledRect.
        /// </returns>
        /// <param name='origin'>
        /// The origin point; used for origin and alignment.
        /// </param>
        /// <param name='width'>
        /// Width.
        /// </param>
        /// <param name='height'>
        /// Height.
        /// </param>
        /// <param name='minPixelWidth'>
        /// Minimum pixel width. If <c>0</c>, no minimum.
        /// </param>
        /// <param name='minPixelHeight'>
        /// Minimum pixel height. If <c>0</c>, no minimum.
        /// </param>
        public static ScaledRect FromOrigin(ScaledRectAlignment origin, ScaledValue width, ScaledValue height,
            float minPixelWidth = 0, float minPixelHeight = 0)
        {
            return new ScaledRect(origin, origin, ScaledValue.zero, ScaledValue.zero, width, height, minPixelWidth, minPixelHeight);
        }

        /// <summary>
        /// Calculates the pixel rect represented by the values of this ScaledRect.
        /// </summary>
        /// <returns>
        /// The pixel rect.
        /// </returns>
        public Rect GetPixelRect()
        {
            return GetPixelRect(new Vector2(Screen.width, Screen.height), Vector2.zero);
        }

        /// <summary>
        /// Gets the pixel rect represented by the values of this ScaledRect, scaled into
        /// a specified window size.
        /// </summary>
        /// <returns>
        /// The pixel rect.
        /// </returns>
        /// <param name='windowSize'>
        /// Window size.
        /// </param>
        public Rect GetPixelRect(Vector2 windowSize)
        {
            return GetPixelRect(windowSize, Vector2.zero);
        }

        /// <summary>
        /// Calculates the pixel rect specified by this ScaledRect, scaled into a specified window
        /// size. If the ScaledRect's width or height are <c>0</c>, uses the corresponding x or y 
        /// value in defaultSize.
        /// </summary>
        /// <returns>
        /// The pixel rect.
        /// </returns>
        /// <param name='windowSize'>
        /// The window size in pixels.
        /// </param>
        /// <param name='defaultSize'>
        /// The default x and y pixel values to use if width or height are <c>0</c>.
        /// </param>
        public Rect GetPixelRect(Vector2 windowSize, Vector2 defaultSize)
        {
            float pixelWidth = Mathf.Max(width.GetPixelValue(windowSize.x), minPixelWidth);
            float pixelHeight = Mathf.Max(height.GetPixelValue(windowSize.y), minPixelHeight);
            Vector2 pixelOrigin = GetPixelOrigin(windowSize);
            Vector2 alignmentFactor = GetAlignmentFactor();
            float pixelX = pixelOrigin.x + (pixelWidth * alignmentFactor.x) + x.GetPixelValue(windowSize.x);
            float pixelY = pixelOrigin.y + (pixelHeight * alignmentFactor.y) + y.GetPixelValue(windowSize.y);
            return new Rect(pixelX, pixelY, pixelWidth, pixelHeight);
        }

        private Vector2 GetPixelOrigin(Vector2 windowSize)
        {
            switch (origin)
            {
                case ScaledRectAlignment.TopLeft:
                    return Vector2.zero;
                case ScaledRectAlignment.TopCenter:
                    return new Vector2(0.5f * windowSize.x, 0);
                case ScaledRectAlignment.TopRight:
                    return new Vector2(windowSize.x, 0);
                case ScaledRectAlignment.MiddleLeft:
                    return new Vector2(0, 0.5f * windowSize.y);
                case ScaledRectAlignment.MiddleCenter:
                    return new Vector2(0.5f * windowSize.x, 0.5f * windowSize.y);
                case ScaledRectAlignment.MiddleRight:
                    return new Vector2(windowSize.x, 0.5f * windowSize.y);
                case ScaledRectAlignment.BottomLeft:
                    return new Vector2(0, windowSize.y);
                case ScaledRectAlignment.BottomCenter:
                    return new Vector2(0.5f * windowSize.x, windowSize.y);
                case ScaledRectAlignment.BottomRight:
                    return windowSize;
                default:
                    return Vector2.zero;
            }
        }

        private Vector2 GetAlignmentFactor()
        {
            switch (alignment)
            {
                case ScaledRectAlignment.TopLeft:
                    return Vector2.zero;
                case ScaledRectAlignment.TopCenter:
                    return new Vector2(-0.5f, 0);
                case ScaledRectAlignment.TopRight:
                    return new Vector2(-1, 0);
                case ScaledRectAlignment.MiddleLeft:
                    return new Vector2(0, -0.5f);
                case ScaledRectAlignment.MiddleCenter:
                    return new Vector2(-0.5f, -0.5f);
                case ScaledRectAlignment.MiddleRight:
                    return new Vector2(-1, -0.5f);
                case ScaledRectAlignment.BottomLeft:
                    return new Vector2(0, -1);
                case ScaledRectAlignment.BottomCenter:
                    return new Vector2(-0.5f, -1);
                case ScaledRectAlignment.BottomRight:
                    return new Vector2(-1, -1);
                default:
                    return Vector2.zero;
            }
        }

    }

}
