/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.AttributeSystem
{
    using Opsive.Shared.Utility;
    using System.Collections.Generic;

    /// <summary>
    /// An definition attribute collection is used to group a set of item attributes.
    /// </summary>
    [System.Serializable]
    public class ItemDefinitionAttributeCollection : AttributeCollection
    {
        public override int Index => 1;

        /// <summary>
        /// Initialize the attribute collection by attaching to an Item Definition.
        /// </summary>
        /// <param name="itemDefinition">The Item Definition to attach.</param>
        /// <param name="force">Force the initialization.</param>
        public virtual void Initialize(ItemDefinition itemDefinition, bool force)
        {
            m_ItemDefinition = itemDefinition;
            m_ItemCategory = m_ItemDefinition.Category;

            base.Initialize(force);
        }

        /// <summary>
        /// Returns the Object that is connected to this Attribute Collection used for setting Scriptable Objects dirty.
        /// Returns the Attached Item Definition.
        /// </summary>
        /// <returns>The Object.</returns>
        public override UnityEngine.Object GetAttachedObject()
        {
            return m_ItemDefinition;
        }

        /// <summary>
        /// Returns the "children" of the attribute by looking into the children of the attached Item Definition.
        /// </summary>
        /// <param name="attribute">The 'parent' attribute.</param>
        /// <param name="includeThisAttribute">If true the provided attribute will be part of the children list.</param>
        /// <returns>Returns a list of the children.</returns>
        public override List<AttributeBase> GetChildrenOfAttribute(AttributeBase attribute, bool includeThisAttribute)
        {
            var children = new List<AttributeBase>();

            if (includeThisAttribute) { children.Add(attribute); }

            var pooledItemDefAllChildren = GenericObjectPool.Get<ItemDefinition[]>();
            var itemDefAllChildrenCount = AttachedItemDefinition.GetAllChildren(ref pooledItemDefAllChildren, false);
            for (int i = 0; i < itemDefAllChildrenCount; i++) {
                var definitionChild = pooledItemDefAllChildren[i];
                if (definitionChild.TryGetAttribute(attribute.Name, out var definitionAttribute) == false) { continue; }
                children.Add(definitionAttribute);
            }
            GenericObjectPool.Return(pooledItemDefAllChildren);

            return children;
        }

        /// <summary>
        /// Returns that 'parent' attribute of the provided attribute by looking in the parent of the attached Item Definition.
        /// </summary>
        /// <param name="attribute">The attribute provided to find its inheritor.</param>
        /// <returns>The inherit attribute.</returns>
        public override AttributeBase GetInheritOfAttribute(AttributeBase attribute)
        {
            ItemDefinition parentItemDefinition = null;
            if (m_ItemDefinition != null) {
                parentItemDefinition = m_ItemDefinition.Parent;
            }

            if (parentItemDefinition != null) {
                if (parentItemDefinition.TryGetAttribute(attribute.Name, out var output)) {
                    return output;
                }
            }

            if (AttachedItemCategory.TryGetDefinitionAttribute(attribute.Name, out var requiredAttribute)) {
                return requiredAttribute;
            }

            return null;
        }

        /// <summary>
        /// Returns the root attribute by looking into the attached ItemDefinition.
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
        protected override void AddAttributeToCategory(AttributeBase newTypeAttribute, ItemCategory sourceCategory)
        {
            sourceCategory.AddDefinitionAttribute(newTypeAttribute);
        }
    }
}
