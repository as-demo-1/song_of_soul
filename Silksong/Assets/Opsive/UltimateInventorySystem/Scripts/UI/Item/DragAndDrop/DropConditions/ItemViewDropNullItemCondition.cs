/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop.DropConditions
{
    using System;
    using UnityEngine;

    [Serializable]
    public class ItemViewDropNullItemCondition : ItemViewDropCondition
    {
        [Tooltip("The source item is null.")]
        [SerializeField] protected bool m_Source = true;
        [Tooltip("The destination item is null.")]
        [SerializeField] protected bool m_Destination = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemViewDropNullItemCondition()
        {
        }

        /// <summary>
        /// Constructor with overloads.
        /// </summary>
        public ItemViewDropNullItemCondition(bool source, bool destination)
        {
            m_Source = source;
            m_Destination = destination;
        }

        /// <summary>
        /// Can the item be dropped.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        /// <returns>True if it can be dropped.</returns>
        public override bool CanDrop(ItemViewDropHandler itemViewDropHandler)
        {
            var source =
                !m_Source ||
                itemViewDropHandler.SourceItemInfo.Item == null;

            var destination =
                !m_Destination ||
                itemViewDropHandler.DestinationItemInfo.Item == null;

            return source && destination;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return $"NullItem [{m_Source},{m_Destination}]";
        }
    }
}