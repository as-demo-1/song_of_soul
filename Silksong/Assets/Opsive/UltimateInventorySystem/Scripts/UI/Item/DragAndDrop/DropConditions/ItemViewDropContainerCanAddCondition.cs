/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop.DropConditions
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The Item View Drop Container Can Add Condition.
    /// </summary>
    [Serializable]
    public class ItemViewDropContainerCanAddCondition : ItemViewDropCondition
    {
        [Tooltip("Check if the source can accept an exchange.")]
        [SerializeField] protected bool m_Source = true;
        [Tooltip("Check if the destination can accept an exchange.")]
        [SerializeField] protected bool m_Destination = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemViewDropContainerCanAddCondition()
        {
        }

        /// <summary>
        /// Constructor with overloads.
        /// </summary>
        public ItemViewDropContainerCanAddCondition(bool source, bool destination)
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
                    itemViewDropHandler.SourceContainer.CanAddItem(
                        itemViewDropHandler.StreamData.DestinationItemInfo,
                        itemViewDropHandler.SourceIndex);

            var destination =
                !m_Destination ||
                    itemViewDropHandler.DestinationContainer.CanAddItem(
                        itemViewDropHandler.StreamData.SourceItemInfo,
                        itemViewDropHandler.DestinationIndex);

            return source && destination;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return $"Container Can Add [{m_Source},{m_Destination}]";
        }
    }

    /// <summary>
    /// The Item View Drop Container Can Add Condition.
    /// </summary>
    [Serializable]
    public class ItemViewDropContainerCanGiveCondition : ItemViewDropCondition
    {
        [Tooltip("Check if the source can accept an exchange.")]
        [SerializeField] protected bool m_Source = true;
        [Tooltip("Check if the destination can accept an exchange.")]
        [SerializeField] protected bool m_Destination = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemViewDropContainerCanGiveCondition()
        {
        }

        /// <summary>
        /// Constructor with overloads.
        /// </summary>
        public ItemViewDropContainerCanGiveCondition(bool source, bool destination)
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
                itemViewDropHandler.SourceContainer.CanGiveItem(
                    itemViewDropHandler.SourceItemInfo,
                    itemViewDropHandler.SourceIndex);

            var destination =
                !m_Destination ||
                itemViewDropHandler.DestinationContainer.CanGiveItem(
                    itemViewDropHandler.DestinationItemInfo,
                    itemViewDropHandler.DestinationIndex);

            return source && destination;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return $"Container Can Add [{m_Source},{m_Destination}]";
        }
    }
}