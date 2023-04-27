/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Storage;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Attribute name binding inspector.
    /// </summary>
    [CustomEditor(typeof(ItemCategoryAttributeNameBinding), true)]
    public class AttributeNameBindingInspector : DatabaseInspectorBase
    {
        protected const string c_ItemCategoryPropertyName = "m_ItemCategory";
        protected const string c_AttributeBindingsPropertyName = "m_AttributeBindings";
        protected override List<string> ExcludedFields => new List<string>()
        {
            c_ItemCategoryPropertyName, c_AttributeBindingsPropertyName
        };

        protected ItemCategoryAttributeNameBinding m_ItemCategoryAttributeNameBinding;
        protected ItemCategoryAttributeBindingView m_ItemCategoryAttributeBindingView;
        protected VisualElement m_SelectedContainer;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_ItemCategoryAttributeNameBinding = target as ItemCategoryAttributeNameBinding;

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_ItemCategoryAttributeNameBinding = target as ItemCategoryAttributeNameBinding;

            m_SelectedContainer = new VisualElement();

            container.Add(m_SelectedContainer);

            Refresh();
        }

        /// <summary>
        /// Refresh the inspector.
        /// </summary>
        protected void Refresh()
        {
            m_SelectedContainer.Clear();
            m_ItemCategoryAttributeBindingView = new ItemCategoryAttributeNameBindingView(
                m_DatabaseField.value as InventorySystemDatabase, typeof(Object));

            m_ItemCategoryAttributeBindingView.SetItemCategory(m_ItemCategoryAttributeNameBinding.ItemCategory);
            m_ItemCategoryAttributeBindingView.SetAttributeBindings(m_ItemCategoryAttributeNameBinding.AttributeBindings);
            m_ItemCategoryAttributeBindingView.OnItemCategoryChanged += (x) =>
            {
                m_ItemCategoryAttributeNameBinding.SetItemCategory(x);
                Opsive.Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemCategoryAttributeNameBinding);
            };

            m_ItemCategoryAttributeBindingView.OnAttributeBindingsChanged += (bindings) =>
            {
                var bindingArray = new AttributeNameBinding[bindings.Count];
                for (int i = 0; i < bindings.Count; i++) { bindingArray[i] = bindings[i] as AttributeNameBinding; }
                m_ItemCategoryAttributeNameBinding.AttributeBindings = bindingArray;
            };

            m_SelectedContainer.Add(m_ItemCategoryAttributeBindingView);
        }
    }
}