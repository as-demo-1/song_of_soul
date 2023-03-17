/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Keeps track of the item indexes within the grid.
    /// </summary>
    public class InventoryGridIndexer
    {
        protected List<ItemInfo> m_CachedItemInfos;
        protected List<ItemInfo> m_TempUnsetItemInfos;
        protected Dictionary<ItemStack, (int index,ItemInfo itemInfo)> m_IndexedItems;

        protected Dictionary<ItemStack, (int index,ItemInfo itemInfo)> m_RemovedDirtyIndexedItems;

        public Dictionary<ItemStack, (int index,ItemInfo itemInfo)> IndexedItems => m_IndexedItems;

        /// <summary>
        /// Constructor.
        /// </summary>
        public InventoryGridIndexer()
        {
            m_CachedItemInfos = new List<ItemInfo>();
            m_TempUnsetItemInfos = new List<ItemInfo>();
            m_IndexedItems = new Dictionary<ItemStack, (int index,ItemInfo itemInfo)>();
            m_RemovedDirtyIndexedItems = new Dictionary<ItemStack, (int index, ItemInfo itemInfo)>();
        }

        /// <summary>
        /// Copy the indexes from another inventory grid indexer.
        /// </summary>
        /// <param name="other">The other indexer.</param>
        public virtual void Copy(InventoryGridIndexer other)
        {
            if (other == null) {
                Clear();
                return;
            }
            SetIndexItems(other.IndexedItems);
        }

        /// <summary>
        /// Clear the indexes.
        /// </summary>
        public virtual void Clear()
        {
            m_IndexedItems.Clear();
        }

        /// <summary>
        /// Set the items indexes.
        /// </summary>
        /// <param name="indexedItems">Dictionary of item stacks and ints.</param>
        public virtual void SetIndexItems(Dictionary<ItemStack, (int index,ItemInfo itemInfo)> indexedItems)
        {
            //Debug.Log("Set indexer "+indexedItems);

            if (indexedItems == null) { return; }
            m_IndexedItems.Clear();

            foreach (var indexedItem in indexedItems) {
                //Debug.Log("Indexing "+indexedItem.Key+" to index "+indexedItem.Value);
                m_IndexedItems[indexedItem.Key] = indexedItem.Value;
            }
        }

        /// <summary>
        /// Set the item stack to an index.
        /// </summary>
        /// <param name="itemStack">The item stack.</param>
        /// <param name="itemStackIndex">The index.</param>
        public virtual void SetStackIndex(ItemStack itemStack, int itemStackIndex)
        {
            m_IndexedItems[itemStack] = (itemStackIndex, (ItemInfo)itemStack);
        }

        /// <summary>
        /// Get the item stack index (-1 if it does not exist).
        /// </summary>
        /// <param name="itemStack">The item stack.</param>
        /// <returns>The index of the item stack.</returns>
        public virtual int GetItemStackIndex(ItemStack itemStack)
        {
            if (m_IndexedItems.TryGetValue(itemStack, out var result)) { return result.index; }

            return -1;
        }

        /// <summary>
        /// Get the item stack at the index.
        /// </summary>
        /// <param name="itemStackIndex">The index.</param>
        /// <returns>The item stack.</returns>
        public virtual ItemStack GetItemStackAtIndex(int itemStackIndex)
        {
            foreach (var keyValuePair in m_IndexedItems) {
                if (keyValuePair.Value.index == itemStackIndex) { return keyValuePair.Key; }
            }

            return null;
        }

        /// <summary>
        /// Can the item move from one index to another.
        /// </summary>
        /// <param name="sourceIndex">The source index.</param>
        /// <param name="destinationIndex">The destination index.</param>
        /// <returns>True if the index can change.</returns>
        public virtual bool CanMoveItem(int sourceIndex, int destinationIndex)
        {
            return true;
        }

        /// <summary>
        /// Move the items.
        /// </summary>
        /// <param name="sourceStack">The source item stack.</param>
        /// <param name="destinationStack">The destination item stack.</param>
        public virtual void MoveItemStackIndex(ItemStack sourceStack, ItemStack destinationStack)
        {
            var sourceIndex = GetItemStackIndex(sourceStack);
            var destinationIndex = GetItemStackIndex(destinationStack);

            if (sourceIndex != -1) { m_IndexedItems[destinationStack] = (sourceIndex, (ItemInfo)destinationStack); }
            if (destinationIndex != -1) { m_IndexedItems[sourceStack] = (destinationIndex, (ItemInfo)sourceStack); }
        }

        /// <summary>
        /// Move items.
        /// </summary>
        /// <param name="sourceIndex">The source index.</param>
        /// <param name="destinationIndex">The destination index.</param>
        public virtual void MoveItemStackIndex(int sourceIndex, int destinationIndex)
        {
            var sourceStack = GetItemStackAtIndex(sourceIndex);
            var destinationStack = GetItemStackAtIndex(destinationIndex);

            if (destinationStack != null) {
                m_IndexedItems[destinationStack] = (sourceIndex, (ItemInfo)destinationStack);
            }

            if (sourceStack != null) {
                m_IndexedItems[sourceStack] = (destinationIndex, (ItemInfo)sourceStack);
            }
        }

        /// <summary>
        /// Sort the indexes using a comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <returns>The sorted item indexes.</returns>
        public virtual ListSlice<ItemInfo> SortItemIndexes(Comparer<ItemInfo> comparer)
        {
            var indexedItems = m_IndexedItems;

            m_CachedItemInfos.Clear();

            var itemInfos = indexedItems.Keys;
            foreach (var itemInfo in itemInfos) {
                m_CachedItemInfos.Add((ItemInfo)itemInfo);
            }

            m_CachedItemInfos.Sort(comparer);

            for (int i = 0; i < m_CachedItemInfos.Count; i++) {
                var itemStack = m_CachedItemInfos[i].ItemStack;
                if (itemStack == null) { continue; }

                indexedItems[itemStack] = (i, (ItemInfo)itemStack);
            }

            return m_CachedItemInfos;
        }

        /// <summary>
        /// Get the ordered items using the indexes.
        /// </summary>
        /// <param name="itemInfos">The item info.</param>
        /// <returns>The sorted items using their indexes.</returns>
        public virtual ListSlice<ItemInfo> GetOrderedItems(ListSlice<ItemInfo> itemInfos)
        {
            var pooledOrderedItemInfos = GenericObjectPool.Get<List<ItemInfo>>();
            pooledOrderedItemInfos.Clear();

            // Find the item stacks to remove the item stacks from the dictionary if they are not part of the input item infos.
            var pooledItemStacksToRemove = GenericObjectPool.Get<List<ItemStack>>();
            pooledItemStacksToRemove.Clear();

            foreach (var indexedItem in m_IndexedItems) {
                var atLeastOneMatch = false;
                var itemStack = indexedItem.Key;
                var index = indexedItem.Value.index;
                var itemInfo = indexedItem.Value.itemInfo;
                
                for (int i = 0; i < itemInfos.Count; i++) {
                    if (itemInfos[i].ItemStack != itemStack) { continue; }
                    //The item stacks might match but the items contained within might not
                    if (itemInfos[i].ItemStack.Item != itemInfo.Item) {
                        continue;
                    }

                    atLeastOneMatch = true;
                    break;
                }

                if (atLeastOneMatch == false) {
                    pooledItemStacksToRemove.Add(itemStack);

                    if (index < 0 || index >= m_CachedItemInfos.Count) {
                        continue;
                    }
                    m_RemovedDirtyIndexedItems.Add(itemStack, (index, m_CachedItemInfos[index]));
                }
            }

            // Find if the item has an existing index.
            for (int i = 0; i < itemInfos.Count; i++) {
                var itemInfo = itemInfos[i];

                var stackIndex = -1;
                (int index, ItemInfo itemInfo) result = (stackIndex, ItemInfo.None);
                var itemIsNotIndexed =
                    itemInfo.ItemStack == null
                    || !m_IndexedItems.TryGetValue(itemInfo.ItemStack, out result)
                    || result.index < 0
                    || result.itemInfo.Item != itemInfo.Item;
                    
                stackIndex = result.index;

                if (itemIsNotIndexed) {
                    m_TempUnsetItemInfos.Add(itemInfo);
                    continue;
                }

                if (stackIndex >= pooledOrderedItemInfos.Count) {
                    pooledOrderedItemInfos.EnsureSize(stackIndex + 1);
                }

                if (pooledOrderedItemInfos[stackIndex].ItemStack == null) {
                    pooledOrderedItemInfos[stackIndex] = itemInfo;
                } else {
                    m_TempUnsetItemInfos.Add(itemInfo);
                }
            }
            
            // Remove the item stacks from the dictionary if they are not part of the input item infos.
            for (int i = 0; i < pooledItemStacksToRemove.Count; i++) { m_IndexedItems.Remove(pooledItemStacksToRemove[i]); }
            pooledItemStacksToRemove.Clear();
            GenericObjectPool.Return(pooledItemStacksToRemove);

            //Find if the item had an existing index
            foreach (var removedDirtyIndexedItem in m_RemovedDirtyIndexedItems) {
                var previousItemStack = removedDirtyIndexedItem.Key;
                var previousIndex = removedDirtyIndexedItem.Value.index;
                var previousItemInfo = removedDirtyIndexedItem.Value.itemInfo;
                
                for (int i = m_TempUnsetItemInfos.Count - 1; i >= 0; i--) {
                    var itemInfo = m_TempUnsetItemInfos[i];

                    if(previousItemInfo.Item != itemInfo.Item){ continue; }
                
                    if (previousIndex >= pooledOrderedItemInfos.Count) {
                        pooledOrderedItemInfos.EnsureSize(previousIndex + 1);
                    }
                
                    pooledOrderedItemInfos[previousIndex] = itemInfo;
                    m_IndexedItems[itemInfo.ItemStack] = (previousIndex, (ItemInfo)itemInfo.ItemStack);
                    m_TempUnsetItemInfos.RemoveAt(i);
                    break;
                }
            }
            
            // Assign a new index to any item which does not yet have one.
            var count = 0;
            for (int i = m_TempUnsetItemInfos.Count - 1; i >= 0; i--) {
                var itemInfo = m_TempUnsetItemInfos[i];
                if (itemInfo.ItemStack == null) { continue; }

                var indexIsSet = false;
                
                for (int j = count; j < pooledOrderedItemInfos.Count; j++) {

                    count++;
                    if (pooledOrderedItemInfos[j].ItemStack != null) { continue; }
                    if (itemInfo.ItemStack == null) { break; }


                    pooledOrderedItemInfos[j] = itemInfo;
                    SetStackIndex(itemInfo.ItemStack, j);
                    indexIsSet = true;
                    break;
                }
                
                if (indexIsSet) { continue; }

                pooledOrderedItemInfos.Add(itemInfo);
                SetStackIndex(itemInfo.ItemStack, count);
                count++;
            }
            m_TempUnsetItemInfos.Clear();
            
            //Cache the list of item Infos.
            m_CachedItemInfos.Clear();
            for (int i = 0; i < pooledOrderedItemInfos.Count; i++) {
                m_CachedItemInfos.Add(pooledOrderedItemInfos[i]);
            }
            
            pooledOrderedItemInfos.Clear();
            GenericObjectPool.Return(pooledOrderedItemInfos);

            return m_CachedItemInfos;
        }

        /// <summary>
        /// Cleanup some data such that the cached indezes can be removed.
        /// We do not cleanup automatically in case an item moves multiple time within one frame.
        /// Example when the item move collection it is removed and then added, which used to forget the index.
        /// </summary>
        public void Cleanup()
        {
            m_RemovedDirtyIndexedItems.Clear();
        }
    }
}