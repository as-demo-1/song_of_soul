using UnityEngine;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// A GUI control that contains text. This is the parent class of GUILabel, GUIButton, and GUIWindow.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class GUIVisibleControl : GUIControl
    {

        /// <summary>
        /// The (optional) localized text table to use.
        /// </summary>
        public LocalizedTextTable localizedText;

        /// <summary>
        /// The text content, or the name of the field in the localized text table.
        /// </summary>
        public string text;

        /// <summary>
        /// The name of the GUI style to use to draw the text.
        /// </summary>
        public string guiStyleName;

        /// <summary>
        /// Gets or sets the alpha (transparency) value.
        /// </summary>
        /// <value>The alpha.</value>
        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has an alpha that isn't fully opaque.
        /// </summary>
        /// <value><c>true</c> if this instance has alpha; otherwise, <c>false</c>.</value>
        public bool HasAlpha
        {
            get { return (Alpha < 0.999f) && Application.isPlaying; }
        }

        /// <summary>
        /// Gets the default GUI style to use for this type of control. It can be overridden on a per-control
        /// basis using guiStyleName.
        /// </summary>
        /// <value>The default GUI style.</value>
        protected virtual GUIStyle DefaultGUIStyle
        {
            get { return GUI.skin.label; }
        }

        /// <summary>
        /// The current GUI style.
        /// </summary>
        /// <value>The GUI style.</value>
        protected GUIStyle GuiStyle
        {
            get { return guiStyle; }
            set { guiStyle = value; }
        }

        private FormattedText formattingToApply = null;
        private bool isFormattingApplied = false;
        private GUIStyle guiStyle = null;
        private Color originalGUIColor = Color.white;
        private float alpha = 1;
        private string originalTextValue = string.Empty;

        public override void Awake()
        {
            base.Awake();
            originalTextValue = text;
        }

        public virtual void Start()
        {
            if (localizedText != null) UseLocalizedText(localizedText);
        }

        public void UseLocalizedText(LocalizedTextTable localizedText)
        {
            this.localizedText = localizedText;
            if (localizedText != null)
            {
                if (localizedText.ContainsField(originalTextValue))
                {
                    text = localizedText[originalTextValue];
                }
            }
        }

        public void ApplyAlphaToGUIColor()
        {
            if (HasAlpha)
            {
                originalGUIColor = GUI.color;
                GUI.color = UnityGUITools.ColorWithAlpha(GUI.color, Alpha);
            }
        }

        public void RestoreGUIColor()
        {
            if (HasAlpha)
            {
                GUI.color = originalGUIColor;
            }
        }

        /// <summary>
        /// Sets the control's text and formatting.
        /// </summary>
        /// <param name='formattedText'>
        /// Formatted text.
        /// </param>
        public virtual void SetFormattedText(FormattedText formattedText)
        {
            text = formattedText.text;
            formattingToApply = formattedText;
            isFormattingApplied = false;
            GuiStyle = null;
            NeedToUpdateLayout = true;
        }

        /// <summary>
        /// Sets the control's text and formatting using just raw text.
        /// </summary>
        /// <param name='text'>
        /// Text.
        /// </param>
        public void SetUnformattedText(string text)
        {
            this.text = text;
            formattingToApply = null;
            guiStyle = null;
            NeedToUpdateLayout = true;
        }

        /// <summary>
        /// Updates the control's layout but not its children.
        /// </summary>
        public override void UpdateLayoutSelf()
        {
            guiStyle = null;
            isFormattingApplied = false;
            ApplyFormatting();
            base.UpdateLayoutSelf();
        }

        /// <summary>
        /// Makes sure the guiStyle property is up-to-date.
        /// </summary>
        protected void SetGUIStyle()
        {
            if (guiStyle == null) guiStyle = UnityGUITools.GetGUIStyle(guiStyleName, DefaultGUIStyle);
        }

        /// <summary>
        /// Applies the formatting recorded in formattingToApply by SetFormattedText().
        /// SetFormattedText() can't apply the formatting directly because it needs to 
        /// run in OnGUI.
        /// </summary>
        protected void ApplyFormatting()
        {
            SetGUIStyle();
            if (!(isFormattingApplied || (formattingToApply == null)))
            {
                text = formattingToApply.text;
                guiStyle = UnityGUITools.ApplyFormatting(formattingToApply, guiStyle);
                isFormattingApplied = true;
            }
        }

        /// <summary>
        /// Auto-sizes the control according to the autoSize settings.
        /// </summary>
        public override void AutoSizeSelf()
        {
            ApplyFormatting();

            // Compute width first. Doesn't handle word-wrapping, which is okay since it doesn't really
            // make sense with auto-width. Also have to temporarily zero out padding since padding values
            // cause CalcSize() to return strange values:
            if (autoSize.autoSizeWidth)
            {
                GUIStyle newGuiStyle = new GUIStyle(guiStyle);
                newGuiStyle.padding = new RectOffset(0, 0, 0, 0);
                float width = newGuiStyle.CalcSize(new GUIContent(text)).x + guiStyle.padding.left + guiStyle.padding.right;
                width = Mathf.Clamp(width, scaledRect.minPixelWidth, autoSize.maxWidth.GetPixelValue(WindowSize.x));
                width += autoSize.padding.left + autoSize.padding.right;
                rect = new Rect(GetAutoSizeX(width), rect.y, width, rect.height);
            }
            if (autoSize.autoSizeHeight)
            {
                float height = guiStyle.CalcHeight(new GUIContent(text), rect.width);
                height = Mathf.Clamp(height, scaledRect.minPixelHeight, autoSize.maxHeight.GetPixelValue(WindowSize.y));
                height += autoSize.padding.top + autoSize.padding.bottom;
                rect = new Rect(rect.x, GetAutoSizeY(height), rect.width, height);
            }
        }

        private float GetAutoSizeX(float width)
        {
            switch (scaledRect.alignment)
            {
                case ScaledRectAlignment.TopLeft:
                case ScaledRectAlignment.MiddleLeft:
                case ScaledRectAlignment.BottomLeft:
                    return rect.x;
                case ScaledRectAlignment.TopCenter:
                case ScaledRectAlignment.MiddleCenter:
                case ScaledRectAlignment.BottomCenter:
                    return rect.x + (0.5f * (rect.width - width));
                case ScaledRectAlignment.TopRight:
                case ScaledRectAlignment.MiddleRight:
                case ScaledRectAlignment.BottomRight:
                    return rect.x + (rect.width - width);
                default:
                    return rect.x;
            }
        }

        private float GetAutoSizeY(float height)
        {
            switch (scaledRect.alignment)
            {
                case ScaledRectAlignment.TopLeft:
                case ScaledRectAlignment.TopCenter:
                case ScaledRectAlignment.TopRight:
                    return rect.y;
                case ScaledRectAlignment.MiddleLeft:
                case ScaledRectAlignment.MiddleCenter:
                case ScaledRectAlignment.MiddleRight:
                    return rect.y + (0.5f * (rect.height - height));
                case ScaledRectAlignment.BottomLeft:
                case ScaledRectAlignment.BottomCenter:
                case ScaledRectAlignment.BottomRight:
                    return rect.y + (rect.height - height);
                default:
                    return rect.y;
            }
        }

        /// <summary>
        /// Plays an audio clip. If the control itself has an AudioSource, it uses it.
        /// Otherwise it uses the AudioSource on the main camera, adding one if the 
        /// main camera doesn't already have one.
        /// </summary>
        /// <param name="audioClip">Audio clip.</param>
        public void PlaySound(AudioClip audioClip)
        {
            if (audioClip != null && Camera.main != null)
            {
                AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = Camera.main.gameObject.AddComponent<AudioSource>();
                }
                audioSource.PlayOneShot(audioClip);
            }
        }

    }

}
