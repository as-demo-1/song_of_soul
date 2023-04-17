/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemObjectBehaviours
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.ItemActions;
    using UnityEngine;

    /// <summary>
    /// A usable Item object.
    /// </summary>
    public class ItemObjectBehaviourHandler : MonoBehaviour, IItemObjectBehaviourHandler
    {
        [Tooltip("The Item Object.")]
        [SerializeField] protected ItemObject m_ItemObject;
        [Tooltip("The Item Object Behaviours.")]
        [SerializeField] protected ItemObjectBehaviour[] m_ItemObjectBehaviours;

        public ItemObject ItemObject => m_ItemObject;

        /// <summary>
        /// Populate the Item Object Use Actions.
        /// </summary>
        protected virtual void Awake()
        {
            if (m_ItemObject == null) {
                m_ItemObject = GetComponent<ItemObject>();
            }
        }

        /// <summary>
        /// Use an item directly.
        /// </summary>
        /// <param name="itemUser">The item user.</param>
        /// <param name="itemActionIndex">The item action index to use.</param>
        public virtual void UseItem(ItemUser itemUser, int itemActionIndex)
        {
            if (m_ItemObjectBehaviours == null) { return; }
            if (itemActionIndex < 0 || itemActionIndex >= m_ItemObjectBehaviours.Length) { return; }

            var itemObjectBehaviour = m_ItemObjectBehaviours[itemActionIndex];

            if (itemObjectBehaviour == null) {
                Debug.LogWarning($"An action component is null for {this}.");
                return;
            }

            if (itemObjectBehaviour.CanUse) {
                itemObjectBehaviour.Use(ItemObject, itemUser);
            }
        }
    }
}