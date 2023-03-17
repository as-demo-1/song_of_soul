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
    /// A required definition attribute collection is used to group a set of category attributes meant to be inherited by itemDefintions.
    /// </summary>
    [System.Serializable]
    public class RequiredItemDefinitionAttributeCollection : AttributeCollection
    {
        public override int Index => 1;

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

            var pooledItemCatAllChildren = GenericObjectPool.Get<ItemCategory[]>();
            var itemCatAllChildrenCount = m_ItemCategory.GetAllChildren(ref pooledItemCatAllChildren, false);
            for (int i = 0; i < itemCatAllChildrenCount; i++) {
                var categoryChild = pooledItemCatAllChildren[i];
                if (categoryChild.TryGetDefinitionAttribute(attribute.Name, out var categoryAttribute) ==
                    false) { continue; }
                children.Add(categoryAttribute);
            }
            GenericObjectPool.Return(pooledItemCatAllChildren);

            var pooledItemCatAllChildrenElements = GenericObjectPool.Get<ItemDefinition[]>();
            var itemCatAllChildrenElementsCount = m_ItemCategory.GetAllChildrenElements(ref pooledItemCatAllChildrenElements);
            for (var i = 0; i < itemCatAllChildrenElementsCount; i++) {
                var definitionChild = pooledItemCatAllChildrenElements[i];
                if (definitionChild == null) {
                    Debug.LogWarning("Something is wrong with the database, an itemDefinition child is missing");
                    continue;
                }
                if (definitionChild.TryGetAttribute(attribute.Name, out var definitionAttribute) == false) { continue; }

                children.Add(definitionAttribute);
            }
            GenericObjectPool.Return(pooledItemCatAllChildrenElements);

            return children;
        }

        /// <summary>
        /// Returns that 'parent' attribute of the provided attribute by looking in the parent of the attached ItemCategory.
        /// </summary>
        /// <param name="attribute">The attribute provided to find its inheritor.</param>
        /// <returns>The inherit attribute.</returns>
        public override AttributeBase GetInheritOfAttribute(AttributeBase attribute)
        {
            var parentCount = m_ItemCategory.ParentsReadOnly.Count;
            for (int i = 0; i < parentCount; i++) {
                //First come First served order of parents matters
                var parent = m_ItemCategory.ParentsReadOnly[i];
                if (parent == null) { continue; }
                if (parent.TryGetDefinitionAttribute(attribute.Name, out var output)) {
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
            source.TryGetDefinitionAttribute(attribute.Name, out var sourceAttribute);
            return sourceAttribute;
        }

        /// <summary>
        /// Removes the old attribute and adds the new attribute to the correct AttributeCollections when retyped.
        /// </summary>
        /// <param name="newTypeAttribute">The new attribute with the new type.</param>
        /// <param name="sourceCategory">The source category.</param>
        /// <param name="previousAttributeValues">A dictionary with the previous values in the attributes.</param>
        protected override void AddAttributeToCategory(AttributeBase newTypeAttribute, ItemCategory sourceCategory)
        {
            sourceCategory.AddDefinitionAttribute(newTypeAttribute);
        }
    }
}
