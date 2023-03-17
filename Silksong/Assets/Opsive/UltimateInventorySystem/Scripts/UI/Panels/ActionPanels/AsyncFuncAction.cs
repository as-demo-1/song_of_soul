/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ActionPanels
{
    using System;
    using UnityEngine;

    /// <summary>
    /// A class used to store an asynchronous function with a name.
    /// </summary>
    /// <typeparam name="T">The type of the action parameter.</typeparam>
    [Serializable]
    public class AsyncFuncAction<T>
    {
        [Tooltip("The name of the action.")]
        [SerializeField] protected string m_Name;

        protected Func<T> m_Func;

        public string Name {
            get => m_Name;
            set => m_Name = value;
        }

        public Func<T> Func {
            get => m_Func;
            set => m_Func = value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="func">The function.</param>
        public AsyncFuncAction(string name, Func<T> func)
        {
            m_Name = name;
            m_Func = func;
        }
    }
}