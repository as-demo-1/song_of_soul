/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.AttributeSystem
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using UnityEngine;

    /// <summary>
    /// Enum for specifying the type of variance of an attribute.
    /// </summary>
    [Serializable]
    public enum VariantType
    {
        Inherit,     //The attribute will return its parent value.
        Override,    //The attribute will return the value specified by the override value.
        Modify       //The attribute will evaluate the modify expression and return the resulting value.
    }

    /// <summary>
    /// Attributes are used to store values of any type. This is the abstract base class which implements all the logic of inheritance and expressions.
    /// </summary>
    [Serializable]
    [UnityEngine.Scripting.Preserve]
    public abstract class AttributeBase //: IEquatable<AttributeBase>
    {
        public event Action<AttributeBase> OnAttributeChanged;

        [Tooltip("The name of the attribute.")]
        [SerializeField] protected string m_Name;
        [Tooltip("If true the attribute value is pre-evaluated in the editor.")]
        [SerializeField] protected bool m_IsPreEvaluated;
        [Tooltip("The variant type will affect which value is fetched, 'Modify' will compute a value by solving the modify Expression.")]
        [SerializeField] protected VariantType m_VariantType;
        [Tooltip("The expression used to modify a value (The expression is dependant on the value type but most expression use [attributeName] <VariantType> and $ with +*/^ operators), Refer to the documentation for more information.")]
        [SerializeField] protected string m_ModifyExpression;
        [Tooltip("The Connection ID is used to save where the attribute comes from, in case a connected attribute is modified.")]
        [SerializeField] protected int m_ConnectionID;

        [Tooltip("The Collection that contains this attribute.")]
        protected AttributeCollection m_Collection;

        public string Name {
            get => m_Name;
            internal set {
                if (m_Name == value) { return; }
                m_Name = value;
                NotifyChange();
            }
        }

        public bool IsPreEvaluated => m_IsPreEvaluated;
        public VariantType VariantType => m_VariantType;
        public string ModifyExpression => m_ModifyExpression;
        public int ConnectionID => m_ConnectionID;

        public ItemCategory AttachedItemCategory => m_Collection?.AttachedItemCategory;
        public ItemDefinition AttachedItemDefinition => m_Collection?.AttachedItemDefinition;
        public Item AttachedItem => m_Collection?.AttachedItem;
        public Type AttributeCollectionType => m_Collection?.GetType();
        public int AttributeCollectionIndex => m_Collection.Index;

        public virtual bool IsMutable => AttachedItem == null
                                         || ReferenceEquals(AttachedItemDefinition.DefaultItem, AttachedItem)
                                         || AttachedItem.IsMutable != false;

        internal AttributeCollection AttributeCollection => m_Collection;

        // Regex Patterns for the modify expression.
        // () are capture groups. (?: ) are non-capture groups. m.Groups[0].Value matches everything, m.Groups[1].Value is the first matching group.
        public const string c_RelativeAttributePattern = @"(?:\s|^|[^\$])\[(.*?)\]";
        public const string c_AbsoluteAttributePattern = @"[\$]\[(.*?)\]";
        public const string c_RelativeOverridePattern = @"(\s|^|[^\$])(?:<Override>|<override>)";
        public const string c_AbsoluteOverridePattern = @"[\$](?:<Override>|<override>)";
        public const string c_RelativeInheritedPattern = @"(\s|^|[^\$])(?:<Inherited>|<inherited>)";
        public const string c_AbsoluteInheritedPattern = @"[\$](?:<Inherited>|<inherited>)";

        #region Initialize

        /// <summary>
        /// Creates an attribute of the type you want at runtime using reflection.
        /// </summary>
        /// <param name="type">The type of the new Attribute.</param>
        /// <param name="name">The name of the new Attribute.</param>
        /// <param name="value">The value of the new Attribute.</param>
        /// <param name="variantType">The variantType of the new Attribute.</param>
        /// <param name="modifyExpression">The modifyExpression of the new Attribute.</param>
        /// <returns>The newly created attribute.</returns>
        public static AttributeBase CreateInstance(Type type, string name, object value = null, VariantType variantType = VariantType.Inherit, string modifyExpression = "")
        {
            //Use reflection to create the instance.
            //Parameters example: string name, int OverrideValue = 0, VariantType variantType = VariantType.Override, string modifyExpression = "" .

            if (type.IsSubclassOf(typeof(AttributeBase)) == false) {
                return null;
            }

            var getTypeMethodInfo = type.GetMethod("GetAttributeValueType");
            Type valueType = null;
            if (getTypeMethodInfo != null) {
                valueType = (Type)getTypeMethodInfo.Invoke(null, null); // (null, null) means calling static method with no parameters
            }

            if (value == null || valueType != value.GetType()) {
                value = Type.Missing;
            }

            var newAttribute = Activator.CreateInstance(type,
                BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance | BindingFlags.OptionalParamBinding, null,
                new object[] { name, value, variantType, modifyExpression },
                CultureInfo.CurrentCulture) as AttributeBase;

            return newAttribute;
        }

        /// <summary>
        /// Default Constructor used by the Editor to create an empty attributeBase.
        /// It is protected so that the subClasses can take advantage of it.
        /// Can be used through Reflection.
        /// </summary>
        protected AttributeBase()
        {
            m_Name = "NewAttribute";
            m_IsPreEvaluated = false;
            m_VariantType = VariantType.Inherit;
            m_ModifyExpression = "";
        }

        /// <summary>
        /// Constructor base should be used by subclass for convenience.
        /// </summary>
        /// <param name="name">The attribute name.</param>
        /// <param name="variantType">The variant Type.</param>
        /// <param name="modifyExpression">The modify expression.</param>
        protected AttributeBase(string name, VariantType variantType = VariantType.Override, string modifyExpression = "")
        {
            m_Name = name;
            m_IsPreEvaluated = false;
            m_VariantType = variantType;
            m_ModifyExpression = modifyExpression;
        }

        /// <summary>
        /// Add this attribute to a collection. An attribute can only be part of a single collection.
        /// </summary>
        /// <param name="collection">The AttributeCollection in which this attribute will be added.</param>
        public void AddedTo(AttributeCollection collection)
        {
            if (m_Collection == null) {
                m_Collection = collection;

                if (m_ConnectionID != 0) { return; }

                AttributeBase source = null;
                
                //For performance get inherit attribute instead of source attribute.
                if (Application.isPlaying) {
                    source = GetInheritAttribute();
                } else {
                    source = GetSourceAttribute();
                }

                if (source == null || object.ReferenceEquals(source, this)) {
                    m_ConnectionID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                } else {
                    m_ConnectionID = source.m_ConnectionID;
                }
                
            } else {
                Debug.LogWarning("The attribute was already added to another collection, it can only be part of one collection at a time.");
            }
        }

        /// <summary>
        /// Deserializes the attribute.
        /// </summary>
        public virtual void Deserialize() { }

        /// <summary>
        /// Serializes the attribute.
        /// </summary>
        public virtual void Serialize()
        {
            ReevaluateValue(false);
        }

        #endregion

        #region Setters

        /// <summary>
        /// Set the variant type. It can fail.
        /// </summary>
        /// <param name="variantType">The new variant type.</param>
        /// <param name="reevaluate">Reevaluate the attribute value.</param>
        public void SetVariantType(VariantType variantType, bool reevaluate = false)
        {
            if (m_VariantType == variantType) { return; }

            var canSet = IsMutable;

#if UNITY_EDITOR
            if (!Application.isPlaying) { canSet = true; }
#endif

            if (!canSet) {
                Debug.LogWarning("You are not allowed to change variantType of an Immutable Attribute.");
                return;
            }

            m_VariantType = variantType;

            if (reevaluate) {
                ReevaluateValue(true);
            } else {
                m_IsPreEvaluated = false;
            }

            NotifyChange();
        }

        /// <summary>
        /// Set the modify expression. It can fail.
        /// </summary>
        /// <param name="modifyExpression">The new modify expression.</param>
        /// <param name="setVariantTypeToModify">set the variant Type to modify.</param>
        /// <param name="reevaluate">Reevaluate the attribute value.</param>
        public void SetModifyExpression(string modifyExpression, bool setVariantTypeToModify, bool reevaluate = true)
        {
            var canSet = IsMutable;

#if UNITY_EDITOR
            if (!Application.isPlaying) { canSet = true; }
#endif

            if (!canSet) {
                Debug.LogWarning("You are not allowed to change the modifyExpression of an Immutable Attribute.");
                return;
            }

            m_ModifyExpression = modifyExpression;
            if (setVariantTypeToModify) { SetVariantType(VariantType.Modify); }

            if (reevaluate) {
                ReevaluateValue(true);
            } else {
                m_IsPreEvaluated = false;
            }

            NotifyChange();
        }

        /// <summary>
        /// Set the override value as an object. This can cause boxing.
        /// </summary>
        /// <param name="overrideValue">The new Override value.</param>
        /// <param name="setVariantTypeToOverride">Set the variant type to override.</param>
        /// <param name="reevaluate">Reevaluate the attribute value.</param>
        public abstract void SetOverrideValueAsObject(object overrideValue, bool setVariantTypeToOverride = true, bool reevaluate = false);

        /// <summary>
        /// Set the override value as an object. This can cause boxing.
        /// </summary>
        /// <param name="overrideValue">The new Override value.</param>
        internal abstract void SetOverrideValueAsObjectWithoutNotify(object overrideValue);

        /// <summary>
        /// Send an event that the attribute changed in some way.
        /// </summary>
        public virtual void NotifyChange()
        {
            m_Collection?.AttributeChanged(this);
            OnAttributeChanged?.Invoke(this);
        }

        #endregion

        #region getters for related objects

        /// <summary>
        /// Returns true if this attribute is part of an attribute collection.
        /// </summary>
        public bool IsPartOfCollection => m_Collection != null;

        /// <summary>
        /// Return the Unity Object on which this attribute is attached.
        /// This is mostly used to know which object should be set dirty when the collection is serialized.
        /// </summary>
        /// <returns>Returns the object containing the Collection that contains this attribute.</returns>
        public UnityEngine.Object GetAttachedObject()
        {
            return m_Collection?.GetAttachedObject();
        }

        /// <summary>
        /// Returns an Item if the collection is attached to an item.
        /// </summary>
        /// <returns>The Item that contains the inherit attribute.</returns>
        public Item GetInheritItem()
        {
            var inheritAttribute = GetInheritAttribute();
            if (inheritAttribute == null) { return null; }
            return inheritAttribute.AttachedItem;
        }

        /// <summary>
        /// Returns an Item Definition if the collection is attached to an item or an Item Definition.
        /// </summary>
        /// <returns>The Item Definition that contains the inherit attribute.</returns>
        public ItemDefinition GetInheritItemDefinition()
        {
            var inheritAttribute = GetInheritAttribute();
            if (inheritAttribute == null) { return null; }
            return inheritAttribute.AttachedItemDefinition;
        }

        /// <summary>
        /// Returns a category if the collection is attached to anything.
        /// </summary>
        /// <returns>The category that contains the inherit attribute.</returns>
        public ItemCategory GetInheritCategory()
        {
            //return m_Collection.AttachedItemCategory;
            var inheritAttribute = GetInheritAttribute();
            if (inheritAttribute == null && m_Collection != null) {
                return m_Collection.AttachedItemCategory;
            }
            return inheritAttribute?.AttachedItemCategory;
        }

        /// <summary>
        /// Get the category where this attribute type/name is first introduced.
        /// </summary>
        /// <returns>The category that contains the first appearance of this attribute name/type.</returns>
        public ItemCategory GetSourceCategory()
        {
            var inheritCategory = GetInheritCategory();
            if (inheritCategory == null) { return null; }
            if (m_Collection.AttachedItemDefinition == null &&
                inheritCategory == m_Collection.AttachedItemCategory) {
                return inheritCategory;
            }

            if (!inheritCategory.TryGetCategoryAttribute(m_Name, out var other)) {
                if (!inheritCategory.TryGetDefinitionAttribute(m_Name, out other)) {
                    inheritCategory.TryGetItemAttribute(m_Name, out other);
                }
            }
            if (other == null) { return inheritCategory; }

            return other.GetSourceCategory();
        }

        /// <summary>
        /// Get the attribute that that is part of the source category.
        /// </summary>
        /// <returns>The attribute that is the root appearance of this attribute name.</returns>
        public AttributeBase GetSourceAttribute()
        {
            return m_Collection?.GetSourceOfAttribute(this);
        }

        /// <summary>
        /// Get the number of sources for this attribute (only checks into parents, not parents of children).
        /// </summary>
        /// <returns>Get the source Count.</returns>
        public int GetThisSourceCount()
        {
            if (m_Collection == null) { return 1; }

            if (GetInheritAttribute() == null) { return 1; }

            var category = m_Collection.AttachedItemCategory;
            var pooledParentCategories = GenericObjectPool.Get<ItemCategory[]>();
            var parentCount = IterationHelper.GetLeafRecursiveDFS(ref pooledParentCategories, category,
                parentCategory => parentCategory.Parents);
            GenericObjectPool.Return(pooledParentCategories);

            var sourceCount = 0;
            for (int i = 0; i < parentCount; i++) {
                var parent = pooledParentCategories[i];
                if (parent.GetAttribute(Name) == null) { continue; }

                sourceCount++;
            }

            return sourceCount;
        }

        /// <summary>
        /// Get all the attribute sources for this attribute (only checks into parents, not parents of children).
        /// </summary>
        /// <param name="attributeSources">The reference to the array of attribute sources (Can be resized).</param>
        /// <returns>Get the sources of this attribute.</returns>
        public ListSlice<AttributeBase> GetThisSources(ref AttributeBase[] attributeSources)
        {
            TypeUtility.ResizeIfNecessary(ref attributeSources, 1);

            if (m_Collection == null || GetInheritAttribute() == null) {
                attributeSources[0] = this;
                return (attributeSources, 0, 1);
            }

            var category = m_Collection.AttachedItemCategory;
            var pooledParentCategories = GenericObjectPool.Get<ItemCategory[]>();
            var parentCount = IterationHelper.GetLeafRecursiveDFS(ref pooledParentCategories, category,
                parentCategory => parentCategory.Parents);
            GenericObjectPool.Return(pooledParentCategories);

            var sourceCount = 0;
            for (int i = 0; i < parentCount; i++) {
                var parent = pooledParentCategories[i];

                var sourceAttribute = parent.GetAttribute(Name);
                if (sourceAttribute == null) { continue; }

                TypeUtility.ResizeIfNecessary(ref attributeSources, sourceCount);

                attributeSources[sourceCount] = sourceAttribute;
                sourceCount++;
            }

            return (attributeSources, 0, sourceCount);
        }

        /// <summary>
        /// Get the source count of the attribute family.
        /// </summary>
        /// <returns>The source count of the attribute family.</returns>
        public int GetFamilySourceCount()
        {
            if (m_Collection == null) { return 1; }

            var category = m_Collection.AttachedItemCategory;
            var pooledSourcesCategory = GenericObjectPool.Get<ItemCategory[]>();
            var pooledChildrenCategories = GenericObjectPool.Get<ItemCategory[]>();
            var pooledParentCategories = GenericObjectPool.Get<ItemCategory[]>();
            var leafChildrenCount = IterationHelper.GetLeafRecursiveDFS(ref pooledChildrenCategories, category,
                x => x.Children);

            var sourceCount = 0;
            for (int i = 0; i < leafChildrenCount; i++) {
                var childCategory = pooledChildrenCategories[i];
                var parentCount = IterationHelper.GetLeafRecursiveDFS(ref pooledParentCategories, childCategory,
                    x => x.Parents);

                for (int j = 0; j < parentCount; j++) {
                    var potentialSourceCategory = pooledParentCategories[j];

                    var sourceAttribute = potentialSourceCategory.GetAttribute(Name);
                    if (sourceAttribute == null) { continue; }

                    var containsSource = false;
                    for (int k = 0; k < sourceCount; k++) {
                        if (pooledSourcesCategory[k] != potentialSourceCategory) { continue; }

                        containsSource = true;
                        break;
                    }

                    if (containsSource) { continue; }

                    TypeUtility.ResizeIfNecessary(ref pooledSourcesCategory, sourceCount);

                    pooledSourcesCategory[sourceCount] = potentialSourceCategory;
                    sourceCount++;

                }
            }

            GenericObjectPool.Return(pooledParentCategories);
            GenericObjectPool.Return(pooledChildrenCategories);
            GenericObjectPool.Return(pooledSourcesCategory);


            return sourceCount;
        }

        /// <summary>
        /// Get the source count of the attribute family.
        /// </summary>
        /// <param name="attributeSources">The reference to the array of attribute sources (Can be resized).</param>
        /// <returns>List slice of the attribute sources.</returns>
        public ListSlice<AttributeBase> GetAttributeFamilySources(ref AttributeBase[] attributeSources)
        {
            TypeUtility.ResizeIfNecessary(ref attributeSources, 1);
            if (m_Collection == null) {
                attributeSources[0] = this;
                return (attributeSources, 0, 1);
            }

            var category = m_Collection.AttachedItemCategory;
            var pooledChildrenCategories = GenericObjectPool.Get<ItemCategory[]>();
            var pooledParentCategories = GenericObjectPool.Get<ItemCategory[]>();
            var leafChildrenCount = IterationHelper.GetLeafRecursiveDFS(ref pooledChildrenCategories, category,
                x => x.Children);

            var sourceCount = 0;
            for (int i = 0; i < leafChildrenCount; i++) {
                var childCategory = pooledChildrenCategories[i];
                var parentCount = IterationHelper.GetLeafConditionalRecursiveDFS(ref pooledParentCategories, childCategory,
                    x => x.Parents, x => x.HasAttribute(m_Name));

                for (int j = 0; j < parentCount; j++) {
                    var potentialSourceCategory = pooledParentCategories[j];

                    var sourceAttribute = potentialSourceCategory.GetAttribute(Name);
                    if (sourceAttribute == null) { continue; }

                    var containsSource = false;
                    for (int k = 0; k < sourceCount; k++) {
                        if (attributeSources[k] != sourceAttribute) { continue; }

                        containsSource = true;
                        break;
                    }

                    if (containsSource) { continue; }

                    TypeUtility.ResizeIfNecessary(ref attributeSources, sourceCount);

                    attributeSources[sourceCount] = sourceAttribute;
                    sourceCount++;

                }
            }

            GenericObjectPool.Return(pooledParentCategories);
            GenericObjectPool.Return(pooledChildrenCategories);

            return (attributeSources, 0, sourceCount);
        }

        /// <summary>
        /// Get all the attributes that is part of its family.
        /// Meaning the attributes that have the same name and type.
        /// </summary>
        /// <returns>The family list of attributes.</returns>
        public List<AttributeBase> GetAttributeFamily()
        {
            var pooledAttributeSources = GenericObjectPool.Get<AttributeBase[]>();
            var sources = GetAttributeFamilySources(ref pooledAttributeSources);

            var attributeFamily = new List<AttributeBase>();
            for (int i = 0; i < sources.Count; i++) {
                var children = sources[i].GetChildrenAttribute(true);
                for (int j = 0; j < children.Count; j++) {
                    if (attributeFamily.Contains(children[j])) { continue; }

                    attributeFamily.Add(children[j]);
                }
            }

            GenericObjectPool.Return(pooledAttributeSources);
            return attributeFamily;
        }

        /// <summary>
        /// Get the Unity Objects of all the attributes in the family.
        /// Mostly used to set the objects dirty when the collection is serialized.
        /// </summary>
        /// <param name="familyObjects">FamilyObjects.</param>
        /// <returns>The objects containing the family attributes.</returns>
        public ListSlice<UnityEngine.Object> GetAttributeObjectFamily(ref UnityEngine.Object[] familyObjects)
        {
            var attributeFamily = GetAttributeFamily();
            var size = attributeFamily.Count;
            TypeUtility.ResizeIfNecessary(ref familyObjects, size);

            for (int i = 0; i < size; i++) {

                familyObjects[i] = attributeFamily[i].GetAttachedObject();
            }

            return (familyObjects, 0, size);
        }

        /// <summary>
        /// Get all the attributes that are children of this attribute.
        /// Meaning any attribute which inherits it and the children of the children and so on.
        /// </summary>
        /// <param name="includeThis">If true this attribute will be taken into account.</param>
        /// <returns>List of children attributes.</returns>
        public List<AttributeBase> GetChildrenAttribute(bool includeThis)
        {
            if (m_Collection == null) {
                return new List<AttributeBase>();
            }

            return m_Collection.GetChildrenOfAttribute(this, includeThis);
        }

        /// <summary>
        /// Get the attribute which this attribute inherits from.
        /// </summary>
        /// <returns>The inherit attribute.</returns>
        public virtual AttributeBase GetInheritAttribute()
        {
            return m_Collection?.GetInheritOfAttribute(this);
        }

        #endregion

        #region rename retype

        /// <summary>
        /// Rename the attribute, of course this renames this attribute and all of its family.
        /// </summary>
        /// <param name="newName">The new name for this attribute.</param>
        /// <returns>True if the renaming was a success.</returns>
        public bool Rename(string newName)
        {
            var sourceCategory = GetSourceCategory();
            var attributeSource = GetSourceAttribute();
            var family = GetAttributeFamily();

            if (sourceCategory == null || attributeSource == null || family == null) {
                m_Name = newName;
                NotifyChange();
                return true;
            }

            var result = true;
            foreach (var attribute in family) {
                var category = attribute.AttachedItemCategory;
                if (category == null) { continue; }

                if (category.IsNewAttributeValid(newName, attributeSource.GetType(), attributeSource.AttributeCollectionIndex)) { continue; }

                result = false;
                break;
            }

            if (!result) { return false; }

            var previousName = m_Name;
            foreach (var attribute in family) {
                attribute.m_Name = newName;
                attribute.NotifyChange();
            }

            return m_Name == newName;
        }

        /// <summary>
        /// Rename the attribute, of course this renames this attribute and all of its family.
        /// </summary>
        /// <param name="newName">The new name for this attribute.</param>
        /// <param name="existingPriority">Prioritize attributes that already exist with the new name.</param>
        /// <returns>True if the renaming was a success.</returns>
        public bool MergeRename(string newName, bool existingPriority)
        {
            var sourceCategory = GetSourceCategory();
            var attributeSource = GetSourceAttribute();
            var family = GetAttributeFamily();

            var previousName = attributeSource.Name;

            if (existingPriority) {
                foreach (var otherAttribute in family) {
                    var category = otherAttribute.AttachedItemCategory;
                    if (category == null) { continue; }

                    var existingAttribute = category.GetAttribute(newName);
                    if (existingAttribute == null) {
                        otherAttribute.m_Name = newName;
                        otherAttribute.NotifyChange();
                        continue;
                    }

                    if (otherAttribute.AttributeCollectionType == typeof(ItemCategoryAttributeCollection)) {
                        category.ItemCategoryAttributeCollection.RemoveAttribute(previousName);
                    }
                    if (otherAttribute.AttributeCollectionType == typeof(RequiredItemDefinitionAttributeCollection)) {
                        category.ItemDefinitionAttributeCollection.RemoveAttribute(previousName);
                    }
                    if (otherAttribute.AttributeCollectionType == typeof(RequiredItemAttributeCollection)) {
                        category.ItemAttributeCollection.RemoveAttribute(previousName);
                    }
                }

                return true;
            }

            foreach (var otherAttribute in family) {
                var category = otherAttribute.AttachedItemCategory;
                if (category == null) { continue; }

                var existingAttribute = category.GetAttribute(newName);
                if (existingAttribute == null) { continue; }

                category.RemoveAttributeFromAll(existingAttribute.Name, Relation.Family, true);
            }

            return Rename(newName);
        }

        /// <summary>
        /// Change the type of this attribute and all of its family.
        /// Note This will remove the current attributes and replace them with new ones.
        /// </summary>
        /// <param name="newType">The new type for this attribute.</param>
        /// <returns>Returns a failed message if something is wrong.</returns>
        public AttributeBase ChangeType(Type newType)
        {
            Type newAttributeType;
            if (typeof(AttributeBase).IsAssignableFrom(newType)) {
                newAttributeType = newType;
            } else {
                newAttributeType = typeof(Attribute<>).MakeGenericType(newType);
            }

            if (GetType() == newAttributeType) { return this; }

            var attachedItem = AttachedItem;
            var attachedItemDefinition = AttachedItemDefinition;
            var attachedItemCategory = AttachedItemCategory;

            if (m_Collection == null) {
                return null;
            }

            if (m_Collection.ChangeAttributeType(this, newType) == false) {
                return null;
            }

            if (attachedItem != null) {
                if (attachedItem.TryGetAttribute(m_Name, out var newAttribute)) {
                    return newAttribute;
                }
            }
            if (attachedItemDefinition != null) {
                if (attachedItemDefinition.TryGetAttribute(m_Name, out var newAttribute)) {
                    return newAttribute;
                }
            }

            if (attachedItemCategory == null || attachedItemCategory.HasAttribute(m_Name) == false) {
                Debug.LogWarning("Failed to change the attribute type.");
                return null;
            }

            return attachedItemCategory.GetAttribute(m_Name);
        }

        #endregion

        #region abstract Functions

        /// <summary>
        /// Check if the current attribute value is equivalent to another object.
        /// </summary>
        /// <param name="otherValue">The other object.</param>
        /// <returns>True if equivalent.</returns>
        public abstract bool ValueEquivalentTo(object otherValue);

        /// <summary>
        /// Returns the type of the value of the attribute.
        /// </summary>
        /// <returns>The value Type.</returns>
        public abstract Type GetValueType();

        /// <summary>
        /// Get the value of the attribute as an object.
        /// </summary>
        /// <returns>The value as an object.</returns>
        public abstract object GetValueAsObject();

        /// <summary>
        /// Get the Inherit value as an object.
        /// </summary>
        /// <returns>The inherit value as an object.</returns>
        public abstract object GetInheritValueAsObject();

        /// <summary>
        /// Get the override value as an object.
        /// </summary>
        /// <returns>The override value as an object.</returns>
        public abstract object GetOverrideValueAsObject();

        /// <summary>
        /// Reevaluate the attribute value to update the preEvaluatedValue.
        /// </summary>
        /// <param name="setAsPreEvaluated">If true the IsPreEvaluated bool will be set to true.</param>
        public abstract void ReevaluateValue(bool setAsPreEvaluated);

        /// <summary>
        /// Get the variant value as an object, this evaluates the value even if IsPreEvaluated is true.
        /// </summary>
        /// <returns>The variant value as an object.</returns>
        public abstract object GetVariantValueAsObject();

        /// <summary>
        /// Duplicates the attribute, by creating a new one that is equivalent to this one.
        /// </summary>
        /// <returns>The new equivalent attribute.</returns>
        public abstract AttributeBase Duplicate();
        
        /// <summary>
        /// Creates a new instance of the attribute with the correct attribute type.
        /// </summary>
        /// <returns>The new equivalent attribute.</returns>
        public abstract AttributeBase NewCopy(string name, VariantType variantType = VariantType.Inherit);

        /// <summary>
        /// Get the modify value by evaluating the modify expression.
        /// </summary>
        /// <param name="modifyValue">Output of the modify value.</param>
        /// <returns>Returns a fail message if something went wrong with the expression evaluation.</returns>
        public abstract bool TryGetModifyValueAsObject(out object modifyValue);

        /// <summary>
        /// Bind the attribute to the binding.
        /// </summary>
        /// <param name="binding">the attribute binding.</param>
        public abstract void Bind(AttributeBinding binding);

        /// <summary>
        /// Unbind the attribute.
        /// </summary>
        public abstract void Unbind(bool applyValueToBinding);

        #endregion

        #region Modify Expression Functions

        /// <summary>
        /// Returns a list of groupCollection with matches of the regex expressions for parameters.
        /// </summary>
        /// <param name="expression">The regex expression to find parameters.</param>
        /// <param name="relative">Check parameters that are relative to this attribute.</param>
        /// <returns>List of matches.</returns>
        public static IReadOnlyList<GroupCollection> ParametersInString(string expression, bool relative)
        {
            var parameters = new List<GroupCollection>();
            var pattern = relative ? c_RelativeAttributePattern : c_AbsoluteAttributePattern;
            var matches = Regex.Matches(expression, pattern);

            foreach (Match m in matches) {
                //m.Groups[0].Value matches everything, m.Groups[1].Value is the first matching group
                parameters.Add(m.Groups);
            }

            return parameters;
        }

        /// <summary>
        /// Replaces the attributes in the string expression with there actual values.
        /// This works for absolute, relative, inherit, override and other attributes.
        /// </summary>
        /// <param name="modifyExpression">The modify expression.</param>
        /// <param name="relativeAttribute">The relative attribute (the root attribute that is trying to evaluate a recursive expression).</param>
        /// <param name="otherAttributes">The other attributes in scope.</param>
        /// <param name="finalExpression">The expression after the process.</param>
        /// <returns>Returns a fail message if something is wrong with the modify expression.</returns>
        public (bool, string) ExpressionRegexReplaceAttributes(string modifyExpression, AttributeBase relativeAttribute, IReadOnlyDictionary<string, AttributeBase> otherAttributes, out string finalExpression)
        {
            finalExpression = modifyExpression;

            //Looking for the absolute attribute parameters.
            var absoluteParameterList = ParametersInString(finalExpression, false);
            for (var i = 0; i < absoluteParameterList.Count; i++) {
                var @group = absoluteParameterList[i];
                var parameter = @group[1].Value;
                if (parameter == Name) {
                    var errorMessage =
                        $"Expression Error: The attribute '{Name}' on the object '{GetAttachedObject()}' cannot reference itself because it will cause an infinit loop";
                    Debug.LogWarning(errorMessage);

                    return (false, errorMessage);
                }
            }

            // Looking for the relative attribute parameters.
            var relativeParameterList = ParametersInString(finalExpression, true);
            for (var i = 0; i < relativeParameterList.Count; i++) {
                var @group = relativeParameterList[i];
                var parameter = @group[1].Value;
                if (parameter == Name) {
                    var errorMessage =
                        $"Expression Error: The attribute '{Name}' on the object '{GetAttachedObject()}' cannot reference itself because it will cause an infinite loop";
                    Debug.LogWarning(errorMessage);

                    return (false, errorMessage);
                }
            }

            //Replace the absolute attribute parameters by their values.
            var resultFromAbsoluteParameters = RegexReplaceAttributesFromString(finalExpression, false, absoluteParameterList, m_Collection, otherAttributes, out var newStringExpressionFromAbsolute);
            if (resultFromAbsoluteParameters.Item1 == false) {
                return resultFromAbsoluteParameters;
            }
            finalExpression = newStringExpressionFromAbsolute;

            var overrideValue = GetOverrideValueAsObject();
            if (overrideValue != null) {
                finalExpression = Regex.Replace(finalExpression, c_AbsoluteOverridePattern, ModifyToString(overrideValue));
            }

            var inheritedValue = GetInheritValueAsObject();
            if (inheritedValue != null) {
                finalExpression = Regex.Replace(finalExpression, c_AbsoluteInheritedPattern, ModifyToString(inheritedValue));
            }

            //Replace the relative attribute parameters by their values.
            var resultFromRelativeParameters = RegexReplaceAttributesFromString(finalExpression, true, relativeParameterList, relativeAttribute.m_Collection, otherAttributes, out var newStringExpressionFromRelative);
            if (resultFromRelativeParameters.Item1 == false) {
                return resultFromRelativeParameters;
            }

            finalExpression = newStringExpressionFromRelative;

            //Relative patterns have to make sure the $ is not in front so it captures whatever is in front of the <>, Therefore we use $1 to add the first capture in front of the value.
            var relativeOverrideValue = relativeAttribute.GetOverrideValueAsObject();
            if (relativeOverrideValue != null) {

                finalExpression = Regex.Replace(finalExpression, c_RelativeOverridePattern, m => string.Format("{0}{1}", m.Groups[1].Value, ModifyToString(relativeOverrideValue)));
            }

            //Can create an infinite loop if the relative attribute is set to inherit. In case the relative attribute is set to inherit the absolute inherited attribute is set instead.
            if (Regex.Match(finalExpression, c_RelativeInheritedPattern).Success == true) {
                if (relativeAttribute.VariantType == VariantType.Inherit || relativeAttribute.VariantType == VariantType.Modify) {
                    if (inheritedValue != null) {
                        finalExpression = Regex.Replace(finalExpression, c_RelativeInheritedPattern, m => string.Format("{0}{1}", m.Groups[1].Value, ModifyToString(inheritedValue)));
                    }
                } else {
                    var relativeInheritedValue = relativeAttribute.GetInheritValueAsObject();
                    if (relativeInheritedValue != null) {
                        finalExpression = Regex.Replace(finalExpression, c_RelativeInheritedPattern, m => string.Format("{0}{1}", m.Groups[1].Value, ModifyToString(relativeInheritedValue)));
                    }
                }
            }

            //Returns finalExpression in the out parameter.
            return (true, null);
        }
        
        /// <summary>
        /// This function returns a string for the object with the appropriate format to be parsed by the expression solver.
        /// </summary>
        /// <param name="value">The value to turn into a string</param>
        /// <returns></returns>
        public string ModifyToString(object value)
        {
            return value is IFormattable formattable ? formattable.ToString("", CultureInfo.InvariantCulture) : value.ToString();
        }

        /// <summary>
        /// Replaces the attributes in the list of GroupCollections with there actual value.
        /// </summary>
        /// <param name="expression">The expression that will have its attributes parameters replaced by their values.</param>
        /// <param name="relative">Whether the attribute is relative or not.</param>
        /// <param name="matchedGroups">All the attribute parameters that need to be replaced in the expression.</param>
        /// <param name="attributeCollection">The collection of attributes that can be used as attribute parameters.</param>
        /// <param name="otherAttributes">External attributes which can be used when solving the expression manually.</param>
        /// <param name="newStringExpression">The expression that comes out once the operation is done.</param>
        /// <returns>Returns a success or fail message.</returns>
        public (bool, string) RegexReplaceAttributesFromString(string expression, bool relative, IReadOnlyList<GroupCollection> matchedGroups, AttributeCollection attributeCollection, IReadOnlyDictionary<string, AttributeBase> otherAttributes, out string newStringExpression)
        {
            newStringExpression = expression;
            var message = "";

            if (attributeCollection == null) {
                var errorMessage = "Expression Error: Missing attribute Collection.";
                Debug.LogWarning(errorMessage);
                return (false, errorMessage);
            }

            for (var i = 0; i < matchedGroups.Count; i++) {
                var @group = matchedGroups[i];
                var parameter = @group[1].Value;

                AttributeBase attribute = null;
                if (attributeCollection.AttachedItem != null &&
                    attributeCollection.AttachedItem.TryGetAttribute(parameter, out attribute)) { } else if (
                    attributeCollection.AttachedItemDefinition != null &&
                    attributeCollection.AttachedItemDefinition.TryGetAttribute(parameter, out attribute)) { } else if (
                    attributeCollection.AttachedItemCategory != null) {
                    if (attributeCollection.AttachedItemCategory.TryGetCategoryAttribute(parameter, out attribute)) { } else if
                    (attributeCollection.AttachedItemCategory.TryGetDefinitionAttribute(parameter,
                        out attribute)) { } else if (attributeCollection.AttachedItemCategory
                        .TryGetItemAttribute(parameter, out attribute)) { }
                } else if (otherAttributes != null && otherAttributes.TryGetValue(parameter, out attribute)) { }

                if (attribute != null) {
                    var pattern = relative
                        ? $@"\[{parameter}\]"
                        : $@"\$\[{parameter}\]";

                    var attributeValue = attribute.GetValueAsObject();
                    var attributeValueText = attributeValue != null ? ModifyToString(attributeValue) : "";

                    newStringExpression = Regex.Replace(newStringExpression, pattern, attributeValueText);

                } else {
                    message += $" ; relative attribute ( {parameter} ) does not exist ; ";
                }
            }

            if (string.IsNullOrWhiteSpace(message) == false) {
                return (false, message);
            }

            return (true, null);
        }

        /// <summary>
        /// A function for simple attribute types that have a value of valueType (or string).
        /// Does a simple find and replace and evaluation of resulting string.
        /// </summary>
        /// <typeparam name="T">The type of the value of the attribute (works on some basic value types and strings only)</typeparam>
        /// <param name="attribute">The attribute that needs its modify Expression evaluated.</param>
        /// <param name="modifyExpression">The Expression of the attribute (note that you can put anything here it does not have to be the attribute.ModifyExpression).</param>
        /// <param name="defaultValue">The default Value in case the expression fails to evaluate.</param>
        /// <param name="relativeAttribute">The relative attribute (the root attribute that is trying to evaluate a recursive expression).</param>
        /// <param name="otherAttributes">The other attributes that are in scope and can be referenced in the expression as parameters.</param>
        /// <param name="evaluatedValue">The value evaluated by the modify Function.</param>
        /// <returns>Returns a success or fails message.</returns>
        public static (bool, string) ValueTypeDefaultModifyFunction<T>(
            AttributeBase attribute, string modifyExpression, T defaultValue,
            AttributeBase relativeAttribute, IReadOnlyDictionary<string, AttributeBase> otherAttributes,
            out object evaluatedValue)
        {
            if (string.IsNullOrEmpty(modifyExpression)) {
                evaluatedValue = defaultValue;
                return (false, "Expression is Empty");
            }

            var (result, message) = attribute.ExpressionRegexReplaceAttributes(
                modifyExpression, relativeAttribute, otherAttributes, out var newExpression);

            if (result == false) {
                evaluatedValue = defaultValue;
                return (result, message);
            }

            var table = new System.Data.DataTable();
            //var e = new Expression(newExpression);
            try {
                //evaluatedValue = e.Evaluate();
                evaluatedValue = table.Compute(newExpression, "");
            } catch (Exception ex) {
                var failMessage = $"Evaluation of expression Failed: {ex.Message}";
                evaluatedValue = defaultValue;
                return (false, failMessage);
            }

            return (true, null);
        }

        #endregion

        #region Equality

        //TODO shouldn't this also compare the override value?
        /// <summary>
        /// Are the attributes equivalent.
        /// </summary>
        /// <param name="other">The other attribute.</param>
        /// <returns>True if the attributes are equivalent.</returns>
        public virtual bool AreEquivalent(AttributeBase other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return m_Name == other.m_Name
                   && m_IsPreEvaluated == other.m_IsPreEvaluated
                   && m_VariantType == other.m_VariantType
                   && m_ModifyExpression == other.m_ModifyExpression;
        }

        #endregion
    }
}