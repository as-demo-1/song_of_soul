/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An item Action that prints the attributes of an item in the console
    /// </summary>
    [System.Serializable]
    public class DebugItemAction : ItemAction
    {
        protected string m_DebugMessage;

        public string DebugMessage => m_DebugMessage;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DebugItemAction()
        {
            m_Name = "Debug";
        }

        /// <summary>
        /// prints all the attributes of the item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            var item = itemInfo.Item;

            if (item == null) {
                m_DebugMessage = "Debug Item Info: The item is NULL";
                Debug.Log(m_DebugMessage);
                return;
            }

            var attributes = new List<AttributeBase>();
            attributes.AddRange(item.GetAttributeList());
            attributes.AddRange(item.ItemDefinition.GetAttributeList());
            attributes.AddRange(item.Category.GetCategoryAttributeList());

            m_DebugMessage = String.Format(
                "Debug Item Info: {0} \n" +
                "With Definition \"{1}\" | Category \"{2}\" | and Attributes:\n {3}",
                itemInfo, item.ItemDefinition, item.Category.name, AttributeCollection.AttributesToString(attributes));

            Debug.Log(m_DebugMessage);
        }

        /// <summary>
        /// True if the item is not null.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if item is not null.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            return true;
        }
    }
}
