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
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Category class is used to organize items. Categories can be chained in a hierarchy by setting the category parent.
    /// The ItemDefinition can be part of multiple categories.
    /// </summary>
    public class ItemCategory : ObjectCategoryBase<ItemCategory, ItemDefinition>, IItemCategoryIdentifier
    {
        [Tooltip(
            "The attributes of the category. These are static constant for all ItemDefinition/Item that are part of this category.")]
        [SerializeField]
        protected ItemCategoryAttributeCollection
            m_ItemCategoryAttributeCollection; //Abstracted from the outside since some functions should not be public outside of this object
        [Tooltip(
            "Attributes that are added to the ItemDefinitions. These are 'static' meaning they won't change once the game starts.")]
        [SerializeField] protected RequiredItemDefinitionAttributeCollection m_RequiredItemDefinitionAttributes;
        [Tooltip(
            "Attributes that are added to the Item. These are either 'mutable' or 'immutable' meaning that the item can either change its values at any time or only once when created, respectively.")]
        [SerializeField] protected RequiredItemAttributeCollection m_RequiredItemAttributes;
        [Tooltip(
            "If true the attributes on the items in this category can be modified at anytime, otherwise they must stay the same once the item is created.")]
        [SerializeField] protected bool m_IsMutable;
        [Tooltip(
            "If true the items in this category will not stack, Unique items can be duplicated and given a new ID when added to a collection to stay unique.")]
        [SerializeField] protected bool m_IsUnique;

        public bool IsMutable {
            get => m_IsMutable;
            internal set {
                if (m_IsMutable == value) { return; }

                m_IsMutable = value;
                m_Dirty = true;
            }
        }

        public bool IsUnique {
            get => m_IsUnique;
            internal set {
                if (m_IsUnique == value) { return; }

                m_IsUnique = value;
                m_Dirty = true;
            }
        }

        internal override bool Dirty {
            get => m_Dirty
                   | (m_ItemCategoryAttributeCollection?.Dirty ?? false)
                   | (m_RequiredItemDefinitionAttributes?.Dirty ?? false)
                   | (m_RequiredItemAttributes?.Dirty ?? false);
            set {
                m_Dirty = value;
                if (m_ItemCategoryAttributeCollection != null) { m_ItemCategoryAttributeCollection.Dirty = value; }

                if (m_RequiredItemDefinitionAttributes != null) { m_RequiredItemDefinitionAttributes.Dirty = value; }

                if (m_RequiredItemAttributes != null) { m_RequiredItemAttributes.Dirty = value; }
            }
        }

        internal ItemCategoryAttributeCollection ItemCategoryAttributeCollection => m_ItemCategoryAttributeCollection;

        internal RequiredItemDefinitionAttributeCollection ItemDefinitionAttributeCollection =>
            m_RequiredItemDefinitionAttributes;

        internal RequiredItemAttributeCollection ItemAttributeCollection => m_RequiredItemAttributes;

        #region Initializing

        /// <summary>
        /// Create an Item Category.
        /// </summary>
        /// <param name="name">The name of the category.</param>
        /// <param name="isMutable">If mutable all the items in this category will be mutable.</param>
        /// <param name="isUnique">If unique all the items in this category will be unique.</param>
        /// <param name="isAbstract">If abstract the category can't have direct ItemDefinitions.</param>
        /// <param name="manager">The inventory system manager where the category will be registered to.</param>
        /// <returns>The created ItemCategory.</returns>
        internal static ItemCategory Create(string name, bool isMutable = true, bool isUnique = true,
            bool isAbstract = false, IInventorySystemManager manager = null)
        {
            //construct the ItemCategory
            var itemCategory = CreateInstance<ItemCategory>();

            itemCategory.Dirty = true;

            itemCategory.m_Manager = manager;
            itemCategory.ID = RandomID.Generate();
            itemCategory.name = name;
            itemCategory.m_IsMutable = isMutable;
            itemCategory.m_IsUnique = isUnique;
            itemCategory.m_IsAbstract = isAbstract;

            itemCategory.m_ItemCategoryAttributeCollection = new ItemCategoryAttributeCollection();
            itemCategory.m_RequiredItemAttributes = new RequiredItemAttributeCollection();
            itemCategory.m_RequiredItemDefinitionAttributes = new RequiredItemDefinitionAttributeCollection();

#if UNITY_EDITOR
            UnityEngine.Random.InitState(name.GetHashCode());
            itemCategory.m_Color = new Color(UnityEngine.Random.value, UnityEngine.Random.value,
                UnityEngine.Random.value, 1);
#endif

            if (!itemCategory.Initialize(true)) { return null; }

            return itemCategory;
        }

        /// <summary>
        /// Initializes the category and registers it.
        /// </summary>
        /// <returns>Returns false if not initialized correctly.</returns>
        public override bool Initialize(bool force)
        {
            if (m_Initialized && !force) { return true; }

            base.Initialize(force);

            // It is required to check .Equals as the interface could point to a unity Object.
            if (InterfaceUtility.IsNotNull(m_Manager)) {
                var success = m_Manager.Register?.ItemCategoryRegister?.Register(this);
                if (success.HasValue) {
                    m_Initialized = true;
                    return success.Value;
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying) {
                m_Initialized = true;
                return true;
            }
#endif

            if (InventorySystemManager.IsNull) {
                m_Initialized = false;
                return false;
            }

            m_Manager = InventorySystemManager.Manager;
            m_Initialized = m_Manager.Register.ItemCategoryRegister.Register(this);
            return m_Initialized;
        }

        /// <summary>
        /// Deserialize the ItemCategory.
        /// </summary>
        internal override void Deserialize()
        {
            base.Deserialize();

            DeserializeAttributeCollections();
        }

        /// <summary>
        /// Deserializes all the attribute collections on the Item category.
        /// </summary>
        protected void DeserializeAttributeCollections()
        {
            if (m_ItemCategoryAttributeCollection == null) {
                m_ItemCategoryAttributeCollection = new ItemCategoryAttributeCollection();
            }

            m_ItemCategoryAttributeCollection.Initialize(this, true);

            if (m_RequiredItemAttributes == null) { m_RequiredItemAttributes = new RequiredItemAttributeCollection(); }

            m_RequiredItemAttributes.Initialize(this, true);

            if (m_RequiredItemDefinitionAttributes == null) {
                m_RequiredItemDefinitionAttributes = new RequiredItemDefinitionAttributeCollection();
            }

            m_RequiredItemDefinitionAttributes.Initialize(this, true);
        }

        /// <summary>
        /// Serialize the category and it's collections.
        /// </summary>
        public override void Serialize()
        {
            base.Serialize();
            m_ItemCategoryAttributeCollection?.Serialize();
            m_RequiredItemDefinitionAttributes?.Serialize();
            m_RequiredItemAttributes?.Serialize();
        }

        #endregion

        #region Add ItemDefinitions and Category parents

        /// <summary>
        /// Can the category add this category as a parent.
        /// </summary>
        /// <param name="otherCategory">The other Category.</param>
        /// <returns>True if the category can be added as parent.</returns>
        public override bool AddParentCondition(ItemCategory otherCategory)
        {
            var valid = base.AddParentCondition(otherCategory);
            if (valid == false) { return false; }

            //Check if mismatch in category attributes.
            var list = otherCategory.GetCategoryAttributeList();
            for (var i = 0; i < list.Count; i++) {
                var newParentCategoryAttribute = list[i];
                if (!TryGetCategoryAttribute(newParentCategoryAttribute.Name, out var existingAttribute)) { continue; }

                if (existingAttribute.GetType() == newParentCategoryAttribute.GetType()) { continue; }

                //Debug.LogWarning($"Cannot add the category '{otherCategory}' as parent. The category has an attribute '{existingAttribute.Name}' with the same name but a different type as the specified child '{this}'.");
                return false;
            }

            //Check if mismatch in Item Definition attributes.
            var bases = otherCategory.GetDefinitionAttributeList();
            for (var i = 0; i < bases.Count; i++) {
                var parentRequired = bases[i];
                foreach (var thisRequired in GetDefinitionAttributeList()) {
                    if (parentRequired.Name == thisRequired.Name &&
                        parentRequired.GetType() != thisRequired.GetType()) {
                        //Debug.LogWarning($"Cannot add the category '{otherCategory}' as parent. The category has an attribute '{parentRequired.Name}' with the same name but a different type as the specified child '{this}'.");
                        return false;
                    }
                }
            }

            //Check if mismatch in Item attributes.
            var requiredItemAttributes = otherCategory.GetItemAttributeList();
            for (var i = 0; i < requiredItemAttributes.Count; i++) {
                var parentRequired = requiredItemAttributes[i];
                foreach (var thisRequired in GetItemAttributeList()) {
                    if (parentRequired.Name == thisRequired.Name &&
                        parentRequired.GetType() != thisRequired.GetType()) {
                        //Debug.LogWarning($"Cannot add the category '{otherCategory}' as parent. The category has an attribute '{parentRequired.Name}' with the same name but a different type as the specified child '{this}'.");

                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a parent to the category.
        /// </summary>
        /// <param name="newParent">The new parent.</param>
        /// <returns>Returns true if the parent was successfully added.</returns>
        public override bool AddParent(ItemCategory newParent)
        {
            var valid = base.AddParent(newParent);
            if (valid == false) { return false; }

            UpdateCategoryAttributes();
            UpdateDefinitionAttributes();
            UpdateItemAttributes();

            return true;
        }

        /// <summary>
        /// Remove a category parent.
        /// </summary>
        /// <param name="parent">The ItemCategory parent that needs to be removed.</param>
        /// <returns>True if the parent was successfully removed.</returns>
        public override bool RemoveParent(ItemCategory parent)
        {
            var valid = base.RemoveParent(parent);
            if (valid == false) { return false; }

            UpdateCategoryAttributes();
            UpdateDefinitionAttributes();
            UpdateItemAttributes();
            return true;
        }

        /// <summary>
        /// Adds a parent to the category.
        /// </summary>
        /// <param name="newParent">The new parent.</param>
        /// <returns>Returns true if the parent was successfully added.</returns>
        internal bool AddParentNoNotify(ItemCategory newParent)
        {
            return base.AddParent(newParent);
        }

        /// <summary>
        /// Remove a category parent.
        /// </summary>
        /// <param name="parent">The ItemCategory parent that needs to be removed.</param>
        /// <returns>True if the parent was successfully removed.</returns>
        internal bool RemoveParentNoNotify(ItemCategory parent)
        {
            return base.RemoveParent(parent);
        }

        #endregion

        #region Add Required Item and ItemDefinition Attributes

        /// <summary>
        /// Update the ItemDefinition attributes, it also updates all the connections so that every object is up to date.
        /// </summary>
        /// <returns>Returns false if something went wrong with the attribute update.</returns>
        public virtual bool UpdateDefinitionAttributes()
        {
            var inheritedAttributeArrayPool = GenericObjectPool.Get<AttributeBase[]>();
            var inheritedAttributesCount = GetInheritedDefinitionAttributes(ref inheritedAttributeArrayPool);

            var result =
                m_RequiredItemDefinitionAttributes.UpdateAttributesToMatchList(
                    (inheritedAttributeArrayPool, 0, inheritedAttributesCount), false);

            ReevaluateDefinitionAttributes();

            var childrenCount = m_Children.Count;
            var childrenArray = m_Children.Array;
            for (int i = 0; i < childrenCount; i++) { childrenArray[i].UpdateDefinitionAttributes(); }

            var itemDefinitionsCount = m_Elements.Count;
            var itemDefinitionsArray = m_Elements.Array;
            for (int i = 0; i < itemDefinitionsCount; i++) { itemDefinitionsArray[i].UpdateAttributes(); }

            GenericObjectPool.Return(inheritedAttributeArrayPool);

            return result;
        }

        /// <summary>
        /// Update the Item attributes, it also updates all the connections so that every object is up to date.
        /// </summary>
        /// <returns>Returns false if something went wrong with the attribute update.</returns>
        public virtual bool UpdateItemAttributes()
        {
            var inheritedAttributeArrayPool = GenericObjectPool.Get<AttributeBase[]>();
            var inheritedAttributesCount = GetInheritedItemAttributes(ref inheritedAttributeArrayPool);

            var result =
                m_RequiredItemAttributes.UpdateAttributesToMatchList(
                    (inheritedAttributeArrayPool, 0, inheritedAttributesCount), false);

            ReevaluateItemAttributes();

            var childrenCount = m_Children.Count;
            var childrenArray = m_Children.Array;
            for (int i = 0; i < childrenCount; i++) { childrenArray[i].UpdateItemAttributes(); }

            var itemRegister = m_Manager?.Register?.ItemRegister;
            if (itemRegister != null) {
                var list = itemRegister.ItemsWithItemCategory(ID, false);
                for (var i = 0; i < list.Count; i++) {
                    var item = list[i];
                    if (item == null || item.ItemDefinition == null) {
                        continue;
                    }
                    item.UpdateAttributes();
                }
            } else {
                var pooledAllChildrenElements = GenericObjectPool.Get<ItemDefinition[]>();
                var allChildrenElementsCount = GetAllChildrenElements(ref pooledAllChildrenElements);
                for (int i = 0; i < allChildrenElementsCount; i++) {
                    pooledAllChildrenElements[i].DefaultItem.UpdateAttributes();
                }

                GenericObjectPool.Return(pooledAllChildrenElements);
            }

            GenericObjectPool.Return(inheritedAttributeArrayPool);

            return result;
        }

        /// <summary>
        /// Add multiple Item Definition Attributes.
        /// </summary>
        /// <param name="requiredAttributes">The required attributes.</param>
        /// <param name="addEvenIfNothingToOverwrite">If true the attributes are added to the collection even if they are new.</param>
        /// <param name="force">Force the override even if the type does not match the current same named attribute.</param>
        public virtual void AddDefinitionAttributes(IReadOnlyList<AttributeBase> requiredAttributes,
            bool addEvenIfNothingToOverwrite = true, bool force = false)
        {
            foreach (var requiredAttribute in requiredAttributes) {
                AddDefinitionAttribute(requiredAttribute, addEvenIfNothingToOverwrite, force);
            }
        }

        /// <summary>
        /// Add an itemDefinition attribute.
        /// </summary>
        /// <param name="requiredAttribute">The required attribute.</param>
        /// <param name="addEvenIfNothingToOverwrite">If true the attributes are added to the collection even if they are new.</param>
        /// <param name="force">Force the override even if the type does not match the current same named attribute.</param>
        /// <returns>False if the attribute was not added correctly.</returns>
        public virtual bool AddDefinitionAttribute(AttributeBase requiredAttribute,
            bool addEvenIfNothingToOverwrite = true, bool force = false)
        {
            var valid = IsNewAttributeValid(requiredAttribute.Name, requiredAttribute.GetType(), 1);
            if (!valid) {
                if (!force) { return false; }

                RemoveAttributeFromAll(requiredAttribute.Name, Relation.Family, false);
            }

            var result =
                m_RequiredItemDefinitionAttributes.OverrideAttribute(requiredAttribute, addEvenIfNothingToOverwrite,
                    force);

            if (!result) { return false; }

            UpdateDefinitionAttributes();

            return true;
        }

        /// <summary>
        /// Add multiple Item attributes.
        /// </summary>
        /// <param name="requiredAttributes">The required attributes.</param>
        /// <param name="addEvenIfNothingToOverwrite">If true the attributes are added to the collection even if they are new.</param>
        /// <param name="force">Force the override even if the type does not match the current same named attribute.</param>
        public virtual void AddItemAttributes(IReadOnlyList<AttributeBase> requiredAttributes,
            bool addEvenIfNothingToOverwrite = true, bool force = false)
        {
            foreach (var requiredAttribute in requiredAttributes) {
                AddItemAttributeInternal(requiredAttribute, addEvenIfNothingToOverwrite, force);
            }
        }

        /// <summary>
        /// Add an Item attribute.
        /// </summary>
        /// <param name="requiredAttribute">The required attribute.</param>
        /// <param name="addEvenIfNothingToOverwrite">If true the attributes are added to the collection even if they are new.</param>
        /// <param name="force">Force the override even if the type does not match the current same named attribute.</param>
        /// <returns>False if the attribute was not added correctly.</returns>
        public virtual bool AddItemAttribute(AttributeBase requiredAttribute, bool addEvenIfNothingToOverwrite = true,
            bool force = false)
        {
            return AddItemAttributeInternal(requiredAttribute, addEvenIfNothingToOverwrite, force);
        }

        /// <summary>
        /// Add an Item attribute.
        /// </summary>
        /// <param name="requiredAttribute">The required attribute.</param>
        /// <param name="addEvenIfNothingToOverwrite">If true the attributes are added to the collection even if they are new.</param>
        /// <param name="force">Force the override even if the type does not match the current same named attribute.</param>
        /// <returns>False if the attribute was not added correctly.</returns>
        protected virtual bool AddItemAttributeInternal(AttributeBase requiredAttribute,
            bool addEvenIfNothingToOverwrite = true, bool force = false)
        {
            var valid = IsNewAttributeValid(requiredAttribute.Name, requiredAttribute.GetType(), 2);
            if (!valid) {
                if (!force) { return false; }

                RemoveAttributeFromAll(requiredAttribute.Name, Relation.Family, false);
            }

            var result =
                m_RequiredItemAttributes.OverrideAttribute(requiredAttribute, addEvenIfNothingToOverwrite, force);

            if (!result) { return false; }

            UpdateItemAttributes();

            return true;
        }

        #endregion

        #region Get Required Item and ItemDefinition Attributes

        /// <summary>
        /// Returns the quantity of required Item definition attributes that are inherited.
        /// </summary>
        /// <returns>Returns the count of itemDefinition attribute inherited.</returns>
        public int GetInheritedDefinitionAttributeCount()
        {
            var count = 0;
            var parentCount = m_Parents.Count;
            var parentArray = m_Parents.Array;
            for (int i = 0; i < parentCount; i++) {
                if (parentArray[i].m_RequiredItemDefinitionAttributes != null) {
                    count += parentArray[i].m_RequiredItemDefinitionAttributes.Count;
                }
            }

            return count;
        }

        /// <summary>
        /// Gets the required itemDefinition attributes.
        /// </summary>
        /// <param name="attributes">Reference to the array of attributes. Can be resized up.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetInheritedDefinitionAttributes(ref AttributeBase[] attributes)
        {
            var index = 0;
            var parentCount = m_Parents.Count;
            var parentArray = m_Parents.Array;
            for (int i = 0; i < parentCount; i++) {
                if (parentArray[i].m_RequiredItemDefinitionAttributes == null) { continue; }

                var itemDefAttributes = parentArray[i].m_RequiredItemDefinitionAttributes;
                for (int j = 0; j < itemDefAttributes.Count; j++) {
                    TypeUtility.ResizeIfNecessary(ref attributes, index);
                    attributes[index] = itemDefAttributes[j];
                    index++;
                }
            }

            return index;
        }

        /// <summary>
        /// Returns a list of the Item definition attributes.
        /// </summary>
        /// <returns>The list of attributes.</returns>
        public IReadOnlyList<AttributeBase> GetDefinitionAttributeList()
        {
            return m_RequiredItemDefinitionAttributes;
        }

        /// <summary>
        /// Returns true if the item definition attribute exist.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>True if the attribute exists in the collection.</returns>
        public bool HasDefinitionAttribute(string attributeName)
        {
            return m_RequiredItemDefinitionAttributes.ContainsAttribute(attributeName);
        }

        /// <summary>
        /// Returns the quantity of required Item attributes that are inherited.
        /// </summary>
        /// <returns>Returns the count of item attribute inherited.</returns>
        public int GetInheritedItemAttributeCount()
        {
            var count = 0;
            var parentCount = m_Parents.Count;
            var parentArray = m_Parents.Array;
            for (int i = 0; i < parentCount; i++) {
                if (parentArray[i].m_RequiredItemAttributes != null) {
                    count += parentArray[i].m_RequiredItemAttributes.Count;
                }
            }

            return count;
        }

        /// <summary>
        /// Gets the required item attributes.
        /// </summary>
        /// <param name="attributes">Reference to the array of attributes. Can be resized up.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetInheritedItemAttributes(ref AttributeBase[] attributes)
        {
            var index = 0;
            var parentCount = m_Parents.Count;
            var parentArray = m_Parents.Array;
            for (int i = 0; i < parentCount; i++) {
                if (parentArray[i].m_RequiredItemAttributes == null) { continue; }

                var itemAttributes = parentArray[i].m_RequiredItemAttributes;
                for (int j = 0; j < itemAttributes.Count; j++) {
                    TypeUtility.ResizeIfNecessary(ref attributes, index);
                    attributes[index] = itemAttributes[j];
                    index++;
                }
            }

            return index;
        }

        /// <summary>
        /// Returns a list of the item attributes.
        /// </summary>
        /// <returns>The attribute list.</returns>
        public IReadOnlyList<AttributeBase> GetItemAttributeList()
        {
            return m_RequiredItemAttributes;
        }

        /// <summary>
        /// Returns true if the item attribute exists.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>True if the attribute exists in the collection.</returns>
        public bool HasItemAttribute(string attributeName)
        {
            return m_RequiredItemAttributes.ContainsAttribute(attributeName);
        }

        /// <summary>
        /// Returns the quantity of required Item and Item Definition attributes.
        /// </summary>
        /// <returns>Returns the count of item and Item Definition attribute.</returns>
        public int GetItemAndDefinitionAttributeCount()
        {
            return m_RequiredItemAttributes.Count + m_RequiredItemDefinitionAttributes.Count;
        }

        /// <summary>
        /// Gets the required item & ItemDefinition attribute at the index provided.
        /// Use in combination with GetAllItemAndDefinitionAttributeCount to loop .
        /// </summary>
        /// <param name="index">The index of the attribute.</param>
        /// <returns>The attribute.</returns>
        public AttributeBase GetItemAndDefinitionAttributeAt(int index)
        {
            if (index < m_RequiredItemAttributes.Count) { return m_RequiredItemAttributes[index]; }

            return m_RequiredItemDefinitionAttributes[index];
        }

        /// <summary>
        /// Returns true if the category has the attribute as a Item or ItemDefinition attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>True if the attribute exists in the collection.</returns>
        public bool HasItemOrDefinitionAttribute(string attributeName)
        {
            return m_RequiredItemDefinitionAttributes.ContainsAttribute(attributeName) ||
                   m_RequiredItemAttributes.ContainsAttribute(attributeName);
        }

        /// <summary>
        /// Try get the Item attribute.
        /// Use this if you do not know the type of the attribute that might come out.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="requiredAttribute">Output of the attribute found.</param>
        /// <returns>False if the attribute does not exist in the collection.</returns>
        public bool TryGetItemAttribute(string attributeName, out AttributeBase requiredAttribute)
        {
            if (m_Initialized == false) { Initialize(false); }

            return m_RequiredItemAttributes.TryGetAttribute(attributeName, out requiredAttribute);
        }

        /// <summary>
        /// Get the item attribute.
        /// Use this if you know the type of the attribute.
        /// </summary>
        /// <typeparam name="T">The type of the attribute.</typeparam>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The attribute.</returns>
        public T GetItemAttribute<T>(string attributeName) where T : AttributeBase
        {
            return m_RequiredItemAttributes.GetAttribute<T>(attributeName);
        }

        /// <summary>
        /// Try get the item attribute value.
        /// Use this if you are only interested in the value of the attribute.
        /// </summary>
        /// <typeparam name="T">The attribute value type.</typeparam>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="value">Output of the value.</param>
        /// <returns>False if the attribute does not exist in the collection.</returns>
        public virtual bool TryGetItemAttributeValue<T>(string attributeName, out T value)
        {
            return m_RequiredItemAttributes.TryGetAttributeValue<T>(attributeName, out value);
        }

        /// <summary>
        /// Try get the itemDefinition attribute.
        /// Use this if you do not know the type of the attribute that might come out.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="requiredAttribute">Output of the attribute.</param>
        /// <returns>False if the attribute does not exists in the collection.</returns>
        public bool TryGetDefinitionAttribute(string attributeName, out AttributeBase requiredAttribute)
        {
            return m_RequiredItemDefinitionAttributes.TryGetAttribute(attributeName, out requiredAttribute);
        }

        /// <summary>
        /// Get the itemDefinition attribute.
        /// Use this if you know the type of the attribute.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The attribute.</returns>
        public T GetDefinitionAttribute<T>(string attributeName) where T : AttributeBase
        {
            return m_RequiredItemDefinitionAttributes.GetAttribute<T>(attributeName);
        }

        /// <summary>
        /// Try get the itemDefinition attribute value.
        /// Use this if you are only interested in the value of the attribute.
        /// </summary>
        /// <typeparam name="T">The attribute value type.</typeparam>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="value">Output of the attribute value.</param>
        /// <returns>False if the attribute does not exist in the collection.</returns>
        public virtual bool TryGetDefinitionAttributeValue<T>(string attributeName, out T value)
        {
            return m_RequiredItemDefinitionAttributes.TryGetAttributeValue<T>(attributeName, out value);
        }

        #endregion

        #region Remove Required Item and ItemDefinition Attributes

        /// <summary>
        /// Remove multiple itemDefinition attribute.
        /// </summary>
        /// <param name="requiredAttributes">The required attributes to remove.</param>
        public void RemoveDefinitionAttributes(IReadOnlyList<AttributeBase> requiredAttributes)
        {
            for (int i = requiredAttributes.Count - 1; i >= 0; i--) {
                RemoveDefinitionAttribute(requiredAttributes[i]);
            }
        }

        /// <summary>
        /// Remove an item Definition attribute, only for this category and its itemDefinitions.
        /// </summary>
        /// <param name="attributeName">The name of the attribute to remove.</param>
        /// <returns>False if the attribute could not be removed.</returns>
        public bool RemoveDefinitionAttribute(string attributeName)
        {
            return RemoveDefinitionAttribute(
                m_RequiredItemDefinitionAttributes.GetAttribute<AttributeBase>(attributeName));
        }

        /// <summary>
        /// Remove an item Definition attribute, only for this category and its itemDefinitions.
        /// </summary>
        /// <param name="attribute">The attribute that should be removed.</param>
        /// <returns>False if the attribute could not be removed.</returns>
        public virtual bool RemoveDefinitionAttribute(AttributeBase attribute)
        {
            if (m_RequiredItemDefinitionAttributes.RemoveAttribute(attribute)) {
                UpdateDefinitionAttributes();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove the attribute for all parent or for the entire family of attributes.
        /// </summary>
        /// <param name="attributeName">The attribute name of the attribute to remove.</param>
        /// <param name="relation">Choose whether to remove that parent attributes or the entire familly.</param>
        /// <param name="removeFromNoneParentSources">Should the attribute be removed from other sources which do not affect this category?</param>
        /// <returns>True if the attribute was removed correctly.</returns>
        public bool RemoveAttributeFromAll(string attributeName, Relation relation, bool removeFromNoneParentSources = false)
        {
            if (relation != Relation.Family && relation != Relation.Parents) { return false; }

            var attribute = GetAttribute(attributeName);

            if (attribute == null || attribute.AttributeCollection == null) { return false; }

            var isItemAttribute = attribute.AttributeCollection == m_RequiredItemAttributes;

            var isDefinitionAttribute = attribute.AttributeCollection == m_RequiredItemDefinitionAttributes;

            var isCategoryAttribute = attribute.AttributeCollection == m_ItemCategoryAttributeCollection;

            if ((isItemAttribute || isDefinitionAttribute || isCategoryAttribute) == false) {
                Debug.LogWarning("Cannot remove an attribute which has a custom attribute collection.");
                return false;
            }

            var pooledSourceAttributes = GenericObjectPool.Get<AttributeBase[]>();
            var sourceAttributes = relation == Relation.Family && removeFromNoneParentSources
                ? attribute.GetAttributeFamilySources(ref pooledSourceAttributes)
                : attribute.GetThisSources(ref pooledSourceAttributes);

            // Remove the attribute in all the categories.
            for (int i = 0; i < sourceAttributes.Count; i++) {
                var sourceCategory = sourceAttributes[i].AttachedItemCategory;

                var pooledAllSourceChildren = GenericObjectPool.Get<ItemCategory[]>();
                var sourceAllChildrenCount = sourceCategory.GetAllChildren(ref pooledAllSourceChildren, true);
                for (int j = 0; j < sourceAllChildrenCount; j++) {
                    var categoryChild = pooledAllSourceChildren[j];

                    if (relation == Relation.Parents && categoryChild.InherentlyContains(this) == false) { continue; }

                    if (isItemAttribute) {
                        categoryChild.m_RequiredItemAttributes.RemoveAttribute(attributeName);
                    } else if (isDefinitionAttribute) {
                        categoryChild.m_RequiredItemDefinitionAttributes.RemoveAttribute(attributeName);
                    } else {
                        categoryChild.m_ItemCategoryAttributeCollection.RemoveAttribute(attributeName);
                    }
                }

                GenericObjectPool.Return(pooledAllSourceChildren);
            }

            // Update the attributes everywhere once the remove attribute has finished.
            for (int i = 0; i < sourceAttributes.Count; i++) {
                var sourceCategory = sourceAttributes[i].AttachedItemCategory;
                if (isItemAttribute) { sourceCategory.UpdateItemAttributes(); } else if (isDefinitionAttribute) {
                    sourceCategory.UpdateDefinitionAttributes();
                } else { sourceCategory.UpdateCategoryAttributes(); }
            }

            GenericObjectPool.Return(pooledSourceAttributes);

            return true;
        }

        /// <summary>
        /// Remove multiple item attributes.
        /// </summary>
        /// <param name="requiredAttributes">The attributes to remove.</param>
        public void RemoveItemAttributes(IReadOnlyList<AttributeBase> requiredAttributes)
        {
            for (int i = requiredAttributes.Count - 1; i >= 0; --i) { RemoveItemAttribute(requiredAttributes[i]); }
        }

        /// <summary>
        /// Remove an item attribute, only for this category and its items.
        /// </summary>
        /// <param name="name">The name of the attribute to remove.</param>
        /// <returns>False if the attribute could not be removed.</returns>
        public bool RemoveItemAttribute(string name)
        {
            return RemoveItemAttribute(m_RequiredItemAttributes.GetAttribute<AttributeBase>(name));
        }

        /// <summary>
        /// Remove an item attribute, only for this category and its items.
        /// </summary>
        /// <param name="attribute">The attribute that should be removed.</param>
        /// <returns>False if the attribute could not be removed.</returns>
        public virtual bool RemoveItemAttribute(AttributeBase attribute)
        {
            if (m_RequiredItemAttributes.RemoveAttribute(attribute)) {
                UpdateItemAttributes();
                return true;
            }

            return false;
        }

        #endregion

        #region Remove Required Item and ItemDefinition Attributes

        /// <summary>
        /// This function removes the attribute from all the collections and returns them in a dictionary. 
        /// </summary>
        /// <param name="attribute">The reference attribute to extract.</param>
        /// <returns>A dictionary containing the names of the object that had the attribute.</returns>
        private Dictionary<IObjectWithIDReadOnly, AttributeBase> ExtractAttributes(AttributeBase attribute)
        {
            //TODO Fix this code by taking all attribute sources not just one.
            var attributes = new Dictionary<IObjectWithIDReadOnly, AttributeBase>();
            var sourceCategory = attribute.GetSourceCategory();

            var pooledItemCategories = GenericObjectPool.Get<ItemCategory[]>();
            var familyCategoryCount = sourceCategory.GetAllChildren(ref pooledItemCategories, true);

            for (int i = 0; i < familyCategoryCount; i++) {
                var categoryAttribute = pooledItemCategories[i].GetAttribute(attribute.Name);

                if (attributes.ContainsKey(pooledItemCategories[i]) == false) {
                    attributes.Add(pooledItemCategories[i], categoryAttribute);
                }
            }

            GenericObjectPool.Return(pooledItemCategories);

            var pooledItemDefinitions = GenericObjectPool.Get<ItemDefinition[]>();
            var familyDefinitionCount = sourceCategory.GetAllChildrenElements(ref pooledItemDefinitions);

            for (int i = 0; i < familyDefinitionCount; i++) {
                var defAttribute = pooledItemDefinitions[i].GetAttribute<AttributeBase>(attribute.Name);
                if (defAttribute == null) {
                    defAttribute = pooledItemDefinitions[i].DefaultItem.GetAttribute<AttributeBase>(attribute.Name);
                }

                if (defAttribute == null) { break; }

                if (attributes.ContainsKey(pooledItemDefinitions[i]) == false) {
                    attributes.Add(pooledItemDefinitions[i], defAttribute);
                }
            }

            GenericObjectPool.Return(pooledItemDefinitions);

            RemoveAttributeFromAll(attribute.Name, Relation.Family, true);

            return attributes;
        }

        /// <summary>
        /// Move an attribute from any collection to the Item attribute collection
        /// </summary>
        /// <param name="attribute">The attribute name to move.</param>
        public virtual void MoveAttributeToItems(AttributeBase attribute)
        {
            var sourceAttribute = attribute.GetSourceAttribute();
            var attributes = ExtractAttributes(sourceAttribute);
            var sourceCategory = sourceAttribute.GetSourceCategory();

            var pooledItemCategories = GenericObjectPool.Get<ItemCategory[]>();
            var familyCategoryCount = sourceCategory.GetAllChildren(ref pooledItemCategories, true);

            for (int i = 0; i < familyCategoryCount; i++) {
                pooledItemCategories[i].AddItemAttribute(attributes[pooledItemCategories[i]]);
            }

            GenericObjectPool.Return(pooledItemCategories);

            var pooledItemDefinitions = GenericObjectPool.Get<ItemDefinition[]>();
            var familyDefinitionCount = sourceCategory.GetAllChildrenElements(ref pooledItemDefinitions);

            for (int i = 0; i < familyDefinitionCount; i++) {
                pooledItemDefinitions[i].DefaultItem.OverrideAttribute(attributes[pooledItemDefinitions[i]]);
            }

            GenericObjectPool.Return(pooledItemDefinitions);
        }

        /// <summary>
        /// Move an attribute from any collection to the Item Definition attribute collection
        /// </summary>
        /// <param name="attribute">The attribute name to move.</param>
        public virtual void MoveAttributeToItemDefinitions(AttributeBase attribute)
        {
            var sourceAttribute = attribute.GetSourceAttribute();
            var attributes = ExtractAttributes(sourceAttribute);
            var sourceCategory = sourceAttribute.GetSourceCategory();

            var pooledItemCategories = GenericObjectPool.Get<ItemCategory[]>();
            var familyCategoryCount = sourceCategory.GetAllChildren(ref pooledItemCategories, true);

            for (int i = 0; i < familyCategoryCount; i++) {
                pooledItemCategories[i].AddDefinitionAttribute(attributes[pooledItemCategories[i]]);
            }

            GenericObjectPool.Return(pooledItemCategories);

            var pooledItemDefinitions = GenericObjectPool.Get<ItemDefinition[]>();
            var familyDefinitionCount = sourceCategory.GetAllChildrenElements(ref pooledItemDefinitions);

            for (int i = 0; i < familyDefinitionCount; i++) {
                pooledItemDefinitions[i].OverrideAttribute(attributes[pooledItemDefinitions[i]]);
            }

            GenericObjectPool.Return(pooledItemDefinitions);
        }

        /// <summary>
        /// Move an attribute from any collection to the Item Category attribute collection
        /// </summary>
        /// <param name="attribute">The attribute name to move.</param>
        public virtual void MoveAttributeToItemCategories(AttributeBase attribute)
        {
            var sourceAttribute = attribute.GetSourceAttribute();
            var attributes = ExtractAttributes(sourceAttribute);
            var sourceCategory = sourceAttribute.GetSourceCategory();

            var pooledItemCategories = GenericObjectPool.Get<ItemCategory[]>();
            var familyCategoryCount = sourceCategory.GetAllChildren(ref pooledItemCategories, true);

            for (int i = 0; i < familyCategoryCount; i++) {
                pooledItemCategories[i].AddOrOverrideCategoryAttribute(attributes[pooledItemCategories[i]]);
            }

            GenericObjectPool.Return(pooledItemCategories);
        }

        #endregion

        #region Get ItemDefinitions and category parents/children

        /// <summary>
        /// Returns true if the category has a child that contains the item itemDefinition provided.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="includeThis">If false it will not check for direct inheritance.</param>
        /// <returns>True if the item is inherited part of the category.</returns>
        public bool InherentlyContains(Item item, bool includeThis = true)
        {
            return InherentlyContains(item?.ItemDefinition, includeThis);
        }

        /// <summary>
        /// Returns true if the category directly contains the item provided.
        /// </summary>
        /// <param name="item">The Item.</param>
        /// <returns>True if the category directly contains the Item.</returns>
        public bool DirectlyContains(Item item)
        {
            return DirectlyContains(item.ItemDefinition);
        }

        /// <summary>
        /// Returns a read only array of the direct parents of the current category.
        /// </summary>
        /// <returns>The direct parents of the current category.</returns>
        public IReadOnlyList<IItemCategoryIdentifier> GetDirectParents()
        {
            return m_Parents;
        }

        /// <summary>
        /// Check if the Item Category contains the other item category.
        /// </summary>
        /// <param name="other">The other item category which could be contained by the category.</param>
        /// <param name="includeThis">Returns true if this category is the other category.</param>
        /// <returns>Return true if the other category is contained by this category.</returns>
        public bool InherentlyContains(IItemCategoryIdentifier other, bool includeThis = true)
        {
            return base.InherentlyContains(other as ItemCategory, includeThis);
        }

        /// <summary>
        /// Check if the Item Category contains the item definition.
        /// </summary>
        /// <param name="itemDefinition">The item definition which could be contained by the category.</param>
        /// <param name="includeThis">Returns true if this category directly contains the item definition.</param>
        /// <returns>Return true if the item definition is contained by this category.</returns>
        public bool InherentlyContains(ItemDefinitionBase itemDefinition, bool includeThis = true)
        {
            return base.InherentlyContains(itemDefinition as ItemDefinition, includeThis);
        }

        /// <summary>
        /// Check if the Item Category contains the item.
        /// </summary>
        /// <param name="item">The item which could be contained by the category.</param>
        /// <param name="includeThis">Returns true if this category directly contains the item.</param>
        /// <returns>Return true if the item is contained by this category.</returns>
        public bool InherentlyContains(IItemIdentifier item, bool includeThis = true)
        {
            return InherentlyContains(item as Item, includeThis);
        }

        #endregion

        #region Category Attributes

        /// <summary>
        /// Gets the required item attributes.
        /// </summary>
        /// <param name="attributes">Reference to the array of attributes. Can be resized up.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetInheritedCategoryAttributes(ref AttributeBase[] attributes)
        {
            var count = 0;
            for (int i = 0; i < m_Parents.Count; i++) {
                if (m_Parents[i].m_ItemCategoryAttributeCollection == null) { continue; }

                var itemCategoryAttributes = m_Parents[i].m_ItemCategoryAttributeCollection;
                for (int j = 0; j < itemCategoryAttributes.Count; j++) {
                    TypeUtility.ResizeIfNecessary(ref attributes, count);
                    attributes[count] = itemCategoryAttributes[j];
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Update the category attribute.
        /// </summary>
        /// <returns>False if something goes wrong with the update.</returns>
        public virtual bool UpdateCategoryAttributes()
        {
            var pooledInheritedAttributes = GenericObjectPool.Get<AttributeBase[]>();
            var inheritedAttributesCount = GetInheritedCategoryAttributes(ref pooledInheritedAttributes);

            var result =
                m_ItemCategoryAttributeCollection.UpdateAttributesToMatchList(
                    (pooledInheritedAttributes, 0, inheritedAttributesCount), false);

            GenericObjectPool.Return(pooledInheritedAttributes);

            ReevaluateCategoryAttributes();

            var childrenCount = m_Children.Count;
            var childrenArray = m_Children.Array;
            for (int i = 0; i < childrenCount; i++) { childrenArray[i].UpdateCategoryAttributes(); }

            return result;
        }

        /// <summary>
        /// Remove a category attribute with the specified name.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>True if the attribute was removed.</returns>
        public bool RemoveCategoryAttribute(string attributeName)
        {
            return RemoveCategoryAttribute(
                m_ItemCategoryAttributeCollection.GetAttribute<AttributeBase>(attributeName));
        }

        /// <summary>
        /// Remove a category attribute.
        /// </summary>
        /// <param name="attribute">The attribute that should be removed.</param>
        /// <returns>True if the attribute was removed.</returns>
        public bool RemoveCategoryAttribute(AttributeBase attribute)
        {
            if (m_ItemCategoryAttributeCollection.RemoveAttribute(attribute)) {
                UpdateCategoryAttributes();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add or override a category attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="addEvenIfNothingToOverwrite">Add the attribute even if one with the same name does not exist in the collection.</param>
        /// <param name="force">Force to add the attribute even if it does not have the same type as the one with the same name, in the collection.</param>
        /// <returns>False if the attribute was not added correctly.</returns>
        public virtual bool AddOrOverrideCategoryAttribute(AttributeBase attribute,
            bool addEvenIfNothingToOverwrite = true, bool force = false)
        {
            var valid = IsNewAttributeValid(attribute.Name, attribute.GetType(), 0);
            if (!valid) {

                // Check if the existing attribute has the same type and is in the category collection.
                var existingAttribute = GetAttribute(attribute.Name);
                if (existingAttribute == null || existingAttribute.GetType() != attribute.GetType() || existingAttribute.AttributeCollectionIndex != 0) {
                    if (!force) { return false; }

                    RemoveAttributeFromAll(attribute.Name, Relation.Family, false);
                }
            }

            var result =
                m_ItemCategoryAttributeCollection.OverrideAttribute(attribute, addEvenIfNothingToOverwrite, force);

            UpdateCategoryAttributes();

            return result;
        }

        /// <summary>
        /// Add and override multiple category attributes.
        /// </summary>
        /// <param name="newAttributes">The attributes.</param>
        /// <param name="addEvenIfNothingToOverwrite">Add the attribute even if one with the same name does not exist in the collection.</param>
        /// <param name="force">Force to add the attribute even if it does not have the same type as the one with the same name, in the collection.</param>
        public virtual void AddOrOverrideCategoryAttributes(ListSlice<AttributeBase> newAttributes,
            bool addEvenIfNothingToOverwrite = true, bool force = false)
        {
            m_ItemCategoryAttributeCollection.OverrideAttributes(newAttributes, addEvenIfNothingToOverwrite, force);
        }

        /// <summary>
        /// Checks if the attribute exists as a category attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>True if the attribute exists in the collection.</returns>
        public virtual bool HasCategoryAttribute(string attributeName)
        {
            return m_ItemCategoryAttributeCollection.ContainsAttribute(attributeName);
        }

        /// <summary>
        /// Try get the category attribute.
        /// Use this if the attribute type unknown.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attribute">Output the attribute.</param>
        /// <returns>True if the attribute exists in the collection.</returns>
        public virtual bool TryGetCategoryAttribute(string attributeName, out AttributeBase attribute)
        {
            return m_ItemCategoryAttributeCollection.TryGetAttribute(attributeName, out attribute);
        }

        /// <summary>
        /// Get the category attribute.
        /// Use this if you know the attribute type.
        /// </summary>
        /// <typeparam name="T">The Attribute type.</typeparam>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The attribute.</returns>
        public virtual T GetCategoryAttribute<T>(string attributeName) where T : AttributeBase
        {
            return m_ItemCategoryAttributeCollection.GetAttribute<T>(attributeName);
        }

        /// <summary>
        /// Try get the category attribute value.
        /// Use this if you are only interested in the attribute value.
        /// </summary>
        /// <typeparam name="T">The attribute value type.</typeparam>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attributeValue">Output the attribute value.</param>
        /// <returns>False if the attribute does not exist in the collection.</returns>
        public virtual bool TryGetCategoryAttributeValue<T>(string attributeName, out T attributeValue)
        {
            return m_ItemCategoryAttributeCollection.TryGetAttributeValue(attributeName, out attributeValue);
        }

        /// <summary>
        /// Returns a list of category attributes.
        /// </summary>
        /// <returns>The list of attributes.</returns>
        public virtual IReadOnlyList<AttributeBase> GetCategoryAttributeList()
        {
            return m_ItemCategoryAttributeCollection;
        }

        /// <summary>
        /// Returns a count of category attributes.
        /// </summary>
        /// <returns>The count of attributes.</returns>
        public virtual int GetCategoryAttributeCount()
        {
            return m_ItemCategoryAttributeCollection.Count;
        }

        /// <summary>
        /// Returns a attribute at the index of category attributes.
        /// </summary>
        /// <param name="index">The index of the attribute.</param>
        /// <returns>The attribute.</returns>
        public virtual AttributeBase GetCategoryAttributeAt(int index)
        {
            return m_ItemCategoryAttributeCollection[index];
        }

        #endregion

        /// <summary>
        /// Returns the count of all the attributes of this category, including category, item and Item Definition attributes.
        /// </summary>
        /// <returns>The count.</returns>
        public int GetAttributesCount()
        {
            return m_ItemCategoryAttributeCollection.Count
                   + m_RequiredItemDefinitionAttributes.Count
                   + m_RequiredItemAttributes.Count;
        }

        /// <summary>
        /// Returns the attribute at the index of all the attributes of this category, including category, item and Item Definition attributes.
        /// </summary>
        /// <returns>The attribute.</returns>
        public AttributeBase GetAttributesAt(int index)
        {
            if (index < m_ItemCategoryAttributeCollection.Count) { return m_ItemCategoryAttributeCollection[index]; }

            index -= m_ItemCategoryAttributeCollection.Count;
            if (index < m_RequiredItemDefinitionAttributes.Count) { return m_RequiredItemDefinitionAttributes[index]; }

            index -= m_RequiredItemDefinitionAttributes.Count;
            if (index < m_RequiredItemAttributes.Count) { return m_RequiredItemAttributes[index]; }

            return null;
        }

        /// <summary>
        /// Returns an attribute that is part of the category, item or Item Definition attributes.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The attribute.</returns>
        public AttributeBase GetAttribute(string attributeName)
        {
            TryGetCategoryAttribute(attributeName, out var attribute);
            if (attribute == null) { TryGetDefinitionAttribute(attributeName, out attribute); }

            if (attribute == null) { TryGetItemAttribute(attributeName, out attribute); }

            return attribute;
        }
        
        /// <summary>
        /// Returns an attribute that is part of the category, item or Item Definition attributes.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>The attribute.</returns>
        public T GetAttribute<T>(string attributeName) where T : AttributeBase
        {
            var attribute = GetCategoryAttribute<T>(attributeName);
            if (attribute == null) { attribute = GetDefinitionAttribute<T>(attributeName); }
            if (attribute == null) { attribute = GetItemAttribute<T>(attributeName); }

            return attribute;
        }

        /// <summary>
        /// Tries to get an attribute that is part of the category, item or Item Definition attributes.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attributeValue">Outputs the attribute value.</param>
        /// <returns>The true if the attribute was found.</returns>
        public bool TryGetAttributeValue<T>(string attributeName, out T attributeValue)
        {
            if (TryGetCategoryAttributeValue<T>(attributeName, out attributeValue)) {
                return true;
            }
            if (TryGetDefinitionAttributeValue<T>(attributeName, out attributeValue)) {
                return true;
            }
            if (TryGetItemAttributeValue<T>(attributeName, out attributeValue)) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the category has a category, item or itemDefinition attribute with the name provided.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <returns>True if the attribute exists in one of the collections.</returns>
        public bool HasAttribute(string attributeName)
        {
            return HasItemOrDefinitionAttribute(attributeName) || HasCategoryAttribute(attributeName);
        }

        /// <summary>
        /// Check if the attribute name and type is valid for a new attribute.
        /// </summary>
        /// <param name="newAttributeName">The new attribute name.</param>
        /// <param name="newAttributeType">The new attribute type.</param>
        /// <param name="collectionIndex">0 -> category, 1 -> item Definition, 2 -> item.</param>
        /// <returns>True if the name and type are valid for this category.</returns>
        public bool IsNewAttributeValid(string newAttributeName, Type newAttributeType, int collectionIndex)
        {
            if (HasAttribute(newAttributeName)) {
                //Cannot add attribute because one with the same name already exists.
                return false;
            }

            var pooledAllChildren = GenericObjectPool.Get<ItemCategory[]>();
            var allChildrenCount = GetAllChildren(ref pooledAllChildren, true);
            for (int i = 0; i < allChildrenCount; i++) {
                var categoryChild = pooledAllChildren[i];
                var existingAttribute = categoryChild.GetAttribute(newAttributeName);

                if (existingAttribute == null) { continue; }

                //Make sure the existing attribute is withing the same collection index.

                if (collectionIndex == existingAttribute.AttributeCollectionIndex) {
                    var existingValueType = existingAttribute.GetValueType();
                    if (existingValueType == newAttributeType || existingAttribute.GetType() == newAttributeType) {
                        continue;
                    }

                    Debug.LogWarning(
                        $"{name}: Cannot add attribute '{newAttributeName}' because a category child '{categoryChild}' " +
                        $"has one with the same name but a different type: current Value Type: {existingValueType} | new Attribute/Value Type: {newAttributeType}.");
                    return false;

                }

                Debug.LogWarning(
                    $"{name}: Cannot add attribute '{newAttributeName}' because a category child '{categoryChild}' " +
                    $"has one with the same name but in a different collection current: {existingAttribute.AttributeCollectionIndex} | new collection Index: {collectionIndex}.");
                return false;

            }

            GenericObjectPool.Return(pooledAllChildren);

            return true;
        }

        /// <summary>
        /// Reevaluate the attribute of all the attributeCollections in this category
        /// </summary>
        public void ReevaluateAllAttributes()
        {
            ReevaluateCategoryAttributes();
            ReevaluateDefinitionAttributes();
            ReevaluateItemAttributes();
        }

        /// <summary>
        /// Reevaluate that attributes for the category attribute collection
        /// </summary>
        public void ReevaluateCategoryAttributes()
        {
            for (int i = 0; i < m_ItemCategoryAttributeCollection.Count; i++) {
                m_ItemCategoryAttributeCollection[i].ReevaluateValue(false);
            }
        }

        /// <summary>
        /// Reevaluate that attributes for the required item attribute collection
        /// </summary>
        public void ReevaluateDefinitionAttributes()
        {
            for (int i = 0; i < m_RequiredItemDefinitionAttributes.Count; i++) {
                m_RequiredItemDefinitionAttributes[i].ReevaluateValue(false);
            }
        }

        /// <summary>
        /// Reevaluate that attributes for the required itemDefinition attribute collection
        /// </summary>
        public void ReevaluateItemAttributes()
        {
            for (int i = 0; i < m_RequiredItemAttributes.Count; i++) {
                m_RequiredItemAttributes[i].ReevaluateValue(false);
            }
        }

        /// <summary>
        /// Returns a custom string.
        /// </summary>
        /// <returns>The custom string.</returns>
        public override string ToString()
        {
            return name;
        }
    }
}