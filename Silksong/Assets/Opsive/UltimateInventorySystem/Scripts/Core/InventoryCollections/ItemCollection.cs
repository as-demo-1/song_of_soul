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
    using System.Collections.Generic;
    using Opsive.UltimateInventorySystem.Utility;
    using UnityEngine;
    using EventHandler = Shared.Events.EventHandler;

    /// <summary>
    /// The purpose of the Item Collection.
    /// </summary>
    public enum ItemCollectionPurpose
    {
        None,       // No specified purpose.
        Main,       // Default collection in an inventory.
        Secondary,  // Custom use. Used by the Ultimate Character Controller integration. If you are using this integration do not use this purpose for your own use.
        Equipped,   // The included items have been equipped. Used by the Ultimate Character Controller integration. If you are using this integration do not use this purpose for your own use.
        Loadout,    // The collection is used for a loadout. The items won't show in the UI. Used by the Ultimate Character Controller integration. If you are using this integration do not use this purpose for your own use.
        Hide,       // Hides collections won't show in the UI.
        Drop        // Used to drop items.
    }

    /// <summary>
    /// ItemCollection Identifier.
    /// </summary>
    [Serializable]
    public struct ItemCollectionID
    {
        [Tooltip("The item collection ID.")]
        [SerializeField] private string m_Name;

        [Tooltip("The purpose of the item collection.")]
        [SerializeField] private ItemCollectionPurpose m_Purpose;

        public string Name => m_Name;
        public ItemCollectionPurpose Purpose => m_Purpose;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="purpose">The purpose.</param>
        public ItemCollectionID(string name, ItemCollectionPurpose purpose)
        {
            m_Name = name;
            m_Purpose = purpose;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="itemCollection">The itemCollection.</param>
        public ItemCollectionID(ItemCollection itemCollection)
        {
            if (itemCollection != null) {
                m_Name = itemCollection.Name;
                m_Purpose = itemCollection.Purpose;
            } else {
                m_Name = "";
                m_Purpose = ItemCollectionPurpose.None;
            }
        }

        /// <summary>
        /// Compare the ItemCollection with the Id to see if it matches.
        /// </summary>
        /// <param name="itemCollection">The itemCollection.</param>
        /// <returns>True if it matches.</returns>
        public bool Compare(ItemCollection itemCollection)
        {
            if (itemCollection == null) { return false; }

            if (string.IsNullOrWhiteSpace(m_Name) == false && itemCollection.Name == m_Name) { return true; }

            if (m_Purpose == ItemCollectionPurpose.None) { return false; }

            return m_Purpose == itemCollection.Purpose;
        }

        /// <summary>
        /// Compare the ItemCollection with the Id to see if it matches.
        /// </summary>
        /// <param name="itemCollection">The itemCollection.</param>
        /// <returns>0 -> not matched, 1 -> purpose match, 2 -> name match, 3 -> name and purpose match.</returns>
        public int CompareWeighted(ItemCollection itemCollection)
        {
            var weight = 0;

            if (itemCollection == null) {
                return 0;
            }

            if (string.IsNullOrWhiteSpace(m_Name) == false && itemCollection.Name == m_Name) {
                weight += 2;
            }

            if (m_Purpose != ItemCollectionPurpose.None && m_Purpose == itemCollection.Purpose) { weight += 1; }

            return weight;
        }

        public static implicit operator ItemCollectionID(string x)
            => new ItemCollectionID(x, ItemCollectionPurpose.None);
        public static implicit operator ItemCollectionID(ItemCollectionPurpose x)
            => new ItemCollectionID(null, x);
    }

    [Serializable]
    public struct ItemOverflowOptions
    {
        [Tooltip("When an item does not fit in the inventory, should it be returned where it came form (if specified)?")]
        [SerializeField] private bool m_ReturnOverflow;
        [Tooltip("Send event")]
        [SerializeField] private bool m_InvokeRejectedEvent;
        [Tooltip("Send event")]
        [SerializeField] private ItemOverflowAction m_OverflowAction;
        
        public bool ReturnOverflow { get => m_ReturnOverflow; set => m_ReturnOverflow = value; }
        public bool InvokeRejectedEvent { get => m_InvokeRejectedEvent; set => m_InvokeRejectedEvent = value; }
        public ItemOverflowAction OverflowAction { get => m_OverflowAction; set => m_OverflowAction = value; }
    }

    /// <summary>
    /// The ItemCollection is used to store a collection of item amounts in an organized way.
    /// This class is generic and can be extended to fit your particular needs.
    /// </summary>
    [Serializable]
    public class ItemCollection
    {
        public event Action OnItemCollectionUpdate;

        [Tooltip("The name of the Item Collection, so that it can be found in an inventory.")]
        [SerializeField] protected string m_Name;
        [Tooltip("The purpose of the Item Collection.")]
        [SerializeField] protected ItemCollectionPurpose m_Purpose;
        [Tooltip("When an item does not fit in the inventory, should it be returned where it came form (if specified)?")]
        [SerializeField] protected ItemOverflowOptions m_OverflowOptions = new ItemOverflowOptions(){ InvokeRejectedEvent = true };
        [Tooltip("The default loadout is added to the collection when it is initialize.")]
        [SerializeField] protected ItemAmounts m_DefaultLoadout;

        protected bool m_UpdateEventDisabled;

        [System.NonSerialized] protected bool m_Initialized = false;
        [System.NonSerialized] protected List<ItemStack> m_ItemStacks;
        [System.NonSerialized] protected IInventory m_Inventory;

        public string Name => m_Name;
        public bool IsInitialized => m_Initialized;
        public IInventory Inventory => m_Inventory;

        public bool UpdateEventDisabled { get => m_UpdateEventDisabled; set => m_UpdateEventDisabled = value; }
        public ItemCollectionPurpose Purpose { get => m_Purpose; internal set => m_Purpose = value; }

        public ItemAmounts DefaultLoadout { get => m_DefaultLoadout; set => m_DefaultLoadout = value; }
        internal List<ItemStack> ItemStacks => m_ItemStacks;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ItemCollection()
        {
            m_Name = "NewItemCollection";
        }

        /// <summary>
        /// Initializes the Item Collection.
        /// </summary>
        /// <param name="owner">The GameObject doing the initialization.</param>
        /// <param name="force">Should the collection be force initialized?</param>
        public virtual void Initialize(IInventory owner, bool force)
        {
            if (m_Initialized && !force && m_Inventory != null) { return; }
            m_Inventory = owner;

            if (m_ItemStacks == null) {
                m_ItemStacks = new List<ItemStack>();
            }
            m_Initialized = true;

            if (Application.isPlaying == false || !force) { return; }

            m_ItemStacks.Clear();
        }

        /// <summary>
        /// Duplicate an item with the same attributes but a different ID.
        /// </summary>
        /// <param name="item">The item to duplicate.</param>
        /// <returns>The duplicated item.</returns>
        protected virtual Item DuplicateItem(Item item)
        {
            if (item == null) { return null; }

            return InventorySystemManager.CreateItem(item);
        }

        /// <summary>
        /// Loads the items within the Item Collection.
        /// </summary>
        public virtual void LoadDefaultLoadout()
        {
            if (m_DefaultLoadout == null) { return; }

            for (int i = 0; i < m_DefaultLoadout.Count; i++) {
                if (m_DefaultLoadout[i].Amount == 0) {
                    continue;
                }

                if (m_DefaultLoadout[i].Item == null || m_DefaultLoadout[i].Item.ItemDefinition == null) {
                    if (Application.isPlaying) {
                        Debug.LogWarning($"Cannot add Item {m_DefaultLoadout[i].Item} with definition " +
                                         $"({(m_DefaultLoadout[i].Item == null ? null : m_DefaultLoadout[i].Item.ItemDefinition)}) " +
                                         $"with quantity {m_DefaultLoadout[i].Amount}, {m_Inventory}.");
                    }
                    continue;
                }

                m_DefaultLoadout[i].Item.Initialize(false);

                AddItem(DuplicateItem(m_DefaultLoadout[i].Item), m_DefaultLoadout[i].Amount);
            }
        }

        /// <summary>
        /// Set the name of the Item Collection.
        /// </summary>
        /// <param name="newName">The new Item Collection name.</param>
        public void SetName(string newName)
        {
            m_Name = newName;
        }

        /// <summary>
        /// Call the events when the collection was changed.
        /// </summary>
        public virtual void UpdateCollection(bool force = false)
        {
            if(m_UpdateEventDisabled && !force){ return; }
            
            EventHandler.ExecuteEvent(this, EventNames.c_ItemCollection_OnUpdate);
            OnItemCollectionUpdate?.Invoke();
        }

        #region HasRegion

        /// <summary>
        /// Determines if the Item Collection contains the item stack.
        /// </summary>
        /// <param name="itemStack">The item stackto check.</param>
        /// <returns>Returns true if that exact stack exists in this collection.</returns>
        public virtual bool ContainsItemStack(ItemStack itemStack)
        {
            if (itemStack == null) { return false; }

            for (int i = 0; i < m_ItemStacks.Count; i++) {
                if (m_ItemStacks[i] == itemStack) { return true; }
            }
            return false;
        }
            
        /// <summary>
        /// Determines if the Item Collection contains the item.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="similarItem">Find an itemInfo with that value equivalent item or just one similar?</param>
        /// <returns>Returns true if the amount of items in the collection is equal or bigger than the amount specified.</returns>
        public virtual bool HasItem(Item item, bool similarItem = true)
        {
            if (item == null) { return false; }

            return GetItemAmount(item,similarItem) >= 1;
        }

        /// <summary>
        /// Determines if the Item Collection contains the item.
        /// </summary>
        /// <param name="itemAmount">The item to check.</param>
        /// <param name="similarItem">Find an itemInfo with that value equivalent item or just one similar?</param>
        /// <returns>Returns true if the amount of items in the collection is equal or bigger than the amount specified.</returns>
        public virtual bool HasItem(ItemAmount itemAmount, bool similarItem = true)
        {
            if (itemAmount.Item == null) { return false; }

            return GetItemAmount(itemAmount.Item,similarItem) >= itemAmount.Amount;
        }

        /// <summary>
        /// Determines if the Item Collection contains the item.
        /// </summary>
        /// <param name="itemInfo">The item to check.</param>
        /// <param name="similarItem">Find an itemInfo with that value equivalent item or just one similar?</param>
        /// <returns>Returns true if the amount of items in the collection is equal or bigger than the amount specified.</returns>
        public virtual bool HasItem(ItemInfo itemInfo, bool similarItem = true)
        {
            if (itemInfo.Item == null) { return false; }

            return GetItemAmount(itemInfo.Item,similarItem) >= itemInfo.Amount;
        }

        /// <summary>
        /// This function checks if the ItemCollection contains an item with the itemDefinition.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition of the item to check.</param>
        /// <param name="checkInherently">Take into account the children of the item definition.</param>
        /// <param name="countStacks">If true count the stacks number not the stacks amounts.</param>
        /// <returns>Returns true if the amount of items with the itemDefinition in the collection is equal or bigger than the amount specified.</returns>
        public virtual bool HasItem(ItemDefinitionAmount itemDefinitionAmount, bool checkInherently, bool countStacks = false)
        {
            if (itemDefinitionAmount.ItemDefinition == null) { return false; }

            var count = GetItemAmount(itemDefinitionAmount.ItemDefinition, checkInherently, countStacks);

            return count >= itemDefinitionAmount.Amount;
        }

        /// <summary>
        /// Checks if Item Collection contains an item with the exact same category provided (does NOT check the category children).
        /// </summary>
        /// <param name="categoryAmount">The category amount of the items being checked.</param>
        /// <param name="checkInherently">Take into account the children of the item category.</param>
        /// <param name="countStacks">If true count the stacks number not the stacks amounts.</param>
        /// <returns>Returns true if the amount of items with the Item Category in the collection is equal or bigger than the amount specified.</returns>
        public virtual bool HasItem(ItemCategoryAmount categoryAmount, bool checkInherently, bool countStacks = false)
        {
            if (categoryAmount.ItemCategory == null) { return false; }

            var count = GetItemAmount(categoryAmount.ItemCategory, checkInherently, countStacks);

            return count >= categoryAmount.Amount;
        }

        /// <summary>
        /// Checks if the collection contains a list of Item Amounts.
        /// </summary>
        /// <param name="itemAmounts">The Item amounts list to check.</param>
        /// <returns>Returns true if the collection has ALL the item amounts in the list.</returns>
        public virtual bool HasItemList(ListSlice<ItemAmount> itemAmounts)
        {
            for (var i = 0; i < itemAmounts.Count; i++) {
                var itemAmount = itemAmounts[i];
                if (HasItem(itemAmount) == false) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the collection contains a list of Item Infos.
        /// </summary>
        /// <param name="itemInfos">The Item info list to check.</param>
        /// <returns>Returns true if the collection has ALL the item amounts in the list.</returns>
        public virtual bool HasItemList(ListSlice<ItemInfo> itemInfos)
        {
            for (var i = 0; i < itemInfos.Count; i++) {
                var itemInfo = itemInfos[i];
                if (HasItem(itemInfo) == false) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Can the item stack.
        /// </summary>
        /// <param name="itemInfo">The itemInfo.</param>
        /// <param name="itemStack">The itemStack.</param>
        /// <returns>True if the item can stack.</returns>
        public virtual bool CanItemStack(ItemInfo itemInfo, ItemStack itemStack)
        {
            return itemInfo.Item.IsUnique == false && itemStack.Item.StackableEquivalentTo(itemInfo.Item);
        }

        #endregion

        #region Add&Remove

        /// <summary>
        /// Add an Item Amount in an organized way.
        /// </summary>
        /// <param name="itemInfo">The Item info being added to the item collection.</param>
        /// <param name="stackTarget">The item stack where you would like the item to be added (can be null).</param>
        protected virtual ItemInfo AddInternal(ItemInfo itemInfo, ItemStack stackTarget, bool notifyAdd = true)
        {
            //Debug.Log(Name+" ItemCollection Add item "+itemInfo);
            var found = false;
            ItemStack addedItemStack = null;

            if (itemInfo.Item.IsUnique == false) {
                if (stackTarget != null && stackTarget.ItemCollection == this
                                        && stackTarget.Item == itemInfo.Item) {

                    stackTarget.SetAmount(itemInfo.Amount + stackTarget.Amount);
                    addedItemStack = stackTarget;
                    found = true;
                }

                if (!found) {
                    for (int i = 0; i < m_ItemStacks.Count; i++) {
                        if (CanItemStack(itemInfo, m_ItemStacks[i]) == false) { continue; }

                        m_ItemStacks[i].SetAmount(itemInfo.Amount + m_ItemStacks[i].Amount);
                        addedItemStack = m_ItemStacks[i];
                        found = true;
                        break;
                    }
                }
            }

            if (!found) {
                addedItemStack = GenericObjectPool.Get<ItemStack>();
                addedItemStack.Initialize((itemInfo.Item, itemInfo.Amount), this);
                m_ItemStacks.Add(addedItemStack);
            }

            itemInfo.Item.AddItemCollection(this);

            if (notifyAdd) { NotifyAdd(itemInfo, addedItemStack); }

            var addedItemInfo = (ItemInfo)(itemInfo.Item, itemInfo.Amount, this, addedItemStack);

            //Debug.Log(Name+" ItemCollection Added item "+addedItemInfo);

            return addedItemInfo;
        }

        /// <summary>
        /// Notify that an item was added in the item collection.
        /// </summary>
        /// <param name="itemInfo">The original item info.</param>
        /// <param name="addedItemStack">the added item stack.</param>
        protected virtual void NotifyAdd(ItemInfo itemInfo, ItemStack addedItemStack)
        {
            if (m_Inventory != null && Application.isPlaying) {
                EventHandler.ExecuteEvent<ItemInfo, ItemStack>(m_Inventory,
                    EventNames.c_Inventory_OnAdd_ItemInfo_ItemStack,
                    itemInfo, addedItemStack);
            }

            UpdateCollection();
        }

        /// <summary>
        /// Add conditions, returns the itemInfo that can be added (or returns null if it cannot).
        /// </summary>
        /// <param name="itemInfo">Can this item be added.</param>
        public virtual ItemInfo? CanAddItem(ItemInfo itemInfo)
        {
            if (itemInfo.Item == null) {
                return null;
            }

            if (itemInfo.Item.IsInitialized == false) {
                Debug.LogError($"The item {itemInfo.Item} is not initialized.");
                return null;
            }

            if (itemInfo.Item.ItemDefinition.IsInitialized == false || itemInfo.Item.Category.IsInitialized == false) {
                Debug.LogError($"The item {itemInfo.Item} Item Definition {itemInfo.Item.ItemDefinition} ID[{itemInfo.Item.ItemDefinition.ID}] " +
                               $"or Item Category {itemInfo.Item.Category} ID[{itemInfo.Item.Category.ID}] are not part of the active database. " +
                               $"Please run the 'Replace Database Objects' script by right-clicking on the folder with the affected prefabs, scriptable objects, or scenes.");
                return null;
            }

            if (itemInfo.Amount < 1) {
                return null;
            }

            if (itemInfo.Item.ItemCollection != null && itemInfo.Item.ItemCollection != this && itemInfo.Item.IsMutable) {
                //The Item was duplicated, and the duplicate was added because the item is already part of another Item Collection.
                itemInfo = (DuplicateItem(itemInfo.Item), itemInfo.Amount, itemInfo);
            }

            if (itemInfo.Item.IsMutable && !itemInfo.Item.IsUnique) {
                var similarItemInfo = GetItemInfo(itemInfo.Item);
                if (similarItemInfo.HasValue) {
                    itemInfo = (itemInfo.Amount, similarItemInfo.Value);
                }
            }

            var newItemInfo = m_Inventory != null ? m_Inventory.CanAddItem(itemInfo, this) : itemInfo;

            return newItemInfo;
        }

        /// <summary>
        /// Add an item to the ItemCollection.
        /// </summary>
        /// <param name="itemInfo">The amount of item being added.</param>
        /// <param name="stackTarget">The item stack where the item should be added.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual ItemInfo AddItem(ItemInfo itemInfo, ItemStack stackTarget = null)
        {
            var canAddItemResult = CanAddItem(itemInfo);
            var canAddAmount = canAddItemResult.HasValue ? canAddItemResult.Value.Amount : 0;
            
            ItemInfo itemInfoAdded = (0, itemInfo.Item, this);
            if (canAddItemResult.HasValue) {
                var item = canAddItemResult.Value.Item;
                var itemInfoToAdd = (item, amountToAdd: canAddAmount, itemInfo);

                if (!item.IsUnique || canAddAmount <= 1) {
                    itemInfoAdded =  AddInternal(itemInfoToAdd, stackTarget);
                } else {
                    var originalResult = AddItem((item, 1, itemInfo), stackTarget);

                    for (int i = 1; i < canAddAmount; i++) { AddItem((DuplicateItem(item), 1, itemInfo), stackTarget); }

                    itemInfoAdded = (canAddAmount, originalResult);
                }
            }
            
            if (itemInfoAdded.Amount < itemInfo.Amount) {
                HandleItemOverflow(itemInfo, ref itemInfoAdded);
            }
           
            
            return itemInfoAdded;
        }

        /// <summary>
        /// Handle Items that do not fit in the Inventory after they are added to an item collection.
        /// </summary>
        /// <param name="originalItemInfo">The original Item Info that was being added.</param>
        /// <param name="itemInfoAdded">The item Info Added.</param>
        protected virtual void HandleItemOverflow(ItemInfo originalItemInfo, ref ItemInfo itemInfoAdded)
        {
            var rejectedItemInfo = new ItemInfo(originalItemInfo.Amount - itemInfoAdded.Amount, originalItemInfo);
            
            if (m_OverflowOptions.ReturnOverflow) {
                if (originalItemInfo.ItemCollection != null) {
                    var returnedItemInfo = originalItemInfo.ItemCollection.AddItem(rejectedItemInfo);
                    itemInfoAdded = (returnedItemInfo.Amount, itemInfoAdded);
                }
            }

            if (m_OverflowOptions.OverflowAction != null) {
                m_OverflowOptions.OverflowAction.HandleItemOverflow(m_Inventory, originalItemInfo, itemInfoAdded, rejectedItemInfo);
            }

            if (m_OverflowOptions.InvokeRejectedEvent) {
                //Part of the item that had to be added was rejected.
                EventHandler.ExecuteEvent<ItemInfo, ItemInfo, ItemInfo>(m_Inventory,
                    EventNames.c_Inventory_OnAddItemRejected_ItemInfoToAdd_ItemInfoAdded_ItemInfoRejected,
                    originalItemInfo,
                    itemInfoAdded,
                    rejectedItemInfo
                );
                
                EventHandler.ExecuteEvent<ItemInfo>(m_Inventory, EventNames.c_Inventory_OnRejected_ItemInfo,
                    rejectedItemInfo);
                
                EventHandler.ExecuteEvent<ItemInfo, ItemInfo, ItemInfo>(m_Inventory,
                    EventNames.c_Inventory_OnAddItemOverflow_ItemInfoToAdd_ItemInfoAdded_ItemInfoRejected,
                    originalItemInfo,
                    itemInfoAdded,
                    rejectedItemInfo
                );
            }
            
            
        }

        /// <summary>
        /// Add an item to the ItemCollection.
        /// </summary>
        /// <param name="item">The item being added.</param>
        /// <param name="amount">The amount of that item being added.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual ItemInfo AddItem(Item item, int amount = 1)
        {
            return AddItem((ItemInfo)(item, amount));
        }

        /// <summary>
        /// Add an item to the ItemCollection.
        /// </summary>
        /// <param name="itemDefinition">The item being added.</param>
        /// <param name="amount">The amount of that item being added.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual ItemInfo AddItem(ItemDefinition itemDefinition, int amount = 1)
        {
            if (itemDefinition == null) { return ItemInfo.None; }
            return AddItem((ItemInfo)(InventorySystemManager.CreateItem(itemDefinition), amount));
        }

        /// <summary>
        /// Add an item to the ItemCollection.
        /// </summary>
        /// <param name="item">The item being added.</param>
        /// <param name="amount">The amount of that item being added.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual ItemInfo AddItem(string itemName, int amount = 1)
        {
            return AddItem((ItemInfo)(InventorySystemManager.CreateItem(itemName), amount));
        }

        /// <summary>
        /// Add a list of ItemAmounts to the collection.
        /// </summary>
        /// <param name="itemAmounts">The itemAmount list to add to the collection.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual int AddItems(ListSlice<ItemAmount> itemAmounts)
        {
            var totalAdded = 0;
            for (var i = 0; i < itemAmounts.Count; i++) {
                var itemAmount = itemAmounts[i];
                totalAdded += AddItem(itemAmount.Item, itemAmount.Amount).Amount;
            }
            return totalAdded;
        }
        
        /// <summary>
        /// Add a list of ItemStacks to the collection.
        /// </summary>
        /// <param name="itemStacks">The itemStack list to add to the collection.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual int AddItems(ListSlice<ItemStack> itemStacks)
        {
            var totalAdded = 0;
            for (var i = 0; i < itemStacks.Count; i++) {
                var itemStack = itemStacks[i];
                totalAdded += AddItem((ItemInfo)itemStack).Amount;
            }
            return totalAdded;
        }

        /// <summary>
        /// Add a list of ItemInfo to the collection.
        /// </summary>
        /// <param name="itemInfos">The itemInfo list to add to the collection.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual int AddItems(ListSlice<ItemInfo> itemInfos)
        {
            var totalAdded = 0;
            for (var i = 0; i < itemInfos.Count; i++) {
                var itemInfo = itemInfos[i];
                totalAdded += AddItem(itemInfo).Amount;
            }
            return totalAdded;
        }

        /// <summary>
        /// Internal method which removes an ItemAmount from the collection.
        /// </summary>
        /// <param name="itemInfo">The item info to remove.</param>
        /// <returns>Returns the number of items removed, 0 if no item was removed.</returns>
        protected virtual ItemInfo RemoveInternal(ItemInfo itemInfo)
        {
            //Debug.Log(Name+" ItemCollection Remove item "+itemInfo);
            var removed = 0;
            ItemStack itemStackToRemove = itemInfo.ItemStack;

            if (itemStackToRemove != null && itemStackToRemove.Item.ID == itemInfo.Item.ID) {
                removed = SimpleInternalItemRemove(itemInfo, removed, itemStackToRemove);
            }

            if (removed <= itemInfo.Amount) {
                for (int i = m_ItemStacks.Count - 1; i >= 0; i--) {
                    if (m_ItemStacks[i].Item.ID != itemInfo.Item.ID) { continue; }

                    itemStackToRemove = m_ItemStacks[i];
                    removed = SimpleInternalItemRemove(itemInfo, removed, itemStackToRemove);
                    if (removed >= itemInfo.Amount) { break; }
                }
            }

            if (removed == 0) {
                return (removed, itemInfo.Item, this);
            }

            if (m_Inventory != null) {
                EventHandler.ExecuteEvent<ItemInfo>(m_Inventory, EventNames.c_Inventory_OnRemove_ItemInfo,
                    (itemInfo.Item, itemInfo.Amount, this, itemStackToRemove));
            }

            UpdateCollection();

            var removedItemInfo = (ItemInfo)(removed, itemInfo.Item, this, itemStackToRemove);

            //Debug.Log(Name+" ItemCollection Removed item "+removedItemInfo);
            return removedItemInfo;
        }

        /// <summary>
        /// Simple remove of an item from a specific item stack.
        /// </summary>
        /// <param name="itemInfo">The item Info to remove.</param>
        /// <param name="removed">The amount already removed.</param>
        /// <param name="itemStackToRemove">The item stack to remove the amount from.</param>
        /// <returns>The amount removed (includes the amount previously removed).</returns>
        private int SimpleInternalItemRemove(ItemInfo itemInfo, int removed, ItemStack itemStackToRemove)
        {
            var remainingToRemove = itemInfo.Amount - removed;
            var newAmount = itemStackToRemove.Amount - remainingToRemove;
            if (newAmount <= 0) {
                removed += itemStackToRemove.Amount;
                m_ItemStacks.Remove(itemStackToRemove);
                itemInfo.Item.RemoveItemCollection(this);
                itemStackToRemove.Reset();
                GenericObjectPool.Return<ItemStack>(itemStackToRemove);
                //TODO is making the itemStack not null ok?
                //itemStackToRemove = null;
            } else {
                removed += remainingToRemove;
                itemStackToRemove.SetAmount(newAmount);
            }

            return removed;
        }

        /// <summary>
        /// Remove an item from the itemCollection.
        /// </summary>
        /// <param name="itemInfo">The item info to remove.</param>
        /// <returns>Returns the item info that can be removed.</returns>
        public virtual ItemInfo? RemoveItemCondition(ItemInfo itemInfo)
        {
            if (itemInfo.Item == null) { return null; }

            if (itemInfo.Amount < 1) {
                return null;
            }

            if (itemInfo.Item.IsMutable && !itemInfo.Item.IsUnique) {
                var similarItemInfo = GetItemInfo(itemInfo.Item);
                if (similarItemInfo.HasValue) {
                    itemInfo = (itemInfo.Amount, similarItemInfo.Value);
                }
            }

            if (itemInfo.ItemCollection != this) { itemInfo = (itemInfo.ItemAmount, this); }

            var result = m_Inventory != null ? m_Inventory.CanRemoveItem(itemInfo) : itemInfo;

            if (result.HasValue == false) { return null; }

            itemInfo = result.Value;

            return itemInfo;
        }

        /// <summary>
        /// Remove an item from the itemCollection.
        /// </summary>
        /// <param name="itemInfo">The amount of item to remove.</param>
        /// <returns>Returns true if the item was removed correctly.</returns>
        public virtual ItemInfo RemoveItem(ItemInfo itemInfo)
        {
            var result = RemoveItemCondition(itemInfo);
            if (result.HasValue == false) { return (0, itemInfo.Item, this); }

            return RemoveInternal((result.Value));
        }

        /// <summary>
        /// Remove an item from the itemCollection.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>Returns true if the item was removed correctly.</returns>
        public virtual ItemInfo RemoveItem(Item item, int amount = 1)
        {
            return RemoveItem((item, amount, this));
        }

        /// <summary>
        /// Remove an item from the itemCollection.
        /// </summary>
        /// <param name="itemDefinition">The item to remove.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>Returns true if the item was removed correctly.</returns>
        public virtual ItemInfo RemoveItem(ItemDefinition itemDefinition, int amount = 1)
        {
            if (itemDefinition == null || amount == 0) { return ItemInfo.None; }

            // The item can be in multiple stacks, for example if it is Unique or in a multiItemStackItem.

            var amountRemoved = 0;
            var amountToRemove = amount;
            ItemInfo lastItemInfoRemoved = ItemInfo.None;
            for (int i = 0; i < amount; i++) {
                var itemInfo = GetItemInfo(itemDefinition, false);

                if (itemInfo.HasValue == false) { break; }

                lastItemInfoRemoved = RemoveItem((amountToRemove, itemInfo.Value));

                amountRemoved += lastItemInfoRemoved.Amount;
                amountToRemove = amount - amountRemoved;
                
                if (amountToRemove == 0) { break; }
            }
            
            return new ItemInfo(amountRemoved, lastItemInfoRemoved);
                
           
        }

        /// <summary>
        /// Remove an item from the itemCollection.
        /// </summary>
        /// <param name="itemName">The item to remove.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>Returns true if the item was removed correctly.</returns>
        public virtual ItemInfo RemoveItem(string itemName, int amount = 1)
        {
            return RemoveItem(InventorySystemManager.GetItemDefinition(itemName), amount);
        }

        /// <summary>
        /// Remove a list of ItemAmounts from the itemCollection.
        /// </summary>
        /// <param name="itemAmounts">The list of itemAmounts to remove.</param>
        /// <returns>Returns true if all the items in the list were removed correctly.</returns>
        public virtual int RemoveItems(ListSlice<ItemAmount> itemAmounts)
        {
            var totalRemoved = 0;
            for (var i = itemAmounts.Count - 1; i >= 0; i--) {
                var itemAmount = itemAmounts[i];
                totalRemoved += RemoveItem(itemAmount.Item, itemAmount.Amount).Amount;
            }

            return totalRemoved;
        }

        /// <summary>
        /// Remove all the items in this itemCollection and leave it completely empty.
        /// </summary>
        public virtual void RemoveAll(bool disableUpdateEventsWhileRemoving = true)
        {
            var previous = m_UpdateEventDisabled;
            if (disableUpdateEventsWhileRemoving) {
                m_UpdateEventDisabled = true;
            }
            
            for (var i = m_ItemStacks.Count - 1; i >= 0; i--) {
                var itemStack = m_ItemStacks[i];
                RemoveItem(itemStack.Item, itemStack.Amount);
            }
            
            if (disableUpdateEventsWhileRemoving) {
                m_UpdateEventDisabled = previous;
                UpdateCollection();
            }
        }

        #endregion

        #region GetReference

        /// <summary>
        /// Get the amount of item that is part of the itemCollection.
        /// </summary>
        /// <param name="item">The item to look for in the collection.</param>
        /// <param name="similarItem">Find an itemInfo with that value equivalent item or just one similar?</param>
        /// <returns>The amount of that item present in the collection.</returns>
        public virtual int GetItemAmount(Item item, bool similarItem = true)
        {
            if (item == null) { return 0; }

            var count = 0;

            for (int i = 0; i < m_ItemStacks.Count; i++) {
                if (similarItem) {
                    if (m_ItemStacks[i].Item.SimilarTo(item)) {
                        count += m_ItemStacks[i].Amount;
                    }
                } else {
                    if (m_ItemStacks[i].Item.ValueEquivalentTo(item)) {
                        count += m_ItemStacks[i].Amount;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Returns the number of items in the collection with the specified Item Definition.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the item definition.</param>
        /// <param name="countStacks">If true count the stacks number not the stacks amounts.</param>
        /// <returns>The number of items in the collection with the specified Item Definition.</returns>
        public virtual int GetItemAmount(ItemDefinition itemDefinition, bool checkInherently, bool countStacks = false)
        {
            if (itemDefinition == null) { return 0; }

            var count = 0;
            for (int i = 0; i < m_ItemStacks.Count; i++) {
                if (checkInherently) {
                    if (itemDefinition.InherentlyContains(m_ItemStacks[i].Item) == false) { continue; }
                } else {
                    if (itemDefinition.DirectlyContains(m_ItemStacks[i].Item) == false) { continue; }
                }
                count += countStacks ? 1 : m_ItemStacks[i].Amount;
            }

            return count;
        }

        /// <summary>
        /// Get the amount of item with the ItemCategory defined and its children, that is part of the itemCollection.
        /// </summary>
        /// <param name="itemCategory">The itemDefinition to look for in the collection.</param>
        /// <param name="checkInherently">Take into account the children of the item definition.</param>
        /// <param name="countStacks">If true count the stacks number not the stacks amounts.</param>
        /// <returns>The amount of items in the collection with this category present in the collection.</returns>
        public virtual int GetItemAmount(ItemCategory itemCategory, bool checkInherently, bool countStacks = false)
        {
            if (itemCategory == null) { return 0; }

            var count = 0;
            for (int i = 0; i < m_ItemStacks.Count; i++) {
                if (checkInherently) {
                    if (itemCategory.InherentlyContains(m_ItemStacks[i].Item) == false) { continue; }
                } else {
                    if (itemCategory.DirectlyContains(m_ItemStacks[i].Item) == false) { continue; }
                }
                count += countStacks ? 1 : m_ItemStacks[i].Amount;
            }

            return count;
        }

        /// <summary>
        /// Get the first valid item info in the item collection
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The itemInfo.</returns>
        public virtual ItemInfo? GetItemInfo(Item item)
        {
            if (item == null) { return null; }

            ItemInfo? similarItemInfo = null;

            for (int i = 0; i < m_ItemStacks.Count; i++) {
                if (!item.IsUnique && m_ItemStacks[i].Item.ItemDefinition == item.ItemDefinition) {
                    similarItemInfo = (ItemInfo)m_ItemStacks[i];
                }

                if (m_ItemStacks[i].Item.ID != item.ID) { continue; }

                return (ItemInfo)m_ItemStacks[i];
            }

            return similarItemInfo;
        }

        /// <summary>
        /// Returns the first itemInfo that matches the description.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the item definition.</param>
        /// <returns>The first item info found in the collection.</returns>
        public virtual ItemInfo? GetItemInfo(ItemDefinition itemDefinition, bool checkInherently = false)
        {
            if (itemDefinition == null) { return null; }

            ItemInfo? similarItemInfo = null;

            for (int i = 0; i < m_ItemStacks.Count; i++) {

                if (m_ItemStacks[i].Item.ItemDefinition == itemDefinition) {
                    return (ItemInfo)m_ItemStacks[i];
                }

                if (!checkInherently || similarItemInfo != null) { continue; }

                if (itemDefinition.InherentlyContains(m_ItemStacks[i].Item)) {
                    similarItemInfo = (ItemInfo)m_ItemStacks[i];
                }
            }

            return similarItemInfo;
        }

        /// <summary>
        /// Returns the first itemInfo that matches the description.
        /// </summary>
        /// <param name="itemCategory">The Item Category to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the item category.</param>
        /// <returns>The first item info found in the collection.</returns>
        public virtual ItemInfo? GetItemInfo(ItemCategory itemCategory, bool checkInherently = false)
        {
            if (itemCategory == null) { return null; }

            ItemInfo? similarItemInfo = null;

            for (int i = 0; i < m_ItemStacks.Count; i++) {

                if (m_ItemStacks[i].Item.Category == itemCategory) {
                    return (ItemInfo)m_ItemStacks[i];
                }

                if (!checkInherently || similarItemInfo != null) { continue; }

                if (itemCategory.InherentlyContains(m_ItemStacks[i].Item)) {
                    similarItemInfo = (ItemInfo)m_ItemStacks[i];
                }
            }

            return similarItemInfo;
        }

        /// <summary>
        /// Get any combination of item amounts be setting your own filter
        /// </summary>
        /// <param name="itemInfos">Reference to the array of item amounts. Can be resized up.</param>
        /// <param name="filterFunc">A function that will be used to filter the result, evaluating to true means it will be part of the result.</param>
        /// <param name="startIndex">The start index, the items will be added to the itemInfos array from that start index.</param>
        /// <returns>The list slice.</returns>
        public virtual ListSlice<ItemInfo> GetItemInfos(ref ItemInfo[] itemInfos,
            Func<ItemInfo, bool> filterFunc, int startIndex = 0)
        {
            var index = startIndex;
            for (int i = 0; i < m_ItemStacks.Count; i++) {
                var itemInfo = (ItemInfo)m_ItemStacks[i];
                if (filterFunc.Invoke(itemInfo) == false) { continue; }

                TypeUtility.ResizeIfNecessary(ref itemInfos, index);

                itemInfos[index] = itemInfo;
                index++;
            }

            return (itemInfos, 0, index);
        }

        /// <summary>
        /// Get any combination of item amounts be setting your own filter
        /// </summary>
        /// <param name="itemInfos">Reference to the array of item amounts. Can be resized up.</param>
        /// <param name="filterParam">Filter parameter used to pass parameters without boxing.</param>
        /// <param name="filterFunc">A function that will be used to filter the result, evaluating to true means it will be part of the result.</param>
        /// <param name="startIndex">The start index, the items will be added to the itemInfos array from that start index.</param>
        /// <returns>The list slice.</returns>
        public virtual ListSlice<ItemInfo> GetItemInfos<T>(ref ItemInfo[] itemInfos, T filterParam,
            Func<ItemInfo, T, bool> filterFunc, int startIndex = 0)
        {
            var index = startIndex;
            for (int i = 0; i < m_ItemStacks.Count; i++) {
                var itemInfo = (ItemInfo)m_ItemStacks[i];
                if (filterFunc.Invoke(itemInfo, filterParam) == false) { continue; }

                TypeUtility.ResizeIfNecessary(ref itemInfos, index);

                itemInfos[index] = itemInfo;
                index++;
            }

            return (itemInfos, 0, index);
        }

        /// <summary>
        /// Get any combination of item amounts be setting your own filter and comparer
        /// </summary>
        /// <param name="itemInfos">Reference to the array of item amounts. Can be resized up.</param>
        /// <param name="filterFunc">A function that will be used to filter the result, evaluating to true means it will be part of the result.</param>
        /// <param name="sortComparer">Comparer used to sort the result after it was filtered.</param>
        /// <param name="startIndex">The start index, the items will be added to the itemInfos array from that start index.</param>
        /// <returns>The list slice.</returns>
        public virtual ListSlice<ItemInfo> GetItemInfos(ref ItemInfo[] itemInfos,
            Func<ItemInfo, bool> filterFunc, Comparer<ItemInfo> sortComparer, int startIndex = 0)
        {
            var slice = GetItemInfos(ref itemInfos, filterFunc, startIndex);
            Array.Sort(itemInfos, 0, slice.Count, sortComparer);

            return slice;
        }

        /// <summary>
        /// Get any combination of item amounts be setting your own filter and comparer
        /// </summary>
        /// <param name="itemInfos">Reference to the array of item amounts. Can be resized up.</param>
        /// <param name="filterParam">Filter parameter used to pass parameters without boxing.</param>
        /// <param name="filterFunc">A function that will be used to filter the result, evaluating to true means it will be part of the result.</param>
        /// <param name="sortComparer">Comparer used to sort the result after it was filtered.</param>
        /// <param name="startIndex">The start index, the items will be added to the itemInfos array from that start index.</param>
        /// <returns>The list slice.</returns>
        public virtual ListSlice<ItemInfo> GetItemInfos<T>(ref ItemInfo[] itemInfos, T filterParam,
            Func<ItemInfo, T, bool> filterFunc, Comparer<ItemInfo> sortComparer, int startIndex = 0)
        {
            var itemAmountSlice = GetItemInfos(ref itemInfos, filterParam, filterFunc, startIndex);
            Array.Sort(itemInfos, 0, itemAmountSlice.Count, sortComparer);

            return itemAmountSlice;
        }

        /// <summary>
        /// Returns a list of all the items that are part of the category.
        /// </summary>
        /// <param name="category">The category parent of the items that are returned.</param>
        /// <param name="itemInfos">Reference to the array of item amounts. Can be resized up.</param>
        /// <param name="checkInherently">If true the children of the category provided will be taken into account.</param>
        /// <returns>A list of all the items that are inherently part of the category.</returns>
        public virtual ListSlice<ItemInfo> GetItemInfosWithCategory(ItemCategory category, ref ItemInfo[] itemInfos, bool checkInherently)
        {
            if (category == null) { return (itemInfos, 0, 0); }

            var index = 0;
            for (int i = 0; i < m_ItemStacks.Count; i++) {
                if (checkInherently) {
                    if (category.InherentlyContains(m_ItemStacks[i].Item) == false) { continue; }
                } else {
                    if (category.DirectlyContains(m_ItemStacks[i].Item) == false) { continue; }
                }

                TypeUtility.ResizeIfNecessary(ref itemInfos, index);

                itemInfos[index] = (ItemInfo)m_ItemStacks[i];
                index++;
            }

            return (itemInfos, 0, index);
        }

        /// <summary>
        /// Returns a list of the items in the collection  with the specified Item Definition.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition to retrieve the amount of.</param>
        /// <param name="itemInfos">Reference to the array of item infos. Can be resized up.</param>
        ///  <param name="checkInherently">Take into account the children of the item definition.</param>
        /// <returns>A list of the items in the collection  with the specified Item Definition.</returns>
        public virtual ListSlice<ItemInfo> GetItemInfosWithDefinition(ItemDefinition itemDefinition, ref ItemInfo[] itemInfos, bool checkInherently)
        {
            if (itemDefinition == null) { return (itemInfos, 0, 0); }

            var index = 0;
            for (int i = 0; i < m_ItemStacks.Count; i++) {

                if (checkInherently) {
                    if (itemDefinition.InherentlyContains(m_ItemStacks[i].Item) == false) { continue; }
                } else {
                    if (itemDefinition.DirectlyContains(m_ItemStacks[i].Item) == false) { continue; }
                }

                TypeUtility.ResizeIfNecessary(ref itemInfos, index);

                itemInfos[index] = (ItemInfo)m_ItemStacks[i];
                index++;
            }

            return (itemInfos, 0, index);
        }

        /// <summary>
        /// Returns a list of the all the items in the Collection.
        /// </summary>
        /// <returns>Returns the all the items in the collection.</returns>
        public virtual ListSlice<ItemInfo> GetAllItemInfos(ref ItemInfo[] itemInfos)
        {
            var index = 0;
            for (int i = 0; i < m_ItemStacks.Count; i++) {

                if (m_ItemStacks[i] == null) { continue; }

                TypeUtility.ResizeIfNecessary(ref itemInfos, index);

                itemInfos[index] = (ItemInfo)m_ItemStacks[i];
                index++;
            }

            return (itemInfos, 0, index);
        }

        /// <summary>
        /// Returns a list of the all the items in the Collection.
        /// </summary>
        /// <returns>Returns the all the items in the collection.</returns>
        public virtual IReadOnlyList<ItemStack> GetAllItemStacks()
        {
            return m_ItemStacks;
        }

        #endregion

        #region Givers

        /// <summary>
        /// Give an item to another Collection.
        /// </summary>
        /// <param name="itemInfo">The item to give.</param>
        /// <param name="targetStack">The stack where the items should be added.</param>
        /// /// <param name="itemRejectedAction">A callback when an item is removed from an inventory.</param>
        /// <returns>Returns the item info that was given to the target stack.</returns>
        public virtual ItemInfo? GiveItem(ItemInfo itemInfo, ItemStack targetStack,
            Action<ItemInfo> itemRejectedAction = null)
        {
            if (itemInfo.Amount == 0) { return null; }

            // Remove first.
            var removedItemInfo = RemoveItem(itemInfo);

            ItemInfo itemInfoToAdd = (itemInfo.Item, removedItemInfo.Amount, this);
            var givenItemInfo = targetStack.ItemCollection.AddItem(itemInfoToAdd, targetStack);
            if (givenItemInfo.Amount != removedItemInfo.Amount) {
                // Failed to add so add it back to the previous collection.
                var rejectedAmount = itemInfoToAdd.Amount - givenItemInfo.Amount;
                itemRejectedAction?.Invoke((rejectedAmount, itemInfoToAdd));
            }

            return givenItemInfo;
        }

        /// <summary>
        /// Give an item to another Collection.
        /// </summary>
        /// <param name="itemInfo">The item to give.</param>
        /// <param name="itemCollection">The itemCollection that will receive the item.</param>
        /// /// <param name="itemRejectedAction">A callback when an item is removed from an inventory.</param>
        /// <returns>Returns the item info that was given to the target itemCollection.</returns>
        public virtual ItemInfo? GiveItem(ItemInfo itemInfo, ItemCollection itemCollection,
            Action<ItemInfo> itemRejectedAction = null)
        {
            if (itemInfo.Amount == 0) { return null; }

            // Remove first.
            var removedItemInfo = RemoveItem(itemInfo);

            ItemInfo itemInfoToAdd = (itemInfo.Item, removedItemInfo.Amount, this);
            var givenItemInfo = itemCollection.AddItem(itemInfoToAdd);
            if (givenItemInfo.Amount != removedItemInfo.Amount) {
                // Failed to add so add it back to the previous collection.
                var rejectedAmount = itemInfoToAdd.Amount - givenItemInfo.Amount;
                itemRejectedAction?.Invoke((rejectedAmount, itemInfoToAdd));
            }

            return givenItemInfo;
        }

        /// <summary>
        /// Give all the items that this itemCollection contains to another collection.
        /// This is used when opening bags/chests that contain itemCollections
        /// </summary>
        /// <param name="otherItemCollection">The itemCollection that will receive the items.</param>
        /// <param name="itemRejectedAction">A callback when an item is removed from an inventory.</param>
        /// <returns>Returns false if anything went wrong while giving the items.</returns>
        public virtual void GiveAllItems(ItemCollection otherItemCollection,
            Action<ItemInfo> itemRejectedAction = null)
        {
            for (int i = m_ItemStacks.Count - 1; i >= 0; i--) {
                var itemStack = m_ItemStacks[i];
                GiveItem((ItemInfo)itemStack, otherItemCollection, itemRejectedAction);
            }
        }

        #endregion

        /// <summary>
        /// Overrides the ToString method.
        /// </summary>
        /// <returns>The overridden string.</returns>
        public override string ToString()
        {
            return string.Format("ItemCollection {0} ({1})", m_Name, m_Purpose);
        }

        /// <summary>
        /// Returns the number of items that could fit if X amount of item was added and that only a certain of new stacks where allowed to be created.
        /// </summary>
        /// <param name="itemInfo">itemInfo containing the item and amount that needs to fit.</param>
        /// <param name="availableAdditionalStacks">The additional ItemStacks which can be added to fit those items.</param>
        /// <returns>Return the amount of item that can fit.</returns>
        public virtual int GetItemAmountFittingInLimitedAdditionalStacks(ItemInfo itemInfo, int availableAdditionalStacks)
        {
            if (itemInfo.Item.IsUnique) {
                return Mathf.Min(itemInfo.Amount, availableAdditionalStacks);
            }

            //If that item exist it can stack on top.
            if(availableAdditionalStacks == 0 && !HasItem(itemInfo.Item)) { return 0; }
                
            return itemInfo.Amount;

        }

        /// <summary>
        /// Get the sum of the attributes as an integer. Includes Int and Float attribute values.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>Returns the sum of the attribute values with the attribute name.</returns>
        public virtual float GetFloatSum(string attributeName)
        {
            return AttributeUtility.GetFloatSum(attributeName, GetAllItemStacks());
        }
        
        /// <summary>
        /// Get the sum of the attributes as an float. Includes Int and Float attribute values.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>Returns the sum of the attribute values with the attribute name.</returns>
        public virtual int GetIntSum(string attributeName)
        {
            return AttributeUtility.GetIntSum(attributeName, GetAllItemStacks());
        }
    }
}
