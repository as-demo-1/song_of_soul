/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.Shared.Editor.Inspectors.Utility;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Editor.Inspectors;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.UI;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewSlotRestrictions;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.Hotbar;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using System;
    using System.Collections.Generic;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Button = UnityEngine.UIElements.Button;

    public class EquipmentDesigner : UIDesignerCreateEditTabContent<
        ItemSlotCollectionView,
        EquipmentDesignerCreator,
        EquipmentDesignerEditor>
    {
        public override string Title => "Equipment";
        public override string Description => "Create an Equipment Item View Slot Container to equip/unequip and show your equipped items.\n" +
                                              "Add restrictions to the item view slots such that they may only take item of certain categories.";
    }

    public class EquipmentDesignerCreator : UIDesignerCreator<ItemSlotCollectionView>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/equipment/";
        protected EnumField m_PanelOption;
        protected UnicodeTextField m_PanelName;
        protected UnicodeTextField m_ItemViewSlotContainerName;
        protected ObjectField m_InventoryField;
        protected ObjectFieldWithNestedInspector<ItemSlotSet, ItemSlotSetInspector> m_ItemSlotSetField;

        protected override void CreateOptionsContent(VisualElement container)
        {
            m_PanelOption = new EnumField("Panel Option", InventoryGridDisplayPanelOption.Simple);
            m_PanelOption.RegisterValueChangedCallback(evt => { Refresh(); });
            container.Add(m_PanelOption);

            m_PanelName = new UnicodeTextField("Panel Name");
            m_PanelName.value = "Equipment Panel";
            container.Add(m_PanelName);

            m_InventoryField = new ObjectField("Inventory");
            m_InventoryField.objectType = typeof(Inventory);
            m_InventoryField.tooltip = "The inventory is optional and will be set on the panel binding.";
            m_InventoryField.RegisterValueChangedCallback(evt => { Refresh(); });
            container.Add(m_InventoryField);

            m_ItemSlotSetField = new ObjectFieldWithNestedInspector<ItemSlotSet, ItemSlotSetInspector>(
                "Item Slot Set",
                null,
                "The Item Slot Set is used to create the correct amount of item slot views.",
                (newValue) =>
                {
                    Refresh();
                });
            container.Add(m_ItemSlotSetField);

            m_ItemViewSlotContainerName = new UnicodeTextField("Equipment Item View Slot Container Name");
            m_ItemViewSlotContainerName.value = "Equipment Item View Slot Container";

        }

        public override bool BuildCondition(bool logWarnings)
        {
            var result = base.BuildCondition(logWarnings);
            if (result == false) { return false; }

            var panelOption = (InventoryGridDisplayPanelOption)m_PanelOption.value;
            if (panelOption == InventoryGridDisplayPanelOption.MainMenu) {
                var rectParent = m_ParentTransform.value as RectTransform;
                var mainMenu = rectParent.gameObject.GetComponentInParent<MainMenu>(true);
                if (mainMenu == null || mainMenu.DisplayPanel.MainContent != rectParent) {
                    m_ConditionHelpBox.SetMessage(
                        "The parent transform must be the main menu main content when creating a main menu inner panel.");
                    return false;
                }
            }

            if (m_ItemSlotSetField.value == null) {
                m_ConditionHelpBox.SetMessage(
                    "An Item Slot Set must be set to create the correct amount of item view slots.");
                return false;
            }

            if (((IDatabaseSwitcher)m_ItemSlotSetField.value).IsComponentValidForDatabase(UIDesignerManager.InventoryMainWindow.Database) == false) {
                m_ConditionHelpBox.SetMessage(
                    "An Item Slot Set must match the current database.");
                return false;
            }

            return true;
        }

        protected override ItemSlotCollectionView BuildInternal()
        {
            var inventory = m_InventoryField.value as Inventory;

            var rectParent = m_ParentTransform.value as RectTransform;

            var panelOption = (InventoryGridDisplayPanelOption)m_PanelOption.value;
            var displayPanel =
                UIDesignerManager.InstantiateSchemaPrefab<DisplayPanel>(UIDesignerSchema.GetPanelPrefab(panelOption),
                    rectParent);
            displayPanel.SetName(m_PanelName.value);
            displayPanel.gameObject.name = m_PanelName.value;

            if (panelOption == InventoryGridDisplayPanelOption.MainMenu) {
                UIDesignerManager.GetTab<MainMenuDesigner>().AddInnerPanel(m_PanelName.value, displayPanel);
            }

            var equipmentView =
                UIDesignerManager.InstantiateSchemaPrefab<ItemSlotCollectionView>(
                    UIDesignerSchema.EquipmentItemViewSlotContainer, displayPanel.MainContent);
            var drawer = equipmentView.gameObject.GetComponent<ItemViewDrawer>();

            drawer.CategoryItemViewSet = UIDesignerSchema.GridCategoryItemViewSet;

            equipmentView.SetName(m_ItemViewSlotContainerName.value);
            equipmentView.gameObject.name = m_ItemViewSlotContainerName.value;

            var itemContainerPanelBinding = displayPanel.gameObject.AddComponent<ItemViewSlotsContainerPanelBinding>();
            itemContainerPanelBinding.ItemViewSlotsContainer = equipmentView;
            if (inventory != null) { itemContainerPanelBinding.Inventory = inventory; }

            itemContainerPanelBinding.m_DrawOnOpen = true;

            equipmentView.ItemSlotSet = m_ItemSlotSetField.value;

            var slotContainerContent = equipmentView.Content;

            //Remove all item view slots.
            while (slotContainerContent.childCount != 0) {
                DestroyGameObject(slotContainerContent.GetChild(0));
            }

            //Add new item view slots.
            var itemSlots = equipmentView.ItemSlotSet.ItemSlots;
            equipmentView.m_ItemSlotItemViewSlots = new ItemViewSlot[itemSlots.Count];
            for (int i = 0; i < itemSlots.Count; i++) {
                var itemSlot = itemSlots[i];
                var itemViewSlot = AddItemViewSlot(slotContainerContent);
                equipmentView.m_ItemSlotItemViewSlots[i] = itemViewSlot;

                itemViewSlot.name = itemSlot.Name + " Item View Slot";
                var itemViewSlotRect = itemViewSlot.transform as RectTransform;

                itemViewSlotRect.anchorMax = new Vector2(0.5f, 0.5f);
                itemViewSlotRect.anchorMin = new Vector2(0.5f, 0.5f);
                itemViewSlotRect.anchoredPosition = new Vector2(
                    i % 2 == 0 ? 100 : -100,
                    i % 2 == 0 ? 150 * (i / 2f) : 150 * (i - 1) / 2f);
            }

            equipmentView.Initialize(false);
            //Set the category restrictions.
            equipmentView.SetItemViewSlotRestrictions();

            Shared.Editor.Utility.EditorUtility.SetDirty(equipmentView);

            return equipmentView;
        }

        public ItemViewSlot AddItemViewSlot(RectTransform slotContainerContent)
        {
            var itemViewSlot =
                UIDesignerManager.InstantiateSchemaPrefab<ItemViewSlot>(UIDesignerSchema.ItemViewSlot, slotContainerContent);
            var itemViewSlotRect = itemViewSlot.transform as RectTransform;

            var itemView =
                UIDesignerManager.InstantiateSchemaPrefab<ItemView>(UIDesignerSchema.ItemViewForGrid, itemViewSlotRect);
            itemViewSlot.m_ItemView = itemView;
            itemViewSlotRect.sizeDelta = (itemView.transform as RectTransform).sizeDelta;

            return itemViewSlot;
        }
    }

    public class EquipmentDesignerEditor : UIDesignerEditor<ItemSlotCollectionView>
    {
        protected DisplayPanelOptions m_DisplayPanelOptions;

        protected EquipmentDesignerEditorOptions m_EquipmentDesignerEditorOptions;

        protected ItemViewSlotsContainerOptions m_CommonItemViewSlotsContainerOptions;

        public EquipmentDesignerEditor()
        {
            m_DisplayPanelOptions = new DisplayPanelOptions();

            m_EquipmentDesignerEditorOptions = new EquipmentDesignerEditorOptions(this);

            m_CommonItemViewSlotsContainerOptions = new ItemViewSlotsContainerOptions();
        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();

            m_TargetOptionsContainer.Add(m_DisplayPanelOptions);
            m_DisplayPanelOptions.Refresh(m_DisplayPanel);

            m_TargetOptionsContainer.Add(m_EquipmentDesignerEditorOptions);
            m_EquipmentDesignerEditorOptions.Refresh();

            m_TargetOptionsContainer.Add(m_CommonItemViewSlotsContainerOptions);
            m_CommonItemViewSlotsContainerOptions.Refresh(m_Target);
        }
    }

    public class EquipmentDesignerEditorOptions : UIDesignerBoxBase
    {
        public override string Title => "Equipment Options";

        public override string Description => "Allows the Item View Slots to be added, removed, or edited.";

        public ItemSlotCollectionView ItemSlotCollectionView => m_EquipmentEditor.Target;

        protected EquipmentDesignerEditor m_EquipmentEditor;

        protected VisualElement m_FixEquipmentViewContainer;
        protected InventoryHelpBox m_HelpBox;
        protected Button m_FixEquipmentViewButton;

        protected List<ItemViewSlot> m_ItemViewSlots;
        protected ReorderableList m_ItemViewSlotReorderableList;

        protected VisualElement m_ItemViewSlotSelectionContainer;
        protected Button m_EditItemViewButton;
        protected List<ItemViewSlotRestriction> m_ItemViewSlotRestrictions;
        protected ReorderableList m_ItemViewSlotRestrictionsReorderableList;

        protected int m_SelectedIndex = -1;
        protected ItemViewSlot m_SelectedItemViewSlot;

        public EquipmentDesignerEditorOptions(EquipmentDesignerEditor editor)
        {
            m_FixEquipmentViewContainer = new VisualElement();
            Add(m_FixEquipmentViewContainer);

            m_HelpBox = new InventoryHelpBox("Valid");

            m_FixEquipmentViewButton = new Button();
            m_FixEquipmentViewButton.text = "Fix Equipment View";
            m_FixEquipmentViewButton.clicked += FixEquipmentView;

            m_ItemViewSlots = new List<ItemViewSlot>();
            m_EquipmentEditor = editor;

            m_ItemViewSlotReorderableList = new ReorderableList(
                m_ItemViewSlots,
                (parent, index) =>
                {
                    var listElement = new Label("New");
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as Label;

                    if (index >= m_ItemViewSlots.Count || m_ItemViewSlots[index] == null) {
                        Debug.LogWarning("Index " + index + " does not exit.");
                        return;
                    }

                    listElement.text = index + ")  " + m_ItemViewSlots[index].gameObject.name;
                }, (parent) =>
                {
                    parent.Add(new Label("Item View Slots"));
                },
                HandleSelectedItemViewSlot,
                () =>
                {
                    m_EquipmentEditor.UIDesignerManager.GetTab<EquipmentDesigner>().DesignerCreator.AddItemViewSlot(ItemSlotCollectionView.Content);
                    Refresh();
                }, (index) =>
                {
                    UIDesignerUtility.DestroyGameObject(m_ItemViewSlots[index]);
                    m_ItemViewSlots.RemoveAt(index);
                    Refresh();
                }, null);

            Add(m_ItemViewSlotReorderableList);

            m_ItemViewSlotSelectionContainer = new VisualElement();
            Add(m_ItemViewSlotSelectionContainer);

            m_EditItemViewButton = new SubMenuButton("Edit Item View", EditItemView);

            m_ItemViewSlotRestrictions = new List<ItemViewSlotRestriction>();
            m_ItemViewSlotRestrictionsReorderableList = new ReorderableList(
                m_ItemViewSlotRestrictions,
                (parent, index) =>
                {
                    var listElement = new Label("New");
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as Label;

                    if (index >= m_ItemViewSlotRestrictions.Count) {
                        Debug.LogWarning("Index " + index + " does not exit.");
                        return;
                    }

                    listElement.text = m_ItemViewSlotRestrictions[index].ToString();
                }, (parent) =>
                {
                    parent.Add(new Label("Item View Slot Restrictions"));
                },
                (index) =>
                {
                    ComponentSelection.Select(m_ItemViewSlotRestrictions[index]);
                },
                () =>
                {
                    InspectorUtility.AddObjectType(typeof(ItemViewSlotRestriction),
                        null,
                        (evt) =>
                        {
                            var target = m_ItemViewSlots[m_SelectedIndex];
                            if (target == null) { return; }

                            var newObj = target.gameObject.AddComponent(evt as Type) as ItemViewSlotRestriction;

                            Refresh();
                        });
                }, (index) =>
                {
                    UIDesignerUtility.RemoveComponent(m_ItemViewSlotRestrictions[index]);
                    m_ItemViewSlotRestrictions.RemoveAt(index);
                    Refresh();
                }, null);
        }

        private void FixEquipmentView()
        {

        }

        private void HandleSelectedItemViewSlot(int index)
        {
            m_SelectedIndex = index;
            m_ItemViewSlotSelectionContainer.Clear();

            if (index < 0 || index > m_ItemViewSlots.Count) {
                m_SelectedItemViewSlot = null;
                return;
            }

            m_SelectedItemViewSlot = m_ItemViewSlots[index];
            if (m_SelectedItemViewSlot == null) { return; }

            ComponentSelection.Select(m_SelectedItemViewSlot);

            m_ItemViewSlotSelectionContainer.Add(new Label(m_SelectedItemViewSlot.gameObject.name));

            m_ItemViewSlotSelectionContainer.Add(m_EditItemViewButton);

            m_ItemViewSlotRestrictions.Clear();
            m_ItemViewSlotRestrictions.AddRange(m_SelectedItemViewSlot.GetComponents<ItemViewSlotRestriction>());
            m_ItemViewSlotRestrictionsReorderableList.Refresh(m_ItemViewSlotRestrictions);
            m_ItemViewSlotSelectionContainer.Add(m_ItemViewSlotRestrictionsReorderableList);
        }

        public void Refresh()
        {
            m_FixEquipmentViewContainer.Clear();

            ItemSlotCollectionView.Initialize(false);

            if (ItemSlotCollectionView.ItemSlotSet == null) {
                m_HelpBox.SetMessage("Item Slot Collection View must have an Item Slot Set assigned");
                m_FixEquipmentViewContainer.Add(m_HelpBox);
            } else {

                if (ItemSlotCollectionView.ItemSlotSet.ItemSlots.Count !=
                       ItemSlotCollectionView.ItemViewSlots.Count) {
                    m_HelpBox.SetMessage("The item View slot count does not match item slot set count");
                    m_FixEquipmentViewContainer.Add(m_HelpBox);
                }
            }


            m_ItemViewSlots.Clear();
            m_ItemViewSlots.AddRange(ItemSlotCollectionView.Content.GetComponentsInChildren<ItemViewSlot>(true));
            m_ItemViewSlotReorderableList.Refresh(m_ItemViewSlots);

            HandleSelectedItemViewSlot(m_SelectedIndex);
        }

        private void EditItemView()
        {
            if (m_SelectedItemViewSlot == null) { return; }

            var itemView = m_SelectedItemViewSlot.ItemView;
            var itemViewDesigner = UIDesignerManager.GetTab<ItemViewDesigner>();
            UIDesignerManager.ChangeTab(itemViewDesigner);
            itemViewDesigner.DesignerEditor.SetTarget(itemView);
        }
    }
}