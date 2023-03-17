/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.UI;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters;
    using System;
    using System.Collections.Generic;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Button = UnityEngine.UIElements.Button;

    public class ItemShapeGridDesigner : UIDesignerCreateEditTabContent<
        ItemShapeGrid,
        ItemShapeGridDesignerCreator,
        ItemShapeGridBuilderEditor>
    {
        public override string Title => "Item Shape Grid";
        public override string Description => "Create an Item Shape Grid using the options below.";
    }

    public class ItemShapeGridDesignerCreator : UIDesignerCreator<ItemShapeGrid>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/item-shape-grid/";
        public override string Title => "Item Shape Grid Creator";
        public override string Description => "Item Shape Grids are similar to Inventory Grids, but they allow you to use multiple slots for a single item.";

        protected EnumField m_PanelOption;
        protected UnicodeTextField m_PanelName;
        protected UnicodeTextField m_ItemViewSlotContainerName;
        protected Vector2IntField m_GridSize;
        protected Vector2Field m_ItemShapeSize;
        protected ObjectField m_InventoryField;
        protected PopupField<string> m_ItemCollectionField;
        protected List<string> m_ItemCollectionFieldChoices;

        protected override void CreateOptionsContent(VisualElement container)
        {
            m_PanelOption = new EnumField("Panel Option", InventoryGridDisplayPanelOption.Simple);
            m_PanelOption.RegisterValueChangedCallback(evt => { Refresh(); });
            container.Add(m_PanelOption);

            m_PanelName = new UnicodeTextField("Panel Name");
            m_PanelName.value = "Item Shape Grid Panel";
            container.Add(m_PanelName);

            m_InventoryField = new ObjectField("Inventory");
            m_InventoryField.objectType = typeof(Inventory);
            m_InventoryField.tooltip = "The Inventory is required as some components must be added to it.";
            m_InventoryField.RegisterValueChangedCallback(evt => { Refresh(); });
            container.Add(m_InventoryField);

            m_ItemViewSlotContainerName = new UnicodeTextField("Grid Name");
            m_ItemViewSlotContainerName.value = "Item Shape Grid";
            container.Add(m_ItemViewSlotContainerName);

            m_GridSize = new Vector2IntField("Grid Size");
            m_GridSize.value = new Vector2Int(8, 8);
            container.Add(m_GridSize);

            m_ItemShapeSize = new Vector2Field("Item Shape Size");
            m_ItemShapeSize.value = new Vector2Int(100, 100);
            container.Add(m_ItemShapeSize);

            m_ItemCollectionFieldChoices = new List<string>() { "ALL (NONE)" };
            m_ItemCollectionField = new PopupField<string>("Item Collection", m_ItemCollectionFieldChoices, 0);
            container.Add(m_ItemCollectionField);

        }

        public override void Refresh()
        {
            m_ItemCollectionFieldChoices.Clear();
            var inventory = m_InventoryField.value as Inventory;
            m_ItemCollectionFieldChoices.Add("ALL (NONE)");
            if (inventory != null) {
                inventory.Initialize(false);
                for (int i = 0; i < inventory.ItemCollections.Count; i++) {
                    m_ItemCollectionFieldChoices.Add(inventory.ItemCollections[i].Name);
                }
            }

            m_ItemCollectionField.index = Mathf.Min(m_ItemCollectionField.index, m_ItemCollectionFieldChoices.Count - 1);
            m_ItemCollectionField.value = m_ItemCollectionFieldChoices[m_ItemCollectionField.index];

            base.Refresh();
        }

        public override bool BuildCondition(bool logWarnings)
        {
            var result = base.BuildCondition(logWarnings);
            if (result == false) { return false; }

            if (m_InventoryField.value == null) {
                m_ConditionHelpBox.SetMessage("An inventory (such as the player inventory) must be assigned to the Inventory field.");
                return false;
            }

            var panelOption = (InventoryGridDisplayPanelOption)m_PanelOption.value;
            if (panelOption == InventoryGridDisplayPanelOption.MainMenu) {
                var rectParent = m_ParentTransform.value as RectTransform;
                var mainMenu = rectParent.gameObject.GetComponentInParent<MainMenu>(true);
                if (mainMenu == null || mainMenu.DisplayPanel.MainContent != rectParent) {
                    m_ConditionHelpBox.SetMessage("The parent transform must be the main menu main content when making a main menu inner panel.");
                    return false;
                }
            }

            return true;
        }

        protected override ItemShapeGrid BuildInternal()
        {
            var inventory = m_InventoryField.value as Inventory;

            //Create Shape Grid Data and Controller
            var itemShapeGridController = inventory.GetComponent<ItemShapeGridController>();
            if (itemShapeGridController == null) {
                itemShapeGridController = inventory.gameObject.AddComponent<ItemShapeGridController>();
                itemShapeGridController.m_ItemShapeGridData = new List<ItemShapeGridData>();
            }

            var itemShapeData = inventory.gameObject.AddComponent<ItemShapeGridData>();
            itemShapeData.m_GridSize = m_GridSize.value;

            itemShapeData.m_ItemCollections = new string[] {m_ItemCollectionField.value};
            itemShapeGridController.m_ItemShapeGridData.Add(itemShapeData);

            //Create UI Grid
            var rectParent = m_ParentTransform.value as RectTransform;

            var panelOption = (InventoryGridDisplayPanelOption)m_PanelOption.value;
            var displayPanel = UIDesignerManager.InstantiateSchemaPrefab<DisplayPanel>(UIDesignerSchema.GetPanelPrefab(panelOption), rectParent);
            displayPanel.SetName(m_PanelName.value);
            displayPanel.gameObject.name = m_PanelName.value;

            if (panelOption == InventoryGridDisplayPanelOption.MainMenu) {
                UIDesignerManager.GetTab<MainMenuDesigner>().AddInnerPanel(m_PanelName.value, displayPanel);
            }

            var itemShapeGrid = UIDesignerManager.InstantiateSchemaPrefab<ItemShapeGrid>(
                UIDesignerSchema.ItemShapeGrid, displayPanel.MainContent);

            var drawer = itemShapeGrid.gameObject.GetComponent<ItemViewDrawer>();
            drawer.CategoryItemViewSet = UIDesignerSchema.ItemShapeCategoryItemViewSet;

            itemShapeGrid.ItemViewSlotPrefab = UIDesignerSchema.ItemShapeViewSlot;

            itemShapeGrid.SetGridSize(m_GridSize.value, false);
            SetItemShapeSize(itemShapeGrid, m_ItemShapeSize.value);
            itemShapeGrid.SetGridSize(m_GridSize.value, true);

            itemShapeGrid.SetName(m_ItemViewSlotContainerName.value);
            itemShapeGrid.gameObject.name = m_ItemViewSlotContainerName.value;

            var itemContainerPanelBinding = displayPanel.gameObject.AddComponent<ItemViewSlotsContainerPanelBinding>();
            itemContainerPanelBinding.ItemViewSlotsContainer = itemShapeGrid;
            itemContainerPanelBinding.Inventory = inventory;

            return itemShapeGrid;
        }

        public void SetItemShapeSize(ItemShapeGrid itemShapeGrid, Vector2 itemShapeSize)
        {
            itemShapeGrid.SeItemShapeSize(itemShapeSize, true);

            var viewPort = itemShapeGrid.transform.GetChild(0) as RectTransform;
            viewPort.sizeDelta = new Vector2(
                m_GridSize.value.x * itemShapeGrid.ItemShapeSize.x,
                m_GridSize.value.y * itemShapeGrid.ItemShapeSize.y);
        }
    }

    public class ItemShapeGridBuilderEditor : UIDesignerEditor<ItemShapeGrid>
    {
        public override string Title => "Item Shape Grid Editor";
        public override string Description => "Edit an existing inventory grid. Any changes made will take effect immediately.";

        protected DisplayPanelOptions m_DisplayPanelOptions;
        protected ItemShapeGridOption m_ItemShapeGridOptions;
        protected ItemViewSlotsContainerOptions m_CommonItemViewSlotsContainerOptions;

        public ItemShapeGridBuilderEditor()
        {

            m_DisplayPanelOptions = new DisplayPanelOptions();

            m_ItemShapeGridOptions = new ItemShapeGridOption(this);

            m_CommonItemViewSlotsContainerOptions = new ItemViewSlotsContainerOptions();

        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();

            m_TargetOptionsContainer.Add(m_DisplayPanelOptions);

            m_TargetOptionsContainer.Add(m_ItemShapeGridOptions);
            m_ItemShapeGridOptions.Refresh(m_Target);

            m_TargetOptionsContainer.Add(m_CommonItemViewSlotsContainerOptions);

            m_DisplayPanelOptions.Refresh(m_DisplayPanel);

            m_CommonItemViewSlotsContainerOptions.Refresh(m_Target);
        }
    }

    public abstract class ItemShapeGridEditorOption : UIDesignerBoxBase
    {
        protected ItemShapeGridBuilderEditor m_GridEditor;

        protected ItemShapeGrid m_ItemShapeGrid;
        protected DisplayPanel DisplayPanel => m_GridEditor.DisplayPanel;

        protected ItemShapeGridEditorOption(ItemShapeGridBuilderEditor gridEditor)
        {
            m_GridEditor = gridEditor;
        }

        public virtual void Refresh(ItemShapeGrid itemShapeGrid)
        {
            m_ItemShapeGrid = itemShapeGrid;
        }
    }

    public class ItemShapeGridOption : ItemShapeGridEditorOption
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/inventory-grid/";
        public override string Title => "Item Shape Grid";

        public override string Description =>
            "The grid selected is a grid with item shapes, options available may be different from standard inventory grids.\n" +
            "Filters work slightly differently. Filters are added on the Inventory GameObject with the Grid Item Shape Data instead of on the UI components.";

        public override Func<Component> SelectTargetGetter => () => m_ItemShapeGrid;

        protected Inventory m_Inventory;
        protected ItemShapeGridController m_ItemShapeGridController;
        protected ItemShapeGridData m_ItemShapeGridData;
        protected ComponentSelectionButton m_SelectGridControllerButton;
        protected ComponentSelectionButton m_SelectGridDataButton;

        protected Label m_GridDataIDLabel;
        protected GridSizeField m_GridSizeField;
        protected Vector2Field m_ItemShapeSizeField;

        protected VisualElement m_DynamicContainer;
        protected FiltersField m_FiltersField;
        protected ItemInfoMultiFilterSorter m_MultiFilterSorter;

        protected InventoryHelpBox m_HelpBox;

        public ItemShapeGridOption(ItemShapeGridBuilderEditor gridEditor) : base(gridEditor)
        {
            m_GridDataIDLabel = new SubTitleLabel();
            Add(m_GridDataIDLabel);

            Add(new SubTitleLabel("Grid and Item Shape Size"));

            m_GridSizeField = new GridSizeField();
            m_GridSizeField.OnValueChanged += ResizeGrid;
            Add(m_GridSizeField);

            var horizontal2 = new VisualElement();
            horizontal2.style.flexDirection = FlexDirection.Row;
            Add(horizontal2);

            m_ItemShapeSizeField = new Vector2Field("Item Shape Size");
            m_ItemShapeSizeField.style.width = 350;
            m_ItemShapeSizeField.tooltip = "The Size in pixel of a 1x1 shape";
            m_ItemShapeSizeField.RegisterValueChangedCallback(evt =>
            {
                m_ItemShapeGrid.SeItemShapeSize(evt.newValue, false);
            });
            horizontal2.Add(m_ItemShapeSizeField);

            var applyItemShapeSizeButton = new Button();
            applyItemShapeSizeButton.text = "Apply Item Shape Size";
            applyItemShapeSizeButton.clicked += () =>
            {
                ResizeItemShape(m_ItemShapeSizeField.value);
            };
            horizontal2.Add(applyItemShapeSizeButton);

            Add(new SubTitleLabel("Item Shape Grid Controller and Data"));

            var horizontal = new VisualElement();
            horizontal.AddToClassList(CommonStyles.s_HorizontalAlignCenter);
            Add(horizontal);

            m_SelectGridControllerButton = new ComponentSelectionButton("Select Item Shap Grid Controller", () => m_ItemShapeGridController);
            horizontal.Add(m_SelectGridControllerButton);

            m_SelectGridDataButton = new ComponentSelectionButton("Select Item Shape Grid Data", () => m_ItemShapeGridData);
            horizontal.Add(m_SelectGridDataButton);

            m_DynamicContainer = new VisualElement();
            Add(m_DynamicContainer);

            m_HelpBox = new InventoryHelpBox("All Good!");
        }

        private void ResizeGrid(Vector2Int newSize)
        {
            m_ItemShapeGrid.SetGridSize(newSize, true);

            if (m_ItemShapeGridData != null) {
                m_ItemShapeGridData.m_GridSize = newSize;
            }

            Refresh(m_ItemShapeGrid);
        }

        private void ResizeItemShape(Vector2 newSize)
        {
            m_ItemShapeGrid.SeItemShapeSize(newSize, true);

            Debug.Log("Resize Item Shape Size");
            Refresh(m_ItemShapeGrid);
        }

        public override void Refresh(ItemShapeGrid inventoryGrid)
        {
            base.Refresh(inventoryGrid);

            m_GridDataIDLabel.text = "Item Shape Grid Data ID: " + m_ItemShapeGrid.ItemShapeGridDataID;
            m_GridSizeField.SetValueNoNotify(m_ItemShapeGrid.GridSize);
            m_ItemShapeSizeField.SetValueWithoutNotify(m_ItemShapeGrid.ItemShapeSize);

            m_DynamicContainer.Clear();

            m_Inventory = DisplayPanel.GetComponent<ItemViewSlotsContainerPanelBinding>().Inventory;
            if (m_Inventory == null) {
                m_HelpBox.SetMessage(
                    "The Inventory is missing from the ItemViewSlotsContainerPanelBinding. " +
                    "Ignore this warning if the inventory is added in the scene at runtime.");
                m_DynamicContainer.Add(m_HelpBox);
                return;
            }

            m_ItemShapeGridController = m_Inventory.GetComponent<ItemShapeGridController>();
            if (m_ItemShapeGridController == null) {
                m_HelpBox.SetMessage(
                    "An Item Shape Grid Controller is missing from the Inventory gameobject.");
                m_DynamicContainer.Add(m_HelpBox);

                CreateButton("Add Item Shape Grid Controller", m_DynamicContainer, () =>
                {
                    m_Inventory.gameObject.AddComponent<ItemShapeGridController>();
                    Refresh(m_ItemShapeGrid);
                });

                return;
            }

            if (m_ItemShapeGridController.m_ItemShapeGridData == null) {
                m_ItemShapeGridController.m_ItemShapeGridData = new List<ItemShapeGridData>();
            }

            m_ItemShapeGridData = m_ItemShapeGridController.GetGridDataWithID(m_ItemShapeGrid.ItemShapeGridDataID);
            if (m_ItemShapeGridData == null) {
                m_HelpBox.SetMessage(
                    $"An Item Shape Grid Data with the ID {m_ItemShapeGrid.ItemShapeGridDataID} is missing from the Inventory gameobject.");
                m_DynamicContainer.Add(m_HelpBox);

                CreateButton($"Add Item Shape Grid Data with ID {m_ItemShapeGrid.ItemShapeGridDataID}", m_DynamicContainer, () =>
                {
                    var gridData = m_Inventory.gameObject.AddComponent<ItemShapeGridData>();
                    gridData.m_ID = m_ItemShapeGrid.ItemShapeGridDataID;
                    gridData.m_GridSize = m_ItemShapeGrid.GridSize;
                    m_ItemShapeGridController.m_ItemShapeGridData.Add(gridData);
                    Refresh(m_ItemShapeGrid);
                });

                return;
            }

            if (m_ItemShapeGrid.GridSize != m_ItemShapeGridData.GridSize) {
                m_HelpBox.SetMessage(
                    $"The Item Shape Grid Size {m_ItemShapeGrid.GridSize} does not match the Item Shape Grid Data Size {m_ItemShapeGridData.GridSize}.");
                m_DynamicContainer.Add(m_HelpBox);

                CreateButton($"Set the Grid Data Size to match {m_ItemShapeGrid.GridSize}", m_DynamicContainer, () =>
                {
                    m_ItemShapeGridData.m_GridSize = m_ItemShapeGrid.GridSize;
                    Refresh(m_ItemShapeGrid);
                });
                return;
            }

            m_MultiFilterSorter = m_ItemShapeGridData.m_ItemInfoFilter as ItemInfoMultiFilterSorter;

            m_FiltersField = new FiltersField("Item Shape Data Filters",
                () => m_MultiFilterSorter,
                () => m_MultiFilterSorter?.m_GridFilters,
                () =>
                {
                    m_MultiFilterSorter = m_ItemShapeGridData.gameObject.AddComponent<ItemInfoMultiFilterSorter>();

                    m_ItemShapeGridData.m_ItemInfoFilter = m_MultiFilterSorter;
                },
                () =>
                {
                    if (m_MultiFilterSorter == null) { return; }

                    for (int i = 0; i < m_MultiFilterSorter.m_GridFilters.Count; i++) {
                        UIDesignerUtility.RemoveComponent(m_MultiFilterSorter.m_GridFilters[i]);
                    }

                    UIDesignerUtility.RemoveComponent(m_MultiFilterSorter);
                },
                (filter) =>
                {
                    if (m_MultiFilterSorter.m_GridFilters == null) {
                        m_MultiFilterSorter.m_GridFilters = new List<ItemInfoFilterSorterBase>();
                    }

                    m_MultiFilterSorter.m_GridFilters.Add(filter);
                },
                (indexToRemove) =>
                {
                    m_MultiFilterSorter.m_GridFilters.RemoveAt(indexToRemove);
                });
            m_DynamicContainer.Add(m_FiltersField);
        }
    }
}