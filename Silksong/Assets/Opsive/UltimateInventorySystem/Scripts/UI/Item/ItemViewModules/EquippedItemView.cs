/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// An Item View component that enables/disables object depending if they are part of an itemCollection.
    /// </summary>
    public class EquippedItemView : ItemViewModule
    {
        [Tooltip("The attribute name for the equipped state, has priority over the collection ID.")]
        [SerializeField] protected string m_EquippedAttributeName = "IsEquipped";
        [Tooltip("The Item Collection that specifies an item is equipped.")]
        [SerializeField] protected ItemCollectionID m_EquippedCollectionID = ItemCollectionPurpose.Equipped;
        [Tooltip("The objects that will be set active if the item is equipped.")]
        [SerializeField] protected GameObject[] m_ActiveIfEquipped;

        protected bool m_IsEquipped = false;

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null) {
                Clear();
                return;
            }

            if (info.Item.TryGetAttributeValue<bool>(m_EquippedAttributeName, out var isEquipped)) {
                m_IsEquipped = isEquipped;
            } else {
                if (info.ItemCollection == null) {
                    Clear();
                    return;
                }
                m_IsEquipped = m_EquippedCollectionID.Compare(info.ItemCollection);
            }

            for (int i = 0; i < m_ActiveIfEquipped.Length; i++) {
                m_ActiveIfEquipped[i].SetActive(m_IsEquipped);
            }
        }

        /// <summary>
        /// Clear the value.
        /// </summary>
        public override void Clear()
        {
            for (int i = 0; i < m_ActiveIfEquipped.Length; i++) {
                m_ActiveIfEquipped[i].SetActive(false);
            }
        }
    }
}