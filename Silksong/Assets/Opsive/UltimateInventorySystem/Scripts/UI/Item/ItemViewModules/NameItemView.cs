/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using UnityEngine;
    using Text = Opsive.Shared.UI.Text;

    /// <summary>
    /// Item View component that shows the item name.
    /// </summary>
    public class NameItemView : ItemViewModule
    {
        [Tooltip("The name text.")]
        [SerializeField] protected Text m_NameText;

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
            m_NameText.text = info.Item.name;
        }

        /// <summary>
        /// Clear the value.
        /// </summary>
        public override void Clear()
        {
            m_NameText.text = string.Empty;
        }
    }
}