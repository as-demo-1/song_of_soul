/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.Shared.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Collection of item actions.
    /// </summary>
    [Serializable]
    public class ItemActionCollection : IReadOnlyList<ItemAction>
    {
        [Tooltip("data used to serialize the item actions.")]
        [SerializeField] protected Serialization[] m_ItemActionsData;

        [System.NonSerialized] protected ResizableArray<ItemAction> m_ItemActions;
        [System.NonSerialized] protected bool m_Initialized;

        public int Count => m_ItemActions?.Count ?? 0;
        public ItemAction this[int index] {
            get => m_ItemActions[index];
            internal set => m_ItemActions[index] = value;
        }

        internal ResizableArray<ItemAction> ItemActions {
            get => m_ItemActions;
            set => m_ItemActions = value;
        }

        /// <summary>
        /// Initialize the collection.
        /// <param name="force">Force the initialization.</param>
        /// </summary>
        public void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }

            // Serialized in the editor with:
            // m_AttributeCollectionData = Serialization.Serialize(m_AttributeList);

            Deserialize();

            m_Initialized = true;
        }

        /// <summary>
        /// Deserializes the attributes and stores them in the list and dictionary.
        /// </summary>
        protected void Deserialize()
        {
            if (m_ItemActionsData != null) {
                if (m_ItemActions == null) { m_ItemActions = new ResizableArray<ItemAction>(); }
                m_ItemActions.Initialize(m_ItemActionsData.Length);
                for (int i = 0; i < m_ItemActionsData.Length; i++) {
                    var itemAction = m_ItemActionsData[i].DeserializeFields(MemberVisibility.Public) as ItemAction;
                    if (itemAction == null) { continue; }
                    itemAction.Initialize(true);
                    m_ItemActions.Add(itemAction);
                }
            } else {
                m_ItemActions = new ResizableArray<ItemAction>();
            }
        }

        /// <summary>
        /// Deserializes the attributes and stores them in the list and dictionary.
        /// </summary>
        public void Serialize()
        {
            var itemActions = new List<ItemAction>();
            for (int i = 0; i < m_ItemActions.Count; i++) {
                itemActions.Add(m_ItemActions[i]);
            }
            m_ItemActionsData = Serialization.Serialize<ItemAction>(itemActions);
        }

        /// <summary>
        /// Enumerator of itemActions.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<ItemAction> GetEnumerator()
        {
            for (int i = 0; i < m_ItemActions.Count; i++) {
                yield return m_ItemActions[i];
            }
        }

        /// <summary>
        /// IEnumerator.
        /// </summary>
        /// <returns>IEnumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}

