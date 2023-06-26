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
    /// Item Binding inspector, used to display that attributes nicely.
    /// </summary>
    [CustomEditor(typeof(ItemBinding), true)]
    public class ItemBindingInspector : DatabaseInspectorBase
    {
        protected const string c_ItemCategoryPropertyName = "m_ItemCategory";
        protected override List<string> ExcludedFields => new List<string>() { c_ItemCategoryPropertyName };

        protected ItemBinding m_ItemBinding;
        protected ItemCategoryAttributeBindingView m_ItemCategoryAttributeBindingView;
        protected VisualElement m_SelectedContainer;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_ItemBinding = target as ItemBinding;

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {

            m_SelectedContainer = new VisualElement();

            container.Add(m_SelectedContainer);

            Refresh();
        }

        /// <summary>
        /// Refresh the view when the database changes.
        /// </summary>
        protected void Refresh()
        {
            m_SelectedContainer.Clear();
            m_ItemCategoryAttributeBindingView = new ItemCategoryAttributeBindingView(m_DatabaseField.value as InventorySystemDatabase, typeof(Object));

            m_ItemBinding.Initialize(false);

            m_ItemCategoryAttributeBindingView.SetItemCategory(
                m_ItemBinding.ItemCategory == null ? null : m_ItemBinding.ItemCategory);
            m_ItemCategoryAttributeBindingView.SetAttributeBindings(m_ItemBinding.AttributeBindings);
            m_ItemCategoryAttributeBindingView.OnItemCategoryChanged += (x) =>
            {
                m_ItemBinding.SetItemCategory(x);
                Serialize();
            };

            m_ItemCategoryAttributeBindingView.OnAttributeBindingsChanged += (bindings) =>
            {
                if (Application.isPlaying) { return; }
                m_ItemBinding.AttributeBindings = bindings.ToArray();
                Serialize();
            };

            m_SelectedContainer.Add(m_ItemCategoryAttributeBindingView);
        }

        /// <summary>
        /// Serialize and dirty the object.
        /// </summary>
        private void Serialize()
        {
            m_ItemBinding.Serialize();
            Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemBinding);
        }
    }
}