/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Utility
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Utility;
    using System;
    using System.Collections.Generic;
    using Opsive.Shared.Editor.UIElements;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Static class that has useful function for attributes.
    /// </summary>
    public static class AttributeEditorUtility
    {
        /// <summary>
        /// Set the value for the protected attributeList field
        /// </summary>
        /// <param name="attributeCollection">The attribute collection.</param>
        /// <param name="newList">The new list of attributes.</param>
        public static void SetAttributeResizableArray(AttributeCollection attributeCollection, ResizableArray<AttributeBase> newList)
        {
            ReflectionUtility.SetNonPublicField(attributeCollection, "m_Attributes", newList);
        }

        /// <summary>
        /// Rename the attribute.
        /// </summary>
        /// <param name="attribute">The attribute to rename.</param>
        /// <param name="newName">The new name.</param>
        public static void RenameAttribute(AttributeBase attribute, string newName)
        {
            RegisterUndoAttributeFamilyConnectionsInternal(attribute, "Rename Attribute");

            //The attribute has multiple sources double check that the user still wants to rename the attribute or if it should create a new one.
            if (attribute.GetFamilySourceCount() > 1) {

                var renameMultiSourcedAttribute = EditorUtility.DisplayDialog("The attribute has multiple sources",
                    $"The attribute '{attribute.Name}' has multiple category sources.\n\n " +
                    "Do you wish to rename anyways?",
                    "Yes",
                    "No");

                if (renameMultiSourcedAttribute == false) {
                    return;
                }
            }

            //Try to rename normally.
            var result = attribute.Rename(newName);

            if (result) {
                ItemCategoryEditorUtility.SerializeCategoryFamilyConnections(attribute.AttachedItemCategory);
                return;
            }

            //Could not rename, try to see if it can be renamed somehow.
            var attributeSource = attribute.GetSourceAttribute();
            var family = attribute.GetAttributeFamily();

            result = true;
            foreach (var otherAttribute in family) {
                var category = otherAttribute.AttachedItemCategory;
                if (category == null) { continue; }

                var existingAttribute = category.GetAttribute(newName);
                if (existingAttribute == null) { continue; }

                if (existingAttribute.AttributeCollectionType != attributeSource.AttributeCollectionType) {
                    result = false;
                    Debug.LogWarning(
                        $"An attribute on the category {category} already has the name '{newName}' in the {existingAttribute.AttributeCollectionType}");
                    break;
                }

                if (existingAttribute.GetValueType() != attributeSource.GetValueType()) {
                    result = false;
                    Debug.LogWarning(
                        $"An attribute on the category {category} already has the name '{newName}' and it is not the same type as your attribute: {existingAttribute.GetValueType()}");
                    break;
                }
            }

            if (result == false) { return; }

            if (EditorUtility.DisplayDialog("Could not rename Attribute",
                $"The attribute '{attribute.Name}' could not be renamed '{newName}'. " +
                "This might be because an attribute in the children/parent categories might already have that attribute.\n " +
                "It appears that the existing attributes can be merged with the new ones since they are in the same collection and have the same type.\n\n " +
                "Would you like to rename and merge existing attributes with the same name? ",
                "Yes",
                "No")) {
                var existingPriority = EditorUtility.DisplayDialog("Rename & merge",
                    "The attribute will now try to merge. Which attribute should take priority?\n" +
                    $"The existing attributes with the new name '{newName}' or the attributes '{attributeSource.Name}' which are about to be renamed?",
                    "Existing", "New");

                result = attributeSource.MergeRename(newName, existingPriority);

                if (result == false) {
                    Debug.LogError(
                        "The attribute was still unable to get renamed, and the existing attribute were removed.");
                }
            }

            SerializeAttributeFamilyAndDirty(attribute);
        }

        /// <summary>
        /// Set the override value of the attribute.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="newOverrideValue">The new override value.</param>
        public static void SetOverrideValueAsObject(AttributeBase attribute, object newOverrideValue)
        {
            Undo.RegisterCompleteObjectUndo(attribute.AttachedItemCategory, "Set Override Value");

            attribute.SetOverrideValueAsObject(newOverrideValue, true, true);

            SerializeAttributes(attribute.AttributeCollection);
            ItemCategoryEditorUtility.SetItemCategoryDirty(attribute.AttachedItemCategory, false);
        }

        /// <summary>
        /// Set the value for the protected modify Expression field
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="newModifyExpression">The new modify expression.</param>
        public static void SetModifyExpression(AttributeBase attribute, string newModifyExpression)
        {
            Undo.RegisterCompleteObjectUndo(attribute.AttachedItemCategory, "Set Expression");

            attribute.SetModifyExpression(newModifyExpression, true, true);

            SerializeAttributes(attribute.AttributeCollection);
            ItemCategoryEditorUtility.SetItemCategoryDirty(attribute.AttachedItemCategory, false);
        }

        /// <summary>
        /// Set the value for the protected variantType field
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="newVariantType">The new variant type.</param>
        public static void SetVariantType(AttributeBase attribute, VariantType newVariantType)
        {
            Undo.RegisterCompleteObjectUndo(attribute.AttachedItemCategory, "Set Variant Type");

            attribute.SetVariantType(newVariantType, true);

            SerializeAttributes(attribute.AttributeCollection);
            ItemCategoryEditorUtility.SetItemCategoryDirty(attribute.AttachedItemCategory, false);
        }

        /// <summary>
        /// Set the value for the protected IsPreEvaluated field
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="newIsPreEvaluated">The new is pre-evaluated value.</param>
        public static void SetIsPreEvaluated(AttributeBase attribute, bool newIsPreEvaluated)
        {
            Undo.RegisterCompleteObjectUndo(attribute.AttachedItemCategory, "Set Attribute PreEvaluate");

            ReflectionUtility.SetNonPublicField(attribute, "m_IsPreEvaluated", newIsPreEvaluated);
            attribute.ReevaluateValue(false);

            SerializeAttributes(attribute.AttributeCollection);
            ItemCategoryEditorUtility.SetItemCategoryDirty(attribute.AttachedItemCategory, false);
        }

        /// <summary>
        /// Set the value for the protected IsPreEvaluated field
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="newAttributeType">The new type for the attribute.</param>
        public static AttributeBase ChangeType(AttributeBase attribute, Type newAttributeType)
        {
            RegisterUndoAttributeFamilyConnectionsInternal(attribute, "Change Attribute Type");

            //The attribute has multiple sources double check that the user still wants to rename the attribute or if it should create a new one.
            if (attribute.GetFamilySourceCount() > 1) {

                var renameMultiSourcedAttribute = EditorUtility.DisplayDialog("The attribute has multiple sources",
                    $"The attribute '{attribute.Name}' has multiple category sources.\n\n " +
                    "Do you wish to change the type anyways?",
                    "Yes",
                    "No");

                if (renameMultiSourcedAttribute == false) {
                    return attribute;
                }
            }

            var newAttribute = attribute.ChangeType(newAttributeType);
            if (newAttribute == null) {
                //The Attribute type could not be changed.
                return null;
            }

            newAttribute.ReevaluateValue(false);

            SerializeAttributeFamilyAndDirty(newAttribute);
            return newAttribute;
        }

        /// <summary>
        /// Serialize all of the attributes to the collection.
        /// </summary>
        /// <param name="attributeCollection">The character to serialize.</param>
        public static void SerializeAttributes(AttributeCollection attributeCollection)
        {
            if (attributeCollection == null) {
                Debug.LogError("The database is corrupt.");
                return;
            }

            var attributes = new List<AttributeBase>();
            for (int i = 0; i < attributeCollection.Count; i++) {
                var attribute = attributeCollection[i];
                attribute.Serialize();
                attributes.Add(attribute);
            }
            attributeCollection.Serialize();

            var arrayStruct = new ResizableArray<AttributeBase>();
            arrayStruct.Initialize(attributes.ToArray(), false);
            SetAttributeResizableArray(attributeCollection, arrayStruct);
        }

        /// <summary>
        /// Serialize all of the abilities to the AbilityData array.
        /// </summary>
        /// <param name="newAttribute">The character to serialize.</param>
        public static void SerializeAttributeFamily(AttributeBase newAttribute)
        {
            var family = newAttribute.GetAttributeFamily();
            foreach (var attribute in family) {
                AttributeCollection collection = ReflectionUtility.GetNonPublicField(attribute, "m_Collection") as AttributeCollection;
                SerializeAttributes(collection);
            }
        }

        /// <summary>
        /// Serialize And set Dirty
        /// </summary>
        /// <param name="target">The target object to serialize.</param>
        /// <param name="attributeCollection">The attribute collection.</param>
        public static void SerializeAttributesAndDirty(UnityEngine.Object target, AttributeCollection attributeCollection)
        {
            SerializeAttributes(attributeCollection);
            if (target != null) { Shared.Editor.Utility.EditorUtility.SetDirty(target); }
        }

        /// <summary>
        /// Serialize And set Dirty
        /// </summary>
        /// <param name="target">The target object to serialize.</param>
        /// <param name="item">The item.</param>
        public static void SerializeAttributesAndDirty(UnityEngine.Object target, Item item)
        {
            SerializeAttributesAndDirty(target, item.ItemAttributeCollection);
        }

        /// <summary>
        /// Serialize all the attributes on a Category
        /// </summary>
        /// <param name="category">The item category to serialize.</param>
        public static void SerializeAttributes(ItemCategory category)
        {
            SerializeAttributesAndDirty(category, category.ItemCategoryAttributeCollection);
            SerializeAttributesAndDirty(category, category.ItemDefinitionAttributeCollection);
            SerializeAttributesAndDirty(category, category.ItemAttributeCollection);
        }

        /// <summary>
        /// Serialize all the attributes on an ItemDefinition
        /// </summary>
        /// <param name="definition">The item definition to serialize.</param>
        public static void SerializeAttributes(ItemDefinition definition)
        {
            if (definition == null) {
                Debug.LogWarning("Something is wrong with the database.");
                return;
            }
            SerializeAttributesAndDirty(definition, definition.ItemDefinitionAttributeCollection);
            SerializeAttributesAndDirty(definition, definition.DefaultItem.ItemAttributeCollection);
        }

        /// <summary>
        /// Serialize all the attributes on an Item
        /// </summary>
        /// <param name="item">The item to serialize.</param>
        public static void SerializeAttributes(Item item)
        {
            if (item == null) {
                return;
            }
            SerializeAttributesAndDirty(null, item.ItemAttributeCollection);
        }

        /// <summary>
        /// Serialize all the children connection of the category
        /// </summary>
        /// <param name="attribute">The attribute parent.</param>
        public static void PreEvaluateAttributeChildren(AttributeBase attribute)
        {
            var children = attribute.GetChildrenAttribute(true);
            for (int i = 0; i < children.Count; i++) { children[i].ReevaluateValue(false); }
            SerializeAttributeFamily(attribute);
        }

        /// <summary>
        /// Serialize the entire family of attribute objects.
        /// </summary>
        /// <param name="category">The category to serialize.</param>
        public static void SerializeAttributeFamilyAndDirty(AttributeBase attribute)
        {
            var pooledObjects = GenericObjectPool.Get<Object[]>();
            var objectsToSerialize = attribute.GetAttributeObjectFamily(ref pooledObjects);
            for (int i = 0; i < objectsToSerialize.Count; i++) {
                var objectToSerialize = objectsToSerialize[i];
                if (objectToSerialize == null) { continue; }
                if (objectToSerialize is ItemCategory category) {
                    ItemCategoryEditorUtility.SetItemCategoryDirty(category, false);
                } else if (objectToSerialize is ItemDefinition definition) {
                    ItemDefinitionEditorUtility.SetItemDefinitionDirty(definition, false);
                }
            }
            GenericObjectPool.Return(pooledObjects);
        }

        /// <summary>
        /// Register all connections of attribute objects for undo.
        /// </summary>
        /// <param name="attribute">The Attribute.</param>
        /// <param name="undoKey">The undo key.</param>
        public static void RegisterUndoAttributeFamilyConnectionsInternal(AttributeBase attribute, string undoKey)
        {
            var pooledObjects = GenericObjectPool.Get<Object[]>();
            var objectsToSerialize = attribute.GetAttributeObjectFamily(ref pooledObjects);
            for (int i = 0; i < objectsToSerialize.Count; i++) {
                var objectToSerialize = objectsToSerialize[i];
                if (objectToSerialize == null) { continue; }
                Undo.RegisterCompleteObjectUndo(objectToSerialize, undoKey);
            }
            GenericObjectPool.Return(pooledObjects);
        }


        // Return a deep clone of an object of type T.
        public static void DeepCopyAttribute(AttributeBase originalAttribute, AttributeBase newAttribute)
        {
            var serialization = Serialization.Serialize(originalAttribute);
            var originalAttributeDeepCopy = serialization.DeserializeFields(MemberVisibility.Public) as AttributeBase;

            //var deepCloneValue = DeepClone(originalAttribute.GetOverrideValueAsObject());
            var deepCloneValue = originalAttributeDeepCopy.GetOverrideValueAsObject();

            newAttribute.SetOverrideValueAsObjectWithoutNotify(deepCloneValue);
            newAttribute.SetModifyExpression(originalAttribute.ModifyExpression, false, false);
            newAttribute.SetVariantType(originalAttribute.VariantType);

        }
    }

}
