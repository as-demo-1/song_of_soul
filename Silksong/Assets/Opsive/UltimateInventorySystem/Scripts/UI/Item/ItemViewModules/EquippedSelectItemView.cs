/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.Views;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Item View UI Component which shows if an item is part of a specific item collection.
    /// </summary>
    public class EquippedSelectItemView : ItemViewModuleSelectable
    {
        [Tooltip("The image that changes when equipped.")]
        [SerializeField] protected Image m_Image;
        [Tooltip("The default sprite.")]
        [SerializeField] protected Sprite m_Default;
        [Tooltip("The selected sprite.")]
        [SerializeField] protected Sprite m_Selected;
        [Tooltip("The equipped sprite.")]
        [SerializeField] protected Sprite m_Equipped;
        [Tooltip("The equipped and selected sprite.")]
        [SerializeField] protected Sprite m_EquippedSelected;
        [Tooltip("The attribute name for the equipped state, has priority over the collection ID.")]
        [SerializeField] protected string m_EquippedAttributeName = "IsEquipped";
        [Tooltip("The equipment itemCollection.")]
        [SerializeField] protected ItemCollectionID m_EquipmentCollectionID = ItemCollectionPurpose.Equipped;

        protected bool m_IsEquipped = false;
        protected bool m_IsSelected = false;

        /// <summary>
        /// Change the background when selected.
        /// </summary>
        /// <param name="select">Should the image be selected?</param>
        public override void Select(bool select)
        {
            m_IsSelected = select;
            if (select) {
                m_Image.sprite = m_IsEquipped ? m_EquippedSelected : m_Selected;
            } else {
                m_Image.sprite = m_IsEquipped ? m_Equipped : m_Default;
            }
        }

        /// <summary>
        /// Sets the item infos.
        /// </summary>
        /// <param name="info">Contains the info about the item.</param>
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

                m_IsEquipped = m_EquipmentCollectionID.Compare(info.ItemCollection);
            }

            Select(m_IsSelected);
        }

        /// <summary>
        /// Clear the Item View.
        /// </summary>
        public override void Clear()
        {
            m_IsEquipped = false;
            base.Clear();
        }
    }
}