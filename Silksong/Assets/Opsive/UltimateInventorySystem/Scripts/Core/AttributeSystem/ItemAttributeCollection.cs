/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.AttributeSystem
{
    using Opsive.Shared.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An item attribute collection is used to group a set of item attributes.
    /// </summary>
    [System.Serializable]
    public class ItemAttributeCollection : AttributeCollection
    {
        public override int Index => 2;

        /// <summary>
        /// Initialize the attribute collection by attaching to an item.
        /// </summary>
        /// <param name="item">The Item to attach to this attributeCollection.</param>
        /// <param name="force">Force the initialization.</param>
        public virtual void Initialize(Item item, bool force)
        {
            m_Item = item;
            m_ItemDefinition = m_Item.ItemDefinition;
            m_ItemCategory = m_ItemDefinition.Category;

            base.Initialize(force);
        }

        /// <summary>
        /// Returns the Object that is connected to this AttributeCollection used for setting Scriptable Objects dirty.
        /// Is null if attached item is not the DefaultItem of the Item Definition.
        /// </summary>
        /// <returns>The Object.</returns>
        public override UnityEngine.Object GetAttachedObject()
        {
            if (m_Item == m_ItemDefinition.DefaultItem) {
                return AttachedItemDefinition;
            }

            return null;
        }

        /// <summary>
        /// Returns the "children" of the attribute by looking into the children of the attached Item.
        /// </summary>
        /// <param name="attribute">The 'parent' attribute.</param>
        /// <param name="includeThisAttribute">If true the provided attribute will be part of the children list.</param>
        /// <returns>Returns a list of the children.</returns>
        public override List<AttributeBase> GetChildrenOfAttribute(AttributeBase attribute, bool includeThisAttribute)
        {
            var children = new List<AttributeBase>();

            if (includeThisAttribute) { children.Add(attribute); }

            if (m_Item == m_ItemDefinition.DefaultItem) {
                if (m_ItemCategory.Manager == null) {

                    var pooledItemDefAllChildren = GenericObjectPool.Get<ItemDefinition[]>();
                    var itemDefAllChildrenCount = m_ItemDefinition.GetAllChildren(ref pooledItemDefAllChildren, false);
                    for (int i = 0; i < itemDefAllChildrenCount; i++) {
                        var definitionChild = pooledItemDefAllChildren[i];
                        if (definitionChild == null || definitionChild.DefaultItem == null) { continue; }
                        if (definitionChild.DefaultItem.TryGetAttribute(attribute.Name, out var defaultItemAttribute) == false) { continue; }
                        children.Add(defaultItemAttribute);
                    }
                    GenericObjectPool.Return(pooledItemDefAllChildren);

                } else {
                    var itemsWithDefinition = m_ItemCategory.Manager.Register.ItemRegister.ItemsWithItemDefinition(AttachedItemDefinition);
                    for (var i = 0; i < itemsWithDefinition.Count; i++) {
                        var item = itemsWithDefinition[i];
                        if (item.TryGetAttribute(attribute.Name, out var itemAttribute) == false) { continue; }

                        children.Add(itemAttribute);
                    }
                }
            }
            return children;
        }

        /// <summary>
        /// Returns that 'parent' attribute of the provided attribute by looking in the parent of the attached Item.
        /// </summary>
        /// <param name="attribute">The attribute provided to find its inheritor.</param>
        /// <returns>The inherit attribute.</returns>
        public override AttributeBase GetInheritOfAttribute(AttributeBase attribute)
        {
            //If  normal == or Equals is used an infinite loop is created because the equals function will compare attribute values, or maybe the item is not yet registered
            if (m_ItemDefinition != null && m_ItemDefinition.DefaultItem != null
                && (!object.ReferenceEquals(m_ItemDefinition.DefaultItem, m_Item)
                || m_ItemDefinition.DefaultItem.ID != m_Item.ID)) {

                if (m_ItemDefinition.DefaultItem.TryGetAttribute(attribute.Name, out var output)) {
                    if (output.AttachedItem.ID != m_ItemDefinition.DefaultItem.ID) {
                        Debug.LogWarning("Warning the output attachedItem.ID is not set correctly.");
                        return null;
                    }

                    return output;
                }
            }

            if (m_ItemDefinition.Parent != null) {
                if (m_ItemDefinition.Parent.DefaultItem.TryGetAttribute(attribute.Name, out var output)) {
                    if (output.AttachedItem.ID != m_ItemDefinition.Parent.DefaultItem.ID) {
                        Debug.LogWarning("Warning the output attachedItem.ID is not set correctly.");
                        return null;
                    }

                    return output;
                }
            }

            if (m_ItemCategory.TryGetItemAttribute(attribute.Name, out var requiredAttribute)) {
                return requiredAttribute;
            }

            return null;
        }

        /// <summary>
        /// Returns the root attribute by looking into the attached Item.
        /// </summary>
        /// <param name="attribute">The attribute provided.</param>
        /// <returns>The source attribute.</returns>
        public override AttributeBase GetSourceOfAttribute(AttributeBase attribute)
        {
            var source = attribute.GetSourceCategory();
            if (source == null) { return null; }
            source.TryGetItemAttribute(attribute.Name, out var sourceAttribute);
            return sourceAttribute;
        }

        /// <summary>
        /// Removes the old attribute and adds the new attribute to the correct AttributeCollections when retyped.
        /// </summary>
        /// <param name="newTypeAttribute">The new attribute with the new type.</param>
        /// <param name="sourceCategory">The source category.</param>
        protected override void AddAttributeToCategory(AttributeBase newTypeAttribute, ItemCategory sourceCategory)
        {
            sourceCategory.AddItemAttribute(newTypeAttribute);
        }
    }
}
