/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using UnityEngine;

    /// <summary>
    /// The base class for a scriptable object with an ItemCollection restriction.
    /// </summary>
    public abstract class ItemRestrictionObject : ScriptableObject
    {
        public abstract IItemRestriction OriginalRestriction { get; }
        public abstract IItemRestriction DuplicateRestriction { get; }
    }
}