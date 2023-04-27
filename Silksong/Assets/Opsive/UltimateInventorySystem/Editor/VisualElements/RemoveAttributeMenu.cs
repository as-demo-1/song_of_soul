/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using System;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class used to create a Generic menu that lets you choose how to remove attributes.
    /// </summary>
    public class RemoveAttributeMenu
    {
        public event Action OnRemove;
        protected AttributeBase m_Attribute;

        /// <summary>
        /// Show the Context Menu.
        /// </summary>
        public void ShowContext()
        {
            var genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove from Parents"), false, RemoveAttributeFromParents);
            genericMenu.AddItem(new GUIContent("Remove from Relevant Family"), false, RemoveAllAttributesFromRelevantFamily);
            genericMenu.AddItem(new GUIContent("Remove from All Family"), false, RemoveAttributesFromAllFamily);
            genericMenu.ShowAsContext();
        }

        /// <summary>
        /// Set the attribute that should be removed.
        /// </summary>
        /// <param name="attribute">The attribute to set.</param>
        public void SetAttribute(AttributeBase attribute)
        {
            m_Attribute = attribute;
        }

        /// <summary>
        /// Remove the attribute from the entire family of category.
        /// </summary>
        private void RemoveAttributesFromAllFamily()
        {
            var category = m_Attribute.AttachedItemCategory;
            ItemCategoryEditorUtility.RegisterUndoItemCategoryFamilyConnections(category, "Remove Attribute");

            category.RemoveAttributeFromAll(m_Attribute.Name, Relation.Family, true);

            ItemCategoryEditorUtility.SerializeCategoryFamilyConnections(category);

            AttributeRemoved();
        }

        /// <summary>
        /// Remove the attribute for that attributes which are relevant to the selected attribute.
        /// </summary>
        private void RemoveAllAttributesFromRelevantFamily()
        {
            var category = m_Attribute.AttachedItemCategory;
            ItemCategoryEditorUtility.RegisterUndoItemCategoryFamilyConnections(category, "Remove Attribute");

            category.RemoveAttributeFromAll(m_Attribute.Name, Relation.Family, true);

            ItemCategoryEditorUtility.SerializeCategoryFamilyConnections(category);

            AttributeRemoved();
        }

        /// <summary>
        /// Remove only the attributes up the chain.
        /// </summary>
        private void RemoveAttributeFromParents()
        {
            var category = m_Attribute.AttachedItemCategory;
            ItemCategoryEditorUtility.RegisterUndoItemCategoryFamilyConnections(category, "Remove Attribute");

            category.RemoveAttributeFromAll(m_Attribute.Name, Relation.Parents);

            ItemCategoryEditorUtility.SerializeCategoryFamilyConnections(category);

            AttributeRemoved();
        }

        /// <summary>
        /// Send an event that the attribute was removed.
        /// </summary>
        private void AttributeRemoved()
        {
            OnRemove?.Invoke();
        }
    }
}