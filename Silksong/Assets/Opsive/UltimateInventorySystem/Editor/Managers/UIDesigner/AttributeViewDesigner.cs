/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.UltimateInventorySystem.UI.Item.AttributeViewModules;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The Attribute view Designer.
    /// </summary>
    public class AttributeViewDesigner : UIDesignerCreateEditTabContent<
        AttributeView,
        AttributeViewBuilderCreator,
        AttributeViewBuilderEditor>
    {
        public override string Title => "Attribute View";

        public override string Description =>
            "Attribute Views are used to display a single attribute value.\n" +
            "It is often added to an item view or an item description using a Category Attribute View Set.";
    }

    public class AttributeViewBuilderCreator : UIDesignerCreator<AttributeView>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/attribute-view/";

        protected override void CreateOptionsContent(VisualElement container)
        {
        }

        protected override AttributeView BuildInternal()
        {
            var prefab = UIDesignerSchema.AttributeView;
            var rectParent = m_ParentTransform.value as RectTransform;
            var attributeView = UIDesignerManager.InstantiateSchemaPrefab<AttributeView>(prefab, rectParent);
            return attributeView;
        }
    }

    public class AttributeViewBuilderEditor : UIDesignerEditor<AttributeView>
    {
        protected override bool RequireDisplayPanel => false;
        protected ViewModulesReorderableList<AttributeView, AttributeViewModule> m_AttributeViewModulesReorderableList;

        public AttributeViewBuilderEditor()
        {
            m_AttributeViewModulesReorderableList = new ViewModulesReorderableList<AttributeView, AttributeViewModule>();
            Add(m_AttributeViewModulesReorderableList);
        }

        protected override void TargetObjectChanged()
        {
            base.TargetObjectChanged();

            m_AttributeViewModulesReorderableList.Refresh(m_Target);
        }
    }
}