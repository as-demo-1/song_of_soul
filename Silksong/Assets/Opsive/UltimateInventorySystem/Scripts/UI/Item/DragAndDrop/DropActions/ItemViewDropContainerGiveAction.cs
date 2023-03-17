/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Item.DragAndDrop.DropActions
{
    using System;
    using UnityEngine;

    /// <summary>
    /// The Item View Drop Container Give Action.
    /// </summary>
    [Serializable]
    public class ItemViewDropContainerGiveAction : ItemViewDropAction
    {
        [Tooltip("Only used if the ItemBoxDropActionsWithConditions is null.")]
        [SerializeField] protected bool m_AddToDestinationContainer = true;
        [Tooltip("Only used if the ItemBoxDropActionsWithConditions is null.")]
        [SerializeField] protected bool m_RemoveFromSourceContainer = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemViewDropContainerGiveAction()
        {
        }

        /// <summary>
        /// Constructor with overloads.
        /// </summary>
        public ItemViewDropContainerGiveAction(bool addToDestination, bool removeFromSource)
        {
            m_AddToDestinationContainer = addToDestination;
            m_RemoveFromSourceContainer = removeFromSource;
        }

        /// <summary>
        /// Drop Action.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        public override void Drop(ItemViewDropHandler itemViewDropHandler)
        {
            if (m_RemoveFromSourceContainer) {
                itemViewDropHandler.StreamData.SourceItemInfo = itemViewDropHandler.SourceContainer.RemoveItem(itemViewDropHandler.StreamData.SourceItemInfo, itemViewDropHandler.SourceIndex);
            }

            if (m_AddToDestinationContainer) {
                itemViewDropHandler.DestinationContainer.AddItem(itemViewDropHandler.StreamData.SourceItemInfo, itemViewDropHandler.DestinationIndex);
            }
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var add = m_AddToDestinationContainer ? "+" : "";
            var remove = m_RemoveFromSourceContainer ? "-" : "";

            return base.ToString() + string.Format("[{0},{1}]", add, remove);
        }
    }

    /// <summary>
    /// The Item View Drop Container Exchange Action.
    /// </summary>
    [Serializable]
    public class ItemViewDropContainerExchangeAction : ItemViewDropAction
    {
        [Tooltip("Remove the item from the source container.")]
        [SerializeField] protected bool m_RemoveFromSourceContainer = true;
        [Tooltip("Remove the item from the destination container.")]
        [SerializeField] protected bool m_RemoveFromDestinationContainer = true;
        [Tooltip("Add to the destination container.")]
        [SerializeField] protected bool m_AddToDestinationContainer = true;
        [Tooltip("Add to the source container.")]
        [SerializeField] protected bool m_AddToSourceContainer = true;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemViewDropContainerExchangeAction()
        {
        }

        /// <summary>
        /// Constructor with overloads.
        /// </summary>
        public ItemViewDropContainerExchangeAction(bool removeFromSource, bool removeFromDestination, bool addToDestination, bool addToSource)
        {
            m_RemoveFromSourceContainer = removeFromSource;
            m_RemoveFromDestinationContainer = removeFromDestination;
            m_AddToDestinationContainer = addToDestination;
            m_AddToSourceContainer = addToSource;
        }

        /// <summary>
        /// Drop Action.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        public override void Drop(ItemViewDropHandler itemViewDropHandler)
        {
            if (m_RemoveFromSourceContainer) {
                itemViewDropHandler.StreamData.SourceItemInfo = itemViewDropHandler.SourceContainer.RemoveItem(itemViewDropHandler.StreamData.SourceItemInfo, itemViewDropHandler.SourceIndex);
            }

            if (m_RemoveFromDestinationContainer) {
                itemViewDropHandler.StreamData.DestinationItemInfo = itemViewDropHandler.DestinationContainer.RemoveItem(itemViewDropHandler.StreamData.DestinationItemInfo, itemViewDropHandler.DestinationIndex);
            }

            if (m_AddToDestinationContainer) {
                itemViewDropHandler.DestinationContainer.AddItem(itemViewDropHandler.StreamData.SourceItemInfo, itemViewDropHandler.DestinationIndex);
            }

            if (m_AddToSourceContainer) {
                itemViewDropHandler.SourceContainer.AddItem(itemViewDropHandler.StreamData.DestinationItemInfo, itemViewDropHandler.SourceIndex);
            }
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var remove = m_RemoveFromSourceContainer ? "s-" : ".";
            var removeDestination = m_RemoveFromDestinationContainer ? "d-" : ".";
            var add = m_AddToDestinationContainer ? "d+" : ".";
            var addSource = m_AddToSourceContainer ? "s+" : ".";

            return base.ToString() + string.Format("[{0},{1},{2},{3}]", remove, removeDestination, add, addSource);
        }
    }
}