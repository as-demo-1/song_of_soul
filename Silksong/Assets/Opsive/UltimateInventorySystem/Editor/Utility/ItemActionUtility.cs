/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Utility
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.ItemActions;

    /// <summary>
    /// The item action utility class.
    /// </summary>
    public static class ItemActionUtility
    {
        /// <summary>
        /// Get the attributeList as a List so that it can be modified
        /// </summary>
        /// <param name="itemActionCollection">The item action collection.</param>
        /// <returns>The resizable array.</returns>
        public static ResizableArray<ItemAction> GetItemActionResizableArray(ItemActionCollection itemActionCollection)
        {
            return itemActionCollection.ItemActions;
        }

        /// <summary>
        /// Set the value for the protected attributeList field
        /// </summary>
        /// <param name="itemActionCollection">The item action collection.</param>
        /// <param name="newList">The new list.</param>
        public static void SetItemActions(ItemActionCollection itemActionCollection, ResizableArray<ItemAction> newList)
        {
            itemActionCollection.ItemActions = newList;
        }

        /// <summary>
        /// Set the value for the protected attributeList field
        /// </summary>
        /// <param name="itemAction">The item action.</param>
        /// <param name="newName">The new name.</param>
        public static void SetName(ItemAction itemAction, string newName)
        {
            itemAction.Name = newName;
        }

        /// <summary>
        /// Serialize And set Dirty.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="itemActionCollection">The item action collection.</param>
        public static void SerializeItemActionsAndDirty(UnityEngine.Object target, ItemActionCollection itemActionCollection)
        {
            SerializeItemActions(itemActionCollection);
            Shared.Editor.Utility.EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// Serialize all of the attributes to the collection.
        /// </summary>
        /// <param name="itemAttributeCollection">The character to serialize.</param>
        public static void SerializeItemActions(ItemActionCollection itemAttributeCollection)
        {
            itemAttributeCollection.Serialize();
        }

    }
}
