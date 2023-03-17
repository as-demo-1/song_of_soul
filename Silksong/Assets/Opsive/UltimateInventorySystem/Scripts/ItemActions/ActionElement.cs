/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using UnityEngine;

    /// <summary>
    /// Abstract class to create actions that can be invoked on Items.
    /// </summary>
    [System.Serializable]
    public abstract class ActionElement
    {
        [Tooltip("The name of the item action.")]
        [DelayedAttribute]
        [SerializeField] protected string m_Name;

        [System.NonSerialized] protected bool m_Initialized = false;

        public string Name {
            get => m_Name;
            set => m_Name = value;
        }

        public bool IsInitialized => m_Initialized;
        public bool CloseOnInvoke { get; set; } = true;

        /// <summary>
        /// Initializes the Item Action.
        /// <param name="force">Force the initialization.</param>
        /// </summary>
        public virtual void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }
            m_Initialized = true;
        }

        /// <summary>
        /// Change the name of the action.
        /// </summary>
        /// <param name="name">The new name.</param>
        public void SetName(string name)
        {
            m_Name = name;
        }

        /// <summary>
        /// Checks if the action can be invoked on the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="inventory">The inventory which has the item (can be null).</param>
        /// <returns>True if it can be invoked.</returns>
        public virtual bool CanInvoke()
        {
            if (m_Initialized == false) {
                Debug.LogWarning("The action is not initialized.");
                return false;
            }

            return CanInvokeInternal();
        }

        /// <summary>
        /// Checks if the action can be invoked on the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="inventory">The inventory which has the item (can be null).</param>
        /// <returns>True if it can be invoked.</returns>
        protected abstract bool CanInvokeInternal();

        /// <summary>
        /// Invoke the action on the item that is inside the inventory after it has been checked that it can be invoked.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="inventory">The inventory which has the item (can be null).</param>
        public virtual void InvokeAction()
        {
            if (CanInvoke() == false) { return; }

            InvokeActionInternal();
        }

        /// <summary>
        /// Internal Invoke, is used by the default InvokeAction.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="inventory">The inventory which has the item (can be null).</param>
        protected abstract void InvokeActionInternal();
    }
}