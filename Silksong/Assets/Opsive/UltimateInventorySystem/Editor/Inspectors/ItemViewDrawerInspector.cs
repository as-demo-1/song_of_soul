/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Inspectors
{
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using Opsive.UltimateInventorySystem.UI.Item;
    using System.Collections.Generic;
    using Opsive.Shared.Editor.UIElements;
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// A custom inspector for the Item View drawer component.
    /// </summary>
    [CustomEditor(typeof(ItemViewDrawer), true)]
    public class ItemViewDrawerInspector : InspectorBase
    {
        protected override List<string> ExcludedFields => new List<string>() { "m_CategoryItemViewSet" };

        protected ItemViewDrawer m_ItemViewDrawer;
        protected ObjectFieldWithNestedInspector<CategoryItemViewSet, CategoryItemViewSetInspector>
            m_CategoryItemViewSet;

        /// <summary>
        /// Initialize the inspector when it is first selected.
        /// </summary>
        protected override void InitializeInspector()
        {
            m_ItemViewDrawer = target as ItemViewDrawer;

            base.InitializeInspector();
        }

        /// <summary>
        /// Create the inspector.
        /// </summary>
        /// <param name="container">The parent container.</param>
        protected override void ShowFooterElements(VisualElement container)
        {
            m_CategoryItemViewSet = new ObjectFieldWithNestedInspector
                <CategoryItemViewSet, CategoryItemViewSetInspector>(
                    "Category Item View Set",
                    m_ItemViewDrawer.CategoryItemViewSet,
                    "The Item Views for each itemCategory.",
                    (newValue) =>
                    {
                        m_ItemViewDrawer.CategoryItemViewSet = newValue;
                        Shared.Editor.Utility.EditorUtility.SetDirty(m_ItemViewDrawer);
                    });

            container.Add(m_CategoryItemViewSet);
        }
    }
}