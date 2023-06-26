/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.AttributeSystem
{
    using Opsive.Shared.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Attribute collection is used to organize your attributes.
    /// </summary>
    [Serializable]
    public abstract class AttributeCollection : IReadOnlyList<AttributeBase>
    {
        public event Action<AttributeBase> OnAttributeChanged;

        [Tooltip("The serialization of the attribute collection, required to serialize a generic collection.")]
        [ForceSerialized] [SerializeField] protected Serialization[] m_AttributeCollectionData;

        [Tooltip("The Category which contains this attribute (can be null).")]
        [System.NonSerialized] protected ItemCategory m_ItemCategory;
        [Tooltip("The item definition which contains this attribute (can be null).")]
        [System.NonSerialized] protected ItemDefinition m_ItemDefinition;
        [Tooltip("The item which contains this attribute (can be null).")]
        [System.NonSerialized] protected Item m_Item;
        [Tooltip("This is true after the attribute is initialized.")]
        [System.NonSerialized] protected bool m_Initialized = false;

        [Tooltip("An array struct created in initialize that stores the attributes in the collection.")]
        protected ResizableArray<AttributeBase> m_Attributes;

        internal ResizableArray<AttributeBase> Attributes => m_Attributes;

        public ItemCategory AttachedItemCategory => m_ItemCategory;
        public ItemDefinition AttachedItemDefinition => m_ItemDefinition;
        public Item AttachedItem => m_Item;

        public int Count => m_Attributes?.Count ?? 0;
        public AttributeBase this[int index] => m_Attributes?[index];

        protected bool m_Dirty;
        internal bool Dirty {
            get => m_Dirty;
            set => m_Dirty = value;
        }

        public abstract int Index { get; }

        #region Static Functions

        /// <summary>
        /// Useful function that convert the attributes in a list to a readable string.
        /// </summary>
        /// <param name="attributes">The list slice of attributes.</param>
        /// <returns>A string representation of the attributes and their values.</returns>
        public static string AttributesToString(ListSlice<AttributeBase> attributes)
        {
            if (attributes.Count <= 0) { return "(none)"; }
            var text = $"{attributes[0].Name} : {attributes[0].GetValueAsObject()}";
            for (int i = 1; i < attributes.Count; i++) {
                text += $"\n{attributes[i].Name} : {attributes[i].GetValueAsObject()}";
            }
            return text;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Initialize the collection.
        /// </summary>
        /// <param name="force">Force the initialization.</param>
        protected void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }

            Deserialize(force);

            m_Initialized = true;
        }

        /// <summary>
        /// Deserializes the attributes and stores them in the list and dictionary.
        /// </summary>
        /// <param name="force">Force the initialization.</param>
        protected void Deserialize(bool force)
        {
            if (m_Attributes != null && !force) { return; }

            if (m_AttributeCollectionData == null) {
                if (m_Attributes == null) { m_Attributes = new ResizableArray<AttributeBase>(); }
                return;
            }

            m_Attributes = new ResizableArray<AttributeBase>();
            m_Attributes.Initialize(m_AttributeCollectionData.Length);
            for (int i = 0; i < m_AttributeCollectionData.Length; i++) {
                var attribute = m_AttributeCollectionData[i].DeserializeFields(MemberVisibility.Public) as AttributeBase;
                if (attribute == null) { continue; }
                if (ContainsAttribute(attribute.Name)) {
                    Debug.LogError($"The attribute collection for {GetAttachedObject()} already has an attribute with name: {attribute.Name}.");
                    continue;
                }
                attribute.AddedTo(this);
                attribute.Deserialize();
                m_Attributes.Add(attribute);
            }
        }

        /// <summary>
        /// Serializes the attributes in the collection by adding the attributes inside AttributeCollectionData.
        /// </summary>
        public void Serialize()
        {
            if (m_Attributes == null) {
                m_AttributeCollectionData = Serialization.Serialize<AttributeBase>(new List<AttributeBase>());
                return;
            }

            var attributes = new List<AttributeBase>();
            for (int i = 0; i < m_Attributes.Count; i++) {
                var attribute = m_Attributes[i];
                attribute.Serialize();
                attributes.Add(attribute);
            }

            m_AttributeCollectionData = Serialization.Serialize<AttributeBase>(attributes);
        }

        #endregion

        #region Add Override

        /// <summary>
        /// Add an attribute to the collection, An attribute can only be part of a single collection.
        /// Therefore if the attribute is already part of a collection it can be duplicated and its clone can be added instead.
        /// </summary>
        /// <param name="attribute">The new attribute that will be added.</param>
        /// <param name="addAsNewIfPartOfCollection">If true the attribute will add a clone of the attribute.</param>
        /// <returns>Returns false if the attribute was not added correctly.</returns>
        public virtual bool AddAttribute(AttributeBase attribute, bool addAsNewIfPartOfCollection = true)
        {
            var newAttribute = attribute;
            if (newAttribute.IsPartOfCollection && addAsNewIfPartOfCollection) {
                newAttribute = newAttribute.Duplicate();
            }
            if (ContainsAttribute(newAttribute.Name) == false) {
                newAttribute.AddedTo(this);
                m_Attributes.Add(newAttribute);
                m_Dirty = true;
                return true;
            }

            Debug.LogWarning($"Cannot add attribute {attribute.Name}, an attribute with the same name already exists.");
            return false;
        }

        /// <summary>
        /// Add multiple attributes.
        /// </summary>
        /// <param name="newAttributes">A list of attributes that is about to be added to the collection.</param>
        public virtual void AddAttributes(ListSlice<AttributeBase> newAttributes)
        {
            if (newAttributes.Count == 0) { return; }

            for (int i = 0; i < newAttributes.Count; i++) { AddAttribute(newAttributes[i]); }
        }

        /// <summary>
        /// Override an attribute. If one with the same name & same type already exist it will be overriden.
        /// </summary>
        /// <param name="newAttribute">The new attribute that will override and existing attribute in the collection.</param>
        /// <param name="addEvenIfNothingToOverwrite">Add the attribute even if none with the same name exist in the collection.</param>
        /// <param name="forceOverride">Force the override even if the type don't match.</param>
        /// <returns>Returns false if the attribute did not get overriden correctly.</returns>
        public virtual bool OverrideAttribute(AttributeBase newAttribute, bool addEvenIfNothingToOverwrite = false, bool forceOverride = false)
        {
            if (TryGetAttribute(newAttribute.Name, out var variantAttribute)) {
                if (variantAttribute.GetType() == newAttribute.GetType() || forceOverride) {
                    RemoveAttribute(variantAttribute);
                    AddAttribute(newAttribute);
                    m_Dirty = true;
                    return true;
                } else {
                    Debug.LogWarning("Variant attribute types do not match, even though they have the same name.");
                    return false;
                }
            } else if (addEvenIfNothingToOverwrite) {
                return AddAttribute(newAttribute);
            }
            return false;
        }

        /// <summary>
        /// Override multiple attributes.
        /// </summary>
        /// <param name="newAttributes">A list of attributes that is about to override existing attribute in the collection.</param>
        /// <param name="addEvenIfNothingToOverwrite">Add the attribute even if none with the same name exist in the collection.</param>
        /// <param name="forceOverride">Force the override even if the type don't match.</param>
        public virtual void OverrideAttributes(ListSlice<AttributeBase> newAttributes, bool addEvenIfNothingToOverwrite = false, bool forceOverride = false)
        {
            if (newAttributes.Count == 0) { return; }

            for (int i = 0; i < newAttributes.Count; i++) {
                OverrideAttribute(newAttributes[i], addEvenIfNothingToOverwrite, forceOverride);
            }
        }

        #endregion

        #region Create & update attributes

        /// <summary>
        /// Adds and removes attributes to/from the collection so that names and type match with the list provided.
        /// </summary>
        /// <param name="requiredAttributes">The attributes that are required to exist in the collection.</param>
        /// <param name="removeAdditionalAttributes">If false the existing attribute that are not in the list are kept.</param>
        /// <returns>Returns false if something fails during the update.</returns>
        public virtual bool UpdateAttributesToMatchList(ListSlice<AttributeBase> requiredAttributes, bool removeAdditionalAttributes)
        {
            if (removeAdditionalAttributes) {
                RemoveAdditionalAttributes(requiredAttributes);
            }
            return CreateNewAttributesIfMissing(requiredAttributes);
        }

        /// <summary>
        /// Creates any attribute in the list that is not yet part of the collection.
        /// </summary>
        /// <param name="requiredAttributes">The attributes that are required to exist in the collection.</param>
        /// <returns>Returns false if something fails while adding the new attributes.</returns>
        public virtual bool CreateNewAttributesIfMissing(ListSlice<AttributeBase> requiredAttributes)
        {
            for (int i = 0; i < requiredAttributes.Count; i++) {
                var requiredAttribute = requiredAttributes[i];
                var attributeType = requiredAttribute.GetType();
                if (TryGetAttribute(requiredAttribute.Name, out var variant)) {
                    if (variant.GetType() != attributeType) {
                        Debug.LogError("Unable to create new Attribute. There are two different Attributes with the same name.");
                        return false;
                    }
                } else {
                    bool foundConnectionMatch = false;
                    for (int j = 0; j < m_Attributes.Count; j++) {
                        var existingAttribute = m_Attributes[j];
                        if (requiredAttribute.ConnectionID == existingAttribute.ConnectionID
                            && existingAttribute.GetType() == attributeType) {
                            existingAttribute.Rename(requiredAttribute.Name);
                            foundConnectionMatch = true;
                        }
                    }

                    if (foundConnectionMatch) { continue; }

                    var attribute = requiredAttribute.NewCopy(requiredAttribute.Name, VariantType.Inherit);
                    /*var attribute = AttributeBase.CreateInstance(attributeType, requiredAttribute.Name,
                        Type.Missing, VariantType.Inherit);*/

                    //Items that are note Default Items should not preevaluate attributes by default. Everything else should
                    if (AttachedItem == null || AttachedItem == AttachedItem.ItemDefinition.DefaultItem) {
                        attribute?.ReevaluateValue(true);
                    }

                    AddAttribute(attribute);
                }
            }

            ReevaluateAll(false);

            return true;
        }

        #endregion

        #region Remove

        /// <summary>
        /// Remove any attribute that is not part of the required list but exists in the collection.
        /// </summary>
        /// <param name="requiredAttributes">The attributes that are required to exist in the collection.</param>
        /// <returns>Returns false if an attribute could not be removed correctly.</returns>
        public virtual bool RemoveAdditionalAttributes(ListSlice<AttributeBase> requiredAttributes)
        {
            if (m_Attributes == null) {
                Debug.LogWarning($"The AttributeCollection '{this}' was not initialized yet you are trying to use it.");
            }
            for (int i = 0; i < m_Attributes.Count; i++) {
                var attribute = m_Attributes[i];
                var match = false;
                for (int j = 0; j < requiredAttributes.Count; j++) {
                    var requiredAttribute = requiredAttributes[j];

                    if ((attribute.Name == requiredAttribute.Name) && attribute.GetType() == requiredAttribute.GetType()) {
                        if (!match) {
                            match = true;
                        } else {
                            Debug.LogWarning("The attributes matched twice, some attribute had been duplicated.");
                        }
                    } else if (attribute.ConnectionID == requiredAttribute.ConnectionID && attribute.GetType() == requiredAttribute.GetType()) {
                        attribute.Rename(requiredAttribute.Name);
                        if (match == true) {
                            Debug.LogWarning("The attributes matched twice, some attribute had been duplicated.");
                        }
                        match = true;
                    }
                }

                if (match == false) {
                    RemoveAttribute(attribute);
                    i--;
                }
            }

            return true;
        }

        /// <summary>
        /// Remove an attribute.
        /// </summary>
        /// <param name="attribute">The attribute that should be removed from the collection.</param>
        /// <returns>True if the attribute was removed.</returns>
        public virtual bool RemoveAttribute(AttributeBase attribute)
        {
            if (attribute == null || !ContainsAttribute(attribute.Name)) { return false; }

            m_Attributes.Remove(attribute);
            m_Dirty = true;
            return true;
        }

        /// <summary>
        /// Remove an attribute with the specified name.
        /// </summary>
        /// <param name="attributeName">The name of the attribute that should be removed from the collection.</param>
        /// <returns>True if the attribute was removed.</returns>
        public virtual bool RemoveAttribute(string attributeName)
        {
            if (TryGetAttribute(attributeName, out var attribute)) {
                RemoveAttribute(attribute);
                return true;
            }
            return false;
        }
        #endregion

        #region Has Get attribute

        /// <summary>
        /// Returns true if an attribute with this name exists in the collection.
        /// </summary>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>Returns true if an attribute with the provided name exists in the collection.</returns>
        public virtual bool ContainsAttribute(string attributeName)
        {
            if (m_Attributes == null) {
                Debug.Log("attributes is null");
                return false;
            }

            for (int i = 0; i < m_Attributes.Count; i++) {
                if (m_Attributes[i].Name == attributeName) { return true; }
            }

            return false;
        }

        /// <summary>
        /// Try get the attribute.
        /// Use this if you do not know the type of the attribute.
        /// </summary>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <param name="attribute">Output of the attribute.</param>
        /// <returns>Returns true if the attribute exists in the collection.</returns>
        public virtual bool TryGetAttribute(string attributeName, out AttributeBase attribute)
        {
            if (m_Attributes == null) {
                attribute = null;
                return false;
            }

            for (int i = 0; i < m_Attributes.Count; i++) {
                if (m_Attributes[i].Name != attributeName) { continue; }

                attribute = m_Attributes[i];
                return true;
            }

            attribute = null;
            return false;
        }

        /// <summary>
        /// Returns the attribute.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <returns>Returns the attribute.</returns>
        public virtual T GetAttribute<T>(string attributeName) where T : AttributeBase
        {
            TryGetAttribute(attributeName, out var attribute);
            return attribute as T;
        }

        /// <summary>
        /// Returns the value of the attribute.
        /// Use this if you only need the value of the attribute.
        /// </summary>
        /// <typeparam name="T">The type of that attribute value</typeparam>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <param name="value">Output of the attribute value.</param>
        /// <returns>Returns true if the attribute exists in the collection.</returns>
        public virtual bool TryGetAttributeValue<T>(string attributeName, out T value)
        {
            if (TryGetAttribute(attributeName, out var attributeBase)) {
                var attribute = attributeBase as Attribute<T>;
                if (attribute != null) {
                    value = attribute.GetValue();
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        #endregion

        #region Rename and ReType (should be used by Attribute only)

        /// <summary>
        /// Changes the type of an attribute in the collection by replacing it by a new attribute with the new type.
        /// </summary>
        /// <param name="attribute">The attribute that needs to change type.</param>
        /// <param name="newType">The new type for the attribute.</param>
        /// <returns>Returns a success or fail message.</returns>
        public virtual bool ChangeAttributeType(AttributeBase attribute, Type newType)
        {
            if (attribute.AttributeCollection == null) {
                Debug.LogWarning("Unable to change type of attribute; Cannot retype a single attribute, create a new one instead.");
                return false;
            }

            var pooledAttributeSources = GenericObjectPool.Get<AttributeBase[]>();
            var allAttributeSources = attribute.GetAttributeFamilySources(ref pooledAttributeSources);

            var attributeSource = GetSourceOfAttribute(attribute);

            Type attributeType;
            if (typeof(AttributeBase).IsAssignableFrom(newType)) {
                attributeType = newType;
            } else {
                attributeType = typeof(Attribute<>).MakeGenericType(newType);
            }

            var newAttribute = Activator.CreateInstance(attributeType) as AttributeBase;
            newAttribute.Name = attribute.Name;
            newAttribute.SetModifyExpression(attributeSource.ModifyExpression, false);
            newAttribute.SetVariantType(attributeSource.VariantType);

            // Retrieve the old values of attributes in case they can be converted to the new attribute value.
            var oldValueType = attributeSource.GetValueType();
            var newValueType = newAttribute.GetValueType();
            Dictionary<Object, AttributeBase> previousAttributes = null;

            var intToFloatConversion = oldValueType == typeof(int) && newValueType == typeof(float);
            var floatToIntConversion = oldValueType == typeof(float) && newValueType == typeof(int);
            if (newValueType == oldValueType ||
                newValueType.IsSubclassOf(oldValueType) ||
                floatToIntConversion ||
                intToFloatConversion) {

                previousAttributes = new Dictionary<Object, AttributeBase>();

                var attributeFamily = attributeSource.GetAttributeFamily();
                for (int i = 0; i < attributeFamily.Count; i++) {
                    var attachedObject = attributeFamily[i].GetAttachedObject();
                    if (attachedObject != null && previousAttributes.ContainsKey(attachedObject)) { continue; }

                    previousAttributes.Add(attachedObject, attributeFamily[i]);
                }
            }

            // Replace all the attributes with the new ones.
            attribute.AttachedItemCategory.RemoveAttributeFromAll(newAttribute.Name, Relation.Family, true);
            for (int i = 0; i < allAttributeSources.Count; i++) {
                var sourceCategory = allAttributeSources[i].AttachedItemCategory;
                AddAttributeToCategory(newAttribute, sourceCategory);
            }

            // Assign the old values if they are compatible.
            if (previousAttributes != null) {

                var attributeFamily = newAttribute.GetAttributeFamily();
                for (int i = 0; i < attributeFamily.Count; i++) {
                    var otherAttribute = attributeFamily[i];
                    var otherObject = otherAttribute.GetAttachedObject();

                    if (previousAttributes.ContainsKey(otherObject) == false) { continue; }

                    var previousAttribute = previousAttributes[otherObject];
                    var value = previousAttribute.GetOverrideValueAsObject();

                    if (intToFloatConversion) {
                        value = (float)(int)value;
                    } else if (floatToIntConversion) {
                        value = (int)(float)value;
                    }

                    otherAttribute.SetOverrideValueAsObjectWithoutNotify(value);
                    otherAttribute.SetModifyExpression(previousAttribute.ModifyExpression, false, false);
                    otherAttribute.SetVariantType(previousAttribute.VariantType, false);
                }
            }

            GenericObjectPool.Return(pooledAttributeSources);
            m_Dirty = true;
            return true;
        }

        #endregion

        /// <summary>
        /// ReEvaluate all the attributes in the collection
        /// </summary>
        public void ReevaluateAll(bool setPreevaluate)
        {
            for (int i = 0; i < m_Attributes.Count; i++) {
                if (setPreevaluate || m_Attributes[i].IsPreEvaluated) {
                    m_Attributes[i].ReevaluateValue(setPreevaluate);
                }
            }
        }

        #region abstract

        /// <summary>
        /// Returns the Object that is connected to this AttributeCollection used for setting ScriptableObjects dirty.
        /// </summary>
        /// <returns>The Object.</returns>
        public abstract UnityEngine.Object GetAttachedObject();

        /// <summary>
        /// Returns the root attribute by looking into the attached Object.
        /// </summary>
        /// <param name="attribute">The attribute provided.</param>
        /// <returns>The source attribute.</returns>
        public abstract AttributeBase GetSourceOfAttribute(AttributeBase attribute);

        /// <summary>
        /// Returns the "children" of the attribute by looking into the children of the attached Object.
        /// </summary>
        /// <param name="attribute">The 'parent' attribute.</param>
        /// <param name="includeThisAttribute">If true the provided attribute will be part of the children list.</param>
        /// <returns>Returns a list of the children.</returns>
        public abstract List<AttributeBase> GetChildrenOfAttribute(AttributeBase attribute, bool includeThisAttribute);

        /// <summary>
        /// Returns that 'parent' attribute of the provided attribute by looking in the parent of the attached Object.
        /// </summary>
        /// <param name="attribute">The attribute provided to find its inheritor.</param>
        /// <returns>The inherit attribute.</returns>
        public abstract AttributeBase GetInheritOfAttribute(AttributeBase attribute);

        /// <summary>
        /// Removes the old attribute and adds the new attribute to the correct AttributeCollections when retyped.
        /// </summary>
        /// <param name="newTypeAttribute">The new attribute with the new type.</param>
        /// <param name="sourceCategory">The source category.</param>
        /// <param name="previousAttributeValues">A dictionary with the previous values in the attributes.</param>
        protected abstract void AddAttributeToCategory(AttributeBase newTypeAttribute, ItemCategory sourceCategory);

        #endregion

        /// <summary>
        /// IEnumarator that iterates over that attributes in the ArrayStruct.
        /// </summary>
        /// <returns>IEnumarator.</returns>
        public IEnumerator<AttributeBase> GetEnumerator()
        {
            if (m_Attributes == null) { yield break; }

            for (int i = 0; i < m_Attributes.Count; i++) { yield return m_Attributes[i]; }
        }

        /// <summary>
        /// Custom IEnumarator, iterates over that attributes in the ArrayStruct.
        /// </summary>
        /// <returns>IEnumarator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Called by the attribute when it is modified.
        /// </summary>
        /// <param name="attribute">The attribute that was modified.</param>
        public void AttributeChanged(AttributeBase attribute)
        {
            m_Dirty = true;
            OnAttributeChanged?.Invoke(attribute);
        }
    }
}
