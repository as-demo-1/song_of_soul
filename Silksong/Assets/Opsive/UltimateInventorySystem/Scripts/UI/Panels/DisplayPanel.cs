/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels
{
    using Core;
    using Opsive.UltimateInventorySystem.Input;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;
    using EventHandler = Shared.Events.EventHandler;

    public struct PanelEventData
    {
        public bool Open;
        public bool OpenBack;
        public DisplayPanel ThisPanel;
        public DisplayPanel PreviousPanel;
        public Selectable PreviousSelectable;
        public bool SelectPrevious;
    }

    /// <summary>
    /// The base class for UI panels.
    /// </summary>
    public class DisplayPanel : MonoBehaviour
    {
        public event Action OnOpen;
        public event Action OnClose;
        public event Action OnOpenBack;

        [Tooltip("A unique name of this display such that the Manager can get a reference to the display easily.")]
        [SerializeField] internal string m_UniqueName;
        [Tooltip("If true the panel manager will close the gameplay panel when opened. Only one menu is opened at the same time.")]
        [SerializeField] internal bool m_IsMenuPanel = false;
        [FormerlySerializedAs("m_NonSelectablePanel")]
        [Tooltip("If true the panel manager won't select this window when it is opened.")]
        [SerializeField] internal bool m_IsNonSelectablePanel = false;
        [Tooltip("If true the panel will start enabled (Does not trigger an open event).")]
        [SerializeField] internal bool m_StartEnabled = false;
        [Tooltip("If true the panel will open on start.")]
        [SerializeField] internal bool m_OpenOnStart = false;
        [Tooltip("If true the panel will be set active when it is opened.")]
        [SerializeField] internal bool m_SetActiveOnOpen = true;
        [Tooltip("If true the panel will bet disabled when closed.")]
        [SerializeField] internal bool m_SetDisableOnClose = true;
        [Tooltip("The selectable which should be selected when the panel is opened.")]
        [SerializeField] protected Selectable m_SelectableOnOpen;
        [Tooltip("The main content used to spawn content in a particular place in the panel.")]
        [SerializeField] internal RectTransform m_MainContent;
        [Tooltip("The display bindings.")]
        [SerializeField] protected List<DisplayPanelBinding> m_Bindings;

        protected DisplayPanelManager m_Manager;
        protected Selectable m_PreviousSelectable;
        protected DisplayPanel m_PreviousPanel;

        protected bool m_IsOpen;
        protected bool m_IsSetup;

        public string UniqueName => m_UniqueName;
        public bool IsOpen => m_IsOpen;
        public bool IsSetup => m_IsSetup;
        public DisplayPanelManager Manager => m_Manager;

        public bool IsMenuPanel => m_IsMenuPanel;
        public bool IsNonSelectablePanel => m_IsNonSelectablePanel;
        public RectTransform MainContent {
            get {
                if (m_MainContent == null) {
                    m_MainContent = transform as RectTransform;
                }
                return m_MainContent;
            }
        }

        public List<DisplayPanelBinding> Bindings => m_Bindings;

        public DisplayPanel PreviousPanel => m_PreviousPanel;
        public Selectable PreviousSelectable => m_PreviousSelectable;

        /// <summary>
        /// The constructor.
        /// </summary>
        public DisplayPanel()
        {
            //Called at awake even if object is disabled
            m_IsOpen = m_StartEnabled;
        }

        /// <summary>
        /// Set up the the UI.
        /// </summary>
        public virtual void Setup(DisplayPanelManager manager, bool force)
        {
            if (m_IsSetup && m_Manager == manager && !force) { return; }
            m_Manager = manager;
            m_Manager.RegisterPanel(this);

            if (m_OpenOnStart) {
                gameObject.SetActive(true);
                m_IsOpen = false;
            } else {
                gameObject.SetActive(m_StartEnabled);
                m_IsOpen = m_StartEnabled;
            }

            m_IsSetup = true;

            if (m_MainContent == null) {
                m_MainContent = transform as RectTransform;
            }

            //Clean up bindings list and initialize it.
            m_Bindings.RemoveAll(x => x == null);

            var localBindings = GetComponents<DisplayPanelBinding>();

            for (int i = 0; i < localBindings.Length; i++) {
                if (m_Bindings.Contains(localBindings[i])) { continue; }
                m_Bindings.Add(localBindings[i]);
            }

            for (int i = 0; i < m_Bindings.Count; i++) {
                m_Bindings[i].Initialize(this, force);
            }
        }

        /// <summary>
        /// Open the menu on start.
        /// </summary>
        private void Start()
        {
            if (m_OpenOnStart) {
                SmartOpen();
            }
        }

        /// <summary>
        /// Toggles the panel Open and Closed depending on it current state.
        /// </summary>
        [ContextMenu("Smart Toggle")]
        public virtual void SmartToggle()
        {
            if (m_IsOpen) {
                SmartClose();
            } else {
                SmartOpen();
            }
        }

        /// <summary>
        /// Opens the panel through the manager.
        /// </summary>
        [ContextMenu("Smart Open")]
        public virtual void SmartOpen()
        {
            m_Manager.OpenPanel(this);
        }

        [ContextMenu("Smart Open", true)]
        public virtual bool SmartOpenContextMenuCondition()
        {
            return Application.isPlaying && !IsOpen;
        }

        /// <summary>
        /// Closes the panel through the manager.
        /// </summary>
        [ContextMenu("Smart Close")]
        public virtual void SmartClose()
        {
            Close();
        }

        [ContextMenu("Smart Close", true)]
        public virtual bool SmartCloseContextMenuCondition()
        {
            return Application.isPlaying && IsOpen;
        }

        /// <summary>
        /// Set the previous panel, which is opened when this panel is closed.
        /// </summary>
        /// <param name="previousPanel">The previous panel.</param>
        public virtual void SetPreviousPanel(DisplayPanel previousPanel)
        {
            m_PreviousPanel = previousPanel;
        }

        /// <summary>
        /// Set the previous selectable, which is selected when the panel is closed.
        /// </summary>
        /// <param name="previousSelectable">The previous selectable.</param>
        public virtual void SetPreviousSelectable(Selectable previousSelectable)
        {
            m_PreviousSelectable = previousSelectable;
        }

        /// <summary>
        /// Toggle the panel open, close.
        /// </summary>
        /// <param name="previousPanel">The previous panel.</param>
        /// <param name="previousSelectable">The previous selectable.</param>
        /// <param name="selectDefault">Select the default selectable.</param>
        /// <param name="selectPrevious">Select the previous window.</param>
        public void ToggleOpenClose(DisplayPanel previousPanel = null, Selectable previousSelectable = null,
            bool selectDefault = true, bool selectPrevious = true)
        {
            if (m_IsOpen) {
                Close(selectPrevious);
            } else {
                Open(previousPanel, previousSelectable, selectDefault);
            }
        }

        /// <summary>
        /// Open the panel.
        /// </summary>
        public virtual void Open()
        {
            Open(null, null, true);
        }

        /// <summary>
        /// Open the panel.
        /// </summary>
        /// <param name="previousPanel">The previous panel.</param>
        /// <param name="previousSelectable">The previous selectable.</param>
        /// <param name="selectDefault">Select the default selectable.</param>
        public virtual void Open(DisplayPanel previousPanel, Selectable previousSelectable = null,
            bool selectDefault = true)
        {
            //This is required even if it is done in OpenInternal,
            //because the selectable needs to be active before getting selected.
            if (m_SetActiveOnOpen) { gameObject.SetActive(true); }

            m_PreviousSelectable = previousSelectable;
            m_PreviousPanel = previousPanel;
            if (selectDefault && m_SelectableOnOpen != null) {
                EventSystemManager.Select(m_SelectableOnOpen.gameObject);
            }

            OpenInternal();
        }

        /// <summary>
        /// Open Back, does not change the state of previous selectable or the like.
        /// </summary>
        public virtual void OpenBack()
        {
            if (m_SetActiveOnOpen) {
                gameObject.SetActive(true);
            }

            m_IsOpen = true;

            //Send this event before the others. Such that the panel manager knows what the selected panel is.
            if (m_Manager != null) {
                EventHandler.ExecuteEvent<PanelEventData>(m_Manager.PanelOwner, EventNames.c_GameObject_OnPanelOpenClose_PanelEventData,
                    new PanelEventData() {
                        Open = true,
                        OpenBack = true,
                        PreviousPanel = m_PreviousPanel,
                        PreviousSelectable = m_PreviousSelectable,
                        ThisPanel = this,
                        SelectPrevious = false
                    });
            }

            OnOpenBack?.Invoke();
            for (int i = 0; i < m_Bindings.Count; i++) { m_Bindings[i].OnOpenBack(); }
        }

        /// <summary>
        /// Open the panel.
        /// </summary>
        protected virtual void OpenInternal()
        {
            if (m_SetActiveOnOpen) {
                gameObject.SetActive(true);
            }

            m_IsOpen = true;

            //Send this event before the others. Such that the panel manager knows what the selected panel is.
            if (m_Manager != null) {
                EventHandler.ExecuteEvent<PanelEventData>(m_Manager.PanelOwner, EventNames.c_GameObject_OnPanelOpenClose_PanelEventData,
                    new PanelEventData() {
                        Open = true,
                        OpenBack = false,
                        PreviousPanel = m_PreviousPanel,
                        PreviousSelectable = m_PreviousSelectable,
                        ThisPanel = this,
                        SelectPrevious = false
                    });
            }

            OnOpen?.Invoke();
            for (int i = 0; i < m_Bindings.Count; i++) { m_Bindings[i].OnOpen(); }

        }

        /// <summary>
        /// Close the panel.
        /// </summary>
        /// <param name="selectPrevious">Select the previous selectable.</param>
        public virtual void Close(bool selectPrevious = true)
        {
            m_IsOpen = false;

            if (selectPrevious) {
                if (m_PreviousPanel != null && m_PreviousPanel != this) {
                    m_PreviousPanel.OpenBack();
                }

                if (m_PreviousSelectable != null) {
                    EventSystemManager.Select(m_PreviousSelectable.gameObject);
                }
            }

            if (m_SetDisableOnClose) {
                gameObject.SetActive(false);
            }

            OnClose?.Invoke();
            for (int i = 0; i < m_Bindings.Count; i++) { m_Bindings[i].OnClose(); }

            if (m_Manager == null) { return; }
            EventHandler.ExecuteEvent<PanelEventData>(m_Manager.PanelOwner, EventNames.c_GameObject_OnPanelOpenClose_PanelEventData,
                new PanelEventData() {
                    Open = false,
                    OpenBack = false,
                    PreviousPanel = m_PreviousPanel,
                    PreviousSelectable = m_PreviousSelectable,
                    ThisPanel = this,
                    SelectPrevious = selectPrevious
                });
        }

        /// <summary>
        /// Get the first display binding of the type provided.
        /// </summary>
        /// <typeparam name="T">The binding type.</typeparam>
        /// <returns>The display binding.</returns>
        public T GetBinding<T>() where T : DisplayPanelBinding
        {
            for (int i = 0; i < m_Bindings.Count; i++) {
                if (m_Bindings[i] is T binding) { return binding; }
            }

            return null;
        }

        /// <summary>
        /// Set the name of the panel.
        /// </summary>
        /// <param name="panelName">The new panel name.</param>
        public void SetName(string panelName)
        {
            m_UniqueName = panelName;
        }
    }
}
