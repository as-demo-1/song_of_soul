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
    /// The Item View Drop Has Name Condition.
    /// </summary>
    [Serializable]
    public class ItemViewDropContainerHasNameCondition : ItemViewDropCondition
    {
        [Tooltip("If this is true the condition will return true if the name DO NOT match.")]
        [SerializeField] protected bool m_DoesNotMatchName;
        [Tooltip("The source container name.")]
        [SerializeField] protected string m_SourceContainerName;
        [Tooltip("The destination container name.")]
        [SerializeField] protected string m_DestinationContainerName;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ItemViewDropContainerHasNameCondition()
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceName">The source container name.</param>
        /// <param name="destinationName">The destination container name.</param>
        public ItemViewDropContainerHasNameCondition(string sourceName, string destinationName)
        {
            m_SourceContainerName = sourceName;
            m_DestinationContainerName = destinationName;
        }

        /// <summary>
        /// Can the item be dropped.
        /// </summary>
        /// <param name="itemViewDropHandler">The Item View Drop Handler.</param>
        /// <returns>True if it can be dropped.</returns>
        public override bool CanDrop(ItemViewDropHandler itemViewDropHandler)
        {
            if (string.IsNullOrWhiteSpace(m_SourceContainerName) == false) {
                var nameMatch = itemViewDropHandler.SourceContainer.ContainerName == m_SourceContainerName;
                if (nameMatch == m_DoesNotMatchName) {
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(m_DestinationContainerName) == false) {
                var nameMatch = itemViewDropHandler.DestinationContainer.ContainerName == m_DestinationContainerName;
                if (nameMatch == m_DoesNotMatchName) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            var not = m_DoesNotMatchName ? "_NOT_" : "";

            var sourceName = string.IsNullOrWhiteSpace(m_SourceContainerName) ? "ANY" :
                string.Format("'{0}'", m_SourceContainerName);
            var destinationName = string.IsNullOrWhiteSpace(m_DestinationContainerName) ? "ANY" :
                string.Format("'{0}'", m_DestinationContainerName);

            return base.ToString() + string.Format("{0}[{1},{2}]", not, sourceName, destinationName);
        }
    }
}