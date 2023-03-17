/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using Opsive.UltimateInventorySystem.Utility;
    using UnityEngine;
    using EventHandler = Shared.Events.EventHandler;
    using Object = UnityEngine.Object;

    /// <summary>
    /// The bare bones inventory, essentially a wrapper for the Item Collections.
    /// </summary>
    public class Inventory : MonoBehaviour, IInventory, IDatabaseSwitcher
    {
        [Tooltip("The item collections serialized data.")]
        [SerializeField] protected ItemRestrictionObject[] m_ItemCollectionRestrictionObjects;
        [Tooltip("The item collections serialized data.")]
        [SerializeField] protected Serialization[] m_ItemCollectionData;

        [Tooltip("The item collections contain the items within your inventory, use them to keep your items organized.")]
        [System.NonSerialized] protected List<ItemCollection> m_ItemCollections;
        [Tooltip("An array of currency owner defined on initialize.")]
        [System.NonSerialized] protected ICurrencyOwner[] m_Currencies;

        protected Action<ICurrencyOwner> m_CurrencyChangedAction;
        [System.NonSerialized] protected bool m_Initialized = false;

        [System.NonSerialized] protected ResizableArray<ItemInfo> m_CachedItemInfos;
        [System.NonSerialized] protected List<IItemRestriction> m_ItemCollectionRestrictions;

        protected bool m_UpdateEventDisabled;

        protected ItemUser m_ItemUser;

        public ItemUser ItemUser {
            get => m_ItemUser;
            set => m_ItemUser = value;
        }

        public IReadOnlyList<ItemCollection> ItemCollectionsReadOnly => m_ItemCollections;
        internal List<ItemCollection> ItemCollections {
            get => m_ItemCollections;
            set => m_ItemCollections = value;
        }

        public IReadOnlyList<ItemInfo> AllItemInfos => m_CachedItemInfos;
        public IReadOnlyList<IItemRestriction> ItemRestrictions => m_ItemCollectionRestrictions;

        public bool IsInitialized => m_Initialized;
        public bool UpdateEventDisabled { get => m_UpdateEventDisabled; set => m_UpdateEventDisabled = value; }

        /// <summary>
        /// Initialized the collection and event.
        /// </summary>
        public virtual void Awake()
        {
            Initialize(true);
        }

        /// <summary>
        /// Initialize the inventory component.
        /// </summary>
        public void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }
            m_Initialized = true;

            if (m_ItemUser == null) {
                m_ItemUser = GetComponent<ItemUser>();
            }

            m_CachedItemInfos = new ResizableArray<ItemInfo>();
            m_CachedItemInfos.Initialize(10);

            Deserialize();
            AssignMainCollection();

            if (Application.isPlaying == false) { return; }

            m_ItemCollectionRestrictions = new List<IItemRestriction>();
            if (m_ItemCollectionRestrictionObjects != null) {
                for (int i = 0; i < m_ItemCollectionRestrictionObjects.Length; i++) {
                    if (m_ItemCollectionRestrictionObjects[i] == null) {
                        Debug.LogWarning($"The ItemCollection Restriction Object {i} is null.", gameObject);
                        continue;
                    }
                    AddRestriction(m_ItemCollectionRestrictionObjects[i].DuplicateRestriction);
                }
            }

            var restrictions = GetComponents<IItemRestriction>();
            for (int i = 0; i < restrictions.Length; i++) {
                AddRestriction(restrictions[i]);
            }

            InitializeItemCollectionArray();

            InitializeCurrencies();

            UpdateInventory();
        }

        /// <summary>
        /// Set the item collection main to index 0 if it does not exist.
        /// </summary>
        protected virtual void AssignMainCollection()
        {
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (m_ItemCollections[i].Purpose == ItemCollectionPurpose.Main) { return; }
            }

            if (m_ItemCollections[0].Purpose == ItemCollectionPurpose.None) {
                m_ItemCollections[0].Purpose = ItemCollectionPurpose.Main;
            }
        }

        /// <summary>
        /// Deserialize the item collections.
        /// </summary>
        protected virtual void Deserialize()
        {
            m_ItemCollections = new List<ItemCollection>();

            if (m_ItemCollectionData == null) {
                if (m_ItemCollections.Count == 0) {
                    m_ItemCollections.Add(new ItemCollection());
                }
                if (m_ItemCollections[0] == null) {
                    m_ItemCollections[0] = new ItemCollection();
                }
                return;
            }

            for (var i = 0; i < m_ItemCollectionData.Length; i++) {
                var data = m_ItemCollectionData[i];

                if (!(data.DeserializeFields(MemberVisibility.Public) is ItemCollection itemCollection)) { continue; }

                itemCollection.Initialize(this, false);
                m_ItemCollections.Add(itemCollection);
            }

            if (m_ItemCollections.Count == 0) {
                m_ItemCollections.Add(new ItemCollection());
                m_ItemCollections[0].Initialize(this, true);
            }
            if (m_ItemCollections[0] == null) {
                m_ItemCollections[0] = new ItemCollection();
                m_ItemCollections[0].Initialize(this, true);
            }
        }

        /// <summary>
        /// Serialize the itemCollections.
        /// </summary>
        public virtual void Serialize()
        {
            m_ItemCollections.RemoveAll(item => item == null);
            m_ItemCollectionData = Serialization.Serialize((IList<ItemCollection>)m_ItemCollections);
        }

        /// <summary>
        /// Initialize the item Collection array.
        /// </summary>
        protected virtual void InitializeItemCollectionArray()
        {
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                var itemCollection = m_ItemCollections[i];
                itemCollection.Initialize(this, true);
                EventHandler.RegisterEvent(itemCollection, EventNames.c_ItemCollection_OnUpdate, () => OnItemCollectionUpdate(itemCollection));
            }

            //Load the default loadout after all collections are initialized in case items overflow to other collections.
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                m_ItemCollections[i].LoadDefaultLoadout();
            }
        }

        /// <summary>
        /// On Item Collection Update.
        /// </summary>
        /// <param name="itemCollection">The item collection.</param>
        protected virtual void OnItemCollectionUpdate(ItemCollection itemCollection)
        {
            UpdateInventory();
        }

        /// <summary>
        /// Attach the currency owner components to the inventory.
        /// </summary>
        public void InitializeCurrencies()
        {
            m_Currencies = GetComponents<CurrencyOwnerBase>();
            m_CurrencyChangedAction = x => UpdateInventory(false);

            for (int i = 0; i < m_Currencies.Length; i++) {
                var currencyOwner = m_Currencies[i];
                EventHandler.RegisterEvent(currencyOwner, EventNames.c_CurrencyOwner_OnUpdate, () => m_CurrencyChangedAction?.Invoke(currencyOwner));
            }
        }

        /// <summary>
        /// Check if the collection can be ignored when searching for items.
        /// </summary>
        /// <param name="itemCollection">The item collection to check.</param>
        /// <returns>It can be ignored if true.</returns>
        protected virtual bool IgnoreCollection(ItemCollection itemCollection)
        {
            if (itemCollection == null) { return true; }
            return itemCollection.Purpose == ItemCollectionPurpose.Loadout ||
                   itemCollection.Purpose == ItemCollectionPurpose.Hide;
        }

        /// <summary>
        /// Add an itemCollection to the inventory.
        /// </summary>
        /// <param name="itemCollection">The item Collection.</param>
        public void AddItemCollection(ItemCollection itemCollection)
        {
            m_ItemCollections.Add(itemCollection);
            itemCollection.Initialize(this, true);
            if (Application.isPlaying) {
                EventHandler.RegisterEvent(itemCollection, EventNames.c_ItemCollection_OnUpdate, () => OnItemCollectionUpdate(itemCollection));
            }
        }

        /// <summary>
        /// Remove an itemCollection in the inventory.
        /// </summary>
        /// <param name="itemCollection">The item Collection.</param>
        public void RemoveItemCollection(ItemCollection itemCollection)
        {
            m_ItemCollections.Remove(itemCollection);
            itemCollection.Initialize(null, true);
            if (Application.isPlaying) {
                EventHandler.UnregisterEvent(itemCollection, EventNames.c_ItemCollection_OnUpdate,
                    () => OnItemCollectionUpdate(itemCollection));
            }
        }

        /// <summary>
        /// The Item Collection, it includes all the items in the inventory.
        /// </summary>
        public virtual ItemCollection MainItemCollection {
            get {
                if (m_ItemCollections.Count == 0) {
                    Debug.LogError("ItemCollection should always have a size of at least 1.");
                    return null;
                }
                return GetItemCollection(ItemCollectionPurpose.Main);
            }
        }

        /// <summary>
        /// Add an item restriction.
        /// </summary>
        /// <param name="itemRestriction">Item restriction to add.</param>
        /// <returns>The itemInfo after the condition. </returns>
        public virtual void AddRestriction(IItemRestriction itemRestriction)
        {
            if (m_ItemCollectionRestrictions.Contains(itemRestriction)) {
                return;
            }
            itemRestriction.Initialize(this, false);
            m_ItemCollectionRestrictions.Add(itemRestriction);
        }

        /// <summary>
        /// Check if the item can be removed from the collection.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>The itemInfo to remove.</returns>
        public virtual void RemoveRestriction(IItemRestriction itemRestriction)
        {
            if (m_ItemCollectionRestrictions.Contains(itemRestriction) == false) {
                return;
            }

            m_ItemCollectionRestrictions.Remove(itemRestriction);
        }

        /// <summary>
        /// Check if the item info can be added to the itemCollection.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="receivingCollection">The receiving collection.</param>
        /// <returns>The itemInfo after the condition. </returns>
        public virtual ItemInfo? CanAddItem(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            for (int i = 0; i < m_ItemCollectionRestrictions.Count; i++) {
                var result = m_ItemCollectionRestrictions[i].CanAddItem(itemInfo, receivingCollection);
                if (result.HasValue == false) { return null; }

                itemInfo = result.Value;
            }

            if (itemInfo.Item == null || itemInfo.Amount <= 0) {
                return null;
            }

            return itemInfo;
        }

        /// <summary>
        /// Check if the item can be removed from the collection.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>The itemInfo to remove.</returns>
        public virtual ItemInfo? CanRemoveItem(ItemInfo itemInfo)
        {
            for (int i = 0; i < m_ItemCollectionRestrictions.Count; i++) {
                var result = m_ItemCollectionRestrictions[i].CanRemoveItem(itemInfo);
                if (result.HasValue == false) { return null; }

                itemInfo = result.Value;
            }

            if (itemInfo.Item == null || itemInfo.Amount <= 0) {
                return null;
            }

            return itemInfo;
        }

        /// <summary>
        /// Get a currency component which has the currency type specified.
        /// </summary>
        /// <typeparam name="T">The currency Type.</typeparam>
        /// <returns>The component which inherits the ICurrency with the currency type specified.</returns>
        public virtual ICurrencyOwner<T> GetCurrencyComponent<T>()
        {
            if (m_Currencies == null) { return null; }
            for (int i = 0; i < m_Currencies.Length; i++) {
                if (m_Currencies[i] is ICurrencyOwner<T>) {
                    return m_Currencies[i] as ICurrencyOwner<T>;
                }
            }

            return null;
        }
        
        /// <summary>
        /// Get a currency component which has the currency type specified.
        /// </summary>
        /// <typeparam name="T">The currency Type.</typeparam>
        /// <returns>The component which inherits the ICurrency with the currency type specified.</returns>
        public virtual CurrencyOwner GetCurrencyOwner()
        {
            if (m_Currencies == null) { return null; }
            for (int i = 0; i < m_Currencies.Length; i++) {
                if (m_Currencies[i] is CurrencyOwner currencyOwner) {
                    return currencyOwner;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the number of item collections in the inventory.
        /// </summary>
        /// <returns>The item collection count.</returns>
        public int GetItemCollectionCount()
        {
            return m_ItemCollections.Count;
        }

        /// <summary>
        /// Get an itemCollection by name.
        /// </summary>
        /// <param name="itemCollectionName">The name.</param>
        /// <returns>The item collection.</returns>
        public ItemCollection GetItemCollection(string itemCollectionName)
        {
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (m_ItemCollections[i].Name == itemCollectionName) { return m_ItemCollections[i]; }
            }

            return null;
        }

        /// <summary>
        /// Get the item collection by purpose.
        /// </summary>
        /// <param name="purpose">The purpose.</param>
        /// <returns>The item collection.</returns>
        public ItemCollection GetItemCollection(ItemCollectionPurpose purpose)
        {
            if (purpose == ItemCollectionPurpose.None) { return null; }

            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (m_ItemCollections[i].Purpose == purpose) { return m_ItemCollections[i]; }
            }

            if (purpose == ItemCollectionPurpose.Main) { return m_ItemCollections[0]; }

            return null;
        }

        /// <summary>
        /// Get the item collection index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item collection.</returns>
        public ItemCollection GetItemCollection(int index)
        {
            if (index < 0 || index >= m_ItemCollections.Count) { return null; }

            return m_ItemCollections[index];
        }

        /// <summary>
        /// Get the item collection by ItemCollectionId.
        /// </summary>
        /// <param name="collectionID">The itemCollection identifier.</param>
        /// <returns>The item collection.</returns>
        public ItemCollection GetItemCollection(ItemCollectionID collectionID)
        {
            var bestMatchWeight = 0;
            ItemCollection bestMatch = null;

            for (int i = 0; i < m_ItemCollections.Count; i++) {
                var match = collectionID.CompareWeighted(m_ItemCollections[i]);
                if (match <= bestMatchWeight) { continue; }

                bestMatchWeight = match;
                bestMatch = m_ItemCollections[i];
            }

            if (bestMatch != null) { return bestMatch; }

            if (collectionID.Purpose == ItemCollectionPurpose.Main) { return m_ItemCollections[0]; }

            return null;
        }

        /// <summary>
        /// Add an item to the an item collection in the inventory.
        /// </summary>
        /// <param name="itemInfo">The amount of item being added.</param>
        /// <param name="stackTarget">The item stack where the item should be added.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual ItemInfo AddItem(ItemInfo itemInfo, ItemStack stackTarget = null)
        {
            if (stackTarget != null && stackTarget.ItemCollection != null && ReferenceEquals(stackTarget.Inventory, this)) {
                return stackTarget.ItemCollection.AddItem(itemInfo, stackTarget);
            }

            return MainItemCollection.AddItem(itemInfo);
        }

        /// <summary>
        /// Add an item to the an item collection in the inventory.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition of item being added.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual ItemInfo AddItem(ItemDefinition itemDefinition, int amount)
        {
            if (itemDefinition == null) { return ItemInfo.None; }

            var itemInfo = (ItemInfo)(InventorySystemManager.CreateItem(itemDefinition), amount);

            return AddItem(itemInfo);
        }
        
        /// <summary>
        /// Add an item to the an item collection in the inventory.
        /// </summary>
        /// <param name="item">The item being added.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual ItemInfo AddItem(Item item, int amount)
        {
            var itemInfo = (ItemInfo)(item, amount);

            return AddItem(itemInfo);
        }

        /// <summary>
        /// Add an item to the an item collection in the inventory.
        /// </summary>
        /// <param name="itemName">The item name being added.</param>
        /// <param name="amount">The amount to add.</param>
        /// <returns>Returns the number of items added, 0 if no item was added.</returns>
        public virtual ItemInfo AddItem(string itemName, int amount)
        {
            var item = InventorySystemManager.CreateItem(itemName);
            if (item == null) {
                Debug.LogWarning($"The Item named '{itemName}' does not exist");
                return ItemInfo.None;
            }

            var itemInfo = (ItemInfo)(item, amount);

            return AddItem(itemInfo);
        }

        /// <summary>
        /// Remove an item from the item collection in the inventory.
        /// </summary>
        /// <param name="itemInfo">The amount of item to remove.</param>
        /// <returns>Returns true if the item was removed correctly.</returns>
        public virtual ItemInfo RemoveItem(ItemInfo itemInfo)
        {
            if (ReferenceEquals(itemInfo.Inventory, this)) { return itemInfo.ItemCollection.RemoveItem(itemInfo); }

            return MainItemCollection.RemoveItem(itemInfo);
        }
        
        /// <summary>
        /// Remove an item from the item collection in the inventory.
        /// </summary>
        /// <param name="item">The item being removed.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>Returns true if the item was removed correctly.</returns>
        public virtual ItemInfo RemoveItem(Item item, int amount)
        {
            return MainItemCollection.RemoveItem((ItemInfo)(item, amount));
        }

        /// <summary>
        /// Remove an item from the item collection in the inventory.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition of item being removed.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>Returns the number of items added, 0 if no item was removed.</returns>
        public virtual ItemInfo RemoveItem(ItemDefinition itemDefinition, int amount)
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
        /// Remove an item from the item collection in the inventory.
        /// </summary>
        /// <param name="itemName">The item name of the item being removed.</param>
        /// <param name="amount">The amount to remove.</param>
        /// <returns>Returns the number of items added, 0 if no item was removed.</returns>
        public virtual ItemInfo RemoveItem(string itemName, int amount)
        {
            var itemDefinition = InventorySystemManager.GetItemDefinition(itemName);
            if (itemDefinition == null) {
                Debug.LogWarning($"The Item named '{itemName}' does not exist");
                return ItemInfo.None;
            }

            return RemoveItem(itemDefinition, amount);
        }
        
        /// <summary>
        /// Remove all the items in the inventory.
        /// </summary>
        /// <param name="removeItemsFromIgnoredCollections">Should items be removed from ignored collections such as loadouts or Hide.</param>
        /// <param name="disableEventsWhileRemoving">Should .</param>
        /// <returns>Returns the number of items added, 0 if no item was removed.</returns>
        public virtual void RemoveAllItems(bool removeItemsFromIgnoredCollections = false, bool disableEventsWhileRemoving = true)
        {
            var previous = m_UpdateEventDisabled;
            if (disableEventsWhileRemoving) {
                m_UpdateEventDisabled = true;
            }
            
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                var itemCollection = m_ItemCollections[i];
                if (removeItemsFromIgnoredCollections == false && IgnoreCollection(itemCollection)) { continue; }

                itemCollection.RemoveAll(disableEventsWhileRemoving);
            }

            if (disableEventsWhileRemoving) {
                m_UpdateEventDisabled = previous;
                UpdateInventory();
            }
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

            return GetItemAmount(item, similarItem) >= 1;
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

            return GetItemAmount(itemAmount.Item, similarItem) >= itemAmount.Amount;
        }

        /// <summary>
        /// Check if the inventory has at least the item amount specified.
        /// </summary>
        /// <param name="itemInfo">The item info to check.</param>
        /// <param name="similarItem">Find an itemInfo with that value equivalent item or just one similar?</param>
        /// <returns>True if the inventory has at least that amount.</returns>
        public virtual bool HasItem(ItemInfo itemInfo, bool similarItem = true)
        {
            if (itemInfo.Item == null) { return true; }

            return GetItemAmount(itemInfo.Item, similarItem) >= itemInfo.Amount;
        }

        /// <summary>
        /// Check if the inventory has at least the item amount specified.
        /// </summary>
        /// <param name="itemDefinitionAmount">The item info to check.</param>
        /// <returns>True if the inventory has at least that amount.</returns>
        public virtual bool HasItem(ItemDefinitionAmount itemDefinitionAmount)
        {
            if (itemDefinitionAmount.ItemDefinition == null) { return true; }

            return GetItemAmount(itemDefinitionAmount.ItemDefinition) >= itemDefinitionAmount.Amount;
        }

        /// <summary>
        /// Determines if the Item Collection contains an item with the Item Definition.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition of the item to check.</param>
        /// <param name="checkInherently">Take into account the children of the Item Definition.</param>
        /// <param name="countStacks">If true count the stacks number not the stacks amounts.</param>
        /// <returns>Returns true if the amount of items with the Item Definition in the collection is equal or bigger than the amount specified.</returns>
        public virtual bool HasItem(ItemDefinitionAmount itemDefinitionAmount, bool checkInherently, bool countStacks = false)
        {
            if (itemDefinitionAmount.ItemDefinition == null) { return false; }

            var count = GetItemAmount(itemDefinitionAmount.ItemDefinition, checkInherently, countStacks);

            return count >= itemDefinitionAmount.Amount;
        }

        /// <summary>
        /// Determines if the Item Collection contains an item with the exact same category provided (does NOT check the category children).
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
        /// Checks if the inventory contains a list of Item Amounts.
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
        /// Checks if the inventory contains a list of Item Infos.
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
        /// Return the amount of item in the inventory.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <param name="similarItem">Find an itemInfo with that value equivalent item or just one similar?</param>
        /// <returns>The amount of that item in the inventory.</returns>
        public virtual int GetItemAmount(Item item, bool similarItem=true)
        {
            var amount = 0;
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                amount += m_ItemCollections[i].GetItemAmount(item, similarItem);
            }

            return amount;
        }

        /// <summary>
        /// Returns the number of items in the collection with the specified Item Definition.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the Item Definition.</param>
        /// <param name="unique">Should count unique items or amounts of items.</param>
        /// <returns>The number of items in the collection with the specified Item Definition.</returns>
        public virtual int GetItemAmount(ItemDefinition itemDefinition, bool checkInherently = false, bool unique = false)
        {
            var amount = 0;
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                amount += m_ItemCollections[i].GetItemAmount(itemDefinition, checkInherently, unique);
            }

            return amount;
        }

        /// <summary>
        /// Returns the number of items in the collection with the specified Item Definition.
        /// </summary>
        /// <param name="itemCategory">The Item Category to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the item category.</param>
        /// <param name="unique">Should count unique items or amounts of items.</param>
        /// <returns>The number of items in the collection with the specified Item Category.</returns>
        public virtual int GetItemAmount(ItemCategory itemCategory, bool checkInherently = false, bool unique = false)
        {
            var amount = 0;
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                amount += m_ItemCollections[i].GetItemAmount(itemCategory, checkInherently, unique);
            }

            return amount;
        }

        /// <summary>
        /// Returns the first itemInfo that matches the description.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The itemInfo.</returns>
        public virtual ItemInfo? GetItemInfo(Item item)
        {
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                var itemInfo = m_ItemCollections[i].GetItemInfo(item);

                if (itemInfo.HasValue) { return itemInfo; }
            }

            return null;
        }

        /// <summary>
        /// Returns the first itemInfo that matches the description.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the item definition.</param>
        /// <returns>The first item info found in the inventory.</returns>
        public virtual ItemInfo? GetItemInfo(ItemDefinition itemDefinition, bool checkInherently = false)
        {
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                var itemInfo = m_ItemCollections[i].GetItemInfo(itemDefinition, checkInherently);

                if (itemInfo.HasValue) { return itemInfo; }
            }

            return null;
        }

        /// <summary>
        /// Returns the first itemInfo that matches the description.
        /// </summary>
        /// <param name="itemCategory">The Item Category to retrieve the amount of.</param>
        /// <param name="checkInherently">Take into account the children of the item category.</param>
        /// <returns>The first item info found in the inventory.</returns>
        public virtual ItemInfo? GetItemInfo(ItemCategory itemCategory, bool checkInherently = false)
        {
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (IgnoreCollection(m_ItemCollections[i])) { continue; }

                var itemInfo = m_ItemCollections[i].GetItemInfo(itemCategory, checkInherently);

                if (itemInfo.HasValue) { return itemInfo; }
            }

            return null;
        }

        /// <summary>
        /// Get any combination of item amounts be setting your own filter
        /// </summary>
        /// <param name="itemInfos">Reference to the array of item amounts. Can be resized up.</param>
        /// <param name="filterParam">Filter parameter used to pass parameters without boxing.</param>
        /// <param name="filterFunc">A function that will be used to filter the result, evaluating to true means it will be part of the result.</param>
        /// <param name="startIndex">The start index, the items will be added to the itemInfos array from that start index.</param>
        /// <returns>The list slice.</returns>
        public virtual ListSlice<ItemInfo> GetItemInfos(ref ItemInfo[] itemInfos, Func<ItemInfo, bool> filterFunc, int startIndex = 0)
        {
            var index = startIndex;
            for (int i = 0; i < m_CachedItemInfos.Count; i++) {
                if (filterFunc.Invoke(m_CachedItemInfos[i]) == false) { continue; }

                TypeUtility.ResizeIfNecessary(ref itemInfos, index);

                itemInfos[index] = m_CachedItemInfos[i];
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
            for (int i = 0; i < m_CachedItemInfos.Count; i++) {
                if (filterFunc.Invoke(m_CachedItemInfos[i], filterParam) == false) { continue; }

                TypeUtility.ResizeIfNecessary(ref itemInfos, index);

                itemInfos[index] = m_CachedItemInfos[i];
                index++;
            }

            return (itemInfos, 0, index);
        }

        /// <summary>
        /// Get any combination of item amounts be setting your own filter and comparer
        /// </summary>
        /// <param name="itemInfos">Reference to the array of item amounts. Can be resized up.</param>
        /// <param name="filterSortParam">Filter and sort parameter used to pass parameters without boxing.</param>
        /// <param name="filterFunc">A function that will be used to filter the result, evaluating to true means it will be part of the result.</param>
        /// <param name="sortComparer">Comparer used to sort the result after it was filtered.</param>
        ///  <param name="startIndex">The start index, the items will be added to the itemInfos array from that start index.</param>
        /// <returns>The list slice.</returns>
        public virtual ListSlice<ItemInfo> GetItemInfos<T>(ref ItemInfo[] itemInfos, T filterSortParam,
            Func<ItemInfo, T, bool> filterFunc, Comparer<ItemInfo> sortComparer, int startIndex = 0)
        {
            var filteredSlice = GetItemInfos(ref itemInfos, filterSortParam, filterFunc, startIndex);
            filteredSlice.Sort(itemInfos, sortComparer);

            return filteredSlice;
        }
        
        /// <summary>
        /// Get the sum of the attributes as an integer. Includes Int and Float attribute values.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>Returns the sum of the attribute values with the attribute name.</returns>
        public virtual float GetFloatSum(string attributeName)
        {
            return AttributeUtility.GetFloatSum(attributeName, (ListSlice<ItemInfo>)m_CachedItemInfos);
        }
        
        /// <summary>
        /// Get the sum of the attributes as an float. Includes Int and Float attribute values.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>Returns the sum of the attribute values with the attribute name.</returns>
        public virtual int GetIntSum(string attributeName)
        {
            return AttributeUtility.GetIntSum(attributeName, (ListSlice<ItemInfo>)m_CachedItemInfos);
        }

        /// <summary>
        /// Currency changed.
        /// </summary>
        /// <param name="currencyOwner">The currency owner.</param>
        protected virtual void OnCurrencyChanged(ICurrencyOwner currencyOwner)
        {
            UpdateInventory(false);
        }

        /// <summary>
        /// Send an event that the inventory changed.
        /// </summary>
        /// <param name="updateInventoryCache">Update the inventory cache if true.</param>
        public virtual void UpdateInventory(bool updateInventoryCache = true, bool force = false)
        {
            if(m_UpdateEventDisabled && !force){ return; }
            
            if (updateInventoryCache) {
                UpdateCachedInventory();
            }
            EventHandler.ExecuteEvent(this, EventNames.c_Inventory_OnUpdate);
        }

        /// <summary>
        /// Update the inventory item amounts list cache.
        /// </summary>
        public virtual void UpdateCachedInventory()
        {
            m_CachedItemInfos.Clear();
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                var itemCollection = m_ItemCollections[i];
                if (IgnoreCollection(itemCollection)) { continue; }

                var allItemStacks = itemCollection.GetAllItemStacks();

                for (int j = 0; j < allItemStacks.Count; j++) {
                    m_CachedItemInfos.Add((ItemInfo)allItemStacks[j]);
                }
            }

            //m_CachedItemInfos.Sort(m_ItemOrderComparer);
        }

        /// <summary>
        /// Unregister on destroy.
        /// </summary>
        private void OnDestroy()
        {
            if (m_ItemCollections != null) {
                for (int i = 0; i < m_ItemCollections.Count; i++) {
                    var itemCollection = m_ItemCollections[i];
                    EventHandler.UnregisterEvent(itemCollection, EventNames.c_ItemCollection_OnUpdate, () => OnItemCollectionUpdate(itemCollection));
                }
            }

            if (m_Currencies != null) {
                for (int i = 0; i < m_Currencies.Length; i++) {
                    var currencyOwner = m_Currencies[i];
                    EventHandler.UnregisterEvent(currencyOwner, EventNames.c_CurrencyOwner_OnUpdate, () => OnCurrencyChanged(currencyOwner));
                }
            }
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            Initialize(false);

            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (m_ItemCollections[i] == null || m_ItemCollections[i].DefaultLoadout == null) { continue; }

                if (m_ItemCollections[i] is IDatabaseSwitcher handler) {
                    if (handler.IsComponentValidForDatabase(database) == false) { return false; }
                }

                for (int j = 0; j < m_ItemCollections[i].DefaultLoadout.Count; j++) {
                    var item = m_ItemCollections[i].DefaultLoadout[j].Item;
                    if (item == null || database.Contains(item)) { continue; }
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            database.Initialize(false);

            Initialize(false);

            var result = new List<Object>();

            bool dirty = false;
            for (int i = 0; i < m_ItemCollections.Count; i++) {
                if (m_ItemCollections[i] == null || m_ItemCollections[i].DefaultLoadout == null) { continue; }

                if (m_ItemCollections[i] is IDatabaseSwitcher handler) {
                    if (handler.IsComponentValidForDatabase(database) == false) {
                        var innerResult = handler.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(database);
                        if (innerResult != null && innerResult.m_ObjectsToDirty != null) {
                            result.AddRange(innerResult.m_ObjectsToDirty);
                        }
                    }
                }

                for (int j = 0; j < m_ItemCollections[i].DefaultLoadout.Count; j++) {
                    var item = m_ItemCollections[i].DefaultLoadout[j].Item;
                    var amount = m_ItemCollections[i].DefaultLoadout[j].Amount;
                    if (item == null || database.Contains(item)) { continue; }

                    dirty = true;

                    var newItem = database.FindSimilar(item);
                    m_ItemCollections[i].DefaultLoadout[j] = (amount, newItem);
                }
            }

            if (!dirty) { return result.ToArray(); }

            Serialize();

            return result.ToArray();
        }
    }
}
