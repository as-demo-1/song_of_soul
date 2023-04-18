/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Opsive.UltimateInventorySystem.Editor")]
namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.Shared.Inventory;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Item is a class that is used to have a reference to the Item Instances. It is usually stored in the inventory.
    /// If the Item is mutable it can change attribute values.
    /// If the Item is immutable it can only change its attributes when it is created.
    /// </summary>
    [System.Serializable]
    public class Item : IItemIdentifier, IObjectWithID
    {

        [Tooltip("The ID of the Item.")]
        [SerializeField] protected uint m_ID;
        [Tooltip("The name of the Item.")]
        [SerializeField] protected string m_Name;
        [Tooltip("The Item Definition ID is used to save the item.")]
        [SerializeField] protected uint m_ItemDefinitionID;
        [Tooltip("The Item Definition of this Item, used to generate the attributes.")]
        [SerializeField] protected ItemDefinition m_ItemDefinition;
        [Tooltip("The collection of attributes for this Item.")]
        [ForceSerialized] [SerializeField] protected ItemAttributeCollection m_ItemAttributeCollection;

        [Tooltip("The ItemCollection which this Item is part of, could be null.")]
        [System.NonSerialized] protected ItemCollection m_ItemCollection;
        [Tooltip("The itemObjects are linked when an item is set to an ItemObject.")]
        [System.NonSerialized] protected ResizableArray<ItemObject> m_ItemObjects;
        [Tooltip("This is true only after the Item is initialized.")]
        [System.NonSerialized] [HideInInspector] protected bool m_Initialized = false;

        public uint ID {
            get => m_ID;
            internal set => m_ID = value;
        }

        public string name {
            get => string.IsNullOrWhiteSpace(m_Name) == false ? m_Name : m_ItemDefinition?.name ?? "NULL";
            set => m_Name = value;
        }

        string IObjectWithID.name {
            get => name;
            set => name = value;
        }

        uint IObjectWithID.ID {
            get => ID;
            set => ID = value;
        }

        public IInventorySystemManager Manager => m_ItemDefinition?.Manager;

        public bool IsInitialized => m_Initialized;
        public ItemDefinition ItemDefinition => m_ItemDefinition;
        public ItemCollection ItemCollection => m_ItemCollection;

        public ItemCategory Category => m_ItemDefinition != null ? m_ItemDefinition.Category : null;
        public bool IsMutable => m_ItemDefinition == null || m_ItemDefinition.IsMutable;
        public bool IsUnique => m_ItemDefinition == null || m_ItemDefinition.IsUnique;

        internal ItemAttributeCollection ItemAttributeCollection => m_ItemAttributeCollection;

        #region Static functions

        /// <summary>
        /// This static functions creates an Item from an Item Definition.
        /// A list of attributes can be provided to override the default attribute values.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition.</param>
        /// <param name="attributeOverrides">Any attribute that should override the default required attributes.</param>
        /// <returns>The created Item.</returns>
        internal static Item Create(ItemDefinition itemDefinition,
            IReadOnlyList<AttributeBase> attributeOverrides = null)
        {
            return Create(itemDefinition, RandomID.Generate(), attributeOverrides);
        }

        /// <summary>
        /// This static functions creates an Item from an Item Definition.
        /// A list of attributes can be provided to override the default attribute values.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition.</param>
        /// <param name="attributeOverrides">Any attribute that should override the default required attributes.</param>
        /// <param name="id">The item ID.</param>
        /// <returns>The created Item.</returns>
        internal static Item Create(ItemDefinition itemDefinition, uint id, IReadOnlyList<AttributeBase> attributeOverrides = null)
        {
            if (itemDefinition == null) {
                Debug.LogWarning("Cannot create Item with null Item Definition.");
                return null;
            }

            var manager = itemDefinition.Manager;

            //If the category is immutable check if item alreadyExists in Inventory System Manager.
            if (itemDefinition.IsMutable == false && itemDefinition.IsUnique == false && manager != null) {
                var registeredItem = manager.Register.ItemRegister.FindItemWithOverrides(itemDefinition, attributeOverrides);
                if (registeredItem != null) { return registeredItem; }
            }

            //Construct the new Item.
            var createdItem = new Item(itemDefinition, id);

            //Initialize does not register because register may need to replace this item by an existing equivalent one if the item is immutable.
            createdItem.Initialize(true);

            //Override any of the attributes.
            if (attributeOverrides != null) {
                createdItem.m_ItemAttributeCollection.OverrideAttributes((attributeOverrides, 0));
            }

            //Register (item can be replaced by an existing registered item).
            if (InterfaceUtility.IsNotNull(manager)) {
                manager.Register.ItemRegister.Register(ref createdItem);
            }

            return createdItem;
        }

        /// <summary>
        /// This static function duplicates an item, meaning the resulting item will have the same attribute values.
        /// A list of attributes can be provided to override the default attribute values.
        /// </summary>
        /// <param name="item">The Item used as reference for the new item.</param>
        /// <param name="attributeOverrides">Any attribute that should override the default required attributes.</param>
        /// <returns>The created Item.</returns>
        internal static Item Create(Item item, IReadOnlyList<AttributeBase> attributeOverrides = null)
        {
            return Create(item, RandomID.Generate(), attributeOverrides);
        }

        /// <summary>
        /// This static function duplicates an item, meaning the resulting item will have the same attribute values.
        /// A list of attributes can be provided to override the default attribute values.
        /// </summary>
        /// <param name="item">The Item used as reference for the new item.</param>
        /// <param name="attributeOverrides">Any attribute that should override the default required attributes.</param>
        /// <param name="id">The item ID.</param>
        /// <returns>The created Item.</returns>
        internal static Item Create(Item item, uint id, IReadOnlyList<AttributeBase> attributeOverrides = null)
        {
            if (item == null) {
                return null;
            }

            //The order matters as the item attributes will be used first
            var itemAttributeOverrides = new List<AttributeBase>();
            itemAttributeOverrides.AddRange(item.m_ItemAttributeCollection);
            if (attributeOverrides != null) {
                itemAttributeOverrides.AddRange(attributeOverrides);
            }

            return Create(item.ItemDefinition, id, itemAttributeOverrides);
        }

        /// <summary>
        /// Equals and == will say whether two items are the same (by reference for mutable, by value for immutable)
        /// AreValueEquivalent will compare two item by value even if they are mutable.
        /// </summary>
        /// <param name="lhs">The left hand side item.</param>
        /// <param name="rhs">The right hand side item.</param>
        public static bool AreValueEquivalent(Item lhs, Item rhs)
        {
            if (ReferenceEquals(rhs, null)) { return false; }
            if (ReferenceEquals(lhs, rhs)) { return true; }
            if (lhs.GetType() != rhs.GetType()) { return false; }

            if (lhs.Category != rhs.Category) { return false; }
            if (lhs.Category == null || rhs.Category == null) { return false; }
            if (lhs.ItemDefinition != rhs.ItemDefinition) { return false; }
            if (lhs.ItemDefinition == null || rhs.ItemDefinition == null) { return false; }

            if (lhs.m_ItemAttributeCollection.Count != rhs.m_ItemAttributeCollection.Count) {
                return false;
            }

            var attributeCount = lhs.m_ItemAttributeCollection.Count;
            for (int i = 0; i < attributeCount; i++) {
                var attribute = lhs.m_ItemAttributeCollection[i];
                if (rhs.TryGetAttribute(attribute.Name, out var otherAttribute)) {

                    if (attribute.VariantType == VariantType.Inherit &&
                        ReferenceEquals(rhs, rhs.ItemDefinition.DefaultItem)) {
                        continue;
                    }
                    if (otherAttribute.VariantType == VariantType.Inherit &&
                        ReferenceEquals(lhs, lhs.ItemDefinition.DefaultItem)) {
                        continue;
                    }

                    if (ReferenceEquals(attribute, otherAttribute)) {
                        continue;
                    }

                    if (attribute.AreEquivalent(otherAttribute)) {
                        continue;
                    }

                    var attributeValue = attribute.GetValueAsObject();
                    var otherAttributeValue = otherAttribute.GetValueAsObject();

                    var equal = ((attributeValue != null) && attributeValue.GetType().IsValueType)
                        ? attributeValue.Equals(otherAttributeValue)
                        : (attributeValue == otherAttributeValue);

                    if (!equal) {
                        return false;
                    }
                } else {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Stackable items must equivalent or for mutable common items the item definition need to be the same. 
        /// </summary>
        /// <param name="lhs">The left hand side item.</param>
        /// <param name="rhs">The right hand side item.</param>
        public static bool AreStackableEquivalent(Item lhs, Item rhs)
        {
            if (ReferenceEquals(rhs, null)) { return false; }
            if (ReferenceEquals(lhs, rhs)) { return true; }
            if (lhs.GetType() != rhs.GetType()) { return false; }

            if (lhs.ItemDefinition != rhs.ItemDefinition) { return false; }

            if (lhs.IsUnique) { return false; }

            if (lhs.IsMutable == false) { return true; }

            return AreValueEquivalent(lhs, rhs);
        }

        /// <summary>
        /// Check if the items are similar enough to be considered interchangeable. 
        /// </summary>
        /// <param name="lhs">The left hand side item.</param>
        /// <param name="rhs">The right hand side item.</param>
        public static bool AreSimilar(Item lhs, Item rhs)
        {
            if (ReferenceEquals(rhs, null)) { return false; }
            if (ReferenceEquals(lhs, rhs)) { return true; }
            if (lhs.GetType() != rhs.GetType()) { return false; }

            if (lhs.ItemDefinition != rhs.ItemDefinition) { return false; }

            if (lhs.IsMutable && lhs.IsUnique) { return false; }

            if (lhs.ID == rhs.ID) { return true; }

            if (lhs.IsMutable && !lhs.IsUnique) { return true; }

            return AreValueEquivalent(lhs, rhs);
        }

        #endregion

        #region Initializing

        /// <summary>
        /// Default Constructor required to create an Item using reflection.
        /// Avoid using this constructor use the Item.Create() static function instead.
        /// </summary>
        public Item()
        {
            ID = RandomID.Generate();
            m_ItemObjects = new ResizableArray<ItemObject>();
        }

        /// <summary>
        /// Item Constructor.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition.</param>
        /// <param name="id">The Item ID.</param>
        protected Item(ItemDefinition itemDefinition, uint id)
        {
            ID = id;
            m_ItemObjects = new ResizableArray<ItemObject>();
            m_ItemDefinition = itemDefinition;
            m_ItemDefinitionID = m_ItemDefinition?.ID ?? 0;
            m_Name = m_ItemDefinition?.name ?? "NULL";
        }

        /// <summary>
        /// Initializes and updates the attributes.
        /// </summary>
        /// <param name="force">Force initialize the object.</param>
        /// <param name="updateAttributes">Update the attributes to make sure they are correct?</param>
        public virtual void Initialize(bool force, bool updateAttributes = true)
        {
            if (m_Initialized && !force) { return; }

            if (m_ItemDefinition == null) {
                if (m_ItemDefinitionID != 0) {
                    m_ItemDefinition = InventorySystemManager.GetItemDefinition(m_ItemDefinitionID);
                }

                if (m_ItemDefinition == null) {
                    Debug.LogError($"Item Definition is null: {name} ID:{m_ID}");
                    return;
                }
            }

            m_ItemDefinitionID = m_ItemDefinition.ID;
            m_Name = m_ItemDefinition.name;

            Deserialize();

            if (m_ItemObjects.Array == null) { m_ItemObjects.Initialize(0); }

            // The item needs to be set as initialized before the attributes are updated or a Infinite loop will occur.
            m_Initialized = true;

            if (updateAttributes) {
                UpdateAttributes();
            }
        }

        /// <summary>
        /// Deserialize the Item.
        /// </summary>
        protected void Deserialize()
        {
            DeserializeAttributeCollections();
        }

        /// <summary>
        /// Deserializes the Attribute Collection, use initializeAttributeCollection instead of this to deserialize once.
        /// </summary>
        protected void DeserializeAttributeCollections()
        {
            if (m_ItemAttributeCollection == null) { m_ItemAttributeCollection = new ItemAttributeCollection(); }
            m_ItemAttributeCollection.Initialize(this, false);
        }

        /// <summary>
        /// Serialize the Item by serializing its attributes.
        /// </summary>
        public void Serialize()
        {
            if (m_ItemAttributeCollection == null) {
                return;
            }

            m_ItemAttributeCollection.Serialize();
        }

        /// <summary>
        /// Returns the Item Definition that the identifier uses.
        /// </summary>
        /// <returns>The Item Definition that the identifier uses.</returns>
        public ItemDefinitionBase GetItemDefinition()
        {
            return m_ItemDefinition;
        }

        /// <summary>
        /// Return the Item Category Identifier for this Item.
        /// </summary>
        /// <returns>The Item Category of the Item.</returns>
        public IItemCategoryIdentifier GetItemCategory()
        {
            return Category;
        }

        #endregion

        #region ItemChanges

        /// <summary>
        /// Used by an itemCollection, this allows the item to know where it belongs to.
        /// Note that immutable items can be part of many itemCollections.
        /// </summary>
        /// <param name="itemCollection">The itemCollection containing this item.</param>
        public virtual void RemoveItemCollection(ItemCollection itemCollection)
        {
            if (ItemDefinition.IsMutable == false) { return; }
            if (m_ItemCollection != itemCollection) {
                Debug.LogWarning("Item was removed from an Item Collection which was not set on the item.");
                return;
            }
            m_ItemCollection = null;
        }

        /// <summary>
        /// Used by an itemCollection, this allows the item to know where it belongs to.
        /// Note that immutable items can be part of many itemCollections.
        /// </summary>
        /// <param name="itemCollection">The ItemCollection that will contain the item.</param>
        public virtual void AddItemCollection(ItemCollection itemCollection)
        {
            if (ItemDefinition.IsMutable == false || m_ItemCollection == itemCollection) { return; }
            if (m_ItemCollection != null && m_ItemCollection != itemCollection) {
                Debug.LogWarning($"The Mutable Item {name} is unable to be added to a new Item Collection {itemCollection} when it is already a member of an existing Item Collection.");
                return;
            }
            m_ItemCollection = itemCollection;
        }

        /// <summary>
        /// Bind the Item to the specified itemObject
        /// </summary>
        /// <param name="itemObject">The item object.</param>
        public void BindToItemObject(ItemObject itemObject)
        {
            if (itemObject == null) { return; }
            if (itemObject.Item != this) { return; }
            if (m_ItemObjects.Contains(itemObject)) { return; }

            m_ItemObjects.Add(itemObject);
        }

        /// <summary>
        /// Breaks binding with ItemObject
        /// </summary>
        /// <param name="itemObject">The item object.</param>
        public void RemoveBindingToItemObject(ItemObject itemObject)
        {
            if (itemObject == null) { return; }
            m_ItemObjects.Remove(itemObject);
        }

        /// <summary>
        /// Check if the item is bound to the ItemObject specified.
        /// </summary>
        /// <param name="itemObject">The itemObject.</param>
        /// <returns>True if the object is bound to the itemObject.</returns>
        public bool IsBoundToItemObject(ItemObject itemObject)
        {
            for (int i = 0; i < m_ItemObjects.Count; i++) {
                if (itemObject == m_ItemObjects[i]) { return true; }
            }

            return false;
        }

        #endregion

        #region ItemObjects

        /// <summary>
        /// Get the number of itemObject binded to this item.
        /// </summary>
        /// <returns>The item object count.</returns>
        public int GetItemObjectCount()
        {
            return m_ItemObjects.Count;
        }

        /// <summary>
        /// Get the Item Object at the index provided.
        /// </summary>
        /// <param name="index">The index to retrieve the object of.</param>
        /// <returns>The Item Object at the index provided.</returns>
        public ItemObject GetItemObjectAt(int index)
        {
            if (index < 0 || index >= m_ItemObjects.Count) {
                return null;
            }
            return m_ItemObjects[index];
        }

        /// <summary>
        /// Get the last added ItemObject.
        /// </summary>
        /// <returns>The last added Item Object.</returns>
        public ItemObject GetLastItemObject()
        {
            if (m_ItemObjects.Count == 0) {
                return null;
            }
            return m_ItemObjects[m_ItemObjects.Count - 1];
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Update the attributes so that they match the category attribute definitions.
        /// Does not match the value, only the names and types.
        /// </summary>
        /// <returns>Returns false if something went wrong with the attributes update.</returns>
        public virtual bool UpdateAttributes()
        {
            if (Category == null) {
                Debug.LogErrorFormat("Category should never be null! ItemDefinition is: {0}", m_ItemDefinition);
                return false;
            }

            var requiredItemAttributes = Category.GetItemAttributeList();
            var result = m_ItemAttributeCollection.UpdateAttributesToMatchList((requiredItemAttributes, 0), true);

            if (this == m_ItemDefinition.DefaultItem) {
                ReevaluateAttributes();
                var itemDefs = GenericObjectPool.Get<ItemDefinition[]>();
                var itemDefChildrenCount = m_ItemDefinition.GetAllChildren(ref itemDefs, false);
                for (int i = 0; i < itemDefChildrenCount; i++) {
                    itemDefs[i]?.DefaultItem?.ReevaluateAttributes();
                }
                GenericObjectPool.Return(itemDefs);
            }

            return result;
        }

        /// <summary>
        /// Override an item attribute, this can only be done on mutable items.
        /// Note that you can only override attributes that have the same name & types as defined in the category.
        /// </summary>
        /// <param name="attribute">The new Attribute.</param>
        /// <returns>Returns false if something went wrong.</returns>
        public virtual bool OverrideAttribute(AttributeBase attribute)
        {
            if (IsMutable == false) {
                //Cannot change attributes on Immutable Items.
                return false;
            }
            var result = m_ItemAttributeCollection.OverrideAttribute(attribute, false);
            UpdateAttributes();
            return result;
        }

        /// <summary>
        /// Override multiple attributes at a time.
        /// This can only be done on mutable items.
        /// Note that you can only override attributes that have the same name & types as defined in the category.
        /// </summary>
        /// <param name="newAttributes">A list of attributes.</param>
        public virtual void OverrideAttributes(IReadOnlyList<AttributeBase> newAttributes)
        {
            if (IsMutable == false) {
                Debug.LogWarning("Cannot change attributes on Immutable Items.");
                return;
            }
            m_ItemAttributeCollection.OverrideAttributes((newAttributes, 0), false);
            UpdateAttributes();
        }

        /// <summary>
        /// Returns true if the item contains an attribute with that attribute name.
        /// By default this function will also look into the itemDefinition and ItemCategory attributes.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="includeItemDefinitionAttributes">Should the Item Definition attributes be checked.</param>
        /// <param name="includeCategoryAttributes">Should the Item Category attributes be checked.</param>
        /// <returns>True if it has the attribute.</returns>
        public virtual bool HasAttribute(string attributeName, bool includeItemDefinitionAttributes = true, bool includeCategoryAttributes = true)
        {
            var result = m_ItemAttributeCollection.ContainsAttribute(attributeName);
            if (result == false && includeItemDefinitionAttributes) {
                result = ItemDefinition.HasAttribute(attributeName, false);
            }
            if (result == false && includeCategoryAttributes) {
                return Category.HasCategoryAttribute(attributeName);
            }
            return result;
        }

        /// <summary>
        /// Try to get the attribute.
        /// Returns false if the attribute was not found.
        /// Use this function if you are not sure if the attribute exists or if you are unsure of its type.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attribute">Output of the attribute.</param>
        /// <param name="includeItemDefinitionAttributes">Should the Item Definition attributes be checked.</param>
        /// <param name="includeCategoryAttributes">Should the Item Category attributes be checked.</param>
        /// <returns>True if the attribute exists.</returns>
        public virtual bool TryGetAttribute(string attributeName, out AttributeBase attribute, bool includeItemDefinitionAttributes = true, bool includeCategoryAttributes = true)
        {
            if (m_Initialized == false) {
                Initialize(false);
            }

            var result = m_ItemAttributeCollection.TryGetAttribute(attributeName, out attribute);
            if (result == false && includeItemDefinitionAttributes) {
                result = ItemDefinition.TryGetAttribute(attributeName, out attribute, false);
            }
            if (result == false && includeCategoryAttributes) {
                return Category.TryGetCategoryAttribute(attributeName, out attribute);
            }

            return result;
        }

        /// <summary>
        /// Try to get the attribute.
        /// Returns false if the attribute was not found.
        /// Use this function if you are not sure if the attribute exists or if you are unsure of its type.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="includeItemDefinitionAttributes">Should the Item Definition attributes be checked.</param>
        /// <param name="includeCategoryAttributes">Should the Item Category attributes be checked.</param>
        /// <returns>True if the attribute exists.</returns>
        public virtual AttributeBase GetAttribute(string attributeName, bool includeItemDefinitionAttributes = true, bool includeCategoryAttributes = true)
        {
            if (m_Initialized == false) {
                Initialize(false);
            }

            AttributeBase attribute = null;

            var result = m_ItemAttributeCollection.TryGetAttribute(attributeName, out attribute);
            if (result == false && includeItemDefinitionAttributes) {
                result = ItemDefinition.TryGetAttribute(attributeName, out attribute, false);
            }
            if (result == false && includeCategoryAttributes) {
                Category.TryGetCategoryAttribute(attributeName, out attribute);
            }

            return attribute;
        }

        /// <summary>
        /// Get the attribute knowing its type in advance.
        /// Use this function if you know that the attribute should exist.
        /// </summary>
        /// <typeparam name="T">The type of the attribute</typeparam>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="includeItemDefinitionAttributes">Should the Item Definition attributes be checked.</param>
        /// <param name="includeCategoryAttributes">Should the Item Category attributes be checked.</param>
        /// <returns>Get the attribute.</returns>
        public virtual T GetAttribute<T>(string attributeName, bool includeItemDefinitionAttributes = true, bool includeCategoryAttributes = true) where T : AttributeBase
        {
            var result = m_ItemAttributeCollection.GetAttribute<T>(attributeName);
            if (result == null && includeItemDefinitionAttributes) {
                result = ItemDefinition.GetAttribute<T>(attributeName, false);
            }
            if (result == null && includeCategoryAttributes) {
                return Category.GetCategoryAttribute<T>(attributeName);
            }

            return result;
        }

        /// <summary>
        /// Try get the attribute value.
        /// Use this function if you only care about the value of the attribute.
        /// </summary>
        /// <typeparam name="T">The attribute value type</typeparam>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attributeValue">Output of the attribute value.</param>
        /// <param name="includeItemDefinitionAttributes">Should the Item Definition attributes be checked.</param>
        /// <param name="includeCategoryAttributes">Should the Item Category attributes be checked.</param>
        /// <returns>True if the attribute exists.</returns>
        public virtual bool TryGetAttributeValue<T>(string attributeName, out T attributeValue, bool includeItemDefinitionAttributes = true, bool includeCategoryAttributes = true)
        {
            if (m_Initialized == false) {
                attributeValue = default;
                return false;
            }
            var result = m_ItemAttributeCollection.TryGetAttributeValue<T>(attributeName, out attributeValue);
            if (result == false && includeItemDefinitionAttributes) {
                result = ItemDefinition.TryGetAttributeValue<T>(attributeName, out attributeValue, false);
            }
            if (result == false && includeCategoryAttributes) {
                return Category.TryGetCategoryAttributeValue<T>(attributeName, out attributeValue);
            }

            return result;
        }

        /// <summary>
        /// Returns all the item attributes (does not include Item Definition or Item Category attributes).
        /// </summary>
        /// <returns>Returns a list of attributes.</returns>
        public IReadOnlyList<AttributeBase> GetAttributeList()
        {
            return m_ItemAttributeCollection;
        }

        /// <summary>
        /// Returns a amount of the attributes relevant to this item.
        /// </summary>
        /// <param name="includeItemDefinitionAttributes">Should the Item Definition attributes be checked.</param>
        /// <param name="includeCategoryAttributes">Should the Item Category attributes be checked.</param>
        /// <returns>The number of attributes.</returns>
        public int GetAttributeCount(bool includeItemDefinitionAttributes = true, bool includeCategoryAttributes = true)
        {
            return m_ItemAttributeCollection.Count
                   + (includeItemDefinitionAttributes ? ItemDefinition.GetAttributeCount(false) : 0)
                   + (includeCategoryAttributes ? Category.GetCategoryAttributeCount() : 0);
        }

        /// <summary>
        /// Returns the attribute at the index provided.
        /// </summary>
        /// <param name="index">The index of the attribute.</param>
        /// <param name="includeItemDefinitionAttributes">Should the Item Definition attributes be checked.</param>
        /// <param name="includeCategoryAttributes">Should the Item Category attributes be checked.</param>
        /// <returns>Returns the attribute.</returns>
        public AttributeBase GetAttributeAt(int index, bool includeItemDefinitionAttributes, bool includeCategoryAttributes)
        {
            if (index < m_ItemAttributeCollection.Count) { return m_ItemAttributeCollection[index]; }

            index -= m_ItemAttributeCollection.Count;

            if (includeItemDefinitionAttributes) {

                if (index < ItemDefinition.GetAttributeCount(false)) { return ItemDefinition.GetAttributeAt(index, false); }

                index -= ItemDefinition.GetAttributeCount(false);
            }

            if (includeCategoryAttributes) {
                if (index < Category.GetCategoryAttributeCount()) { return Category.GetCategoryAttributeAt(index); }
            }

            return null;
        }

        /// <summary>
        /// Reevaluate the item attributes.
        /// </summary>
        public void ReevaluateAttributes()
        {
            for (int i = 0; i < m_ItemAttributeCollection.Count; i++) {
                if (m_ItemAttributeCollection[i].IsPreEvaluated) {
                    m_ItemAttributeCollection[i].ReevaluateValue(false);
                }
            }
        }

        #endregion

        #region Equality

        /// <summary>
        /// Checks if this item is value equivalent to another. Equivalent and Equals is not the same!
        /// </summary>
        /// <param name="otherItem">The other item.</param>
        /// <returns>True if equivalent.</returns>
        public bool ValueEquivalentTo(Item otherItem)
        {
            return AreValueEquivalent(this, otherItem);
        }

        /// <summary>
        /// Stackable items must equivalent or for mutable common items the item definition need to be the same. 
        /// </summary>
        /// <param name="otherItem">The other item.</param>
        /// <returns>True if equivalent.</returns>
        public bool StackableEquivalentTo(Item otherItem)
        {
            return AreStackableEquivalent(this, otherItem);
        }

        /// <summary>
        /// Check if the items are similar enough to be considered interchangeable.
        /// </summary>
        /// <param name="otherItem">The other item.</param>
        /// <returns>True if equivalent.</returns>
        public bool SimilarTo(Item otherItem)
        {
            return AreSimilar(this, otherItem);
        }

        #endregion

        /// <summary>
        /// Returns the custom string.
        /// </summary>
        /// <returns>The custom string.</returns>
        public override string ToString()
        {
            return string.Format("{0} ({1})", m_ItemDefinition, ID);
        }
    }
}