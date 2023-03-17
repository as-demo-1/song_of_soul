/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.Shared.Editor.Inspectors.Utility;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using Opsive.UltimateInventorySystem.UI.Views;
    using System;
    using System.Collections.Generic;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class ItemViewDesigner : UIDesignerCreateEditTabContent<
        ItemView,
        ItemViewBuilderCreator,
        ItemViewBuilderEditor>
    {
        public override string Title => "Item View";

        public override string Description =>
            "Item Views are used to display items and can be customized to show the interested information.\n" +
            "Once complete create a prefab and assign it to you Category Item View Set.\n" +
            "In some cases having a single complex Item View makes more sense compared to having multiple simple ones. Check the documentation to learn more.";
    }

    public class ItemViewBuilderCreator : UIDesignerCreator<ItemView>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/item-view/";

        private enum ItemViewOption
        {
            ForGrid,
            ForList,
            ForShop,
            ForItemShape,
            ForIngredient,
            ForInventoryMonitor,
        }

        protected EnumField m_Preset;

        protected override void CreateOptionsContent(VisualElement container)
        {
            m_Preset = new EnumField("Item View Preset", ItemViewOption.ForGrid);
            container.Add(m_Preset);
        }

        protected override ItemView BuildInternal()
        {
            ItemView prefab = null;
            switch ((ItemViewOption)m_Preset.value) {
                case ItemViewOption.ForGrid:
                    prefab = UIDesignerSchema.ItemViewForGrid;
                    break;
                case ItemViewOption.ForList:
                    prefab = UIDesignerSchema.ItemViewForList;
                    break;
                case ItemViewOption.ForShop:
                    prefab = UIDesignerSchema.ItemViewForShop;
                    break;
                case ItemViewOption.ForItemShape:
                    prefab = UIDesignerSchema.ItemViewForItemShape;
                    break;
                case ItemViewOption.ForIngredient:
                    prefab = UIDesignerSchema.ItemViewForIngredient;
                    break;
                case ItemViewOption.ForInventoryMonitor:
                    prefab = UIDesignerSchema.ItemViewForInventoryMonitor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var defaultItemView = UIDesignerManager.InstantiateSchemaPrefab<ItemView>(prefab, m_ParentTransform.value as RectTransform);
            return defaultItemView;
        }
    }

    public class ItemViewBuilderEditor : UIDesignerEditor<ItemView>
    {
        protected override bool RequireDisplayPanel => false;
        protected ViewModulesReorderableList<ItemView, ItemViewModule> m_ItemViewModulesReorderableList;

        public ItemViewBuilderEditor()
        {
            m_ItemViewModulesReorderableList = new ViewModulesReorderableList<ItemView, ItemViewModule>();
            Add(m_ItemViewModulesReorderableList);
        }

        protected override void TargetObjectChanged()
        {
            base.TargetObjectChanged();

            m_ItemViewModulesReorderableList.Refresh(m_Target);
        }
    }

    public class ViewModulesReorderableList<Tv, Tm> : VisualElement where Tv : View where Tm : ViewModule
    {
        protected Tv m_View;
        protected List<ViewModule> m_ViewModules;
        protected ReorderableList m_ReorderableList;

        public ViewModulesReorderableList()
        {
            m_ViewModules = new List<ViewModule>();

            m_ReorderableList = new ReorderableList(
                m_ViewModules,
                (parent, index) =>
                {
                    var listElement = new Label("New");
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as Label;

                    if (index >= m_ViewModules.Count) {
                        Debug.LogWarning("Index " + index + " does not exit.");
                        return;
                    }

                    listElement.text = m_ViewModules[index].ToText();
                }, (parent) =>
                {
                    parent.Add(new Label(typeof(Tv).Name + " Modules"));
                },
                (index) =>
                {
                    ComponentSelection.Select(m_ViewModules[index]);
                }, AddNewViewModule,
                (index) =>
                {
                    UIDesignerUtility.RemoveComponent(m_ViewModules[index]);
                    m_ViewModules.RemoveAt(index);
                }, null);
        }

        private void AddNewViewModule()
        {
            InspectorUtility.AddObjectType(typeof(ViewModule),
                (type) =>
                {
                    if (type.IsSubclassOfRawGeneric(typeof(ViewModule<>)) && !type.IsSubclassOf(typeof(Tm))) { return false; }

                    return true;
                },
                (evt) =>
                {
                    var target = m_View;
                    if (target == null) { return; }

                    var newObj = target.gameObject.AddComponent(evt as Type) as ViewModule;

                    Refresh(m_View);

                    m_ReorderableList.SelectedIndex = m_ViewModules.Count - 1;
                });
        }

        public void Refresh(Tv view)
        {
            m_View = view;

            Clear();
            m_ViewModules.Clear();

            Shared.Editor.Utility.EditorUtility.SetDirty(m_View);

            if (m_View == null) {
                return;
            }

            m_View.RetrieveModules();
            Shared.Editor.Utility.EditorUtility.SetDirty(m_View);

            m_ViewModules.AddRange(m_View.Modules);
            m_ReorderableList.Refresh(m_ViewModules);
            Add(m_ReorderableList);
        }
    }
}