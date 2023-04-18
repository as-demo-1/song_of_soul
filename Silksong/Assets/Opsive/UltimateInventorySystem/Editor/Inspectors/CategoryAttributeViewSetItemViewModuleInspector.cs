/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using System.Collections.Generic;
    using Opsive.Shared.Editor.UIElements;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// A custom inspector for the item hot bar grid component.
    /// </summary>
    [CustomEditor(typeof(CategoryAttributeViewSetItemViewModule), true)]
    public class CategoryAttributeViewSetItemViewModuleInspector : InspectorBase
    {
        protected override List<string> ExcludedFields => new List<string>() { "m_CategoryAttributeViewSet" };

        protected CategoryAttributeViewSetItemViewModule m_CategoryAttributeViewSetItemViewModule;
        protected ObjectFieldWithNestedInspector<CategoryAttributeViewSet, CategoryAttributeViewSetInspector>
            m_CategoryAttributeBoxSet;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_CategoryAttributeViewSetItemViewModule = target as CategoryAttributeViewSetItemViewModule;

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_CategoryAttributeBoxSet = new ObjectFieldWithNestedInspector
                <CategoryAttributeViewSet, CategoryAttributeViewSetInspector>(
                    "Category Attribute View Set",
                    m_CategoryAttributeViewSetItemViewModule.m_CategoryAttributeViewSet,
                    "The attribute views for each itemCategory.",
                    (newValue) =>
                    {
                        m_CategoryAttributeViewSetItemViewModule.m_CategoryAttributeViewSet = newValue;
                        Shared.Editor.Utility.EditorUtility.SetDirty(m_CategoryAttributeViewSetItemViewModule);
                    });

            container.Add(m_CategoryAttributeBoxSet);
        }
    }
}