/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    /// <summary>
    /// Base class for Object Amount Views
    /// </summary>
    /// <typeparam name="T">The Object Type.</typeparam>
    public abstract class InventoryObjectAmountView<T> : ViewName
        where T : class
    {
        public event Action<IObjectAmount<T>> OnValueChanged;

        protected IntegerField m_IntegerField;

        protected InventorySystemDatabase m_Database;

        protected IObjectAmount<T> m_ObjectAmount;

        protected abstract T ObjectFieldValue { get; set; }

        protected bool m_Interactable;

        /// <summary>
        /// Constructor to set up the view.
        /// </summary>
        public InventoryObjectAmountView(InventorySystemDatabase database)
        {
            m_Database = database;
            m_IntegerField = new IntegerField();
            m_IntegerField.RegisterValueChangedCallback(evt =>
            {
                InvokeOnValueChanged(CreateNewObjectAmount(ObjectFieldValue, m_IntegerField.value));
                Refresh(false);
            });
            Add(m_IntegerField);

            AddToClassList(ControlTypeStyles.s_ObjectAmountView);
        }

        /// <summary>
        /// Refresh the view with a new value.
        /// </summary>
        /// <param name="objectAmount">The object amount.</param>
        public void Refresh(IObjectAmount<T> objectAmount)
        {
            m_ObjectAmount = objectAmount;
            Refresh(false);
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        /// <param name="notify">True if you want to invoke OnValueChanged.</param>
        public virtual void Refresh(bool notify)
        {
            var needsRefresh = false;

            if (m_IntegerField.value != m_ObjectAmount.Amount) {
                m_IntegerField.value = m_ObjectAmount.Amount;
                needsRefresh = true;
            }

            if (ObjectFieldValue != m_ObjectAmount.Object) {
                ObjectFieldValue = m_ObjectAmount.Object;

                RefreshInternal();

                needsRefresh = true;
            }

            if (needsRefresh && notify) {
                OnValueChanged?.Invoke(m_ObjectAmount);
            }
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public override void Refresh()
        {
            Refresh(false);
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public override void Refresh(object obj)
        {
            Refresh(obj as IObjectAmount<T>);
        }

        /// <summary>
        /// Invoke the OnValue changed
        /// </summary>
        /// <param name="objectAmount">The object amount.</param>
        public virtual void InvokeOnValueChanged(IObjectAmount<T> objectAmount)
        {
            OnValueChanged?.Invoke(objectAmount);
        }

        /// <summary>
        /// Set if the Amount View is interactable
        /// </summary>
        /// <param name="interactable">true if interactable.</param>
        public virtual void SetInteractable(bool interactable)
        {
            m_Interactable = interactable;

            m_IntegerField.SetEnabled(m_Interactable);
        }

        /// <summary>
        /// Refresh the Icon.
        /// </summary>
        public abstract void RefreshInternal();

        /// <summary>
        /// Create a new ObjectAmount.
        /// </summary>
        /// <param name="obj">The new Object.</param>
        /// <param name="amount">The new Amount.</param>
        /// <returns>The new ObjectAmount.</returns>
        public abstract IObjectAmount<T> CreateNewObjectAmount(T obj, int amount);
    }
}