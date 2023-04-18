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
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// ItemDefinition stores the constant attributes of an item. Item Definitions variants can be created by adding a ParentItemDefinition.
    /// </summary>
    public class ItemDefinition : ItemDefinitionBase, ICategoryElement<ItemCategory, ItemDefinition>, IObjectWithID, IDirtyable//, ISerializationCallbackReceiver
    {
        [Tooltip("The ID of the Definition.")]
        [SerializeField] protected uint m_ID;
        [Tooltip("The category of this definition, it defines all the attributes this definition should have and more.")]
        [SerializeField] protected ItemCategory m_Category;
        [Tooltip("The parent of this itemDefinition, used to inherit any properties of the parents.")]
        [SerializeField] protected ItemDefinition m_Parent;
        [Tooltip("The serialization data for the children of this definition, used to find the descendents without going through the entire database.")]
        [SerializeField] protected Serialization m_ChildrenData;
        [Tooltip("The attributes of the definition. These are constant for all Items with this definition.")]
        [SerializeField] protected ItemDefinitionAttributeCollection m_ItemDefinitionAttributeCollection;
        [Tooltip("The default item for this definition. This will define what attribute values the items created from this definition will have.")]
        [SerializeField] protected Item m_DefaultItem;

#if UNITY_EDITOR
        [Tooltip("Used in editor to add an icon next to the definition name.")]
        [SerializeField] internal Sprite m_EditorIcon;
#endif

        [System.NonSerialized] protected bool m_Initialized = false;
        [System.NonSerialized] protected ResizableArray<ItemDefinition> m_Children;

        protected bool m_Dirty;
        internal bool Dirty {
            get => m_Dirty
                   | (m_ItemDefinitionAttributeCollection?.Dirty ?? false)
                   | (m_DefaultItem?.ItemAttributeCollection?.Dirty ?? false);
            set {
                m_Dirty = value;
                if (m_ItemDefinitionAttributeCollection != null) {
                    m_ItemDefinitionAttributeCollection.Dirty = value;
                }
                if (m_DefaultItem?.ItemAttributeCollection != null) {
                    m_DefaultItem.ItemAttributeCollection.Dirty = value;
                }
            }
        }
        bool IDirtyable.Dirty {
            get => Dirty;
            set => Dirty = value;
        }

        public uint ID {
            get => m_ID;
            internal set {
                if (m_ID == value) { return; }
                m_ID = value;
                m_Dirty = true;
            }
        }

        uint IObjectWithID.ID {
            get => ID;
            set {
                if (m_ID == value) { return; }
                m_ID = value;
                m_Dirty = true;
            }
        }

        public IInventorySystemManager Manager => m_Category?.Manager;

        public ItemDefinition Parent => m_Parent;
        public ItemCategory Category => m_Category;
        public bool IsMutable => m_Category == null || m_Category.IsMutable;
        public bool IsUnique => m_Category == null || m_Category.IsUnique;
        public virtual Item DefaultItem => m_DefaultItem;
        public IReadOnlyList<ItemDefinition> ChildrenReadOnly => m_Children;
        public bool IsInitialized => m_Initialized;

        internal ResizableArray<ItemDefinition> Children => m_Children;
        internal ItemDefinitionAttributeCollection ItemDefinitionAttributeCollection => m_ItemDefinitionAttributeCollection;

        #region Initializing

        /// <summary>
        /// Creates an ItemDefinition using a category.
        /// </summary>
        /// <param name="name">The name of the Item Definition.</param>
        /// <param name="category">The name of the Item Category.</param>
        /// <param name="parentDefinition">The parent Item Definition, if there is one.</param>
        /// <param name="definitionAttributesOverrides">A list of attribute overrides for the definition attributes (optional).</param>
        /// <param name="defaultItemAttributesOverrides">A list of attribute overrides for the default item attributes (optional).</param>
        /// <returns>The created Item Definition.</returns>
        internal static ItemDefinition Create(string name, ItemCategory category, ItemDefinition parentDefinition = null, IReadOnlyList<AttributeBase> definitionAttributesOverrides = null, IReadOnlyList<AttributeBase> defaultItemAttributesOverrides = null)
        {
            if (category == null || category.IsAbstract) {
                Debug.LogWarning("Cannot create ItemDefinition with null or abstract Category");
                return null;
            }

            // Construct
            var itemDefinition = CreateInstance<ItemDefinition>();
            itemDefinition.ID = RandomID.Generate();
            itemDefinition.name = name;
            itemDefinition.m_Category = category;

            itemDefinition.m_ItemDefinitionAttributeCollection = new ItemDefinitionAttributeCollection();

            if (itemDefinition.m_Category != null) {
                itemDefinition.m_Category.AddElement(itemDefinition);
            }

            // Register
            var result = itemDefinition.Initialize(true);
            if (result == false) { return null; }

            // Add override attributes for ItemDefinition
            if (definitionAttributesOverrides != null) {
                itemDefinition.m_ItemDefinitionAttributeCollection.OverrideAttributes((definitionAttributesOverrides, 0));
            }

            // Add override attributes for the Default Item
            itemDefinition.OverrideDefaultItemAttributeValues((defaultItemAttributesOverrides, 0));

            if (parentDefinition != null) {
                itemDefinition.SetParentInternal(parentDefinition);
            }

            return itemDefinition;
        }

        /// <summary>
        /// Initializes the itemDefinition attributes & registers the definition.
        /// </summary>
        /// <param name="force">Force initialize the object.</param>
        /// <param name="updateAttributes">Update the attributes to make sure they are correct?</param>
        /// <returns>Returns true if the itemDefinition was registered correctly.</returns>
        public virtual bool Initialize(bool force, bool updateAttributes = true)
        {
            if (m_Initialized && !force) { return true; }

            if (m_Category == null) {
                Debug.LogWarning("Category is null, The category must be specified");
                m_Initialized = false;
                return false;
            }

            m_Category.AddElement(this);

            Deserialize();

            if (m_DefaultItem != null) {
                m_DefaultItem.Initialize(true);
            }

            // ReSharper disable once ReplaceWithSingleAssignment.False
            var isRegistered = false;

            // It is required to check .Equals as the interface could point to a unity Object.
            if (InterfaceUtility.IsNotNull(Manager)) {
                var success = Manager.Register?.ItemDefinitionRegister?.Register(this);
                if (success.HasValue) {

                    if (success.Value == false) {
                        m_Initialized = false;
                        return false;
                    }

                    //Item needs to be created after the itemDefinitions is registered.
                    if (m_DefaultItem != null) { m_DefaultItem.Initialize(true); } else {
                        m_DefaultItem = Manager.Factory.CreateItem(this);
                    }

                    if (updateAttributes) { UpdateAttributes(); }

                    m_Initialized = true;
                    return true;
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying) { isRegistered = true; }
#endif

            m_Initialized = isRegistered;
            if (!isRegistered) {
                return false;
            }

            //Item needs to be created after the itemDefinitions is registered.
            //This is called when the Editor creates an ItemDefinition.
            if (m_DefaultItem != null) {
                m_DefaultItem.Initialize(true);
            } else {
                m_DefaultItem = Item.Create(this);
            }

            if (updateAttributes) {
                UpdateAttributes();
            }

            return true;
        }

        /*/// <summary>
        /// Unity Serialize Callback.
        /// </summary>
        public void OnBeforeSerialize()
        {
            Serialize();
        }

        /// <summary>
        /// Unity Deserialize Callback.
        /// </summary>
        public void OnAfterDeserialize()
        {
            Deserialize();
        }*/

        /// <summary>
        /// Deserialize all the properties of the ItemDefinition.
        /// </summary>
        internal void Deserialize()
        {
            DeserializeAttributeCollections();
            DeserializeRelationships();
        }

        /// <summary>
        /// Deserialize the attribute collection.
        /// </summary>
        protected void DeserializeAttributeCollections()
        {
            if (m_ItemDefinitionAttributeCollection == null) { m_ItemDefinitionAttributeCollection = new ItemDefinitionAttributeCollection(); }
            m_ItemDefinitionAttributeCollection.Initialize(this, true);
        }

        /// <summary>
        /// Create the ArrayStruct for the children.
        /// </summary>
        protected void DeserializeRelationships()
        {
            if (m_ChildrenData != null && m_ChildrenData.Values != null && m_ChildrenData.Values.Length > 0) {
                var obj = m_ChildrenData.DeserializeFields(MemberVisibility.Public);
                m_Children = obj != null ? (ResizableArray<ItemDefinition>)obj : new ResizableArray<ItemDefinition>();
            } else {
                m_Children = new ResizableArray<ItemDefinition>();
            }
        }

        /// <summary>
        /// Serialize all the properties of the itemDefinition.
        /// </summary>
        public void Serialize()
        {
            SerializeChildren();
            if (m_ItemDefinitionAttributeCollection != null) {
                m_ItemDefinitionAttributeCollection.Serialize();
            }

            if (m_DefaultItem != null) {
                m_DefaultItem.Serialize();
            }
        }

        /// <summary>
        /// Serializes the Children ArrayStruct to Serialization data. 
        /// </summary>
        protected void SerializeChildren()
        {
            m_Children.Truncate();
            m_ChildrenData = Serialization.Serialize(m_Children);
        }

        /// <summary>
        /// Change the attribute values of the DefaultItem
        /// </summary>
        public void OverrideDefaultItemAttributeValues(ListSlice<AttributeBase> defaultItemAttributesOverrides)
        {
            if (defaultItemAttributesOverrides.Count == 0) { return; }
            m_DefaultItem.ItemAttributeCollection.OverrideAttributes(defaultItemAttributesOverrides);

        }

        /// <summary>
        /// Creates an IItemIdentifier based off of the definition.
        /// </summary>
        /// <returns>An IItemIdentifier based off of the definition.</returns>
        public override IItemIdentifier CreateItemIdentifier()
        {
            // It is required to check .Equals as the interface could point to a unity Object.
            if (InterfaceUtility.IsNotNull(Manager)) {
                return Manager.Factory?.CreateItem(this);
            }

            return InventorySystemManager.Factory.CreateItem(this);
        }

        #endregion

        #region Add Parent & Children

        /// <summary>
        /// Set the ItemCategory.
        /// Only itemDefinitions with the same ItemCategory can have a parent/child relationship.
        /// </summary>
        /// <param name="itemCategory">The new itemCategory.</param>
        /// <returns>Returns a success or fail message.</returns>
        internal void SetCategoryWithoutNotify(ItemCategory itemCategory)
        {
            m_Category = itemCategory;
        }

        /// <summary>
        /// Set the ItemCategory.
        /// Only itemDefinitions with the same ItemCategory can have a parent/child relationship.
        /// </summary>
        /// <param name="itemCategory">The new itemCategory.</param>
        /// <param name="relation">Set the ItemCategory to related ItemDefinitions.</param>
        /// <returns>Returns a success or fail message.</returns>
        public virtual bool SetCategory(ItemCategory itemCategory, Relation relation)
        {
            var result = SetCategoryInternal(itemCategory, relation);
            if (result) {
                UpdateAttributes();
            }
            return result;
        }

        /// <summary>
        /// Set the Item Category.
        /// Only Item Definition with the same ItemCategory can have a parent/child relationship.
        /// </summary>
        /// <param name="newItemCategory">The new Item Category.</param>
        /// <param name="relation">Set the ItemCategory to related Item Definition.</param>
        /// <returns>Returns a success or fail message.</returns>
        protected virtual bool SetCategoryInternal(ItemCategory newItemCategory, Relation relation)
        {
            if (newItemCategory == m_Category) {
                //ItemCategory is already set.
                return false;
            }
            if (newItemCategory == null) {
                Debug.LogWarning("Cannot set null Item Category.");
                return false;
            }
            if (newItemCategory.IsAbstract) {
                Debug.LogWarning("Item Category is abstract.");
                return false;
            }

            switch (relation) {
                case Relation.None: {
                        if (m_Category != null) {
                            m_Category.RemoveElement(this);
                        }
                        var previousParent = Parent;
                        RemoveParent();
                        for (var i = ChildrenReadOnly.Count - 1; i >= 0; i--) {
                            var child = ChildrenReadOnly[i];
                            if (child == null) {
                                Debug.LogError($"The database is corrupt, please fix the Item Definition: {this}.");
                                continue;
                            }

                            child.SetParent(previousParent);
                            child.Initialize(true);
                        }

                        m_Category = newItemCategory;

                        SetParent(previousParent);
                        Initialize(true);
                        break;
                    }
                case Relation.Parents: {
                        for (var i = ChildrenReadOnly.Count - 1; i >= 0; i--) {
                            var child = ChildrenReadOnly[i];
                            if (child == null) {
                                Debug.LogError($"The database is corrupt, please fix the Item Definition: {this}.");
                                continue;
                            }

                            child.RemoveParent();
                            child.Initialize(true);
                        }

                        SetFamilyCategory(newItemCategory);
                        break;
                    }
                case Relation.Children:
                    RemoveParent();
                    SetFamilyCategory(newItemCategory);
                    break;
                case Relation.Family:
                    SetFamilyCategory(newItemCategory);
                    break;
            }

            m_Dirty = true;
            return true;
        }

        /// <summary>
        /// Set the category for this family of ItemDefinitions.
        /// </summary>
        /// <param name="newItemCategory">The new ItemCategory.</param>
        private void SetFamilyCategory(ItemCategory newItemCategory)
        {
            var itemDefs = GenericObjectPool.Get<ItemDefinition[]>();
            var itemDefFamilyCount = GetAllFamily(ref itemDefs);
            for (int i = itemDefFamilyCount - 1; i >= 0; i--) {
                var itemDef = itemDefs[i];
                if (itemDef.Category != null && itemDef.Category.IsInitialized) {
                    itemDef.Category.RemoveElement(itemDef);
                }
                var previousParent = itemDef.Parent;

                itemDef.m_Category = newItemCategory;

                itemDef.SetParent(previousParent);
                itemDef.Initialize(true);
            }

            GenericObjectPool.Return(itemDefs);
            m_Dirty = true;
        }

        /// <summary>
        /// Set the parent of the ItemDefinition.
        /// Only itemDefinitions with the same ItemCategory can have a parent/child relationship.
        /// </summary>
        /// <param name="parent">The new parent.</param>
        /// <returns>Returns a success or fail message.</returns>
        internal void SetParentWithoutNotify(ItemDefinition parent)
        {
            m_Parent = parent;
        }

        /// <summary>
        /// Set the parent of the ItemDefinition.
        /// Only itemDefinitions with the same ItemCategory can have a parent/child relationship.
        /// </summary>
        /// <param name="parent">The new parent.</param>
        /// <returns>Returns a success or fail message.</returns>
        public virtual bool SetParent(ItemDefinition parent)
        {
            var result = SetParentInternal(parent);
            if (result) {
                UpdateAttributes();
            }
            return result;
        }

        /// <summary>
        /// Set the parent of the ItemDefinition.
        /// </summary>
        /// <param name="newParent">The new parent.</param>
        /// <returns>Returns a success or fail message.</returns>
        protected virtual bool SetParentInternal(ItemDefinition newParent)
        {
            if (newParent == null) {
                RemoveParent();
                return true;
            }

            var valid = SetParentCondition(newParent);
            if (valid == false) { return false; }

            if (m_Parent != null) {
                m_Parent.m_Children.Remove(this);
                m_Parent.m_Dirty = true;
            }

            m_Parent = newParent;

            newParent.m_Children.Add(this);
            newParent.m_Dirty = true;

            DefaultItem.ItemAttributeCollection.ReevaluateAll(false);

            m_Dirty = true;
            return true;
        }

        /// <summary>
        /// Sets the conditions to be a parent.
        /// </summary>
        /// <param name="otherItemDefinition">The desired parent.</param>
        /// <returns>If true the parent can be set.</returns>
        public virtual bool SetParentCondition(ItemDefinition otherItemDefinition)
        {
            if (otherItemDefinition == null) { return true; }

            var itemDefs = GenericObjectPool.Get<ItemDefinition[]>();

            var thisAllParentsCount = GetAllParents(ref itemDefs, true);
            if (itemDefs.Contains(otherItemDefinition, 0, thisAllParentsCount)) {
                //The itemDefinition is already a descendant of the specified ItemDefinition.
                GenericObjectPool.Return(itemDefs);
                return false;
            }

            var newParentAllParentsCount = otherItemDefinition.GetAllParents(ref itemDefs, true);
            if (itemDefs.Contains(this, 0, newParentAllParentsCount)) {
                //The itemDefinition is a ancestor of the the specified itemDefinition.

                GenericObjectPool.Return(itemDefs);
                return false;
            }

            GenericObjectPool.Return(itemDefs);

            //Check if mismatch in category
            if (Category == null) {
                m_Category = otherItemDefinition.Category;
            } else if (otherItemDefinition.Category != Category) {
                //The itemDefinition category does not match the parent Category.
                return false;

            }

            return true;
        }

        /// <summary>
        /// Removes the Parent.
        /// </summary>
        public virtual void RemoveParent()
        {
            if (m_Parent == null) { return; }

            m_Parent.m_Children.Remove(this);
            m_Parent.m_Dirty = true;

            m_Parent = null;
            m_Dirty = true;
        }

        #endregion

        #region Get Related ItemDefinitions

        /// <summary>
        /// Returns the children of all generations.
        /// </summary>
        /// <param name="children">Reference to an Item Definition array. Can be resized up.</param>
        /// <param name="includeThis">If true this Item Definition will be part of the result.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetAllChildren(ref ItemDefinition[] children, bool includeThis)
        {
            return IterationHelper.GetAllRecursive(ref children, includeThis, this, x => x?.m_Children, true);
        }

        /// <summary>
        /// Get the children of all generation with the generation.
        /// The list is sorted by generation.
        /// </summary>
        /// <param name="includeThis">If true this Item Definition will be part of the result.</param>
        /// <param name="level">The generation level in case you do not start from 0.</param>
        /// <returns>The list of definitions and their levels.</returns>
        public List<(ItemDefinition, int)> GetAllChildrenWithLevel(bool includeThis = false, int level = 0)
        {
            var childrenWithLevel = GetAllChildrenWithLevelInternal(includeThis, level);
            childrenWithLevel.Sort((x1, x2) => x1.Item2.CompareTo(x2.Item2));
            return childrenWithLevel;
        }

        /// <summary>
        /// Get the children of all generation with the generation.
        /// </summary>
        /// <param name="includeThis">If true this Item Definition will be part of the result.</param>
        /// <param name="level">The generation level in case you do not start from 0.</param>
        /// <returns>The list of definitions and their levels.</returns>
        protected List<(ItemDefinition, int)> GetAllChildrenWithLevelInternal(bool includeThis = false, int level = 0)
        {
            var allChildren = new List<(ItemDefinition, int)>();
            if (includeThis) {
                allChildren.Add((this, level));
            }
            var childrenArray = m_Children.Array;
            var arrayLength = m_Children.Count;
            for (int i = 0; i < arrayLength; i++) {
                allChildren.AddRange(childrenArray[i].GetAllChildrenWithLevelInternal(true, level + 1));
            }
            return allChildren;
        }

        /// <summary>
        /// Returns the parents of all generations.
        /// </summary>
        /// <param name="parents">Reference to an Item Definition array. Can be resized up.</param>
        /// <param name="includeThis">If true this Item Definition will be part of the result.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetAllParents(ref ItemDefinition[] parents, bool includeThis)
        {
            return IterationHelper.GetAllRecursiveDFS(ref parents, includeThis, this, x => x.m_Parent);
        }

        /// <summary>
        /// Get the parents of all generation with the generation.
        /// The list is sorted by generation.
        /// </summary>
        /// <param name="includeThis">If true this Item Definition will be part of the result.</param>
        /// <param name="level">The generation level in case you do not start from 0.</param>
        /// <returns>The list of definitions and their levels.</returns>
        public List<(ItemDefinition, int)> GetAllParentsWithLevel(bool includeThis = false, int level = 0)
        {
            var parentsWithLevel = GetAllParentsWithLevelInternal(includeThis, level);
            parentsWithLevel.Sort((x1, x2) => { return x1.Item2.CompareTo(x2.Item2); });
            return parentsWithLevel;
        }

        /// <summary>
        /// Get the parents of all generation with the generation.
        /// </summary>
        /// <param name="includeThis">If true this Item Definition will be part of the result.</param>
        /// <param name="level">The generation level in case you do not start from 0.</param>
        /// <returns>The list of definitions and their levels.</returns>
        protected List<(ItemDefinition, int)> GetAllParentsWithLevelInternal(bool includeThis = false, int level = 0)
        {
            var allParents = new List<(ItemDefinition, int)>();
            if (includeThis) {
                allParents.Add((this, level));
            }
            if (m_Parent != null) {
                allParents.AddRange(m_Parent.GetAllParentsWithLevelInternal(true, level + 1));
            }
            return allParents;
        }

        /// <summary>
        /// Get the root parent.
        /// </summary>
        /// <returns>The root parent.</returns>
        public ItemDefinition GetRoot()
        {
            var parent = m_Parent;
            var root = this;
            while (parent != null) {
                root = parent;
                parent = parent.m_Parent;
            }
            return root;
        }

        /// <summary>
        /// Returns all the ItemDefinitions that are related to this one.
        /// Parents and children.
        /// </summary>
        /// <param name="family">Reference to an ItemDefinition array. Can be resized up.</param>
        /// <returns>A count of the assigned elements of the array.</returns>
        public int GetAllFamily(ref ItemDefinition[] family)
        {
            var root = GetRoot();
            return root.GetAllChildren(ref family, true);
        }

        /// <summary>
        /// Returns the parent of the current definition.
        /// </summary>
        /// <returns>The parent of the current definition. Can be null.</returns>
        public override ItemDefinitionBase GetParent() { return m_Parent; }

        /// <summary>
        /// Check if the item definition contains the item.
        /// </summary>
        /// <param name="itemIdentifier">The item to check if it is contained in the definition.</param>
        /// <returns>Returns true if the item is part of the definition.</returns>
        public override bool InherentlyContains(IItemIdentifier itemIdentifier)
        {
            return InherentlyContains(itemIdentifier as Item);
        }

        /// <summary>
        /// Returns the category of the current identifier.
        /// </summary>
        /// <returns>The category of the current identifier. Can be null.</returns>
        public override IItemCategoryIdentifier GetItemCategory()
        {
            return m_Category;
        }

        /// <summary>
        /// Determines if the Item Definition contains another.
        /// </summary>
        /// <param name="other">The other Item Definition.</param>
        /// <returns>True if the other Item Definition.</returns>
        public bool InherentlyContains(ItemDefinition other)
        {
            var itemDefs = GenericObjectPool.Get<ItemDefinition[]>();

            var otherAllParentsCount = other.GetAllParents(ref itemDefs, true);
            if (itemDefs.Contains(this, 0, otherAllParentsCount)) {
                GenericObjectPool.Return(itemDefs);
                return true;
            }
            GenericObjectPool.Return(itemDefs);

            return false;
        }

        /// <summary>
        /// Determines if the Item Definition contains item.
        /// </summary>
        /// <param name="item">The Item.</param>
        /// <returns>True if the the item is child.</returns>
        public bool InherentlyContains(Item item)
        {
            return InherentlyContains(item.ItemDefinition);
        }

        /// <summary>
        /// Determines if the Item Definition contains another.
        /// </summary>
        /// <param name="other">The other Item Definition.</param>
        /// <returns>True if the other Item Definition.</returns>
        public bool DirectlyContains(ItemDefinition other)
        {
            if (m_Children.Contains(other, 0, m_Children.Count)) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the Item Definition contains the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if the other itemDefinition.</returns>
        public bool DirectlyContains(Item item)
        {
            return item.ItemDefinition == this;
        }

        /// <summary>
        /// Determines if the Item Definition is from the same family as the item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if the definition root is the same.</returns>
        public bool SameFamilyAs(Item item)
        {
            return SameFamilyAs(item?.ItemDefinition);
        }

        /// <summary>
        /// Determines if the Item Definition is from the same family as the item.
        /// </summary>
        /// <param name="itemDefinition">The itemDefinition.</param>
        /// <returns>True if the definition root is the same.</returns>
        public bool SameFamilyAs(ItemDefinition itemDefinition)
        {
            if (itemDefinition == null) { return false; }
            return GetRoot() == itemDefinition.GetRoot();
        }

        #endregion

        #region Attributes

        /// <summary>
        /// Update the itemDefinition attributes so that it matches the ones defined in the category.
        /// </summary>
        /// <returns>False if something went wrong.</returns>
        public virtual bool UpdateAttributes()
        {
            var requiredItemDefinitionAttributes = Category.GetDefinitionAttributeList();
            var result = m_ItemDefinitionAttributeCollection.UpdateAttributesToMatchList((requiredItemDefinitionAttributes, 0), true);

            ReevaluateAttributes();

            //We also need to reevaluate all the children otherwise we do not sync the inheritance.
            var itemDefs = GenericObjectPool.Get<ItemDefinition[]>();
            var itemDefChildrenCount = GetAllChildren(ref itemDefs, false);
            for (int i = 0; i < itemDefChildrenCount; i++) {
                itemDefs[i]?.ReevaluateAttributes();
            }
            GenericObjectPool.Return(itemDefs);

            return result;
        }

        /// <summary>
        /// Override an ItemDefinition attribute.
        /// </summary>
        /// <param name="attribute">The new attribute.</param>
        /// <returns>False if the attribute could not override another.</returns>
        public virtual bool OverrideAttribute(AttributeBase attribute)
        {
            var result = m_ItemDefinitionAttributeCollection.OverrideAttribute(attribute, false);
            UpdateAttributes();
            return result;
        }

        /// <summary>
        /// Override an ItemDefinition attribute.
        /// </summary>
        /// <param name="newAttributes">A list of attributes used to override existing attributes.</param>
        public virtual void OverrideAttributes(ListSlice<AttributeBase> newAttributes)
        {
            m_ItemDefinitionAttributeCollection.OverrideAttributes(newAttributes, false);
            UpdateAttributes();
        }

        /// <summary>
        /// Returns true if the attribute exists in the ItemDefinition.
        /// Can also return the category attributes if specified.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="includeCategoryAttributes">If true, it will also search in the attribute category attributes.</param>
        /// <returns>True if the attribute exists.</returns>
        public virtual bool HasAttribute(string attributeName, bool includeCategoryAttributes = true)
        {
            var result = m_ItemDefinitionAttributeCollection.ContainsAttribute(attributeName);
            if (result == false && includeCategoryAttributes) {
                return Category.HasCategoryAttribute(attributeName);
            }
            return result;
        }

        /// <summary>
        /// Try get the itemDefinition attribute.
        /// Use this if you do not know the type of the attribute.
        /// Can also return the category attributes if specified.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attribute">Output of the attribute found.</param>
        /// <param name="includeCategoryAttributes">If true, it will also search in the attribute category attributes.</param>
        /// <returns>True if the attribute exists.</returns>
        public bool TryGetAttribute(string attributeName, out AttributeBase attribute, bool includeCategoryAttributes = true)
        {
            var result = m_ItemDefinitionAttributeCollection.TryGetAttribute(attributeName, out attribute);
            if (result == false && includeCategoryAttributes) {
                return Category.TryGetCategoryAttribute(attributeName, out attribute);
            }
            return result;
        }

        /// <summary>
        /// Get the itemDefinition attribute.
        /// Use this if you know the type of the attribute.
        /// Can also return the category attributes if specified.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="includeCategoryAttributes">If true, it will also search in the attribute category attributes.</param>
        /// <returns>The attribute, can be null.</returns>
        public virtual T GetAttribute<T>(string attributeName, bool includeCategoryAttributes = true) where T : AttributeBase
        {
            var result = m_ItemDefinitionAttributeCollection.GetAttribute<T>(attributeName);
            if (result == null && includeCategoryAttributes) {
                return Category.GetCategoryAttribute<T>(attributeName);
            }
            return result;
        }

        /// <summary>
        /// Returns the value of the itemDefinition attribute.
        /// Use this if you only need the attribute value.
        /// Can also return the category attributes if specified.
        /// </summary>
        /// <typeparam name="T">The attribute value type</typeparam>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="attributeValue">output of the attribute value.</param>
        /// <param name="includeCategoryAttributes">If true, it will also search in the attribute category attributes.</param>
        /// <returns>True if the attribute exists.</returns>
        public virtual bool TryGetAttributeValue<T>(string attributeName, out T attributeValue, bool includeCategoryAttributes = true)
        {
            var result = m_ItemDefinitionAttributeCollection.TryGetAttributeValue<T>(attributeName, out attributeValue);
            if (result == false && includeCategoryAttributes) {
                return Category.TryGetCategoryAttributeValue<T>(attributeName, out attributeValue);
            }
            return result;
        }

        /// <summary>
        /// Returns a list of the attributes in the ItemDefinition.
        /// </summary>
        /// <returns>The list of attributes.</returns>
        public virtual IReadOnlyList<AttributeBase> GetAttributeList()
        {
            return m_ItemDefinitionAttributeCollection;
        }

        /// <summary>
        /// Returns a count of the attributes in the ItemDefinition.
        /// Can also tak into account the category attributes if specified.
        /// </summary>
        /// <param name="includeCategoryAttributes">Should the count include the category attributes.</param>
        /// <returns>The number of attributes.</returns>
        public virtual int GetAttributeCount(bool includeCategoryAttributes)
        {
            return m_ItemDefinitionAttributeCollection.Count
                   + (includeCategoryAttributes ? Category.GetCategoryAttributeCount() : 0);
        }

        /// <summary>
        /// Returns an attribute with the index of the attributes in the ItemDefinition.
        /// Can also tak into account the category attributes if specified.
        /// </summary>
        /// <param name="index">Index of the attribute.</param>
        /// <param name="includeCategoryAttributes">Should the count include the category attributes.</param>
        /// <returns>The attribute at the index provided.</returns>
        public virtual AttributeBase GetAttributeAt(int index, bool includeCategoryAttributes)
        {
            if (index < m_ItemDefinitionAttributeCollection.Count) {
                return m_ItemDefinitionAttributeCollection[index];
            }

            if (includeCategoryAttributes) {
                index -= m_ItemDefinitionAttributeCollection.Count;

                if (index < Category.GetCategoryAttributeCount()) {
                    return Category.GetCategoryAttributeAt(index);
                }
            }

            return null;
        }

        /// <summary>
        /// Reevaluate the attributes of the itemDefinition.
        /// </summary>
        public void ReevaluateAttributes()
        {
            for (int i = 0; i < m_ItemDefinitionAttributeCollection.Count; i++) {
                if (m_ItemDefinitionAttributeCollection[i].IsPreEvaluated) {
                    m_ItemDefinitionAttributeCollection[i].ReevaluateValue(false);
                }
            }

            DefaultItem.ReevaluateAttributes();
        }

        #endregion

        /// <summary>
        /// Returns a custom string.
        /// </summary>
        /// <returns>The custom string.</returns>
        public override string ToString()
        {
            if (this == null) { return "NULL"; }

            return name;
        }
    }
}
