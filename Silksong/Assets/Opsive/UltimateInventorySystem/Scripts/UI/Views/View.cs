/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Views
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The base class for box uis.
    /// </summary>
    public abstract class View : MonoBehaviour
    {
        [Tooltip("The target graphic for the item view.")]
        [SerializeField] protected Graphic m_TargetGraphic;
        [Tooltip("The canvas group used to fade in/out the entire box.")]
        [SerializeField] protected CanvasGroup m_CanvasGroup;
        [Tooltip("The box UI components. This field is set automatically on validate.")]
        [SerializeField] protected List<ViewModule> m_Modules;

        protected bool m_Initialized = false;
        protected bool m_IsSet = false;
        protected bool m_IsSelected = false;
        protected IViewSlot m_ViewSlot;

        public bool IsSet => m_IsSet;
        public bool IsSelected => m_IsSelected;
        public Graphic TargetGraphic => m_TargetGraphic;
        public RectTransform RectTransform => (RectTransform)transform;
        public CanvasGroup CanvasGroup => m_CanvasGroup;
        public IReadOnlyList<ViewModule> Modules => m_Modules;
        public IViewSlot ViewSlot => m_ViewSlot;

        /// <summary>
        /// Initialize the box modules array.
        /// </summary>
        private void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize the box modules array.
        /// </summary>
        /// <param name="force">Force the initialization.</param>
        protected virtual void Initialize(bool force)
        {
            if (!force && m_Initialized) { return; }

            if (m_CanvasGroup == null) {
                m_CanvasGroup = GetComponent<CanvasGroup>();
            }

            RetrieveModules();
            for (int i = 0; i < m_Modules.Count; i++) { m_Modules[i].Initialize(this); }

            m_Initialized = true;
        }

        /// <summary>
        /// Initialize the modules.
        /// </summary>
        internal void RetrieveModules()
        {
            if (m_Modules == null) {
                m_Modules = new List<ViewModule>();
            }

            //Clean up bindings list and initialize it.
            m_Modules.RemoveAll(x => x == null);

            var localModules = GetComponents<ViewModule>();

            for (int i = 0; i < localModules.Length; i++) {
                if (m_Modules.Contains(localModules[i])) { continue; }

                m_Modules.Add(localModules[i]);
            }

        }

        /// <summary>
        /// Select the box.
        /// </summary>
        /// <param name="select">Select or Deselect.</param>
        public virtual void Select(bool select)
        {
            m_IsSelected = select;

            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i] is IViewModuleSelectable selectComponent) {
                    selectComponent.Select(m_IsSelected);
                }
            }
        }

        /// <summary>
        /// Click the box.
        /// </summary>
        public virtual void Click()
        {
            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i] is IViewModuleClickable clickComponent) {
                    clickComponent.Click();
                }
            }
        }

        /// <summary>
        /// Clear the box, Reset from pool.
        /// </summary>
        public virtual void Clear()
        {
            Initialize(false);

            if (m_CanvasGroup != null) {
                m_CanvasGroup.interactable = true;
                m_CanvasGroup.blocksRaycasts = true;
            }

            m_IsSet = false;

            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i] == null) { continue; }
                m_Modules[i].Clear();
            }
        }

        /// <summary>
        /// Hide/Show the View.
        /// </summary>
        /// <param name="hide">Hide or show.</param>
        public virtual void Hide(bool hide)
        {
            m_CanvasGroup.alpha = hide ? 0 : 1;
            m_CanvasGroup.interactable = !hide;
            m_CanvasGroup.blocksRaycasts = !hide;
        }

        /// <summary>
        /// Hide/Show the View.
        /// </summary>
        /// <param name="hide">Hide or show.</param>
        public virtual void SetViewSlot(IViewSlot viewSlot)
        {
            m_ViewSlot = viewSlot;

            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i] is IViewModuleWithSlot withSlot) {
                    withSlot.SetViewSlot(m_ViewSlot);
                }
            }
        }

        /// <summary>
        /// Invoke an action on all modules of type T.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        public virtual void InvokeOnAllModules<T>(Action<T> action)
        {
            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i] is T moduleWithType) {
                    action.Invoke(moduleWithType);
                }
            }
        }

        public T GetViewModule<T>() where T : ViewModule
        {
            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i] is T moduleWithType) {
                    return moduleWithType;
                }
            }

            return null;
        }
    }



    /// <summary>
    /// Generic abstract class for box UIs.
    /// </summary>
    /// <typeparam name="T">The box UI value type.</typeparam>
    public abstract class View<T> : View
    {
        protected T m_CurrentValue;
        protected T m_PreviousValue;

        public T PreviousValue => m_PreviousValue;
        public T CurrentValue => m_CurrentValue;

        /// <summary>
        /// Set the value of the box.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public virtual void SetValue(T newValue)
        {
            m_PreviousValue = m_CurrentValue;
            m_CurrentValue = newValue;
            for (int i = 0; i < m_Modules.Count; i++) {
                if (m_Modules[i] is ViewModule<T> tcomponent) {
                    tcomponent.SetValue(newValue);
                }
            }
            m_IsSet = true;
        }

        /// <summary>
        /// Refresh the box.
        /// </summary>
        public void Refresh()
        {
            SetValue(m_CurrentValue);
        }

        /// <summary>
        /// Clear the box.
        /// </summary>
        public override void Clear()
        {
            m_PreviousValue = m_CurrentValue;
            m_CurrentValue = default(T);
            base.Clear();
            
        }
    }
}