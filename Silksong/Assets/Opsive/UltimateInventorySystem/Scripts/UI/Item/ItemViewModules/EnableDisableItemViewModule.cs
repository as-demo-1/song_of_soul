/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using UnityEngine;

    /// <summary>
    /// A Item View UI component used to enable disable gameobjects depending on whether an item was assigned to the slot or not.
    /// </summary>
    public class EnableDisableItemViewModule : ItemViewModule
    {
        [Tooltip("Enable these gameobjects only if an item is set.")]
        [SerializeField] protected GameObject[] m_EnableWhenItem;
        [Tooltip("Enable these gameobjects only if there are no items set.")]
        [SerializeField] protected GameObject[] m_EnableWhenNoItem;

        /// <summary>
        /// Set the item info.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null || info.Item.IsInitialized == false) {
                Clear();
                return;
            }

            EnableDisableObjects(true);
        }

        /// <summary>
        /// Clear the component.
        /// </summary>
        public override void Clear()
        {
            EnableDisableObjects(false);
        }

        /// <summary>
        /// Enable or disable the gamobject whther an item was set or not.
        /// </summary>
        /// <param name="itemIsValid">Was a valid item set?</param>
        private void EnableDisableObjects(bool itemIsValid)
        {
            for (int i = 0; i < m_EnableWhenItem.Length; i++) { m_EnableWhenItem[i].SetActive(itemIsValid); }

            for (int i = 0; i < m_EnableWhenNoItem.Length; i++) { m_EnableWhenNoItem[i].SetActive(!itemIsValid); }
        }
    }
}