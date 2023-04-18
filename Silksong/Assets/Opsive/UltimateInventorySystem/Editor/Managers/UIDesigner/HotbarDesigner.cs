/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.Hotbar;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using System.Collections.Generic;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class HotbarDesigner : UIDesignerCreateEditTabContent<
        ItemHotbar,
        HotbarDesignerCreator,
        HotbarDesignerEditor>
    {
        public override string Title => "Hotbar";
        public override string Description => "Create an Item Hotbar where items can be assigned to slots and used.";
    }

    public class HotbarDesignerCreator : UIDesignerCreator<ItemHotbar>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/item-hotbar/";
        public IntegerField m_NumberOfSlots;

        protected override void CreateOptionsContent(VisualElement container)
        {
            m_NumberOfSlots = new IntegerField("Number of Slots");
            m_NumberOfSlots.value = 3;
            m_NumberOfSlots.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue < 0) {
                    m_NumberOfSlots.SetValueWithoutNotify(1);
                    return;
                }
            });
            container.Add(m_NumberOfSlots);
        }

        protected override ItemHotbar BuildInternal()
        {
            var rectParent = m_ParentTransform.value as RectTransform;

            var panel = UIDesignerManager.InstantiateSchemaPrefab<DisplayPanel>(UIDesignerSchema.ItemHotbarPanel, rectParent);

            var hotbar = panel.GetComponentInChildren<ItemHotbar>(true);

            var drawer = hotbar.GetComponent<ItemViewDrawer>();
            drawer.CategoryItemViewSet = UIDesignerSchema.ItemHotbarViewSet;

            var itemActionBinding = hotbar.GetComponent<ItemViewSlotsContainerCategoryItemActionSetBinding>();
            if (itemActionBinding != null) {
                itemActionBinding.m_CategoryItemActionSet = UIDesignerSchema.ItemHotbarActionSet;
            }

            var slotContainerContent = hotbar.Content;

            //Remove all children.
            while (slotContainerContent.childCount != 0) {
                DestroyGameObject(slotContainerContent.GetChild(0));
            }

            //Add new children.
            for (int i = 0; i < m_NumberOfSlots.value; i++) { AddItemViewSlot(slotContainerContent); }

            return hotbar;
        }

        public void AddItemViewSlot(RectTransform slotContainerContent)
        {
            var itemViewSlot =
                UIDesignerManager.InstantiateSchemaPrefab<ItemViewSlot>(UIDesignerSchema.ItemViewSlot, slotContainerContent);
            var itemViewSlotRect = itemViewSlot.transform as RectTransform;

            var itemView =
                UIDesignerManager.InstantiateSchemaPrefab<ItemView>(UIDesignerSchema.ItemViewForHotbar, itemViewSlotRect);
            itemViewSlot.m_ItemView = itemView;
            itemViewSlotRect.sizeDelta = (itemView.transform as RectTransform).sizeDelta;
        }
    }

    public class HotbarDesignerEditor : UIDesignerEditor<ItemHotbar>
    {
        protected DisplayPanelOptions m_DisplayPanelOptions;

        protected HotbarDesignerEditorOptions m_HotbarDesignerEditorOptions;

        protected ItemViewSlotsContainerOptions m_CommonItemViewSlotsContainerOptions;

        public HotbarDesignerEditor()
        {

            m_DisplayPanelOptions = new DisplayPanelOptions();

            m_HotbarDesignerEditorOptions = new HotbarDesignerEditorOptions(this);

            m_CommonItemViewSlotsContainerOptions = new ItemViewSlotsContainerOptions();
        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();
            m_TargetOptionsContainer.Add(m_DisplayPanelOptions);
            m_DisplayPanelOptions.Refresh(m_DisplayPanel);

            m_TargetOptionsContainer.Add(m_HotbarDesignerEditorOptions);
            m_HotbarDesignerEditorOptions.Refresh();

            var slotsContainer = m_Target;
            if (slotsContainer == null) {
                Debug.LogError("The Item View Slots Container is missing a hotbar.");
                return;
            }

            m_TargetOptionsContainer.Add(m_CommonItemViewSlotsContainerOptions);
            m_CommonItemViewSlotsContainerOptions.Refresh(slotsContainer);
        }
    }

    public class HotbarDesignerEditorOptions : UIDesignerBoxBase
    {
        public override string Title => "Hotbar Options";

        public override string Description => "You can add and remove Item View Slots here.\n" +
                                              "You could also duplicate the Item View Slots in the hierarchy.";

        public ItemHotbar ItemHotbar => m_HotbarEditor.Target;
        public ItemViewSlotsContainer ItemViewSlotsContainer => m_HotbarEditor.Target;

        protected HotbarDesignerEditor m_HotbarEditor;

        protected List<ItemViewSlot> m_ItemViewSlots;
        protected ReorderableList m_ItemViewSlotReorderableList;

        public HotbarDesignerEditorOptions(HotbarDesignerEditor editor)
        {
            m_ItemViewSlots = new List<ItemViewSlot>();
            m_HotbarEditor = editor;

            m_ItemViewSlotReorderableList = new ReorderableList(
                m_ItemViewSlots,
                (parent, index) =>
                {
                    var listElement = new Label("New");
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as Label;

                    if (index >= m_ItemViewSlots.Count) {
                        Debug.LogWarning("Index " + index + " does not exit.");
                        return;
                    }

                    listElement.text = index + ") Item View Slot ";
                }, (parent) =>
                {
                    parent.Add(new Label("Item View Modules"));
                },
                (index) =>
                {
                    ComponentSelection.Select(m_ItemViewSlots[index]);
                }, () =>
                {
                    m_HotbarEditor.UIDesignerManager.GetTab<HotbarDesigner>().DesignerCreator.AddItemViewSlot(ItemViewSlotsContainer.Content);
                    Refresh();
                }, (index) =>
                {
                    UIDesignerUtility.DestroyGameObject(m_ItemViewSlots[index]);
                    m_ItemViewSlots.RemoveAt(index);
                    Refresh();
                }, null);

            Add(m_ItemViewSlotReorderableList);
        }

        public void Refresh()
        {
            m_ItemViewSlots.Clear();
            m_ItemViewSlots.AddRange(ItemViewSlotsContainer.Content.GetComponentsInChildren<ItemViewSlot>(true));
            m_ItemViewSlotReorderableList.Refresh(m_ItemViewSlots);
        }
    }
}