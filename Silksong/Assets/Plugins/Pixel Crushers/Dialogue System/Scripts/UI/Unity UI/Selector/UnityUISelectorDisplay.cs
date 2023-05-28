// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Replaces the Selector/ProximitySelector's OnGUI method with a method
    /// that enables or disables new Unity UI controls.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UnityUISelectorDisplay : MonoBehaviour
    {

        /// <summary>
        /// The main graphic (optional). Assign this if you have created an entire 
        /// panel for the selector.
        /// </summary>
        public UnityEngine.UI.Graphic mainGraphic = null;

        /// <summary>
        /// The text for the name of the current selection.
        /// </summary>
        public UnityEngine.UI.Text nameText = null;

        /// <summary>
        /// The text for the use message (e.g., "Press spacebar to use").
        /// </summary>
        public UnityEngine.UI.Text useMessageText = null;

        public Color inRangeColor = Color.yellow;

        public Color outOfRangeColor = Color.gray;

        /// <summary>
        /// The graphic to show if the selection is in range.
        /// </summary>
        public UnityEngine.UI.Graphic reticleInRange = null;

        /// <summary>
        /// The graphic to show if the selection is out of range.
        /// </summary>
        public UnityEngine.UI.Graphic reticleOutOfRange = null;

        [Serializable]
        public class AnimationTransitions
        {
            public string showTrigger = "Show";
            public string hideTrigger = "Hide";
        }

        public AnimationTransitions animationTransitions = new AnimationTransitions();

        private Selector selector = null;

        private ProximitySelector proximitySelector = null;

        private string defaultUseMessage = string.Empty;

        private Usable usable = null;

        private bool lastInRange = false;

        private UsableUnityUI usableUnityUI = null;

        private Animator animator = null;

        private bool previousUseDefaultGUI;

        private bool started = false;

        protected float CurrentDistance
        {
            get
            {
                return (selector != null) ? selector.CurrentDistance : 0;
            }
        }

        private void Awake()
        {
            Tools.DeprecationWarning(this);
        }

        public void Start()
        {
            started = true;
            FindUIElements();
            ConnectDelegates();
            DeactivateControls();
        }

        public void FindUIElements()
        {
            // Only find if none of the critical UI elements are assigned:
            var elements = UnityUISelectorElements.instance;
            if (elements == null) elements = SearchForElements(DialogueManager.instance.transform);
            if (elements != null) UnityUISelectorElements.instance = elements;
            if (mainGraphic == null && nameText == null && reticleInRange == null)
            {
                if (elements == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: UnityUISelectorDisplay can't find UI elements", this);
                }
                else
                {
                    if (mainGraphic == null) mainGraphic = elements.mainGraphic;
                    if (nameText == null) nameText = elements.nameText;
                    if (useMessageText == null) useMessageText = elements.useMessageText;
                    inRangeColor = elements.inRangeColor;
                    outOfRangeColor = elements.outOfRangeColor;
                    if (reticleInRange == null) reticleInRange = elements.reticleInRange;
                    if (reticleOutOfRange == null) reticleOutOfRange = elements.reticleOutOfRange;
                    animationTransitions = elements.animationTransitions;
                }
            }
            if (mainGraphic != null) animator = mainGraphic.GetComponentInChildren<Animator>();
        }

        private UnityUISelectorElements SearchForElements(Transform t)
        {
            if (t == null) return null;
            var elements = t.GetComponent<UnityUISelectorElements>();
            if (elements != null) return elements;
            foreach (Transform child in t)
            {
                elements = SearchForElements(child);
                if (elements != null) return elements;
            }
            return null;
        }

        public void OnEnable()
        {
            if (started) ConnectDelegates();
        }

        public void OnDisable()
        {
            DisconnectDelegates();
        }

        private void ConnectDelegates()
        {
            DisconnectDelegates(); // Make sure we're not connecting twice.
            selector = GetComponent<Selector>();
            if (selector != null)
            {
                previousUseDefaultGUI = selector.useDefaultGUI;
                selector.useDefaultGUI = false;
                selector.SelectedUsableObject += OnSelectedUsable;
                selector.DeselectedUsableObject += OnDeselectedUsable;
                defaultUseMessage = selector.defaultUseMessage;
            }
            proximitySelector = GetComponent<ProximitySelector>();
            if (proximitySelector != null)
            {
                previousUseDefaultGUI = proximitySelector.useDefaultGUI;
                proximitySelector.useDefaultGUI = false;
                proximitySelector.SelectedUsableObject += OnSelectedUsable;
                proximitySelector.DeselectedUsableObject += OnDeselectedUsable;
                if (string.IsNullOrEmpty(defaultUseMessage)) defaultUseMessage = proximitySelector.defaultUseMessage;
            }
        }

        private void DisconnectDelegates()
        {
            selector = GetComponent<Selector>();
            if (selector != null)
            {
                selector.useDefaultGUI = previousUseDefaultGUI;
                selector.SelectedUsableObject -= OnSelectedUsable;
                selector.DeselectedUsableObject -= OnDeselectedUsable;
            }
            proximitySelector = GetComponent<ProximitySelector>();
            if (proximitySelector != null)
            {
                proximitySelector.useDefaultGUI = previousUseDefaultGUI;
                proximitySelector.SelectedUsableObject -= OnSelectedUsable;
                proximitySelector.DeselectedUsableObject -= OnDeselectedUsable;
            }
            HideControls();
        }

        private void OnSelectedUsable(Usable usable)
        {
            this.usable = usable;
            usableUnityUI = (usable != null) ? usable.GetComponentInChildren<UsableUnityUI>() : null;
            if (usableUnityUI != null)
            {
                usableUnityUI.Show(GetUseMessage());
            }
            else
            {
                ShowControls();
            }
            lastInRange = !IsUsableInRange();
            UpdateDisplay(!lastInRange);
        }

        private void OnDeselectedUsable(Usable usable)
        {
            if (usableUnityUI != null)
            {
                usableUnityUI.Hide();
                usableUnityUI = null;
            }
            else
            {
                HideControls();
            }
            this.usable = null;
        }

        private string GetUseMessage()
        {
            return string.IsNullOrEmpty(usable.overrideUseMessage) ? defaultUseMessage : usable.overrideUseMessage;
        }

        private void ShowControls()
        {
            if (usable == null) return;
            Tools.SetGameObjectActive(mainGraphic, true);
            Tools.SetGameObjectActive(nameText, true);
            Tools.SetGameObjectActive(useMessageText, true);
            if (nameText != null) nameText.text = usable.GetName();
            if (useMessageText != null) useMessageText.text = GetUseMessage();
            if (CanTriggerAnimations() && !string.IsNullOrEmpty(animationTransitions.showTrigger))
            {
                animator.SetTrigger(animationTransitions.showTrigger);
            }
        }

        private void HideControls()
        {
            if (CanTriggerAnimations() && !string.IsNullOrEmpty(animationTransitions.hideTrigger))
            {
                animator.SetTrigger(animationTransitions.hideTrigger);
            }
            else
            {
                DeactivateControls();
            }
        }

        private void DeactivateControls()
        {
            Tools.SetGameObjectActive(nameText, false);
            Tools.SetGameObjectActive(useMessageText, false);
            Tools.SetGameObjectActive(reticleInRange, false);
            Tools.SetGameObjectActive(reticleOutOfRange, false);
            Tools.SetGameObjectActive(mainGraphic, false);
        }

        private bool IsUsableInRange()
        {
            return (usable != null) && (CurrentDistance <= usable.maxUseDistance);
        }

        public void Update()
        {
            if (usable != null)
            {
                UpdateDisplay(IsUsableInRange());
            }
        }

        public void OnConversationStart(Transform actor)
        {
            HideControls();
        }

        public void OnConversationEnd(Transform actor)
        {
            ShowControls();
        }

        private void UpdateDisplay(bool inRange)
        {
            if ((usable != null) && (inRange != lastInRange))
            {
                lastInRange = inRange;
                if (usableUnityUI != null)
                {
                    usableUnityUI.UpdateDisplay(inRange);
                }
                else
                {
                    UpdateText(inRange);
                    UpdateReticle(inRange);
                }
            }
        }

        private void UpdateText(bool inRange)
        {
            Color color = inRange ? inRangeColor : outOfRangeColor;
            if (nameText != null) nameText.color = color;
            if (useMessageText != null) useMessageText.color = color;
        }

        private void UpdateReticle(bool inRange)
        {
            Tools.SetGameObjectActive(reticleInRange, inRange);
            Tools.SetGameObjectActive(reticleOutOfRange, !inRange);
        }

        private bool CanTriggerAnimations()
        {
            return (animator != null) && (animationTransitions != null);
        }

    }

}
