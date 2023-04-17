/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Item View component that shows other items icons.
    /// </summary>
    public class ItemSlotsItemView : ItemViewModule
    {
        [Tooltip("The slotted item icons.")]
        [SerializeField] protected Image[] m_Icons;
        [Tooltip("The slots attribute name. Of type ItemAmounts.")]
        [SerializeField] protected string m_SlotsAttributeName = "Slots";
        [Tooltip("The icon attribute name. Of type Sprite.")]
        [SerializeField] protected string m_IconAttributeName = "Icon";

        /// <summary>
        /// Set the value.
        /// </summary>
        /// <param name="info">The item info.</param>
        public override void SetValue(ItemInfo info)
        {
            if (info.Item == null || info.Item.IsInitialized == false) {
                Clear();
                return;
            }

            if (!info.Item.TryGetAttributeValue<ItemAmounts>(m_SlotsAttributeName, out var slots)) {
                Clear();
                return;
            }

            if (slots == null) {
                Clear();
                return;
            }

            for (int i = 0; i < slots.Count; i++) {
                if (slots[i].Item == null) {
                    m_Icons[i].gameObject.SetActive(false);
                    continue;
                }

                if (slots[i].Item.TryGetAttributeValue<Sprite>(m_IconAttributeName, out var icon)) {
                    m_Icons[i].gameObject.SetActive(true);
                    m_Icons[i].sprite = icon;
                } else { m_Icons[i].gameObject.SetActive(false); }
            }

            for (int i = slots.Count; i < m_Icons.Length; i++) {
                m_Icons[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Clear the value.
        /// </summary>
        public override void Clear()
        {
            for (int i = 0; i < m_Icons.Length; i++) {
                m_Icons[i].gameObject.SetActive(false);
            }
        }
    }
}