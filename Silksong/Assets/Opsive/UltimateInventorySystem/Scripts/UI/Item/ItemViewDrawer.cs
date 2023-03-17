/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// Item View drawer is used to draw Item Viewes.
    /// </summary>
    public class ItemViewDrawer : ViewDrawer<ItemInfo>
    {

        [FormerlySerializedAs("m_CategoryItemBoxSet")]
        [Tooltip("The Item Viewes for each itemCategory.")]
        [SerializeField] protected CategoryItemViewSet m_CategoryItemViewSet;

        public CategoryItemViewSet CategoryItemViewSet {
            get => m_CategoryItemViewSet;
            set => m_CategoryItemViewSet = value;
        }

        /// <summary>
        /// Get the Box prefab for the item specified.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>The box prefab game object.</returns>
        public override GameObject GetViewPrefabFor(ItemInfo itemInfo)
        {
            if (m_CategoryItemViewSet == null) {
                Debug.LogError("The item View Drawer is missing a reference to a Category Item View Set.", gameObject);
                return null;
            }

            return m_CategoryItemViewSet.FindItemViewPrefabForItem(itemInfo.Item);
        }

    }
}
