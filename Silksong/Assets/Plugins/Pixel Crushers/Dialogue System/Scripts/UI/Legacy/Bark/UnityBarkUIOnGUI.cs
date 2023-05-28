using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// This is a "child" component created by UnityBarkUI. For efficiency, this child component
    /// contains the OnGUI() method. This allows UnityBarkUI to enable and disable OnGUI() as 
    /// necessary, since Unity has substantial overhead for every OnGUI() call, even if it exits 
    /// immediately.
    /// 
    /// UnityBarkUI adds this component automatically if it doesn't already exist on the NPC.
    /// However, if you want to add an offset for the text, you can add this component
    /// manually and set the offset property.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class UnityBarkUIOnGUI : MonoBehaviour
    {

        public Vector3 offset = Vector3.zero;
        public float maxWidth = 0;

        protected GUISkin guiSkin = null;
        protected string guiStyleName = null;
        protected GUIStyle guiStyle = null;
        protected FormattedText formattingToApply = null;
        protected TextStyle textStyle = TextStyle.None;
        protected Color textStyleColor = Color.black;
        protected Vector2 size;
        protected string message = null;
        protected float alpha = 1f;
        protected Transform myTransform;
        protected Transform absolutePosition;

        /// <summary>
        /// The offset to the character's head, computed to be the top of the CharacterController
        /// or CapsuleCollider.
        /// </summary>
        protected Vector3 offsetToHead = Vector3.zero;

        /// <summary>
        /// Indicates whether this bark UI is currently showing a bark.
        /// </summary>
        /// <value><c>true</c> if playing; otherwise, <c>false</c>.</value>
        public bool IsPlaying
        {
            get { return enabled; }
        }

        /// <summary>
        /// Gets the position of the bark text in world space. This value is only valid
        /// what a bark is playing.
        /// </summary>
        /// <value>The bark position.</value>
        public Vector3 BarkPosition { get; private set; }

        /// <summary>
        /// The screen position of the bark text.
        /// </summary>
        protected Vector3 screenPos = Vector3.zero;

        public virtual void Awake()
        {
            myTransform = transform;
        }

        /// <summary>
        /// Starts this instance by computing the offset to the head of the character.
        /// </summary>
        public virtual void Start()
        {
            ComputeOffsetToHead();
            enabled = false;
        }

        protected void ComputeOffsetToHead()
        {
            CharacterController controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                offsetToHead = new Vector3(0, controller.height, 0);
            }
            else
            {
                CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
                if (capsuleCollider != null)
                {
                    offsetToHead = new Vector3(0, capsuleCollider.height, 0);
                }
                else
                {
                    BoxCollider boxCollider = GetComponent<BoxCollider>();
                    if (boxCollider != null)
                    {
                        offsetToHead = new Vector3(0, boxCollider.center.y + boxCollider.size.y, 0);
                    }
                    else
                    {
                        SphereCollider sphereCollider = GetComponent<SphereCollider>();
                        if (sphereCollider != null)
                        {
                            offsetToHead = new Vector3(0, sphereCollider.center.y + sphereCollider.radius, 0);
                        }
                        else
                        {
                            offsetToHead = Vector3.zero;
                        }
                    }
                }
            }
            offsetToHead += offset;
        }

        /// <summary>
        /// Barks a subtitle. Does not observe formatting codes in the subtitle's FormattedText, 
        /// instead using the formatting settings defined on this component.
        /// </summary>
        /// <param name='subtitle'>
        /// Subtitle to bark.
        /// </param>
        /// <param name='duration'>
        /// Duration.
        /// </param>
        /// <param name='guiSkin'>
        /// GUI skin.
        /// </param>
        /// <param name='guiStyleName'>
        /// GUI style name.
        /// </param>
        /// <param name='textStyle'>
        /// Text style.
        /// </param>
        /// <param name='includeName'> 
        /// Set <c>true</c> to include the barker's name in the text.
        /// </param>
        /// <param name='textPosition'> 
        /// The absolute text position to use; if not set, text is positioned above the collider.
        /// </param>
        public virtual void Show(Subtitle subtitle, float duration, GUISkin guiSkin, string guiStyleName, TextStyle textStyle, bool includeName, Transform textPosition)
        {
            Show(subtitle, duration, guiSkin, guiStyleName, textStyle, Color.black, includeName, textPosition);
        }

        /// <summary>
        /// Barks a subtitle. Does not observe formatting codes in the subtitle's FormattedText, 
        /// instead using the formatting settings defined on this component.
        /// </summary>
        /// <param name='subtitle'>
        /// Subtitle to bark.
        /// </param>
        /// <param name='duration'>
        /// Duration.
        /// </param>
        /// <param name='guiSkin'>
        /// GUI skin.
        /// </param>
        /// <param name='guiStyleName'>
        /// GUI style name.
        /// </param>
        /// <param name='textStyle'>
        /// Text style.
        /// </param>
        /// <param name='includeName'> 
        /// Set <c>true</c> to include the barker's name in the text.
        /// </param>
        /// <param name='textPosition'> 
        /// The absolute text position to use; if not set, text is positioned above the collider.
        /// </param>
        public virtual void Show(Subtitle subtitle, float duration, GUISkin guiSkin, string guiStyleName, TextStyle textStyle, Color textStyleColor, bool includeName, Transform textPosition)
        {
            this.message = includeName ? string.Format("{0}: {1}", new System.Object[] { subtitle.speakerInfo.Name, subtitle.formattedText.text }) : subtitle.formattedText.text;
            this.formattingToApply = subtitle.formattedText;
            this.guiSkin = guiSkin;
            this.guiStyleName = guiStyleName;
            this.guiStyle = null;
            this.textStyle = textStyle;
            this.textStyleColor = textStyleColor;
            this.alpha = 1f;
            absolutePosition = textPosition;
            UpdateBarkPosition();
            enabled = true;
        }

        public IEnumerator FadeOut(float fadeDuration)
        {
            float startTime = Time.time;
            float endTime = startTime + fadeDuration;
            while (Time.time < endTime)
            {
                float elapsed = Time.time - startTime;
                alpha = 1 - Mathf.Clamp(elapsed / fadeDuration, 0, 1);
                yield return null;
            }
            enabled = false;
        }

        /// <summary>
        /// Draws the bark text using Unity GUI.
        /// </summary>
        public virtual void OnGUI()
        {
            GUI.skin = UnityGUITools.GetValidGUISkin(guiSkin);
            if (guiStyle == null)
            {
                guiStyle = UnityGUITools.ApplyFormatting(formattingToApply, new GUIStyle(UnityGUITools.GetGUIStyle(guiStyleName, GUI.skin.label)));
                guiStyle.alignment = TextAnchor.UpperCenter;
                size = guiStyle.CalcSize(new GUIContent(message));
                if ((maxWidth >= 1) && (size.x > maxWidth))
                {
                    size = new Vector2(maxWidth, guiStyle.CalcHeight(new GUIContent(message), maxWidth));
                }
            }
            UpdateBarkPosition();
            guiStyle.normal.textColor = UnityGUITools.ColorWithAlpha(guiStyle.normal.textColor, alpha);
            if (screenPos.z < 0) return;
            Rect rect = new Rect(screenPos.x - (size.x / 2), (Screen.height - screenPos.y) - (size.y / 2), size.x, size.y);
            UnityGUITools.DrawText(rect, message, guiStyle, textStyle, textStyleColor);
        }

        protected void UpdateBarkPosition()
        {
            if (Camera.main == null) return;
            if (myTransform == null) myTransform = transform;
            BarkPosition = (absolutePosition != null)
                ? (absolutePosition.position + offset)
                    : (myTransform.position + offsetToHead);
            screenPos = Camera.main.WorldToScreenPoint(BarkPosition);
        }

    }

}
