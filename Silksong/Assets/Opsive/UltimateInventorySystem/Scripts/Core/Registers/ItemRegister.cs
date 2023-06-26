/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.Registers
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The register of items.
    /// </summary>
    public class ItemRegister : InventoryObjectIDOnlyRegister<Item>
    {
        protected Dictionary<uint, List<Item>> m_ItemsOrganizedByCategoryID;
        protected Dictionary<uint, List<Item>> m_ItemsOrganizedByItemDefinitionID;

        public Dictionary<uint, List<Item>> ItemsOrganizedByCategoryID => m_ItemsOrganizedByCategoryID;
        public Dictionary<uint, List<Item>> ItemsOrganizedByItemDefinitionID => m_ItemsOrganizedByItemDefinitionID;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="register">The inventory system register.</param>
        public ItemRegister(InventorySystemRegister register) : base(register)
        {
            m_ItemsOrganizedByCategoryID = new Dictionary<uint, List<Item>>();
            m_ItemsOrganizedByItemDefinitionID = new Dictionary<uint, List<Item>>();
        }

        /// <summary>
        /// Register Conditions.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if the item can bre registered.</returns>
        public override bool RegisterConditions(Item item)
        {
            if (base.RegisterConditions(item) == false) { return false; }

            if (item.Category == null) {
                Debug.LogWarning("Cannot register Item with null category.");
                return false;
            }

            if (item.ItemDefinition == null) {
                Debug.LogWarning("Cannot register Item with null Item Definition.");
                return false;
            }

            if (m_Register.ItemCategoryRegister.IsRegistered(item.Category) == false) {
                Debug.LogWarning("The category of the Item is not registered.");
                return false;
            }

            if (m_Register.ItemDefinitionRegister.IsRegistered(item.ItemDefinition) == false) {
                Debug.LogWarning("The Item Definition of the Item is not registered.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Registers the item.
        /// </summary>
        /// <param name="item">The item (can be replaced).</param>
        /// <returns>True if it was registered.</returns>
        public virtual bool Register(ref Item item)
        {
            if (RegisterConditions(item) == false) { return false; }

            if (item.IsMutable) {
                if (IsIDEmpty(item)) { AssignNewID(item); }
            } else {
                var foundMatch = false;
                if (m_ItemsOrganizedByItemDefinitionID.TryGetValue(item.ItemDefinition.ID, out var itemDefItemList)) {
                    foreach (var registeredItem in itemDefItemList) {
                        
                        if (ReferenceEquals(item, registeredItem) == false) {

                            if (item.ValueEquivalentTo(registeredItem) == false) {
                                continue;
                            }
                        }

                        //An immutable equivalent item is already registered, changing the item reference to point to the registered item.
                        item = registeredItem;
                        foundMatch = true;
                        break;
                    }
                }
                if (foundMatch == false) {
                    //No equivalent registered item were found, registering this item instance.
                    if (IsIDEmpty(item)) { AssignNewID(item); }
                }
            }

            var addToDictionaries = false;

            if (m_DictionaryByID.TryGetValue(item.ID, out var registered)) {
                if (ReferenceEquals(registered, item) == false) {
                    AssignNewID(item);
                    addToDictionaries = true;
                }
            } else {
                addToDictionaries = true;
            }

            if (addToDictionaries) {
                AddInternal(item);
            }

            return true;
        }

        /// <summary>
        /// Add an item to the lists and dictionaries.
        /// </summary>
        /// <param name="item">The item.</param>
        protected override void AddInternal(Item item)
        {
            if (item == null || item.ItemDefinition == null || item.Category == null) { return; }

            base.AddInternal(item);

            if (m_ItemsOrganizedByCategoryID.TryGetValue(item.Category.ID, out var categoryItemList) == false) {
                categoryItemList = new List<Item>();
                m_ItemsOrganizedByCategoryID.Add(item.Category.ID, categoryItemList);
            }
            categoryItemList.Add(item);

            if (m_ItemsOrganizedByItemDefinitionID.TryGetValue(item.ItemDefinition.ID, out var itemDefItemList) == false) {
                itemDefItemList = new List<Item>();
                m_ItemsOrganizedByItemDefinitionID.Add(item.ItemDefinition.ID, itemDefItemList);
            }
            itemDefItemList.Add(item);

        }

        /// <summary>
        /// Remove internal removes the object form the dictionaries.
        /// </summary>
        /// <param name="item">The object.</param>
        protected override void RemoveInternal(Item item)
        {
            base.RemoveInternal(item);

            if (m_ItemsOrganizedByCategoryID.TryGetValue(item.ItemDefinition.Category.ID, out var categoryItemList)) {
                categoryItemList.Remove(item);
                if (categoryItemList.Count == 0) {
                    m_ItemsOrganizedByCategoryID.Remove(item.ItemDefinition.Category.ID);
                }
            }

            if (m_ItemsOrganizedByItemDefinitionID.TryGetValue(item.ItemDefinition.ID, out var itemDefItemList)) {
                itemDefItemList.Remove(item);
                if (itemDefItemList.Count == 0) {
                    m_ItemsOrganizedByItemDefinitionID.Remove(item.ItemDefinition.ID);
                }
            }
        }

        /// <summary>
        /// Delete an itemCategory.
        /// </summary>
        /// <param name="item">The item.</param>
        public override void Delete(Item item)
        {
            Unregister(item);
        }

        /// <summary>
        /// Returns all the items that have the ItemDefinition provided.
        /// </summary>
        /// <param name="itemDefinition">Item definition ID.</param>
        /// <param name="includeChildren">Include the children.</param>
        /// <returns>A list of items with the item definition provided.</returns>
        public virtual IReadOnlyList<Item> ItemsWithItemDefinition(ItemDefinition itemDefinition, bool includeChildren = false)
        {
            return ItemsWithItemDefinition(itemDefinition.ID, includeChildren);
        }

        /// <summary>
        /// Returns all the items that have the ItemDefinition provided.
        /// </summary>
        /// <param name="id">Item definition ID.</param>
        /// <param name="includeChildren">Include the children.</param>
        /// <returns>A list of items with the item definition.</returns>
        public virtual IReadOnlyList<Item> ItemsWithItemDefinition(uint id, bool includeChildren = false)
        {
            if (!includeChildren) {
                if (m_ItemsOrganizedByItemDefinitionID.TryGetValue(id, out var itemListNoChild)) {
                    return itemListNoChild;
                }
            }

            var itemList = new List<Item>();
            if (m_Register.ItemDefinitionRegister.TryGetValue(id, out var parentItemDef)) {
                var pooledParentItemDefAllChildren = GenericObjectPool.Get<ItemDefinition[]>();
                var parentItemDefAllChildrenCount = parentItemDef.GetAllChildren(ref pooledParentItemDefAllChildren, true);
                for (int i = 0; i < parentItemDefAllChildrenCount; i++) {
                    if (m_ItemsOrganizedByItemDefinitionID.TryGetValue(pooledParentItemDefAllChildren[i].ID, out var itemDefItemList)) {
                        itemList.AddRange(itemDefItemList);
                    }
                }
                GenericObjectPool.Return(pooledParentItemDefAllChildren);
            }

            return itemList;
        }

        /// <summary>
        /// Returns all the items that have the category provided.
        /// </summary>
        /// <param name="itemCategory">Category ID.</param>
        /// <param name="includeChildren">Include the children.</param>
        /// <returns>A list of items with the item category.</returns>
        public virtual IReadOnlyList<Item> ItemsWithItemCategory(ItemCategory itemCategory, bool includeChildren = false)
        {
            return ItemsWithItemCategory(itemCategory.ID, includeChildren);
        }

        /// <summary>
        /// Returns all the items that have the category provided.
        /// </summary>
        /// <param name="id">Category ID.</param>
        /// <param name="includeChildren">Include the children.</param>
        /// <returns>A list of items with the item category.</returns>
        public virtual IReadOnlyList<Item> ItemsWithItemCategory(uint id, bool includeChildren = false)
        {
            if (!includeChildren) {
                if (m_ItemsOrganizedByCategoryID.TryGetValue(id, out var itemListNoChild)) {
                    return itemListNoChild;
                }
            }

            var itemList = new List<Item>();
            if (!m_Register.ItemCategoryRegister.TryGetValue(id, out var categoryParent)) {
                Debug.LogWarning($"The Item Category with ID {id} was not found.");
                return itemList;
            }

            var pooledCategoryParentAllChildren = GenericObjectPool.Get<ItemCategory[]>();
            var categoryParentAllChildrenCount = categoryParent.GetAllChildren(ref pooledCategoryParentAllChildren, true);
            for (int i = 0; i < categoryParentAllChildrenCount; i++) {
                var categoryChild = pooledCategoryParentAllChildren[i];
                if (m_ItemsOrganizedByCategoryID.TryGetValue(categoryChild.ID, out var categoryItemList)) {
                    itemList.AddRange(categoryItemList);
                }
            }
            GenericObjectPool.Return(pooledCategoryParentAllChildren);

            return itemList;
        }

        /// <summary>
        /// Find an item with specific attribute values.
        /// </summary>
        /// <param name="itemDef">The item definition.</param>
        /// <param name="attributesOverrides">A list of the attribute overrides.</param>
        /// <returns>The item that corresponds to the search.</returns>
        public virtual Item FindItemWithOverrides(ItemDefinition itemDef, IReadOnlyList<AttributeBase> attributesOverrides)
        {
            if (m_ItemsOrganizedByItemDefinitionID.TryGetValue(itemDef.ID, out var itemDefItemList)) {

                var attributeListCount = itemDef.Category.GetItemAttributeList().Count;

                foreach (var registeredItem in itemDefItemList) {

                    bool foundTheItem = true;
                    for (int i = 0; i < attributeListCount; i++) {
                        var registeredItemAttribute = registeredItem.GetAttributeAt(i, false, false);
                        AttributeBase matchedAttribute = null;

                        if (attributesOverrides != null) {
                            for (int j = 0; j < attributesOverrides.Count; j++) {
                                var attributeOverride = attributesOverrides[j];

                                if (attributeOverride.Name == registeredItemAttribute.Name) {
                                    matchedAttribute = attributeOverride;
                                    break;
                                }
                            }
                        }

                        if (matchedAttribute == null) {
                            if (registeredItemAttribute.VariantType == VariantType.Inherit) {
                                continue;
                            }

                            foundTheItem = false;
                            break;
                        } else {
                            if (matchedAttribute.AreEquivalent(registeredItemAttribute)) { continue; }
                            foundTheItem = false;
                            break;
                        }
                    }

                    if (foundTheItem) { return registeredItem; }
                }
            }

            return null;
        }
    }
}