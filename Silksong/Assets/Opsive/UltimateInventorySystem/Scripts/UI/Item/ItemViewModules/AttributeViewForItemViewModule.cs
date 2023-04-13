/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.ItemViewModules
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.UI.Item.AttributeViewModules;
    using UnityEngine;

    /// <summary>
    /// Item View component that shows the item name.
    /// </summary>
    public class AttributeViewForItemViewModule : ItemViewModule
    {
        [Tooltip("The Attribute name.")]
        [SerializeField] protected string m_AttributeName;
        [Tooltip("The categories attribute UIs.")]
        [SerializeField] internal AttributeView m_AttributeView;

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

            var attribute = info.Item.GetAttribute(m_AttributeName);

            var attributeInfo = new AttributeInfo(attribute, info);
            m_AttributeView.SetValue(attributeInfo);
        }

        /// <summary>
        /// Clear the value.
        /// </summary>
        public override void Clear()
        {
            m_AttributeView.Clear();
        }
    }
}