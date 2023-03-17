/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using System;

    /// <summary>
    /// Settable action element.
    /// </summary>
    [System.Serializable]
    public class SettableActionElement : ActionElement
    {
        protected Action m_Action;
        protected Func<bool> m_Condition;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        /// <param name="condition">The condition.</param>
        public SettableActionElement(string name, Action action, Func<bool> condition)
        {
            SetName(name);
            SetAction(action);
            SetCondition(condition);
        }

        /// <summary>
        /// Set the action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void SetAction(Action action)
        {
            m_Action = action;
        }

        /// <summary>
        /// Set the condition.
        /// </summary>
        /// <param name="condition">The condition.</param>
        public void SetCondition(Func<bool> condition)
        {
            m_Condition = condition;
        }

        /// <summary>
        /// Can the action be invoked.
        /// </summary>
        /// <returns>True if the action can be invoked.</returns>
        protected override bool CanInvokeInternal()
        {
            return m_Condition?.Invoke() ?? false;
        }

        /// <summary>
        /// Invoke the action.
        /// </summary>
        protected override void InvokeActionInternal()
        {
            m_Action?.Invoke();
        }
    }
}