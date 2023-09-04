using UnityEngine;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Implements IBarkUI using Unity GUI to show bark text above a character's head. For 
    /// efficiency, this component adds a "child" component called UnityBarkUIOnGUI that contains
    /// the OnGUI() method. This allows UnityBarkUI to enable and disable OnGUI() as necessary,
    /// since Unity has substantial overhead for every OnGUI() call, even if it exits immediately.
    /// If you want to set a text offset, you can add the UnityBarkUIOnGUI manually instead.
    /// </summary>
    [AddComponentMenu("")] // Deprecated
    public class UnityBarkUI : AbstractBarkUI
    {

        /// <summary>
        /// The absolute text position. If not set, the text is positioned above the collider.
        /// </summary>
        public Transform textPosition;

        /// <summary>
        /// The GUI skin to use for the bark text. Uses the default skin if this is unassigned.
        /// </summary>
        public GUISkin guiSkin;

        /// <summary>
        /// The name of the GUI style to use. If unassigned, defaults to label.
        /// </summary>
        public string guiStyleName;

        /// <summary>
        /// Set <c>true</c> to include the barker's name in the text.
        /// </summary>
        public bool includeName;

        /// <summary>
        /// The duration in seconds to show the bark text before fading it out.
        /// </summary>
        public float duration = 4f;

        /// <summary>
        /// The duration in seconds to fade out.
        /// </summary>
        public float fadeDuration = 0.5f;

        /// <summary>
        /// The text effect to use for the bark text.
        /// </summary>
        public TextStyle textStyle = TextStyle.Shadow;

        /// <summary>
        /// The color of the text style's outline or shadow.
        /// </summary>
        public Color textStyleColor = Color.black;

        /// <summary>
        /// The text display setting. Defaults to use the same subtitle setting as the Dialogue
        /// Manager, but you can also set it to always show or always hide the text.
        /// </summary>
        public BarkSubtitleSetting textDisplaySetting = BarkSubtitleSetting.SameAsDialogueManager;

        /// <summary>
        /// Set <c>true</c> to keep the bark text onscreen until the sequence ends.
        /// </summary>
        public bool waitUntilSequenceEnds = false;

        /// <summary>
        /// Set <c>true</c> to run a raycast to the player. If the ray is blocked (e.g., a wall
        /// blocks visibility to the player), don't show the bark.
        /// </summary>
        public bool checkIfPlayerVisible = true;

        /// <summary>
        /// The layer mask to use when checking for player visibility.
        /// </summary>
        public LayerMask visibilityLayerMask = 1;

        /// <summary>
        /// Indicates whether the bark UI should show the text, based on the textDisplaySetting.
        /// </summary>
        /// <value>
        /// <c>true</c> to show text; otherwise, <c>false</c>.
        /// </value>
        public bool showText
        {
            get
            {
                return (textDisplaySetting == BarkSubtitleSetting.Show) ||
                    ((textDisplaySetting == BarkSubtitleSetting.SameAsDialogueManager) && DialogueManager.displaySettings.subtitleSettings.showNPCSubtitlesDuringLine);
            }
        }

        protected UnityBarkUIOnGUI unityBarkUIOnGUI = null;

        protected Transform playerCameraTransform = null;

        protected Collider playerCameraCollider = null;

        protected float secondsLeft = 0;

        public virtual void Awake()
        {
            CheckUnityBarkUIOnGUI();
        }

        public virtual void OnDestroy()
        {
            Destroy(unityBarkUIOnGUI);
            unityBarkUIOnGUI = null;
        }

        protected void CheckUnityBarkUIOnGUI()
        {
            if (unityBarkUIOnGUI == null)
            {
                unityBarkUIOnGUI = GetComponent<UnityBarkUIOnGUI>();
                if (unityBarkUIOnGUI == null)
                {
                    unityBarkUIOnGUI = gameObject.AddComponent<UnityBarkUIOnGUI>();
                }
            }
        }

        /// <summary>
        /// Indicates whether a bark is currently playing.
        /// </summary>
        /// <value>
        /// <c>true</c> if playing; otherwise, <c>false</c>.
        /// </value>
        public override bool isPlaying
        {
            get { return secondsLeft > 0; }
        }

        /// <summary>
        /// Barks a subtitle. Does not observe formatting codes in the subtitle's FormattedText, 
        /// instead using the formatting settings defined on this component.
        /// </summary>
        /// <param name='subtitle'>
        /// Subtitle to bark.
        /// </param>
        public override void Bark(Subtitle subtitle)
        {
            if (showText && (subtitle != null) && !string.IsNullOrEmpty(subtitle.formattedText.text))
            {
                if ((Camera.main == null) && DialogueDebug.logWarnings) Debug.LogWarning(string.Format("{0}: There is no camera in the scene marked MainCamera, but UnityBarkUI requires one.", DialogueDebug.Prefix));
                CheckUnityBarkUIOnGUI();
                unityBarkUIOnGUI.Show(subtitle, duration, guiSkin, guiStyleName, textStyle, textStyleColor, includeName, textPosition);
                CheckPlayerCameraTransform();
                StopAllCoroutines();
                secondsLeft = Mathf.Approximately(0, duration) ? DialogueManager.GetBarkDuration(subtitle.formattedText.text) : duration;
            }
        }

        /// <summary>
        /// Updates the seconds left and hides the label if time is up.
        /// Also updates the UnityBarkUIOnGUI component's bark position.
        /// </summary>
        public virtual void Update()
        {
            if (secondsLeft > 0)
            {
                secondsLeft -= Time.deltaTime;
                if (checkIfPlayerVisible) CheckPlayerVisibility();
                if ((secondsLeft <= 0) && !waitUntilSequenceEnds) Hide();
            }
        }

        public void OnBarkEnd(Transform actor)
        {
            if (waitUntilSequenceEnds) Hide();
        }

        public override void Hide()
        {
            if (unityBarkUIOnGUI.enabled)
            {
                StartCoroutine(unityBarkUIOnGUI.FadeOut(fadeDuration));
            }
            secondsLeft = 0;
        }

        protected void CheckPlayerVisibility()
        {
            CheckPlayerCameraTransform();
            bool canSeePlayer = true;
            if (playerCameraTransform != null)
            {
                RaycastHit hit;
                if (Physics.Linecast(unityBarkUIOnGUI.BarkPosition, playerCameraTransform.position, out hit, visibilityLayerMask))
                {
                    canSeePlayer = (hit.collider == playerCameraCollider);
                }
            }
            if (unityBarkUIOnGUI != null)
            {
                if (unityBarkUIOnGUI.enabled && !canSeePlayer)
                {
                    unityBarkUIOnGUI.enabled = false;
                }
                else if (!unityBarkUIOnGUI.enabled && canSeePlayer)
                {
                    unityBarkUIOnGUI.enabled = true;
                }
            }
        }

        protected void CheckPlayerCameraTransform()
        {
            if (playerCameraTransform == null)
            {
                if (Camera.main != null)
                {
                    playerCameraTransform = Camera.main.transform;
                    playerCameraCollider = (playerCameraTransform != null) ? playerCameraTransform.GetComponent<Collider>() : null;
                }
            }
        }

        //--- Utility gizmo for checking player visibility code:
        //void OnDrawGizmos() {
        //	if (secondsLeft > 0) {
        //		Gizmos.DrawLine(unityBarkUIOnGUI.BarkPosition, playerCameraTransform.position);
        //	}
        //}

    }

}
