/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Input;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// This component is used to prevent UI from being deselected.
    /// If nothing is selected, it will select the last selected selectable. 
    /// </summary>
    public class PreventSelectableDeselection : MonoBehaviour
    {
        [Tooltip("The Display Panel Manager. Used to get panel events.")]
        [SerializeField] protected DisplayPanelManager m_DisplayManager;
        [Tooltip("If the Gameplay Panel is selected should this script allow UI Deselection?")]
        [SerializeField] protected bool m_AllowDeselectionWhileGameplayPanelSelected;

        EventSystem m_EventSystem;
        GameObject m_LastSelection;

        /// <summary>
        /// Register to the Display Manager event.
        /// </summary>
        private void Awake()
        {
            if (m_DisplayManager == null) {
                m_DisplayManager = GetComponent<DisplayPanelManager>();
            }
        }

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected virtual void Start()
        {
            m_EventSystem = EventSystemManager.GetEvenSystemFor(gameObject);

            if (m_AllowDeselectionWhileGameplayPanelSelected && m_DisplayManager.GameplayPanel != null) {
                EventHandler.RegisterEvent<bool>(m_DisplayManager.PanelOwner, EventNames.c_GameObject_OnGameplayPanelSelected_Bool, HandleGameplayPanelSelected);
                HandleGameplayPanelSelected(m_DisplayManager.SelectedDisplayPanel == m_DisplayManager.GameplayPanel);
            }
        }

        /// <summary>
        /// Check the event system selection, and select the previous one if null.
        /// </summary>
        protected virtual void Update()
        {
            if (m_EventSystem.currentSelectedGameObject != null && m_EventSystem.currentSelectedGameObject != m_LastSelection)
                m_LastSelection = m_EventSystem.currentSelectedGameObject;
            else if (m_LastSelection != null && m_EventSystem.currentSelectedGameObject == null)
                m_EventSystem.SetSelectedGameObject(m_LastSelection);
        }

        /// <summary>
        /// Handle the gameplay panel being selected.
        /// </summary>
        /// <param name="gameplaySelected">Is the gameplay panel selected.</param>
        protected virtual void HandleGameplayPanelSelected(bool gameplaySelected)
        {
            var preventDeselection = !gameplaySelected;

            if (enabled == preventDeselection) { return; }

            if (preventDeselection == false) { m_EventSystem.SetSelectedGameObject(null); }

            enabled = preventDeselection;
        }

        /// <summary>
        /// Unregister from the event.
        /// </summary>
        protected virtual void OnDestroy()
        {
            EventHandler.UnregisterEvent<bool>(m_DisplayManager.PanelOwner, EventNames.c_GameObject_OnGameplayPanelSelected_Bool, HandleGameplayPanelSelected);
        }
    }
}
