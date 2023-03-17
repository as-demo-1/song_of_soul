/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.SaveSystem
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using System;
    using UnityEngine.Serialization;

    /// <summary>
    /// A struct of an ID and an amount, used to save amounts of an object with an ID.
    /// </summary>
    [Serializable]
    public struct IDAmountSaveData
    {
        public uint ID;
        public int Amount;

        public IDAmountSaveData(uint id, int amount)
        {
            ID = id;
            Amount = amount;
        }
    }
    
    /// <summary>
    /// A struct of an ID and an amount, used to save amounts of an object with an ID.
    /// </summary>
    [Serializable]
    public struct ItemStackSaveData
    {
        public uint ItemID;
        public int Amount;
        public int CollectionHash;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <param name="amount">The item amount.</param>
        /// <param name="collectionHash">The item collection hash.</param>
        public ItemStackSaveData(uint id, int amount, int collectionHash)
        {
            ItemID = id;
            Amount = amount;
            CollectionHash = collectionHash;
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="itemStack">The item stack.</param>
        public ItemStackSaveData(ItemStack itemStack)
        {
            if (itemStack == null) {
                ItemID = 0;
                Amount = -1;
                CollectionHash = -1;
                return;
            }
            
            ItemID = itemStack.Item?.ID ?? 0;
            Amount = itemStack.Amount;
            CollectionHash = itemStack.ItemCollection?.Name?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Check if an item stack matches the save data.
        /// </summary>
        /// <param name="itemStack">The item stack.</param>
        /// <returns>True if the save data match the item stack.</returns>
        public bool Match(ItemStack itemStack)
        {
            if (itemStack == null) { return false; }

            if (itemStack.Item == null) { return false; }

            if (itemStack.Item.ID != ItemID) { return false; }

            if (itemStack.Amount != Amount) { return false; }
            
            if (GetCollectionHash(itemStack.ItemCollection) != CollectionHash) { return false; }

            return true;
        }

        /// <summary>
        /// Get the collection hash.
        /// </summary>
        /// <param name="itemCollection">The item collection.</param>
        /// <returns></returns>
        public int GetCollectionHash(ItemCollection itemCollection)
        {
            return itemCollection?.Name?.GetHashCode() ?? 0;
        }
    }
}