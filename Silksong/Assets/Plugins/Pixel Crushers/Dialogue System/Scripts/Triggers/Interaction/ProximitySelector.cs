// Copyright (c) Pixel Crushers. All rights reserved.

using PixelCrushers.DialogueSystem.UnityGUI;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This component implements a proximity-based selector that allows the player to move into
    /// range and use a usable object. 
    /// 
    /// To mark an object usable, add the Usable component and a collider to it. The object's
    /// layer should be in the layer mask specified on the Selector component.
    /// 
    /// The proximity selector tracks the most recent usable object whose trigger the player has
    /// entered. It displays a targeting reticle and information about the object. If the target
    /// is in range, the inRange reticle texture is displayed; otherwise the outOfRange texture is
    /// displayed.
    /// 
    /// If the player presses the use button (which defaults to spacebar and Fire2), the targeted
    /// object will receive an "OnUse" message.
    /// 
    /// You can hook into SelectedUsableObject and DeselectedUsableObject to get notifications
    /// when the current target has changed and Enabled and Disabled when the component is 
    /// enabled or disabled.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class ProximitySelector : MonoBehaviour
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
        /// If <c>true</c>, uses a default OnGUI to display a selection message and
        /// targeting reticle.
        /// </summary>
        [Tooltip("Use a default OnGUI to display selection message and targeting reticle.")]
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
        /// The color of the information labels when the target is in range.
        /// </summary>
        [Tooltip("Color of the information labels when the target is in range.")]
        public Color color = Color.yellow;

        /// <summary>
        /// The text style for the text.
        /// </summary>
        public TextStyle textStyle = TextStyle.Shadow;

        /// <summary>
        /// The color of the text style's outline or shadow.
        /// </summary>
        [Tooltip("Color of the text style's outline or shadow.")]
        public Color textStyleColor = Color.black;

        /// <summary>
        /// The default use message. This can be overridden in the target's Usable component.
        /// </summary>
        [Tooltip("Default use message. This can be overridden in the target's Usable component.")]
        public string defaultUseMessage = "(spacebar to interact)";

        /// <summary>
        /// The key that sends an OnUse message.
        /// </summary>
        [Tooltip("Key that sends an OnUse message.")]
        public KeyCode useKey = KeyCode.Space;

        /// <summary>
        /// The button that sends an OnUse message.
        /// </summary>
        [Tooltip("Input button that sends an OnUse message.")]
        public string useButton = "Fire2";

        /// <summary>
        /// Tick to enable touch triggering.
        /// </summary>
        [Tooltip("Enable touch triggering.")]
        public bool enableTouch = false;

        /// <summary>
        /// If touch triggering is enabled and there's a touch in this area,
        /// the selector triggers.
        /// </summary>
        public ScaledRect touchArea = new ScaledRect(ScaledRect.empty);

        /// <summary>
        /// If ticked, the OnUse message is broadcast to the usable object's children.
        /// </summary>
        [Tooltip("Broadcast OnUse message to Usable object's children.")]
        public bool broadcastToChildren = true;

        /// <summary>
        /// The actor transform to send with OnUse. Defaults to this transform.
        /// </summary>
        [Tooltip("Actor transform to send with OnUse. Defaults to this transform.")]
        public Transform actorTransform = null;

        public UsableUnityEvent onSelectedUsable = new UsableUnityEvent();

        public UsableUnityEvent onDeselectedUsable = new UsableUnityEvent();

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

        /// <summary>
        /// Gets the current usable.
        /// </summary>
        /// <value>The usable.</value>
        public Usable CurrentUsable { 
            get { return currentUsable; } 
            set { SetCurrentUsable(value); }
        }

        /// <summary>
        /// Gets the GUI style.
        /// </summary>
        /// <value>The GUI style.</value>
        public GUIStyle GuiStyle { get { SetGuiStyle(); return guiStyle; } }

        /// <summary>
        /// Keeps track of which usable objects' triggers the selector is currently inside.
        /// </summary>
        protected List<Usable> usablesInRange = new List<Usable>();

        /// <summary>
        /// The current usable that will receive an OnUse message if the player hits the use button.
        /// </summary>
        protected Usable currentUsable = null;

        protected string currentHeading = string.Empty;

        protected string currentUseMessage = string.Empty;

        protected bool toldListenersHaveUsable = false;

        /// <summary>
        /// Caches the GUI style to use when displaying the selection message in OnGUI.
        /// </summary>
        protected GUIStyle guiStyle = null;

        protected float guiStyleLineHeight = 16f;

        protected const float MinTimeBetweenUseButton = 0.5f;
        protected float timeToEnableUseButton = 0;

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
            bool found = false;
            if (GetComponent<Collider>() != null) found = true;
#if USE_PHYSICS2D || !UNITY_2018_1_OR_NEWER
            if (!found && GetComponent<Collider2D>() != null) found = true;
#endif
            if (!found && DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Proximity Selector requires a collider, but it has no collider component. If your project is 2D, did you enable 2D support? (Tools > Pixel Crushers > Dialogue System > Welcome Window)", this);
        }

        public virtual void OnConversationStart(Transform actor)
        {
            timeToEnableUseButton = Time.time + MinTimeBetweenUseButton;
        }

        public virtual void OnConversationEnd(Transform actor)
        {
            timeToEnableUseButton = Time.time + MinTimeBetweenUseButton;
        }

        /// <summary>
        /// Sends an OnUse message to the current selection if the player presses the use button.
        /// </summary>
        protected virtual void Update()
        {
            // Exit if disabled or paused:
            if (!enabled || (Time.timeScale <= 0)) return;

            // If the currentUsable went missing (was destroyed, deactivated, or we changed scene), tell listeners:
            if (toldListenersHaveUsable && (currentUsable == null || !currentUsable.enabled || !currentUsable.gameObject.activeInHierarchy))
            {
                SetCurrentUsable(null);
                OnDeselectedUsableObject(null);
                toldListenersHaveUsable = false;
            }

            // If the player presses the use key/button, send the OnUse message:
            if (IsUseButtonDown()) UseCurrentSelection();
        }

        protected void OnSelectedUsableObject(Usable usable)
        {
            if (SelectedUsableObject != null) SelectedUsableObject(usable);
            onSelectedUsable.Invoke(usable);
            if (usable != null)
            {
                usable.OnSelectUsable();
            }
        }

        protected void OnDeselectedUsableObject(Usable usable)
        {
            if (DeselectedUsableObject != null) DeselectedUsableObject(usable);
            onDeselectedUsable.Invoke(usable);
            if (usable != null)
            {
                usable.OnDeselectUsable();
            }
        }

        /// <summary>
        /// Calls OnUse on the current selection.
        /// </summary>
        public virtual void UseCurrentSelection()
        {
            if ((currentUsable != null) && currentUsable.enabled && (currentUsable.gameObject != null) && (Time.time >= timeToEnableUseButton))
            {
                currentUsable.OnUseUsable();
                if (currentUsable != null)
                {
                    var fromTransform = (actorTransform != null) ? actorTransform : this.transform;
                    if (broadcastToChildren)
                    {
                        currentUsable.gameObject.BroadcastMessage("OnUse", fromTransform, SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        currentUsable.gameObject.SendMessage("OnUse", fromTransform, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether the player has just pressed the use button.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the use button/key is down; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsUseButtonDown()
        {
            if (DialogueManager.IsDialogueSystemInputDisabled()) return false;
            if (enableTouch && IsTouchDown()) return true;
            return ((useKey != KeyCode.None) && InputDeviceManager.IsKeyDown(useKey))
                || (!string.IsNullOrEmpty(useButton) && DialogueManager.GetInputButtonDown(useButton));
        }

        protected bool IsTouchDown()
        {
            if (Input.touchCount >= 1)
            {
                foreach (Touch touch in Input.touches)
                {
                    Vector2 screenPosition = new Vector2(touch.position.x, Screen.height - touch.position.y);
                    if (touchArea.GetPixelRect().Contains(screenPosition)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If we entered a trigger, check if it's a usable object. If so, update the selection
        /// and raise the SelectedUsableObject event.
        /// </summary>
        /// <param name='other'>
        /// The trigger collider.
        /// </param>
        protected void OnTriggerEnter(Collider other)
        {
            CheckTriggerEnter(other.gameObject);
        }

        /// <summary>
        /// If we just left a trigger, check if it's the current selection. If so, clear the
        /// selection and raise the DeselectedUsableObject event. If we're still in range of
        /// any other usables, select one of them.
        /// </summary>
        /// <param name='other'>
        /// The trigger collider.
        /// </param>
        protected void OnTriggerExit(Collider other)
        {
            CheckTriggerExit(other.gameObject);
        }

#if USE_PHYSICS2D || !UNITY_2018_1_OR_NEWER

        /// <summary>
        /// If we entered a 2D trigger, check if it's a usable object. If so, update the selection
        /// and raise the SelectedUsableObject event.
        /// </summary>
        /// <param name='other'>
        /// The 2D trigger collider.
        /// </param>
        protected void OnTriggerEnter2D(Collider2D other)
        {
            CheckTriggerEnter(other.gameObject);
        }

        /// <summary>
        /// If we just left a 2D trigger, check if it's the current selection. If so, clear the
        /// selection and raise the DeselectedUsableObject event. If we're still in range of
        /// any other usables, select one of them.
        /// </summary>
        /// <param name='other'>
        /// The 2D trigger collider.
        /// </param>
        protected void OnTriggerExit2D(Collider2D other)
        {
            CheckTriggerExit(other.gameObject);
        }

#endif

        protected virtual void CheckTriggerEnter(GameObject other)
        {
            if (!enabled) return;
            Usable usable = other.GetComponent<Usable>();
            if (usable != null && usable.enabled)
            {
                SetCurrentUsable(usable);
                if (!usablesInRange.Contains(usable)) usablesInRange.Add(usable);
                OnSelectedUsableObject(usable);
                toldListenersHaveUsable = true;
            }
        }

        protected virtual void CheckTriggerExit(GameObject other)
        {
            Usable usable = other.GetComponent<Usable>();
            if (usable != null)
            {
                RemoveUsableFromDetectedList(usable);
            }
        }

        public virtual void RemoveGameObjectFromDetectedList(GameObject other)
        {
            if (other != null) RemoveUsableFromDetectedList(other.GetComponent<Usable>());
        }

        public virtual void RemoveUsableFromDetectedList(Usable usable)
        {
            if (usable != null)
            {
                if (usablesInRange.Contains(usable)) usablesInRange.Remove(usable);
                if (currentUsable == usable)
                {
                    OnDeselectedUsableObject(usable);
                    toldListenersHaveUsable = false;
                    usablesInRange.RemoveAll(x => x == null || !x.gameObject.activeInHierarchy);
                    if (usablesInRange.Count > 0)
                    {
                        var newUsable = usablesInRange[0];
                        SetCurrentUsable(newUsable);
                        OnSelectedUsableObject(newUsable);
                        toldListenersHaveUsable = true;
                    }
                    else
                    {
                        SetCurrentUsable(null);
                    }
                }
            }
        }

        public virtual void SetCurrentUsable(Usable usable)
        {
            if (usable == currentUsable) return;
            if (currentUsable != null)
            {
                currentUsable.disabled -= OnUsableDisabled;
                if (currentUsable != usable)
                {
                    OnDeselectedUsableObject(currentUsable);
                }
            }
            currentUsable = usable;
            if (usable != null)
            {
                usable.disabled -= OnUsableDisabled;
                usable.disabled += OnUsableDisabled; 
                currentHeading = currentUsable.GetName();
                currentUseMessage = DialogueManager.GetLocalizedText(string.IsNullOrEmpty(currentUsable.overrideUseMessage) ? defaultUseMessage : currentUsable.overrideUseMessage);
            }
            else
            {
                currentHeading = string.Empty;
                currentUseMessage = string.Empty;
            }
        }

        protected virtual void OnUsableDisabled(Usable usable)
        {
            if (usable != null)
            {
                RemoveUsableFromDetectedList(usable);
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
            if (guiStyle == null && (Event.current.type == EventType.Repaint || currentUsable != null))
            {
                SetGuiStyle();
            }
            if (currentUsable != null)
            {
                GUI.skin = guiSkin;
                UnityGUITools.DrawText(new Rect(0, 0, Screen.width, Screen.height), currentHeading, guiStyle, textStyle, textStyleColor);
                UnityGUITools.DrawText(new Rect(0, guiStyleLineHeight, Screen.width, Screen.height), currentUseMessage, guiStyle, textStyle, textStyleColor);
            }
        }

        protected void SetGuiStyle()
        {
            guiSkin = UnityGUITools.GetValidGUISkin(guiSkin);
            GUI.skin = guiSkin;
            guiStyle = new GUIStyle(string.IsNullOrEmpty(guiStyleName) ? GUI.skin.label : (GUI.skin.FindStyle(guiStyleName) ?? GUI.skin.label));
            guiStyle.alignment = alignment;
            guiStyle.normal.textColor = color;
            guiStyleLineHeight = guiStyle.CalcSize(new GUIContent("Ay")).y;
        }

    }

}
