/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Views
{
    using UnityEngine;

    /// <summary>
    /// Base class for box UI components.
    /// </summary>
    public abstract class ViewModule : MonoBehaviour
    {
        private RectTransform m_RectTransform;

        protected View m_BaseView;

        public RectTransform RectTransform {
            get {
                if (m_RectTransform == null) {
                    m_RectTransform = transform as RectTransform;
                }

                return m_RectTransform;
            }
        }

        public View BaseView => m_BaseView;

        /// <summary>
        /// Clear the ox component.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="view">The box object which uses this module.</param>
        public virtual void Initialize(View view)
        {
            m_BaseView = view;
        }

        /// <summary>
        /// Alternative to ToString, used in the editor to display a readable name.
        /// </summary>
        public virtual string ToText()
        {
            return GetType().Name;
        }
    }

    /// <summary>
    /// Generic base class for box UI components.
    /// </summary>
    /// <typeparam name="T">The box module value type.</typeparam>
    public abstract class ViewModule<T> : ViewModule
    {
        protected View<T> m_View;

        public View<T> View => m_View;
        public T CurrentValue => m_View.CurrentValue;

        /// <summary>
        /// Set the box value.
        /// </summary>
        /// <param name="info">The new value.</param>
        public abstract void SetValue(T info);

        /// <summary>
        /// Initialize the module.
        /// </summary>
        /// <param name="view">The box object which uses this module.</param>
        public override void Initialize(View view)
        {
            base.Initialize(view);
            m_View = view as View<T>;
        }
    }
}