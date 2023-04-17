/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// An ItemCollection that can have multiple stacks of an item.
    /// </summary>
    [Serializable]
    public class MultiStackItemCollection : ItemCollection
    {
        [Tooltip("The stack limit for a stack of an immutable item.")]
        [SerializeField] protected int m_DefaultStackSizeLimit = 99;
        [Tooltip("The attribute name used for limiting the stack size for an immutable item.")]
        [SerializeField] protected string m_StackSizeLimitAttributeName = "StackSizeLimit";

        public int DefaultStackSizeLimit {
            get { return m_DefaultStackSizeLimit; }
            set { m_DefaultStackSizeLimit = value; }
        }

        public string StackSizeLimitAttributeName {
            get { return m_StackSizeLimitAttributeName; }
            set { m_StackSizeLimitAttributeName = value; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public MultiStackItemCollection()
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stackSizeLimit">The stack size limit.</param>
        /// <param name="stackSizeLimitAttributeName">The stack size limit attribute name.</param>
        public MultiStackItemCollection(int stackSizeLimit, string stackSizeLimitAttributeName)
        {
            m_DefaultStackSizeLimit = stackSizeLimit;
            m_StackSizeLimitAttributeName = stackSizeLimitAttributeName;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stackSizeLimit">The stack size limit.</param>
        public MultiStackItemCollection(int stackSizeLimit)
        {
            m_DefaultStackSizeLimit = stackSizeLimit;
            m_StackSizeLimitAttributeName = "";
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="stackSizeLimitAttributeName">The stack size limit attribute name.</param>
        public MultiStackItemCollection(string stackSizeLimitAttributeName)
        {
            m_DefaultStackSizeLimit = int.MaxValue;
            m_StackSizeLimitAttributeName = stackSizeLimitAttributeName;
        }

        /// <summary>
        /// Get the last valid item info in the item collection.
        /// This is required to pass some unit tests, as it will remove Mutable Common items from the last stack and not the first.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The itemInfo.</returns>
        public override ItemInfo? GetItemInfo(Item item)
        {
            if (item == null) { return null; }

            ItemInfo? similarItemInfo = null;

            for (int i = m_ItemStacks.Count - 1; i >= 0; i--) {
                if (!item.IsUnique && m_ItemStacks[i].Item.ItemDefinition == item.ItemDefinition) {
                    similarItemInfo = (ItemInfo)m_ItemStacks[i];
                }

                if (m_ItemStacks[i].Item.ID != item.ID) { continue; }

                return (ItemInfo)m_ItemStacks[i];
            }

            return similarItemInfo;
        }

        /// <summary>
        /// Internal method which removes an ItemAmount from the collection.
        /// </summary>
        /// <param name="itemInfo">The item info to remove.</param>
        /// <returns>Returns the number of items removed, 0 if no item was removed.</returns>
        protected override ItemInfo RemoveInternal(ItemInfo itemInfo)
        {
            var removed = 0;
            var amountToRemove = itemInfo.Amount;
            var previousIndexWithItem = -1;
            var maxStackSize = GetMaxStackSize(itemInfo.Item);
            ItemStack itemStackRemoved = null;

            if (m_ItemStacks.Contains(itemInfo.ItemStack)) {
                itemStackRemoved = itemInfo.ItemStack;
                previousIndexWithItem = RemoveItemFromStack(m_ItemStacks.IndexOf(itemStackRemoved),
                    itemInfo, previousIndexWithItem, maxStackSize, ref amountToRemove, ref removed);
            }

            for (int i = m_ItemStacks.Count - 1; i >= 0; i--) {
                if (amountToRemove <= 0) { break; }

                if (m_ItemStacks[i].Item.ID != itemInfo.Item.ID) { continue; }
                if (itemStackRemoved == m_ItemStacks[i]) { continue; }

                itemStackRemoved = m_ItemStacks[i];

                previousIndexWithItem = RemoveItemFromStack(i, itemInfo, previousIndexWithItem,
                    maxStackSize, ref amountToRemove, ref removed);
            }

            if (removed == 0) {
                return (removed, itemInfo.Item, this);
            }

            if (m_Inventory != null) {
                EventHandler.ExecuteEvent<ItemInfo>(m_Inventory, EventNames.c_Inventory_OnRemove_ItemInfo,
                    (itemInfo.Item, removed, this, itemStackRemoved));
            }

            UpdateCollection();

            return (removed, itemInfo.Item, this, itemStackRemoved);
        }

        /// <summary>
        /// Removes an item amount from a specific stack.
        /// </summary>
        /// <param name="i">The index of the item stack.</param>
        /// <param name="itemInfo">The item info to remove.</param>
        /// <param name="previousIndexWithItem">The previous Index with the item.</param>
        /// <param name="maxStackSize">The max stack size.</param>
        /// <param name="amountToRemove">The amount to remove.</param>
        /// <param name="removed">The amount removed.</param>
        /// <returns>The index with the item.</returns>
        private int RemoveItemFromStack(int i, ItemInfo itemInfo, int previousIndexWithItem,
            int maxStackSize, ref int amountToRemove, ref int removed)
        {
            var itemStackRemoved = m_ItemStacks[i];

            if (previousIndexWithItem != -1) {
                var mergedAmount = itemStackRemoved.Amount + m_ItemStacks[previousIndexWithItem].Amount;
                if (mergedAmount > maxStackSize) {
                    m_ItemStacks[previousIndexWithItem].SetAmount(maxStackSize);
                    itemStackRemoved.SetAmount(mergedAmount - maxStackSize);
                } else {
                    m_ItemStacks[previousIndexWithItem].SetAmount(mergedAmount);
                    itemStackRemoved.SetAmount(0);
                }
            }

            if (amountToRemove == 0) { return previousIndexWithItem; }

            var newAmount = itemStackRemoved.Amount - amountToRemove;
            if (newAmount <= 0) {
                amountToRemove = -newAmount;
                removed += itemStackRemoved.Amount;
                m_ItemStacks.RemoveAt(i);
                itemInfo.Item.RemoveItemCollection(this);
                itemStackRemoved.Reset();
                GenericObjectPool.Return<ItemStack>(itemStackRemoved);
            } else {
                removed += amountToRemove;
                amountToRemove = 0;
                itemStackRemoved.SetAmount(newAmount);
                previousIndexWithItem = i;
            }

            return previousIndexWithItem;
        }

        /// <summary>
        /// Returns the number of items that could fit if X amount of item was added and that only a certain of new stacks where allowed to be created.
        /// </summary>
        /// <param name="itemInfo">itemInfo containing the item and amount that needs to fit.</param>
        /// <param name="availableAdditionalStacks">The additional ItemStacks which can be added to fit those items.</param>
        /// <returns>Return the amount of item that can fit.</returns>
        public override int GetItemAmountFittingInLimitedAdditionalStacks(ItemInfo itemInfo, int availableAdditionalStacks)
        {
            var amountToAdd = itemInfo.Amount;

            var maxStackSize = GetMaxStackSize(itemInfo.Item);

            for (int i = 0; i < m_ItemStacks.Count; i++) {
                var itemStack = m_ItemStacks[i];
                if (CanItemStack(itemInfo, itemStack) == false) { continue; }

                if (itemStack.Amount == maxStackSize) { continue; }

                var totalToSet = itemStack.Amount + amountToAdd;
                var sizeDifference = totalToSet - maxStackSize;

                if (sizeDifference <= 0) {
                    amountToAdd = 0;
                    break;
                }

                amountToAdd = sizeDifference;
            }

            var stacksToAdd = amountToAdd / maxStackSize;
            var remainderStack = amountToAdd % maxStackSize;


            if (availableAdditionalStacks > stacksToAdd) {
                return itemInfo.Amount;
            }
            if (availableAdditionalStacks == stacksToAdd) {
                return itemInfo.Amount - remainderStack;
            }

            return itemInfo.Amount - remainderStack - maxStackSize * (stacksToAdd - availableAdditionalStacks);
        } 

        /// <summary>
        /// Add an ItemAmount in an organized way.
        /// </summary>
        /// <param name="itemInfo">The item info being added to the item collection.</param>
        /// <param name="targetStack">The item stack where the item should be added to (Can be null).</param>
        protected override ItemInfo AddInternal(ItemInfo itemInfo, ItemStack targetStack, bool notifyAdd = true)
        {
            var amountToAdd = itemInfo.Amount;

            var maxStackSize = GetMaxStackSize(itemInfo.Item);
            ItemStack addedItemStack = null;

            if (m_ItemStacks.Contains(targetStack)) {
                addedItemStack = targetStack;
                var amountAdded = IncreaseStackAmount(targetStack, maxStackSize, ref amountToAdd);
                if (amountAdded > 0 && notifyAdd) {
                    NotifyAdd((amountAdded, itemInfo), addedItemStack);
                }
            }

            for (int i = 0; i < m_ItemStacks.Count; i++) {
                var itemStack = m_ItemStacks[i];
                if (CanItemStack(itemInfo, itemStack) == false) { continue; }
                addedItemStack = itemStack;

                var amountAdded = IncreaseStackAmount(itemStack, maxStackSize, ref amountToAdd);
                if (amountAdded > 0 && notifyAdd) {
                    NotifyAdd((amountAdded, itemInfo), addedItemStack);
                }
            }
            
            itemInfo.Item.AddItemCollection(this);

            var stacksToAdd = amountToAdd / maxStackSize;
            var remainderStack = amountToAdd % maxStackSize;

            for (int i = 0; i < stacksToAdd; i++) {
                var newItemStack = GenericObjectPool.Get<ItemStack>();
                newItemStack.Initialize((itemInfo.Item, maxStackSize), this);
                addedItemStack = newItemStack;
                m_ItemStacks.Add(newItemStack);
                if (notifyAdd) {
                    NotifyAdd((addedItemStack.Amount, itemInfo), addedItemStack);
                }
            }

            if (remainderStack != 0) {
                var newItemStack = GenericObjectPool.Get<ItemStack>();
                newItemStack.Initialize((itemInfo.Item, remainderStack), this);
                addedItemStack = newItemStack;
                m_ItemStacks.Add(newItemStack);
                if (notifyAdd) {
                    NotifyAdd((addedItemStack.Amount, itemInfo), addedItemStack);
                }
            }

            return (itemInfo.Item, itemInfo.Amount, this, addedItemStack);
        }

        /// <summary>
        /// Increase the stack amount.
        /// </summary>
        /// <param name="targetItemStack">The item stack to increase the amount from.</param>
        /// <param name="maxStackSize">The max stack size.</param>
        /// <param name="amountToAdd">The amount to add.</param>
        /// <returns>The amount added to the stack.</returns>
        private int IncreaseStackAmount(ItemStack targetItemStack, int maxStackSize, ref int amountToAdd)
        {
            if (targetItemStack.Amount == maxStackSize) { return 0; }

            var originalAmountToAdd = amountToAdd;
            var totalToSet = targetItemStack.Amount + amountToAdd;
            var sizeDifference = totalToSet - maxStackSize;

            if (sizeDifference <= 0) {
                targetItemStack.SetAmount(totalToSet);
                amountToAdd = 0;
            } else {
                targetItemStack.SetAmount(maxStackSize);
                amountToAdd = sizeDifference;
            }

            var amountAdded = originalAmountToAdd - amountToAdd;

            return amountAdded;
        }

        /// <summary>
        /// Get the max stack size for the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The maximum stack size.</returns>
        protected virtual int GetMaxStackSize(Item item)
        {
            if (item.TryGetAttributeValue<int>(m_StackSizeLimitAttributeName, out var maxStackSize) == false) {
                maxStackSize = m_DefaultStackSizeLimit;
            }

            return maxStackSize;
        }
    }
}