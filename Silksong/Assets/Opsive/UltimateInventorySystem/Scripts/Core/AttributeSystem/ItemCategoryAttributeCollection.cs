/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.AttributeSystem
{
    using Opsive.Shared.Utility;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An category attribute collection is used to group a set of item attributes.
    /// </summary>
    [System.Serializable]
    public class ItemCategoryAttributeCollection : AttributeCollection
    {
        public override int Index => 0;

        /// <summary>
        /// Initialize attribute collection by attaching an itemCategory.
        /// </summary>
        /// <param name="itemCategory">The itemCategory to attach.</param>
        /// <param name="force">Force the initialization.</param>
        public virtual void Initialize(ItemCategory itemCategory, bool force)
        {
            m_ItemCategory = itemCategory;

            base.Initialize(force);
        }

        /// <summary>
        /// Returns the Object that is connected to this AttributeCollection used for setting ScriptableObjects dirty.
        /// Return the AttachedItemCategory.
        /// </summary>
        /// <returns>The Object.</returns>
        public override UnityEngine.Object GetAttachedObject()
        {
            return m_ItemCategory;
        }

        /// <summary>
        /// Returns the "children" of the attribute by looking into the children of the attached ItemCategory.
        /// </summary>
        /// <param name="attribute">The 'parent' attribute.</param>
        /// <param name="includeThisAttribute">If true the provided attribute will be part of the children list.</param>
        /// <returns>Returns a list of the children.</returns>
        public override List<AttributeBase> GetChildrenOfAttribute(AttributeBase attribute, bool includeThisAttribute)
        {
            var children = new List<AttributeBase>();

            if (includeThisAttribute) { children.Add(attribute); }

            var pooledItemDefAllChildren = GenericObjectPool.Get<ItemCategory[]>();
            var itemDefAllChildrenCount = m_ItemCategory.GetAllChildren(ref pooledItemDefAllChildren, false);
            for (int i = 0; i < itemDefAllChildrenCount; i++) {
                var categoryChild = pooledItemDefAllChildren[i];
                if (categoryChild.TryGetCategoryAttribute(attribute.Name, out var categoryAttribute) == false) { continue; }

                children.Add(categoryAttribute);
            }
            GenericObjectPool.Return(pooledItemDefAllChildren);

            return children;
        }

        /// <summary>
        /// Returns that 'parent' attribute of the provided attribute by looking in the parent of the attached ItemCategory.
        /// </summary>
        /// <param name="attribute">The attribute provided to find its inheritor.</param>
        /// <returns>The inherit attribute.</returns>
        public override AttributeBase GetInheritOfAttribute(AttributeBase attribute)
        {
            var parentsCount = m_ItemCategory.ParentsReadOnly.Count;
            for (int i = 0; i < parentsCount; i++) {
                if (m_ItemCategory?.ParentsReadOnly[i] == null) {
                    Debug.LogWarning($"Category {m_ItemCategory} and or parent is null, this should never happen.");
                    continue;
                }
                if (m_ItemCategory.ParentsReadOnly[i].TryGetCategoryAttribute(attribute.Name, out var output)) {
                    return output;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the root attribute by looking into the attached ItemCategory.
        /// </summary>
        /// <param name="attribute">The attribute provided.</param>
        /// <returns>The source attribute.</returns>
        public override AttributeBase GetSourceOfAttribute(AttributeBase attribute)
        {
            var source = attribute.GetSourceCategory();
            if (source == null) { return null; }
            source.TryGetCategoryAttribute(attribute.Name, out var sourceAttribute);
            return sourceAttribute;
        }

        /// <summary>
        /// Removes the old attribute and adds the new attribute to the correct AttributeCollections when retyped.
        /// </summary>
        /// <param name="newTypeAttribute">The new attribute with the new type.</param>
        /// <param name="sourceCategory">The source category.</param>
        protected override void AddAttributeToCategory(AttributeBase newTypeAttribute, ItemCategory sourceCategory)
        {
            sourceCategory.AddOrOverrideCategoryAttribute(newTypeAttribute);
        }
    }
}
