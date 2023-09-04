// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEngine.Events;
using PixelCrushers.DialogueSystem.UnityGUI;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This component implements a selector that allows the player to target and use a usable 
    /// object. 
    /// 
    /// To mark an object usable, add the Usable component and a collider to it. The object's
    /// layer should be in the layer mask specified on the Selector component.
    /// 
    /// The selector can be configured to target items under the mouse cursor or the middle of
    /// the screen. When a usable object is targeted, the selector displays a targeting reticle
    /// and information about the object. If the target is in range, the inRange reticle 
    /// texture is displayed; otherwise the outOfRange texture is displayed.
    /// 
    /// If the player presses the use button (which defaults to spacebar and Fire2), the targeted
    /// object will receive an "OnUse" message.
    /// 
    /// You can hook into SelectedUsableObject and DeselectedUsableObject to get notifications
    /// when the current target has changed and Enabled and Disabled when the component is 
    /// enabled or disabled.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class Selector : MonoBehaviour
    {

        /// <summary>
        /// This class defines the textures and size of the targeting reticle.
        /// </summary>
        [System.Serializable]
        public class Reticle
        {
            public Texture2D inRange;
            public Texture2D outOfRange;
            public float width = 64f;
            public float height = 64f;
        }

        /// <summary>
        /// Specifies how to target: center of screen or under the mouse cursor. This is where it raycasts to.
        /// </summary>
        public enum SelectAt { CenterOfScreen, MousePosition, CustomPosition }

        /// <summary>
        /// Specifies whether to compute range from the targeted object (distance to the camera
        /// or distance to the selector's game object).
        /// </summary>
        public enum DistanceFrom { Camera, GameObject, ActorTransform }

        /// <summary>
        /// Specifies whether to do 2D or 3D raycasts.
        /// </summary>
        public enum Dimension { In2D, In3D }

        /// <summary>
        /// The default layermask is just the Default layer.
        /// </summary>
        private static LayerMask DefaultLayer = 1;

        /// <summary>
        /// How to target (center of screen or under mouse cursor). Default is center of screen.
        /// </summary>
        [Tooltip("How to target. This is where the raycast points to.")]
        public SelectAt selectAt = SelectAt.CenterOfScreen;

        /// <summary>
        /// The layer mask to use when targeting objects. Objects on others layers are ignored.
        /// </summary>
        [Tooltip("Layer mask to use when targeting objects; objects on others layers are ignored.")]
        public LayerMask layerMask = DefaultLayer;

        /// <summary>
        /// How to compute range to targeted object. Default is from the camera.
        /// </summary>
        [Tooltip("How to compute range to targeted object.")]
        public DistanceFrom distanceFrom = DistanceFrom.Camera;

        /// <summary>
        /// The max selection distance. The selector won't target objects farther than this.
        /// </summary>
        [Tooltip("Don't target objects farther than this; targets may still be unusable if beyond their usable range.")]
        public float maxSelectionDistance = 30f;

        /// <summary>
        /// Specifies whether to run 2D or 3D raycasts.
        /// </summary>
        public Dimension runRaycasts = Dimension.In3D;

        /// <summary>
        /// Set <c>true</c> to check all objects within the raycast range for usables.
        /// If <c>false</c>, the check stops on the first hit, even if it's not a usable.
        /// This prevents selection through walls.
        /// </summary>
        [Tooltip("Check all objects within raycast range for usables, even passing through obstacles.")]
        public bool raycastAll = false;

        /// <summary>
        /// If <c>true</c>, uses a default OnGUI to display a selection message and
        /// targeting reticle.
        /// </summary>
        [Tooltip("")]
        public bool useDefaultGUI = true;

        /// <summary>
        /// The GUI skin to use for the target's information (name and use message).
        /// </summary>
        [Tooltip("GUI skin to use for the target's information (name and use message).")]
        public GUISkin guiSkin;

        /// <summary>
        /// The name of the GUI style in the skin.
        /// </summary>
        [Tooltip("Name of the GUI style in the skin.")]
        public string guiStyleName = "label";

        /// <summary>
        /// The text alignment.
        /// </summary>
        public TextAnchor alignment = TextAnchor.UpperCenter;

        /// <summary>
        /// The text style for the text.
        /// </summary>
        public TextStyle textStyle = TextStyle.Shadow;

        /// <summary>
        /// The color of the text style's outline or shadow.
        /// </summary>
        public Color textStyleColor = Color.black;

        /// <summary>
        /// The color of the information labels when the target is in range.
        /// </summary>
        [Tooltip("Color of the information labels when target is in range.")]
        public Color inRangeColor = Color.yellow;

        /// <summary>
        /// The color of the information labels when the target is out of range.
        /// </summary>
        [Tooltip("Color of the information labels when target is out of range.")]
        public Color outOfRangeColor = Color.gray;

        /// <summary>
        /// The reticle images.
        /// </summary>
        public Reticle reticle;

        /// <summary>
        /// The key that sends an OnUse message.
        /// </summary>
        public KeyCode useKey = KeyCode.Space;

        /// <summary>
        /// The button that sends an OnUse message.
        /// </summary>
        public string useButton = "Fire2";

        /// <summary>
        /// The default use message. This can be overridden in the target's Usable component.
        /// </summary>
        [Tooltip("Default use message; can be overridden in the target's Usable component")]
        public string defaultUseMessage = "(spacebar to interact)";

        /// <summary>
        /// If ticked, the OnUse message is broadcast to the usable object's children.
        /// </summary>
        [Tooltip("Tick to also broadcast to the usable object's children")]
        public bool broadcastToChildren = true;

        /// <summary>
        /// The actor transform to send with OnUse. Defaults to this transform.
        /// </summary>
        [Tooltip("Actor transform to send with OnUse; defaults to this transform")]
        public Transform actorTransform = null;

        /// <summary>
        /// If set, show this alert message if attempt to use something beyond its usable range.
        /// </summary>
        [Tooltip("If set, show this alert message if attempt to use something beyond its usable range")]
        public string tooFarMessage = string.Empty;

        public UsableUnityEvent onSelectedUsable = new UsableUnityEvent();

        public UsableUnityEvent onDeselectedUsable = new UsableUnityEvent();

        /// <summary>
        /// The too far event handler.
        /// </summary>
        public UnityEvent tooFarEvent = new UnityEvent();

        /// <summary>
        /// If <c>true</c>, draws gizmos.
        /// </summary>
        [Tooltip("Tick to draw gizmos in Scene view")]
        public bool debug = false;

        /// <summary>
        /// Gets or sets the custom position used when the selectAt is set to SelectAt.CustomPosition.
        /// You can use, for example, to slide around a targeting icon onscreen using a gamepad.
        /// </summary>
        /// <value>
        /// The custom position.
        /// </value>
        public Vector3 CustomPosition { get; set; }

        /// <summary>
        /// Gets the current selection.
        /// </summary>
        /// <value>The selection.</value>
        public Usable CurrentUsable
        {
            get { return usable; }
            set { SetCurrentUsable(value); }
        }

        /// <summary>
        /// Gets the distance from the current usable.
        /// </summary>
        /// <value>The current distance.</value>
        public float CurrentDistance { get { return distance; } }

        /// <summary>
        /// Gets the GUI style.
        /// </summary>
        /// <value>The GUI style.</value>
        public GUIStyle GuiStyle { get { SetGuiStyle(); return guiStyle; } }

        /// <summary>
        /// Occurs when the selector has targeted a usable object.
        /// </summary>
        public event SelectedUsableObjectDelegate SelectedUsableObject = null;

        /// <summary>
        /// Occurs when the selector has untargeted a usable object.
        /// </summary>
        public event DeselectedUsableObjectDelegate DeselectedUsableObject = null;

        public event System.Action Enabled = null;

        public event System.Action Disabled = null;

        protected GameObject selection = null; // Currently under the selection point.
        protected Usable usable = null; // Usable component of the current selection.
        protected GameObject clickedDownOn = null; // Selection when the mouse button was pressed down.
        protected string heading = string.Empty;
        protected string useMessage = string.Empty;
        protected float distance = 0;
        protected GUIStyle guiStyle = null;
        protected float guiStyleLineHeight = 16f;

        protected Ray lastRay = new Ray();
        protected RaycastHit lastHit = new RaycastHit();
        protected RaycastHit[] lastHits = null;
        protected int numLastHits = 0;
        protected const int MaxHits = 100;
#if USE_PHYSICS2D || !UNITY_2018_1_OR_NEWER
        protected RaycastHit2D[] lastHits2D = null;
#endif
        protected bool hasReportedInvalidCamera = false;
        protected bool hasCheckedDefaultInputManager = false;
        protected bool isUsingDefaultInputManager = true;

        protected virtual void Reset()
        {
#if UNITY_EDITOR
            var selectorUseStandardUIElements = gameObject.GetComponent<SelectorUseStandardUIElements>();
            if (selectorUseStandardUIElements == null)
            {
                if (UnityEditor.EditorUtility.DisplayDialog("Use Unity UI for Selector?", "Add a 'Selector Use Standard UI Elements' component to allow the Selector to use Unity UI? Otherwise it will use legacy Unity GUI. You can customize the Unity UI prefab assigned to the Dialogue Manager's Instantiate Prefabs component.", "Add", "Don't Add"))
                {
                    gameObject.AddComponent<SelectorUseStandardUIElements>();
                }
            }
#endif
        }

        protected virtual void OnEnable()
        {
            Enabled?.Invoke();
        }

        protected virtual void OnDisable()
        {
            Disabled?.Invoke();
        }

        public virtual void Start()
        {
            if (Camera.main == null) Debug.LogError("Dialogue System: The scene is missing a camera tagged 'MainCamera'. The Selector may not behave the way you expect.", this);
        }

        /// <summary>
        /// Runs a raycast to see what's under the selection point. Updates the selection and
        /// calls the selection delegates if the selection has changed. If the player hits the
        /// use button, sends an OnUse message to the selection.
        /// </summary>
        protected virtual void Update()
        {
            // Exit if disabled or paused:
            if (!enabled || (Time.timeScale <= 0)) return;

            // Exit if there's no camera:
            if (UnityEngine.Camera.main == null) return;

#if !USE_NEW_INPUT // (In new Input System, IsPointerOverGameObject returns true for all GameObjects, not just UI objects, so skip this check until Input System is fixed.)
            // Exit if using mouse selection and is over a UI element:
            if ((selectAt == SelectAt.MousePosition) && (UnityEngine.EventSystems.EventSystem.current != null) && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
#endif

            // Raycast 2D or 3D:
            switch (runRaycasts)
            {
                case Dimension.In2D:
                    Run2DRaycast();
                    break;
                default:
                case Dimension.In3D:
                    Run3DRaycast();
                    break;
            }

            // If the player presses the use key/button on a target:
            if (IsUseButtonDown()) UseCurrentSelection();
        }

        /// <summary>
        /// Calls OnUse on the current selection.
        /// </summary>
        public virtual void UseCurrentSelection()
        {
            if (usable != null && usable.enabled && usable.gameObject.activeInHierarchy)
            {
                clickedDownOn = null;
                if (distance <= usable.maxUseDistance)
                {
                    usable.OnUseUsable();
                    if (usable != null)
                    {
                        // If within range, send the OnUse message:
                        var fromTransform = (actorTransform != null) ? actorTransform : this.transform;
                        if (broadcastToChildren)
                        {
                            usable.gameObject.BroadcastMessage("OnUse", fromTransform, SendMessageOptions.DontRequireReceiver);
                        }
                        else
                        {
                            usable.gameObject.SendMessage("OnUse", fromTransform, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }
                else
                {

                    // Otherwise report too far if configured to do so:
                    if (!string.IsNullOrEmpty(tooFarMessage))
                    {
                        DialogueManager.ShowAlert(tooFarMessage);
                    }
                    tooFarEvent.Invoke();
                }
            }
        }

        protected virtual void Run2DRaycast()
        {
#if USE_PHYSICS2D || !UNITY_2018_1_OR_NEWER

            var mainCamera = UnityEngine.Camera.main;
            if (!mainCamera.orthographic && !hasReportedInvalidCamera)
            {
                hasReportedInvalidCamera = true;
                Debug.LogWarning("In 2D mode, Selector requires an orthographic camera.", this);
            }

            if (raycastAll)
            {

                // Run Physics2D.RaycastAll:
                if (lastHits2D == null) lastHits2D = new RaycastHit2D[MaxHits];
                int numHits = Physics2D.RaycastNonAlloc(mainCamera.ScreenToWorldPoint(GetSelectionPoint()), Vector2.zero, lastHits2D, maxSelectionDistance, layerMask);
                bool foundUsable = false;
                for (int i = 0; i < numHits; i++)
                {
                    var hit = lastHits2D[i];
                    float hitDistance = (distanceFrom == DistanceFrom.Camera) ? 0
                        : (distanceFrom == DistanceFrom.GameObject || actorTransform == null)
                            ? Vector3.Distance(gameObject.transform.position, hit.collider.transform.position)
                            : Vector3.Distance(actorTransform.position, hit.collider.transform.position);
                    if (selection == hit.collider.gameObject)
                    {
                        foundUsable = true;
                        distance = hitDistance;
                        break;
                    }
                    else
                    {
                        Usable hitUsable = hit.collider.GetComponent<Usable>();
                        if (hitUsable != null && hitUsable.enabled)
                        {
                            foundUsable = true;
                            distance = hitDistance;
                            SetCurrentUsable(hitUsable);
                            break;
                        }
                    }
                }
                if (!foundUsable)
                {
                    DeselectTarget();
                }

            }
            else
            {

                // Cast a ray and see what we hit:
                RaycastHit2D hit;
                hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(GetSelectionPoint()), Vector2.zero, maxSelectionDistance, layerMask);
                if (hit.collider != null)
                {
                    distance = (distanceFrom == DistanceFrom.Camera) ? 0
                        : (distanceFrom == DistanceFrom.GameObject || actorTransform == null)
                            ? Vector3.Distance(gameObject.transform.position, hit.collider.transform.position)
                            : Vector3.Distance(actorTransform.position, hit.collider.transform.position);
                    if (selection != hit.collider.gameObject)
                    {
                        Usable hitUsable = hit.collider.GetComponent<Usable>();
                        if (hitUsable != null && hitUsable.enabled)
                        {
                            heading = string.Empty;
                            useMessage = string.Empty;
                            SetCurrentUsable(hitUsable);
                        }
                        else
                        {
                            DeselectTarget();
                        }
                    }
                }
                else
                {
                    DeselectTarget();
                }
            }
#endif
        }

        protected virtual void Run3DRaycast()
        {
            Ray ray = UnityEngine.Camera.main.ScreenPointToRay(GetSelectionPoint());
            lastRay = ray;

            // New Variable rayCastDistance is used below for the raycasts instead of maxSelectionDistance to be able to set it to infinity (if using DistanceFrom.GameObject) instead of maxSelectionDistance:
            // Credit: Daniel D. (Thank you!)
            float raycastDistance = (distanceFrom == DistanceFrom.GameObject) ? Mathf.Infinity : maxSelectionDistance;

            if (raycastAll)
            {

                // Run RaycastAll:
                if (lastHits == null) lastHits = new RaycastHit[MaxHits];
                numLastHits = Physics.RaycastNonAlloc(ray, lastHits, raycastDistance, layerMask);
                bool foundUsable = false;
                for (int i = 0; i < numLastHits; i++)
                {
                    var hit = lastHits[i];
                    float hitDistance = (distanceFrom == DistanceFrom.Camera) ? hit.distance
                        : (distanceFrom == DistanceFrom.GameObject || actorTransform == null)
                            ? Vector3.Distance(gameObject.transform.position, hit.collider.transform.position)
                            : Vector3.Distance(actorTransform.position, hit.collider.transform.position);
                    if (selection == hit.collider.gameObject)
                    {
                        foundUsable = true;
                        distance = hitDistance;
                        break;
                    }
                    else
                    {
                        Usable hitUsable = hit.collider.GetComponent<Usable>();
                        if (hitUsable != null && hitUsable.enabled && hitDistance <= maxSelectionDistance)
                        {
                            foundUsable = true;
                            distance = hitDistance;
                            SetCurrentUsable(hitUsable);
                            break;
                        }
                    }
                }
                if (!foundUsable)
                {
                    DeselectTarget();
                }

            }
            else
            {

                // Cast a ray and see what we hit:
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, maxSelectionDistance, layerMask))
                {
                    distance = (distanceFrom == DistanceFrom.Camera) ? hit.distance
                        : (distanceFrom == DistanceFrom.GameObject || actorTransform == null)
                            ? Vector3.Distance(gameObject.transform.position, hit.collider.transform.position)
                            : Vector3.Distance(actorTransform.position, hit.collider.transform.position);
                    Usable hitUsable = hit.collider.GetComponent<Usable>();
                    if (hitUsable != null && hitUsable.enabled)
                    {
                        if (selection != hit.collider.gameObject)
                        {
                            SetCurrentUsable(hitUsable);
                        }
                    }
                    else
                    {
                        DeselectTarget();
                    }
                }
                else
                {
                    DeselectTarget();
                }
                lastHit = hit;
            }
        }

        public virtual void SetCurrentUsable(Usable usable)
        {
            if (usable == this.usable) return;
            if (usable == null)
            {
                DeselectTarget();
            }
            else
            {
                if (this.usable != null && this.usable != usable) DeselectTarget();
                this.usable = usable;
                usable.disabled -= OnUsableDisabled;
                usable.disabled += OnUsableDisabled;
                selection = usable.gameObject;
                heading = string.Empty;
                useMessage = string.Empty;
                OnSelectedUsableObject(usable);
            }
        }

        protected void OnSelectedUsableObject(Usable usable)
        {
            if (SelectedUsableObject != null) SelectedUsableObject(usable);
            onSelectedUsable.Invoke(usable);
            if (usable != null) usable.OnSelectUsable();
        }

        protected void OnDeselectedUsableObject(Usable usable)
        {
            if (DeselectedUsableObject != null) DeselectedUsableObject(usable);
            onDeselectedUsable.Invoke(usable);
            if (usable != null) usable.OnDeselectUsable();
        }

        protected virtual void DeselectTarget()
        {
            if (usable != null) usable.disabled -= OnUsableDisabled;
            OnDeselectedUsableObject(usable);
            usable = null;
            selection = null;
            heading = string.Empty;
            useMessage = string.Empty;
        }

        protected virtual void OnUsableDisabled(Usable usable)
        {
            if (usable == this.usable)
            {
                DeselectTarget();
            }
        }

        protected virtual bool IsUseButtonDown()
        {
            if (DialogueManager.IsDialogueSystemInputDisabled()) return false;

            // First check for button down to remember what was selected at the time:
            if (!string.IsNullOrEmpty(useButton) && DialogueManager.getInputButtonDown(useButton))
            {
                clickedDownOn = selection;
            }

            // Check for use key or button (only if releasing button on same selection):
            if ((useKey != KeyCode.None) && InputDeviceManager.IsKeyDown(useKey)) return true;
            if (!string.IsNullOrEmpty(useButton))
            {
                if (DialogueManager.instance != null &&
                    DialogueManager.getInputButtonDown == DialogueManager.instance.StandardGetInputButtonDown && IsUsingDefaultInputManager())
                {
                    return InputDeviceManager.IsButtonUp(useButton) && (selection == clickedDownOn);
                }
                else
                {
                    return DialogueManager.GetInputButtonDown(useButton);
                }
            }
            return false;
        }

        protected virtual bool IsUsingDefaultInputManager()
        {
            // Make sure we're not using Rewired, since we don't want to use clickedDownOn with Rewired:
            if (!hasCheckedDefaultInputManager)
            {
                hasCheckedDefaultInputManager = true;
                var inputDeviceManagerRewiredType = RuntimeTypeUtility.GetTypeFromName("PixelCrushers.RewiredSupport.InputDeviceManagerRewired");
                var isRewiredPresent = (inputDeviceManagerRewiredType != null) && (GameObjectUtility.FindFirstObjectByType(inputDeviceManagerRewiredType) != null);
                isUsingDefaultInputManager = !isRewiredPresent;
            }
            return isUsingDefaultInputManager;
        }

        protected virtual Vector3 GetSelectionPoint()
        {
            switch (selectAt)
            {
                case SelectAt.MousePosition:
                    return InputDeviceManager.GetMousePosition();
                case SelectAt.CustomPosition:
                    return CustomPosition;
                default:
                case SelectAt.CenterOfScreen:
                    return new Vector3(Screen.width / 2, Screen.height / 2);
            }
        }

        /// <summary>
        /// If useDefaultGUI is <c>true</c> and a usable object has been targeted, this method
        /// draws a selection message and targeting reticle.
        /// </summary>
        public virtual void OnGUI()
        {
            if (!enabled) return;
            if (!useDefaultGUI) return;
            if (guiStyle == null && (Event.current.type == EventType.Repaint || usable != null))
            {
                SetGuiStyle();
            }
            if (usable != null)
            {
                bool inUseRange = (distance <= usable.maxUseDistance);
                guiStyle.normal.textColor = inUseRange ? inRangeColor : outOfRangeColor;
                if (string.IsNullOrEmpty(heading))
                {
                    heading = usable.GetName();
                    useMessage = DialogueManager.GetLocalizedText(string.IsNullOrEmpty(usable.overrideUseMessage) ? defaultUseMessage : usable.overrideUseMessage);
                }
                UnityGUITools.DrawText(new Rect(0, 0, Screen.width, Screen.height), heading, guiStyle, textStyle, textStyleColor);
                UnityGUITools.DrawText(new Rect(0, guiStyleLineHeight, Screen.width, Screen.height), useMessage, guiStyle, textStyle, textStyleColor);
                Texture2D reticleTexture = inUseRange ? reticle.inRange : reticle.outOfRange;
                if (reticleTexture != null) GUI.Label(new Rect(0.5f * (Screen.width - reticle.width), 0.5f * (Screen.height - reticle.height), reticle.width, reticle.height), reticleTexture);
            }
        }

        protected void SetGuiStyle()
        {
            guiSkin = UnityGUITools.GetValidGUISkin(guiSkin);
            GUI.skin = guiSkin;
            guiStyle = new GUIStyle(string.IsNullOrEmpty(guiStyleName) ? GUI.skin.label : (GUI.skin.FindStyle(guiStyleName) ?? GUI.skin.label));
            guiStyle.alignment = alignment;
            guiStyleLineHeight = guiStyle.CalcSize(new GUIContent("Ay")).y;
        }

        /// <summary>
        /// Draws raycast result gizmos.
        /// </summary>
        public virtual void OnDrawGizmos()
        {
            if (!debug) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(lastRay.origin, lastRay.origin + lastRay.direction * maxSelectionDistance);
            if (raycastAll)
            {
                if (lastHits != null)
                {
                    for (int i = 0; i < numLastHits; i++)
                    {
                        var hit = lastHits[i];
                        var usable = hit.collider.GetComponent<Usable>();
                        bool isUsable = (usable != null) && usable.enabled;
                        Gizmos.color = isUsable ? Color.green : Color.red;
                        Gizmos.DrawWireSphere(hit.point, 0.2f);
                    }
                }
            }
            else
            {
                if (lastHit.collider != null)
                {
                    var usable = lastHit.collider.GetComponent<Usable>();
                    bool isUsable = (usable != null) && usable.enabled;
                    Gizmos.color = isUsable ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(lastHit.point, 0.2f);
                }
            }
        }

    }

}
