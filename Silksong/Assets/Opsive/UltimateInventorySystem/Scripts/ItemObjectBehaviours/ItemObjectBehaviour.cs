/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemObjectBehaviours
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.ItemActions;
    using UnityEngine;

    /// <summary>
    /// An abstract class used to create actions for item objects.
    /// </summary>
    public abstract class ItemObjectBehaviour : MonoBehaviour
    {
        protected float m_NextUseTime;

        /// <summary>
        /// Can the item object be used.
        /// </summary>
        public virtual bool CanUse => Time.time >= m_NextUseTime;

        /// <summary>
        /// Use the item object.
        /// </summary>
        /// <param name="itemObject">The item object.</param>
        /// <param name="itemUser">The item user.</param>
        public abstract void Use(ItemObject itemObject, ItemUser itemUser);
    }
}