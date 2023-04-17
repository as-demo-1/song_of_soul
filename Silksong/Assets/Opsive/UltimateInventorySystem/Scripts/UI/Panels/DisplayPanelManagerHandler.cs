/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Input;
    using System;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// This component allows for inputs to open & close panels using inputs on the panel owner.
    /// It is also used to enable/disable the cursor when gameplay panel is active. 
    /// </summary>
    public class DisplayPanelManagerHandler : MonoBehaviour
    {
        /// <summary>
        /// Hot bar input.
        /// </summary>
        [Serializable]
        public class OpenToggleInput : SimpleInput
        {
            public bool Toggle;
            public string PanelName;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="panelName">The name of the input.</param>
            /// <param name="input">The input that use the item in the hotbar.</param>
            public OpenToggleInput(bool toggle, string panelName, string inputName, InputType inputType) : base(inputName, inputType)
            {
                Toggle = toggle;
                PanelName = panelName;
            }
        }
        
        [Tooltip("The display panel manager.")]
        [SerializeField] protected DisplayPanelManager m_DisplayPanelManager;
        [Tooltip("Should the cursor be disabled while the gameplay panel is selected.")]
        [SerializeField] protected bool m_DisableCursorWhileGameplay;
        [Tooltip("Should pressing the close panel input be used to enable the cursor even when gameplay panel is selected.")]
        [SerializeField] protected bool m_EnableCursorWithClose;
        [Tooltip("The input used to close the currently selected panel.")]
        [SerializeField] protected SimpleInput m_ClosePanelInput 
            = new SimpleInput("Close Panel",InputType.ButtonDown);
        [Tooltip("Open or toggle a specific panel.")]
        [SerializeField] protected OpenToggleInput[] m_OpenTogglePanelInput = new OpenToggleInput[] 
        {
            new OpenToggleInput(true, "Main Menu", "Open Panel", InputType.ButtonDown)
        };
        
        protected PlayerInput m_PlayerInput;
        protected bool m_DisableCursor;
        
        /// <summary>
        /// Initialize.
        /// </summary>
        protected virtual void Start()
        {
            if (m_DisplayPanelManager == null) {
                m_DisplayPanelManager = GetComponent<DisplayPanelManager>();
            }

            m_DisplayPanelManager.OnPanelOwnerAssigned += ChangedPanelOwner;
            
            ChangedPanelOwner();
        }

        /// <summary>
        /// Handle the panel Owner being changed by listening to events on that new panel owner.
        /// </summary>
        protected virtual void ChangedPanelOwner()
        {
            if (m_DisplayPanelManager == null || m_DisplayPanelManager.PanelOwner == null) {
                m_PlayerInput = null;
                enabled = false;
                EventHandler.UnregisterEvent<bool>(m_DisplayPanelManager.PanelOwner, EventNames.c_GameObject_OnGameplayPanelSelected_Bool, HandleGameplayPanelSelected);
                return;
            }

            m_PlayerInput = m_DisplayPanelManager.PanelOwner.gameObject.GetCachedComponent<PlayerInput>();
            EventHandler.RegisterEvent<bool>(m_DisplayPanelManager.PanelOwner, EventNames.c_GameObject_OnGameplayPanelSelected_Bool, HandleGameplayPanelSelected);
            enabled = true;
            
            HandleGameplayPanelSelected(m_DisplayPanelManager.SelectedDisplayPanel == m_DisplayPanelManager.GameplayPanel);
        }

        /// <summary>
        /// Handle the gameplay panel being selected.
        /// </summary>
        /// <param name="gameplaySelected">was the gameplay panel selected?</param>
        protected virtual void HandleGameplayPanelSelected(bool gameplaySelected)
        {
            if(m_DisableCursorWhileGameplay == false){ return; }

            m_DisableCursor = gameplaySelected;
            if (m_DisableCursor && Cursor.visible) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else if (!m_DisableCursor && !Cursor.visible) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        /// <summary>
        /// Check inputs in the update function.
        /// </summary>
        protected virtual void Update()
        {
            //Close Panel
            if (m_ClosePanelInput.CheckInput(m_PlayerInput)) {
                m_DisplayPanelManager.CloseSelectedPanel();
            }

            //Open/Toggle Panel
            for (int i = 0; i < m_OpenTogglePanelInput.Length; i++) {
                if (m_OpenTogglePanelInput[i].CheckInput(m_PlayerInput)) {
                    if (m_OpenTogglePanelInput[i].Toggle) {
                        m_DisplayPanelManager.TogglePanel(m_OpenTogglePanelInput[i].PanelName);
                    } else {
                        m_DisplayPanelManager.OpenPanel(m_OpenTogglePanelInput[i].PanelName);
                    }
                }
            }
            
            // Enable the cursor if the escape key is pressed. Disable the cursor if it is visbile but should be disabled upon press.
            if (m_EnableCursorWithClose && m_PlayerInput.GetButtonDown(m_ClosePanelInput.InputName)) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else if (Cursor.visible && m_DisableCursor && !IsPointerOverUI() && (m_PlayerInput.GetButtonDown("Fire1") || m_PlayerInput.GetButtonDown("Fire2"))) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
#if UNITY_EDITOR
            // The cursor should be visible when the game is paused.
            if (!Cursor.visible && UnityEditor.EditorApplication.isPaused && !m_DisableCursor) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
#endif
        }
        
        /// <summary>
        /// Returns true if the pointer is over a UI element.
        /// </summary>
        /// <returns>True if the pointer is over a UI element.</returns>
        public virtual bool IsPointerOverUI()
        {
            var eventSystem = EventSystemManager.GetEvenSystemFor(gameObject); 
            if (eventSystem != null && eventSystem.IsPointerOverGameObject()) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Unregister on destroy.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if(m_DisplayPanelManager == null || m_DisplayPanelManager.PanelOwner == null){return;}
            
            EventHandler.UnregisterEvent<bool>(m_DisplayPanelManager.PanelOwner, EventNames.c_GameObject_OnGameplayPanelSelected_Bool, HandleGameplayPanelSelected);
        }
    }
}