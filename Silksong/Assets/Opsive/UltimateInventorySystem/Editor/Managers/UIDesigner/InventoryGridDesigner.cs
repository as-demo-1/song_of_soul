/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.Shared.Editor.Inspectors.Utility;
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters.InventoryGridFilters;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers.GridFilterSorters.InventoryGridSorters;
    using System;
    using System.Collections.Generic;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.UIElements;
    using Button = UnityEngine.UIElements.Button;

    public enum InventoryGridDisplayPanelOption
    {
        Basic,
        Simple,
        Floating,
        MainMenu,
    }

    public enum InventoryGridOption
    {
        Grid,            //Simple grid.
        List            //Simple list.
    }

    public class InventoryGridDesigner : UIDesignerCreateEditTabContent<
        InventoryGrid,
        InventoryGridDesignerCreator,
        InventoryGridBuilderEditor>
    {
        public override string Title => "Inventory Grid";
        public override string Description => "Create an inventory Grid using the options below.";
    }

    public class InventoryGridDesignerCreator : UIDesignerCreator<InventoryGrid>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/inventory-grid/";
        public override string Title => "Inventory Grid Creator";
        public override string Description => " Inventory Grids are used for main inventories, storage, chests, etc.";

        protected EnumField m_PanelOption;
        protected UnicodeTextField m_PanelName;
        protected EnumField m_GridOption;
        protected UnicodeTextField m_ItemViewSlotContainerName;
        protected Vector2IntField m_GridSize;
        protected IntegerField m_ListLength;
        protected ObjectField m_InventoryField;

        protected VisualElement m_GridOptionContainer;

        protected override void CreateOptionsContent(VisualElement container)
        {
            m_PanelOption = new EnumField("Panel Option", InventoryGridDisplayPanelOption.Simple);
            m_PanelOption.RegisterValueChangedCallback(evt => { Refresh(); });
            container.Add(m_PanelOption);

            m_PanelName = new UnicodeTextField("Panel Name");
            m_PanelName.value = "My Inventory Grid Panel";
            container.Add(m_PanelName);

            m_InventoryField = new ObjectField("Inventory");
            m_InventoryField.objectType = typeof(Inventory);
            m_InventoryField.tooltip = "The inventory is optional, it will simply be set on the panel binding.";
            m_InventoryField.RegisterValueChangedCallback(evt => { Refresh(); });
            container.Add(m_InventoryField);

            m_GridOption = new EnumField("Grid Option", InventoryGridOption.Grid);
            m_GridOption.RegisterValueChangedCallback(evt => { Refresh(); });
            container.Add(m_GridOption);

            m_ItemViewSlotContainerName = new UnicodeTextField("Inventory Grid Name");
            m_ItemViewSlotContainerName.value = "Inventory Grid";

            m_GridOptionContainer = new VisualElement();
            container.Add(m_GridOptionContainer);

            //Inventory Grid additional options
            m_GridSize = new Vector2IntField("Grid Size");
            m_GridSize.value = new Vector2Int(4, 4);

            m_ListLength = new IntegerField("List Length");
            m_ListLength.value = 5;

            RefreshGridOptionContainer(InventoryGridOption.Grid);
        }

        public override void Refresh()
        {
            RefreshGridOptionContainer((InventoryGridOption)m_GridOption.value);
            base.Refresh();
        }

        public void RefreshGridOptionContainer(InventoryGridOption option)
        {
            m_GridOptionContainer.Clear();

            if (option == InventoryGridOption.Grid) {
                m_GridOptionContainer.Add(new SubDescriptionLabel(
                    "A normal inventory grid."));
                m_GridOptionContainer.Add(m_ItemViewSlotContainerName);
                m_GridOptionContainer.Add(m_GridSize);
            } else if (option == InventoryGridOption.List) {
                m_GridOptionContainer.Add(new SubDescriptionLabel(
                    "An inventory list is simply a grid with a single column. " +
                    "This option will use the list prefabs from the UI Designer schema."));
                m_GridOptionContainer.Add(m_ItemViewSlotContainerName);
                m_GridOptionContainer.Add(m_ListLength);
            }
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
                    m_ConditionHelpBox.SetMessage("The parent transform must be the main menu main content when making a main menu inner panel.");
                    return false;
                }
            }

            return true;
        }

        protected override InventoryGrid BuildInternal()
        {
            var inventory = m_InventoryField.value as Inventory;

            var rectParent = m_ParentTransform.value as RectTransform;

            var panelOption = (InventoryGridDisplayPanelOption)m_PanelOption.value;
            var displayPanel = UIDesignerManager.InstantiateSchemaPrefab<DisplayPanel>(UIDesignerSchema.GetPanelPrefab(panelOption), rectParent);
            displayPanel.SetName(m_PanelName.value);
            displayPanel.gameObject.name = m_PanelName.value;

            if (panelOption == InventoryGridDisplayPanelOption.MainMenu) {
                UIDesignerManager.GetTab<MainMenuDesigner>().AddInnerPanel(m_PanelName.value, displayPanel);
            }

            var gridOption = (InventoryGridOption)m_GridOption.value;

            var inventoryGrid = UIDesignerManager.InstantiateSchemaPrefab<InventoryGrid>(
                UIDesignerSchema.InventoryGrid, displayPanel.MainContent);

            var drawer = inventoryGrid.gameObject.GetComponent<ItemViewDrawer>();


            if (gridOption == InventoryGridOption.Grid) {
                drawer.CategoryItemViewSet = UIDesignerSchema.GridCategoryItemViewSet;
                inventoryGrid.Grid.GridSize = m_GridSize.value;

            } else if (gridOption == InventoryGridOption.List) {
                drawer.CategoryItemViewSet = UIDesignerSchema.ListCategoryItemViewSet;
                inventoryGrid.Grid.GridSize = new Vector2Int(1, m_ListLength.value);

                var layoutGroup = inventoryGrid.Grid.GridEventSystem.Content.GetComponent<GridLayoutGroup>();
                if (layoutGroup != null) {
                    layoutGroup.cellSize = new Vector2(550, 120);
                }

            }

            inventoryGrid.Grid.GridEventSystem.ButtonPrefab = UIDesignerSchema.ItemViewSlot.gameObject;
            inventoryGrid.Grid.OnValidate();
            inventoryGrid.SetName(m_ItemViewSlotContainerName.value);
            inventoryGrid.gameObject.name = m_ItemViewSlotContainerName.value;

            var itemContainerPanelBinding = displayPanel.gameObject.AddComponent<ItemViewSlotsContainerPanelBinding>();
            itemContainerPanelBinding.ItemViewSlotsContainer = inventoryGrid;
            if (inventory != null) {
                itemContainerPanelBinding.Inventory = inventory;
                if (inventory.GetComponent<InventoryGridIndexData>() == null) {
                    inventory.gameObject.AddComponent<InventoryGridIndexData>();
                }
            }

            return inventoryGrid;
        }

        public InventoryGrid CreateShopInventory(RectTransform rectParent)
        {
            var displayPanel = UIDesignerManager.InstantiateSchemaPrefab<DisplayPanel>(
                UIDesignerSchema.GetPanelPrefab(InventoryGridDisplayPanelOption.Basic), rectParent);

            var panelName = "Shop Inventory Grid Panel";
            displayPanel.SetName(panelName);
            displayPanel.gameObject.name = panelName;

            var inventoryGrid = UIDesignerManager.InstantiateSchemaPrefab<InventoryGrid>(UIDesignerSchema.InventoryGrid, displayPanel.MainContent);
            var drawer = inventoryGrid.gameObject.GetComponent<ItemViewDrawer>();

            drawer.CategoryItemViewSet = UIDesignerSchema.ShopCategoryItemViewSet;
            //Default Grid Size for shop
            inventoryGrid.Grid.GridSize = new Vector2Int(1, 5);

            var layoutGroup = inventoryGrid.Grid.GridEventSystem.Content.GetComponent<GridLayoutGroup>();
            if (layoutGroup != null) {
                layoutGroup.cellSize = new Vector2(550, 120);
            }

            inventoryGrid.Grid.GridEventSystem.ButtonPrefab = UIDesignerSchema.ItemViewSlot.gameObject;
            inventoryGrid.Grid.OnValidate();

            var inventoryGridName = "Shop Inventory Grid";
            inventoryGrid.SetName(inventoryGridName);
            inventoryGrid.gameObject.name = inventoryGridName;

            var itemContainerPanelBinding = displayPanel.gameObject.AddComponent<ItemViewSlotsContainerPanelBinding>();
            itemContainerPanelBinding.ItemViewSlotsContainer = inventoryGrid;

            inventoryGrid.m_UseGridIndex = false;

            return inventoryGrid;
        }
    }

    public class InventoryGridBuilderEditor : UIDesignerEditor<InventoryGrid>
    {
        public override string Title => "Inventory Grid Editor";
        public override string Description => "Edit an existing inventory grid. Any changes made will take effect immediately.";

        protected DisplayPanelOptions m_DisplayPanelOptions;
        protected LayoutGroupOption m_LayoutGroupOption;
        protected GridNavigationOptions m_GridNavigationOptions;
        protected InventoryGridTabOptions m_InventoryGridTabsOptions;
        protected FiltersAndSorterOptions m_FiltersAndSorterOptions;
        protected ItemViewSlotsContainerOptions m_CommonItemViewSlotsContainerOptions;

        public InventoryGridBuilderEditor()
        {

            m_DisplayPanelOptions = new DisplayPanelOptions();

            m_LayoutGroupOption = new LayoutGroupOption();

            m_GridNavigationOptions = new GridNavigationOptions();

            m_InventoryGridTabsOptions = new InventoryGridTabOptions(this);

            m_FiltersAndSorterOptions = new FiltersAndSorterOptions(this);

            m_CommonItemViewSlotsContainerOptions = new ItemViewSlotsContainerOptions();

        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();

            m_TargetOptionsContainer.Add(m_DisplayPanelOptions);

            m_TargetOptionsContainer.Add(m_LayoutGroupOption);
            m_LayoutGroupOption.Refresh(m_Target?.Grid);

            m_TargetOptionsContainer.Add(m_GridNavigationOptions);
            m_GridNavigationOptions.Refresh(m_Target?.Grid);

            m_TargetOptionsContainer.Add(m_FiltersAndSorterOptions);
            m_FiltersAndSorterOptions.Refresh(m_Target);

            m_TargetOptionsContainer.Add(m_InventoryGridTabsOptions);

            m_TargetOptionsContainer.Add(m_CommonItemViewSlotsContainerOptions);

            m_DisplayPanelOptions.Refresh(m_DisplayPanel);

            m_CommonItemViewSlotsContainerOptions.Refresh(m_Target);
            m_InventoryGridTabsOptions.Refresh(m_Target);


        }
    }

    public abstract class InventoryGridEditorOption : UIDesignerBoxBase
    {
        protected InventoryGridBuilderEditor m_InventoryGridEditor;

        protected InventoryGrid m_InventoryGrid;
        protected DisplayPanel DisplayPanel => m_InventoryGridEditor.DisplayPanel;

        protected InventoryGridEditorOption(InventoryGridBuilderEditor gridEditor)
        {
            m_InventoryGridEditor = gridEditor;
        }

        public virtual void Refresh(InventoryGrid inventoryGrid)
        {
            m_InventoryGrid = inventoryGrid;
        }
    }

    public class InventoryGridTabOptions : InventoryGridEditorOption
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/inventory-grid/";
        public override string Title => "Grid Tabs";

        public override string Description =>
            "Add tabs which will filter the grid such that it can show different items in each tab.";
        public override Func<Component> SelectTargetGetter => () => m_TabControl;

        protected VisualElement m_Container;
        protected Button m_CreateTab;
        protected TabControl m_TabControl;
        protected InventoryGridTabControlBinding m_TabControlBinding;
        protected List<TabToggle> m_TabToggles;
        protected ReorderableList m_TabsReorderableList;
        protected VisualElement m_TabSelectedContainer;
        protected FiltersField m_FiltersField;
        protected ItemInfoMultiFilterSorter m_MultiFilterSorter;

        protected int m_PreviouslySelectedIndex = -1;

        public InventoryGridTabOptions(InventoryGridBuilderEditor gridEditor) : base(gridEditor)
        {

            m_TabToggles = new List<TabToggle>();

            m_TabsReorderableList = new ReorderableList(
                m_TabToggles,
                (parent, index) =>
                {
                    var listElement = new Label("New");
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as Label;

                    if (index >= m_TabToggles.Count) {
                        Debug.LogWarning("Index " + index + " does not exit.");
                        return;
                    }

                    listElement.text = m_TabToggles[index].GetText();
                }, (parent) =>
                {
                    parent.Add(new Label("Inventory Tab Toggles"));
                },
                (index) =>
                {
                    HandleTabSelected(index);
                }, () =>
                {
                    AddTab();
                }, (index) =>
                {
                    RemoveTab(index);
                }, (i1, i2) =>
                {
                    m_TabToggles[i2].transform.SetSiblingIndex(i2);
                    Refresh(m_InventoryGridEditor.Target);
                });

            m_Container = new VisualElement();
            m_TabSelectedContainer = new VisualElement();

            Add(m_Container);
        }

        public override void Refresh(InventoryGrid inventoryGrid)
        {
            m_Container.Clear();
            m_TabSelectedContainer.Clear();

            base.Refresh(inventoryGrid);

            if (inventoryGrid == null) { return; }

            m_TabControlBinding = m_InventoryGrid.GetComponent<InventoryGridTabControlBinding>();
            m_TabControl = m_TabControlBinding != null ? m_TabControlBinding.m_TabControl : null;

            if (m_TabControl == null) {
                m_CreateTab = CreateButton("Create Inventory Tabs", m_Container, BuildInventoryTabs);
                return;
            }

            m_TabToggles.Clear();
            m_TabToggles.AddRange(m_TabControl.m_Content.GetComponentsInChildren<TabToggle>());

            m_TabsReorderableList.Refresh();

            m_CreateTab = CreateButton("Delete Inventory Tabs", m_Container, DeleteInventoryTabs);

            m_Container.Add(m_TabsReorderableList);
            m_Container.Add(m_TabSelectedContainer);

            HandleTabSelected(m_PreviouslySelectedIndex);
        }

        private void HandleTabSelected(int index)
        {
            m_TabSelectedContainer.Clear();
            m_PreviouslySelectedIndex = index;

            if (index < 0 || index >= m_TabToggles.Count) {
                return;
            }

            var selectedTab = m_TabToggles[index];

            var tabSelect = new ComponentSelectionButton("Select Tab Toggle", () => selectedTab);
            tabSelect.SelectInHierarchy();
            m_TabSelectedContainer.Add(tabSelect);

            var tabName = new UnicodeTextField("Tab Name");
            tabName.value = selectedTab.GetText();
            tabName.RegisterValueChangedCallback(evt =>
            {
                selectedTab.name = "Tab Toggle: " + evt.newValue;
                selectedTab.SetText(evt.newValue);
                var listElement = m_TabsReorderableList.ListItems[m_PreviouslySelectedIndex].ItemContents.ElementAt(0) as Label;
                listElement.text = evt.newValue;
                Shared.Editor.Utility.EditorUtility.SetDirty(selectedTab);
                Shared.Editor.Utility.EditorUtility.SetDirty(selectedTab.gameObject);
            });
            m_TabSelectedContainer.Add(tabName);

            m_MultiFilterSorter = selectedTab.GetComponent<InventoryTabData>()?.m_ItemInfoFilter as ItemInfoMultiFilterSorter;

            m_FiltersField = new FiltersField("Tab Filters",
                () => m_MultiFilterSorter,
                () => m_MultiFilterSorter?.m_GridFilters,
                () =>
                {
                    m_MultiFilterSorter = selectedTab.gameObject.AddComponent<ItemInfoMultiFilterSorter>();
                    var inventoryTabData = selectedTab.GetComponent<InventoryTabData>();
                    if (inventoryTabData == null) {
                        inventoryTabData = selectedTab.gameObject.AddComponent<InventoryTabData>();
                    }

                    inventoryTabData.m_ItemInfoFilter = m_MultiFilterSorter;
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
            m_TabSelectedContainer.Add(m_FiltersField);
        }

        private void AddTab()
        {
            var tabToggle = UIDesignerManager.InstantiateSchemaPrefab<TabToggle>(UIDesignerSchema.TabToggle, m_TabControl.m_Content);
            tabToggle.gameObject.AddComponent<InventoryTabData>();
            m_TabToggles.Add(tabToggle);
            m_TabsReorderableList.Refresh(m_TabToggles);
        }

        private void RemoveTab(int index)
        {
            if (index < 0 || index >= m_TabToggles.Count) { return; }

            var tab = m_TabToggles[index];
            DestroyGameObject(tab);

            m_TabToggles.RemoveAt(index);
            m_TabsReorderableList.Refresh(m_TabToggles);
        }

        private void BuildInventoryTabs()
        {
            var tabsParent = CreateRectTransform(DisplayPanel.MainContent);
            m_TabControl = m_InventoryGrid.gameObject.AddComponent<TabControl>();

            tabsParent.name = "Tab Control";
            tabsParent.anchorMin = new Vector2(0.5f, 1);
            tabsParent.anchorMax = new Vector2(0.5f, 1);
            tabsParent.pivot = new Vector2(0.5f, 1);
            tabsParent.anchoredPosition = new Vector2(0, 0);

            tabsParent.sizeDelta = new Vector2(500, 100);

            var layoutGroup = tabsParent.gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;

            m_TabControl.m_Content = tabsParent;
            m_InventoryGrid.Grid.m_TabControl = m_TabControl;

            m_TabControlBinding = m_InventoryGrid.gameObject.AddComponent<InventoryGridTabControlBinding>();
            m_TabControlBinding.m_TabControl = m_TabControl;

            AddTab();
            AddTab();
            AddTab();

            m_InventoryGridEditor.Refresh();

        }

        private void DeleteInventoryTabs()
        {
            m_TabControlBinding = m_TabControl.GetComponent<InventoryGridTabControlBinding>();
            RemoveComponent(m_TabControlBinding);
            DestroyGameObject(m_TabControl.m_Content);
            RemoveComponent(m_TabControl);

            m_InventoryGridEditor.Refresh();
        }
    }

    public class FiltersField : VisualElement
    {
        protected Func<Component> m_GetTarget;
        protected Func<IReadOnlyList<ItemInfoFilterSorterBase>> m_GetFilters;
        protected Action m_CreateMainFilter;
        protected Action m_DeleteMainFilter;
        protected Action<ItemInfoFilterSorterBase> m_AddFilter;
        protected Action<int> m_RemoveFilter;

        private string m_Title;

        protected CreateSelectDeleteContainer m_CreateSelectDeleteContainer;

        protected List<FilterSorter> m_FilterList;
        protected ReorderableList m_ReorderableList;

        public FiltersField(string title, Func<Component> getTarget, Func<IReadOnlyList<ItemInfoFilterSorterBase>> getFilters, Action onCreate, Action onDelete, Action<ItemInfoFilterSorterBase> onAdd, Action<int> onRemove)
        {
            m_Title = title;
            m_GetTarget = getTarget;
            m_GetFilters = getFilters;
            m_CreateMainFilter = onCreate;
            m_DeleteMainFilter = onDelete;
            m_AddFilter = onAdd;
            m_RemoveFilter = onRemove;

            m_CreateSelectDeleteContainer = new CreateSelectDeleteContainer(title, CreateFilters, DeleteFilters, m_GetTarget);

            m_FilterList = new List<FilterSorter>();

            m_ReorderableList = new ReorderableList(m_FilterList,
                (container, index) =>
                {
                    container.Add(new Label("Missing"));
                },
                (container, index) =>
                {
                    var label = container.ElementAt(0) as Label;
                    label.text = m_FilterList[index]?.ToString() ?? "NULL";
                },
                (container) =>
                {
                    container.Add(new Label(m_Title));
                },
                (index) =>
                {
                    ComponentSelection.Select(m_FilterList[index]);
                }, () =>
                {
                    InspectorUtility.AddObjectType(typeof(ItemInfoFilterSorterBase),
                        (type) =>
                        {
                            if (type == typeof(ItemInfoMultiFilterSorter) || type == typeof(InventorySearchFilter) ||
                                type == typeof(InventoryGridSorterDropDown)) { return false; }
                            return true;
                        },
                        (evt) =>
                        {
                            var target = m_GetTarget();
                            if (target == null) { return; }

                            var newGridFilter = target.gameObject.AddComponent(evt as Type) as ItemInfoFilterSorterBase;

                            m_FilterList.Add(newGridFilter);
                            m_AddFilter?.Invoke(newGridFilter);

                            Refresh();
                            m_ReorderableList.SelectedIndex = m_FilterList.Count - 1;
                        });
                }, (index) =>
                {
                    var element = m_FilterList[index];

                    m_FilterList.RemoveAt(index);
                    m_RemoveFilter?.Invoke(index);

                    UIDesignerUtility.RemoveComponent(element);
                },
                (i1, i2) =>
                {
                    //reorder
                }
            );

            Refresh();
        }

        private void CreateFilters()
        {
            m_CreateMainFilter?.Invoke();
            Refresh();
        }

        private void DeleteFilters()
        {
            m_DeleteMainFilter?.Invoke();
            Refresh();
        }

        public void DeleteMultiFilter(ItemInfoMultiFilterSorter multiFilterSorter)
        {
            if (multiFilterSorter == null) {
                return;
            }

            for (int i = 0; i < multiFilterSorter.m_GridFilters.Count; i++) {
                UIDesignerUtility.RemoveComponent(multiFilterSorter.m_GridFilters[i]);
            }

            UIDesignerUtility.RemoveComponent(multiFilterSorter);
        }

        public void Refresh()
        {
            Clear();
            m_FilterList.Clear();
            m_CreateSelectDeleteContainer.Refresh();

            Add(m_CreateSelectDeleteContainer);

            if (m_CreateSelectDeleteContainer.HasTarget == false) {
                return;
            }

            var list = m_GetFilters.Invoke();

            if (list != null) {
                m_FilterList.AddRange(list);
            }

            m_ReorderableList.Refresh(m_FilterList);
            Add(m_ReorderableList);
        }
    }

    public class FiltersAndSorterOptions : InventoryGridEditorOption
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/inventory-grid/";

        public override string Title => "Filters and Sorters";
        public override string Description => "Filters and sorters can be added on the grid directly or swapped dynamically at runtime.\n" +
                                              "Default, search, and sort filters can be set for dynamic user interaction.\n" +
                                              "Changes are applied immediately.";

        protected FiltersField m_DefaultFilterSorters;
        protected ItemInfoMultiFilterSorter m_DefaultMultiFilter;

        protected FiltersField m_NameSearchBarFilterField;
        protected InventorySearchFilter m_InventorySearchFilter;
        protected ItemInfoMultiFilterSorter m_InventorySearchMultiFilter;

        protected FiltersField m_SorterDropDownField;
        protected InventoryGridSorterDropDown m_InventoryGridSorterDropDown;

        protected VisualElement m_Container;

        public FiltersAndSorterOptions(InventoryGridBuilderEditor gridEditor) : base(gridEditor)
        {
            m_Container = new VisualElement();

            Add(m_Container);

            m_DefaultFilterSorters = new FiltersField("Default Filters",
                () => m_DefaultMultiFilter,
                () => m_DefaultMultiFilter?.m_GridFilters,
                () =>
                {
                    m_DefaultMultiFilter = m_InventoryGrid.gameObject.AddComponent<ItemInfoMultiFilterSorter>();

                    m_InventoryGrid.Grid.m_FilterSorterBase = m_DefaultMultiFilter;
                },
                () =>
                {
                    if (m_DefaultMultiFilter == null) { return; }

                    if (m_DefaultMultiFilter.m_GridFilters != null) {
                        for (int i = 0; i < m_DefaultMultiFilter.m_GridFilters.Count; i++) {
                            UIDesignerUtility.RemoveComponent(m_DefaultMultiFilter.m_GridFilters[i]);
                        }
                    }

                    UIDesignerUtility.RemoveComponent(m_DefaultMultiFilter);
                },
                (filter) =>
                {
                    if (m_DefaultMultiFilter.m_GridFilters == null) {
                        m_DefaultMultiFilter.m_GridFilters = new List<ItemInfoFilterSorterBase>();
                    }

                    m_DefaultMultiFilter.m_GridFilters.Add(filter);
                },
                (indexToRemove) =>
                {
                    m_DefaultMultiFilter.m_GridFilters.RemoveAt(indexToRemove);
                });

            m_NameSearchBarFilterField = new FiltersField("Name Search Bar",
                () => m_InventorySearchFilter,
                () => m_InventorySearchMultiFilter?.m_GridFilters,
                () =>
                {
                    m_InventorySearchFilter =
                        UIDesignerManager.InstantiateSchemaPrefab<InventorySearchFilter>(
                            UIDesignerManager.UIDesignerSchema.GridSearchBar, DisplayPanel.MainContent);
                    m_InventorySearchMultiFilter = m_InventorySearchFilter.gameObject.AddComponent<ItemInfoMultiFilterSorter>();

                    m_InventorySearchFilter.m_InventoryGrid = m_InventoryGrid;
                    m_InventorySearchFilter.m_BindSorterWhileSearching = m_InventorySearchMultiFilter;
                },
                () =>
                {
                    if (m_InventorySearchFilter == null) { return; }

                    DestroyGameObject(m_InventorySearchFilter);
                },
                (filter) =>
                {
                    if (m_InventorySearchMultiFilter.m_GridFilters == null) {
                        m_InventorySearchMultiFilter.m_GridFilters = new List<ItemInfoFilterSorterBase>();
                    }

                    m_InventorySearchMultiFilter.m_GridFilters.Add(filter);
                },
                (indexToRemove) =>
                {
                    m_InventorySearchMultiFilter.m_GridFilters.RemoveAt(indexToRemove);
                });

            m_SorterDropDownField = new FiltersField("Sort Drop Down",
                () => m_InventoryGridSorterDropDown,
                () => m_InventoryGridSorterDropDown?.m_GridSorters,
                () =>
                {
                    m_InventoryGridSorterDropDown =
                        UIDesignerManager.InstantiateSchemaPrefab<InventoryGridSorterDropDown>(
                            UIDesignerManager.UIDesignerSchema.GridSortDropDown, DisplayPanel.MainContent);

                    m_InventoryGridSorterDropDown.m_InventoryGrid = m_InventoryGrid;
                },
                () =>
                {
                    DestroyGameObject(m_InventoryGridSorterDropDown);
                },
                (filter) =>
                {
                    if (m_InventoryGridSorterDropDown.m_GridSorters == null) {
                        m_InventoryGridSorterDropDown.m_GridSorters = new List<ItemInfoSorterBase>();
                    }

                    m_InventoryGridSorterDropDown.m_GridSorters.Add(filter as ItemInfoSorterBase);
#if TEXTMESH_PRO_PRESENT
                    m_InventoryGridSorterDropDown.m_Dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(filter.ToString()));
#endif
                },
                (indexToRemove) =>
                {
                    m_InventoryGridSorterDropDown.m_GridSorters.RemoveAt(indexToRemove);
                    //remove at index +1, first index is "Sort"
                    m_InventoryGridSorterDropDown.m_Dropdown.options.RemoveAt(indexToRemove + 1);
                });

        }

        public override void Refresh(InventoryGrid inventoryGrid)
        {

            m_Container.Clear();

            base.Refresh(inventoryGrid);
            if (inventoryGrid == null) {
                return;
            }

            if (inventoryGrid.Grid == null) {
                Debug.LogError("The Inventory Grid does not have an Item Info Grid");
                return;
            }

            m_DefaultMultiFilter = m_InventoryGrid.Grid.m_FilterSorterBase as ItemInfoMultiFilterSorter;
            m_InventoryGridSorterDropDown = DisplayPanel.GetComponentInChildren<InventoryGridSorterDropDown>();
            m_InventorySearchFilter = DisplayPanel.GetComponentInChildren<InventorySearchFilter>();
            m_InventorySearchMultiFilter = m_InventorySearchFilter?.m_BindSorterWhileSearching as ItemInfoMultiFilterSorter;

            m_DefaultFilterSorters.Refresh();
            m_NameSearchBarFilterField.Refresh();
            m_SorterDropDownField.Refresh();

            m_Container.Add(new SubTitleLabel("Default Filters"));
            m_Container.Add(m_DefaultFilterSorters);

            m_Container.Add(new SubTitleLabel("Name Search bar Filter"));
            m_Container.Add(m_NameSearchBarFilterField);

            m_Container.Add(new SubTitleLabel("Drop Down Sorter"));
            m_Container.Add(m_SorterDropDownField);
        }
    }


    public static class RectTransformUtility
    {

        public static void MoveCenterRightToCenterDown(RectTransform rect, bool clockwise = true)
        {
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.anchorMin = new Vector2(0.5f, 0f);

            if (clockwise) {
                rect.localEulerAngles = new Vector3(0f, 0f, rect.localEulerAngles.z - 90f);
            } else {
                rect.localEulerAngles = new Vector3(0f, 0f, rect.localEulerAngles.z + 90f);
            }

            rect.sizeDelta = new Vector2(
                Mathf.Abs(rect.sizeDelta.x),
                Mathf.Abs(rect.sizeDelta.y)
            );

            rect.anchoredPosition = new Vector2(
                0f,
                rect.sizeDelta.x / 2f);
        }

        public static void MoveCenterRightToCenterUp(RectTransform rect, bool clockwise = false)
        {
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchorMin = new Vector2(0.5f, 1f);

            if (clockwise) {
                rect.localEulerAngles = new Vector3(0f, 0f, rect.localEulerAngles.z - 90f);
            } else {
                rect.localEulerAngles = new Vector3(0f, 0f, rect.localEulerAngles.z + 90f);
            }


            rect.sizeDelta = new Vector2(
                Mathf.Abs(rect.sizeDelta.x),
                Mathf.Abs(rect.sizeDelta.y)
            );

            rect.anchoredPosition = new Vector2(
                0f,
                -rect.sizeDelta.x / 2f);
        }

        public static void MoveCenterRightToCenterLeft(RectTransform rect)
        {
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.localEulerAngles = new Vector3(0f, 0f, rect.localEulerAngles.z + 180f);

            rect.sizeDelta = new Vector2(
                Mathf.Abs(rect.sizeDelta.x),
                Mathf.Abs(rect.sizeDelta.y)
            );

            rect.anchoredPosition = new Vector2(
                rect.sizeDelta.y / 2f,
                0f);
        }
    }
}