/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Crafting;
    using Opsive.UltimateInventorySystem.UI;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using Opsive.UltimateInventorySystem.UI.Menus.Crafting;
    using System;
    using System.Collections.Generic;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.UIElements;

    /// <summary>
    /// The crafting Menu Grid Panel Option
    /// </summary>
    public enum CraftingMenuGridDisplayPanelOption
    {
        Basic,
        MainMenu,
    }

    /// <summary>
    /// The Crafting Designer.
    /// </summary>
    public class CraftingDesigner : UIDesignerCreateEditTabContent<
        CraftingMenu,
        CraftingBuilderCreator,
        CraftingDesignerEditor>
    {
        public override string Title => "Crafting";
        public override string Description => "Create a crafting menu using the options below.";
    }

    public class CraftingBuilderCreator : UIDesignerCreator<CraftingMenu>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/crafting/";
        protected EnumField m_PanelOption;


        protected override void CreateOptionsContent(VisualElement container)
        {
            m_PanelOption = new EnumField("Panel Option", CraftingMenuGridDisplayPanelOption.Basic);
            m_PanelOption.RegisterValueChangedCallback(evt => { Refresh(); });
            container.Add(m_PanelOption);
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

        protected override CraftingMenu BuildInternal()
        {
            var rectParent = m_ParentTransform.value as RectTransform;

            var panelOption = (ShopDisplayPanelOption)m_PanelOption.value;
            var craftingMenu = UIDesignerManager.InstantiateSchemaPrefab<CraftingMenu>(UIDesignerSchema.CraftingMenu, rectParent);

            if (panelOption == ShopDisplayPanelOption.MainMenu) {
                craftingMenu.DisplayPanel.m_IsMenuPanel = false;
                craftingMenu.DisplayPanel.m_SetDisableOnClose = false;
                UIDesignerManager.GetTab<MainMenuDesigner>().AddInnerPanel("Crafting", craftingMenu.DisplayPanel);
            } else {
                craftingMenu.DisplayPanel.m_IsMenuPanel = true;
                craftingMenu.DisplayPanel.m_SetDisableOnClose = true;
            }

            return craftingMenu;
        }
    }

    public class CraftingDesignerEditor : UIDesignerEditor<CraftingMenu>
    {
        private CrafterOptions m_CrafterOptions;

        private RecipePanelOptions m_RecipePanelOptions;

        protected LayoutGroupOption m_LayoutGroupOption;
        protected GridNavigationOptions m_GridNavigationOptions;
        protected RecipeGridTabOptions m_RecipeGridTabOptions;

        public CraftingDesignerEditor()
        {

            m_CrafterOptions = new CrafterOptions(this);

            m_RecipePanelOptions = new RecipePanelOptions(this);

            m_LayoutGroupOption = new LayoutGroupOption();

            m_GridNavigationOptions = new GridNavigationOptions();

            m_RecipeGridTabOptions = new RecipeGridTabOptions(this);
        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();

            m_CrafterOptions.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_CrafterOptions);

            m_RecipePanelOptions.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_RecipePanelOptions);

            m_TargetOptionsContainer.Add(m_LayoutGroupOption);
            m_LayoutGroupOption.Refresh(m_Target?.m_CraftingRecipeGrid);

            m_TargetOptionsContainer.Add(m_GridNavigationOptions);
            m_GridNavigationOptions.Refresh(m_Target?.m_CraftingRecipeGrid);

            m_TargetOptionsContainer.Add(m_RecipeGridTabOptions);
            m_RecipeGridTabOptions.Refresh(m_Target);
        }
    }

    public abstract class CraftingMenuEditorOption : UIDesignerBoxBase
    {
        protected CraftingDesignerEditor m_CraftingMenuEditor;

        protected CraftingMenu m_CraftingMenu;
        protected VisualElement m_Container;

        protected CraftingMenuEditorOption(CraftingDesignerEditor menuEditor)
        {
            m_CraftingMenuEditor = menuEditor;
            m_Container = new VisualElement();
            Add(m_Container);
        }

        public virtual void Refresh(CraftingMenu menu)
        {
            m_CraftingMenu = menu;
            m_Container.Clear();
        }
    }

    public class CrafterOptions : CraftingMenuEditorOption
    {
        public override string Title => "Crafter";

        public override string Description =>
            "The Crafter component can be added directly in the menu, or you may have multiple Crafter components that uses the same UI.\n" +
            "Assign the Crafter to the Crafting Menu in code before opening it in order to use the same menu for multiple Crafter components.";

        public override Func<Component> SelectTargetGetter => () => m_Crafter;

        private Crafter m_Crafter;

        private CreateSelectDeleteContainer m_CreateSelectDeleteCrafter;

        public CrafterOptions(CraftingDesignerEditor menuEditor) : base(menuEditor)
        {
            m_CreateSelectDeleteCrafter = new CreateSelectDeleteContainer(
                "Crafter",
                AddRemoveCrafter,
                AddRemoveCrafter,
                () => m_Crafter);
        }

        public override void Refresh(CraftingMenu menu)
        {
            base.Refresh(menu);
            m_Crafter = m_CraftingMenu.GetComponent<Crafter>();

            m_CreateSelectDeleteCrafter.Refresh();
            m_Container.Add(m_CreateSelectDeleteCrafter);

            if (m_CreateSelectDeleteCrafter.HasTarget == false) {
                return;
            }
        }

        private void AddRemoveCrafter()
        {
            if (m_Crafter != null) {
                //remove
                RemoveComponent(m_Crafter);
                return;
            }

            var crafter = m_CraftingMenu.gameObject.AddComponent<Crafter>();

            Refresh(m_CraftingMenu);
        }
    }

    public class RecipePanelOptions : CraftingMenuEditorOption
    {
        public override string Title => "Recipe Panel";

        public override string Description =>
            "The Recipe Panel shows all the descriptions of the selected recipe ingredients and result.\n" +
            "Add or remove ingredient views and descriptions below.";

        public override Func<Component> SelectTargetGetter => () => m_RecipePanel;

        private RecipePanel m_RecipePanel;

        private CreateSelectDeleteContainer m_CreateSelectDeleteRecipePanel;

        List<ItemViewWithDescription> m_IngredientItems;
        private ReorderableList m_ReorderableList;

        public RecipePanelOptions(CraftingDesignerEditor menuEditor) : base(menuEditor)
        {
            m_CreateSelectDeleteRecipePanel = new CreateSelectDeleteContainer("Recipe Panel",
                AddRemoveRecipePanel,
                AddRemoveRecipePanel,
                () => m_RecipePanel);

            m_IngredientItems = new List<ItemViewWithDescription>();
            m_ReorderableList = new ReorderableList(
                m_IngredientItems,
                (parent, index) =>
                {
                    var listElement = new ItemViewWithDescriptionListElement();
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as ItemViewWithDescriptionListElement;

                    if (index >= m_IngredientItems.Count) {
                        Debug.LogWarning("Index " + index + " does not exist.");
                        return;
                    }

                    listElement.Refresh(m_IngredientItems[index]);
                }, (parent) =>
                {
                    parent.Add(new Label("Ingredient Item Views and Descriptions"));
                },
                (index) =>
                {
                    return 35;
                },
                (index) =>
                {
                    //Nothing on select.
                }, () =>
                {
                    var itemView = UIDesignerManager.InstantiateSchemaPrefab<ItemView>(UIDesignerSchema.ItemViewForIngredient, m_RecipePanel.m_IngredientViewParent);
                    var itemDescription = UIDesignerManager.InstantiateSchemaPrefab<ItemDescriptionBase>(UIDesignerSchema.ItemDescriptionSmall, m_RecipePanel.m_IngredientDescriptionParent);

                    var categoryAttributeViewSetItemViewModule = itemDescription.GetComponent<CategoryAttributeViewSetItemViewModule>();

                    if (categoryAttributeViewSetItemViewModule != null) {
                        categoryAttributeViewSetItemViewModule.m_CategoryAttributeViewSet =
                            UIDesignerSchema.CategoryAttributeViewSet;
                    }

                    m_RecipePanel.m_IngredientItems.Add(new ItemViewWithDescription() { ItemView = itemView, ItemDescription = itemDescription });
                    Refresh(m_CraftingMenu);
                }, (index) =>
                {

                    var itemView = m_RecipePanel.m_IngredientItems[index].ItemView;
                    var itemDescription = m_RecipePanel.m_IngredientItems[index].ItemDescription;

                    DestroyGameObject(itemView);
                    DestroyGameObject(itemDescription);
                    m_RecipePanel.m_IngredientItems.RemoveAt(index);
                    Refresh(m_CraftingMenu);
                }, null);
        }

        public override void Refresh(CraftingMenu menu)
        {
            base.Refresh(menu);
            m_RecipePanel = m_CraftingMenu.m_RecipePanel;

            m_CreateSelectDeleteRecipePanel.Refresh();
            m_Container.Add(m_CreateSelectDeleteRecipePanel);

            if (m_CreateSelectDeleteRecipePanel.HasTarget == false) {
                return;
            }

            m_Container.Add(new SubTitleLabel("Add/Remove ingredient views and descriptions."));

            m_Container.Add(new SubDescriptionLabel(
                "You may use an Shop Add Remove Binding component to dynamically add and remove items bought or sold."));

            if (m_RecipePanel.m_IngredientItems == null) {
                m_RecipePanel.m_IngredientItems = new List<ItemViewWithDescription>();
            }

            m_IngredientItems = m_RecipePanel.m_IngredientItems;

            m_ReorderableList.Refresh(m_RecipePanel.m_IngredientItems);
            m_Container.Add(m_ReorderableList);

        }

        private void AddRemoveRecipePanel()
        {
            if (m_RecipePanel != null) {
                //remove
                DestroyGameObject(m_RecipePanel);
                return;
            }

            var recipePanel = UIDesignerManager.InstantiateSchemaPrefab<RecipePanel>(UIDesignerSchema.RecipePanel, m_CraftingMenu.DisplayPanel.MainContent);
            m_CraftingMenu.m_RecipePanel = recipePanel;

            Refresh(m_CraftingMenu);
        }

        public class ItemViewWithDescriptionListElement : VisualElement
        {
            private ItemViewWithDescription m_ItemViewWithDescription;

            private VisualElement m_ItemViewContainer;
            private VisualElement m_ItemDescriptionContainer;

            private ComponentSelectionButton m_SelectItemView;
            private ComponentSelectionButton m_SelectItemDescription;

            public ItemViewWithDescriptionListElement()
            {
                style.flexDirection = FlexDirection.Row;

                m_ItemViewContainer = new VisualElement();
                m_ItemViewContainer.style.flexDirection = FlexDirection.Row;
                Add(m_ItemViewContainer);

                m_ItemDescriptionContainer = new VisualElement();
                m_ItemDescriptionContainer.style.flexDirection = FlexDirection.Row;
                Add(m_ItemDescriptionContainer);

                m_SelectItemView = new ComponentSelectionButton("Select Item View", () => m_ItemViewWithDescription.ItemView);
                m_SelectItemDescription = new ComponentSelectionButton("Select Item Description", () => m_ItemViewWithDescription.ItemDescription);
            }

            public void Refresh(ItemViewWithDescription itemViewWithDescription)
            {
                m_ItemViewWithDescription = itemViewWithDescription;

                m_ItemViewContainer.Clear();
                m_ItemDescriptionContainer.Clear();

                if (itemViewWithDescription.ItemView == null) {
                    m_ItemViewContainer.Add(new Label("Item View is null"));
                } else {
                    m_ItemViewContainer.Add(m_SelectItemView);
                }

                if (itemViewWithDescription.ItemDescription == null) {
                    m_ItemDescriptionContainer.Add(new Label("Item Description is null"));
                } else {
                    m_ItemDescriptionContainer.Add(m_SelectItemDescription);
                }
            }
        }
    }

    public class RecipeGridTabOptions : CraftingMenuEditorOption
    {
        public override string Title => "Grid Tabs";

        public override string Description =>
            "Add tabs which will filter the grid such that it can show different recipes in each tab.";

        public override Func<Component> SelectTargetGetter => () => m_TabControl;

        protected CreateSelectDeleteContainer m_CreateSelectDeleteTabControl;
        protected TabControl m_TabControl;
        protected List<TabToggle> m_TabToggles;
        protected ReorderableList m_TabsReorderableList;
        protected VisualElement m_TabSelectedContainer;

        public RecipeGridTabOptions(CraftingDesignerEditor gridEditor) : base(gridEditor)
        {
            m_CreateSelectDeleteTabControl = new CreateSelectDeleteContainer("Crafting Tabs",
                CreateTabs,
                DeleteTabs,
                () => m_TabControl);


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
                    m_TabToggles[i1].transform.SetSiblingIndex(i2);
                    m_TabToggles[i2].transform.SetSiblingIndex(i1);
                });
            m_TabSelectedContainer = new VisualElement();
        }

        public override void Refresh(CraftingMenu craftingMenu)
        {
            m_Container.Clear();
            m_TabSelectedContainer.Clear();

            base.Refresh(craftingMenu);

            if (craftingMenu == null) { return; }

            m_TabControl = m_CraftingMenu.m_CraftingRecipeGrid.m_TabControl;

            m_CreateSelectDeleteTabControl.Refresh();
            m_Container.Add(m_CreateSelectDeleteTabControl);

            if (m_CreateSelectDeleteTabControl.HasTarget == false) {
                return;
            }

            m_TabToggles.Clear();
            m_TabToggles.AddRange(m_TabControl.m_Content.GetComponentsInChildren<TabToggle>());

            m_Container.Add(m_TabsReorderableList);
            m_Container.Add(m_TabSelectedContainer);
        }

        private void HandleTabSelected(int index)
        {
            m_TabSelectedContainer.Clear();
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
            });
            m_TabSelectedContainer.Add(tabName);

            var categoryTabData = selectedTab.GetComponent<CraftingTabData>();

            if (categoryTabData == null) {
                m_TabSelectedContainer.Add(new Label("Category Tab Data does not exist on tab!"));
            } else {
                var itemCategoryName =
                    categoryTabData.ItemCategory == null ? "NULL" : categoryTabData.ItemCategory.name;
                var craftingCategoryName =
                    categoryTabData.CraftingCategory == null ? "NULL" : categoryTabData.CraftingCategory.name;
                m_TabSelectedContainer.Add(new Label($"Item Category Filter: {itemCategoryName} || Crafting Category Filter: {craftingCategoryName}"));
            }


        }

        private void AddTab()
        {
            var tabToggle = UIDesignerManager.InstantiateSchemaPrefab<TabToggle>(UIDesignerSchema.TabToggle, m_TabControl.m_Content);
            tabToggle.gameObject.AddComponent<CraftingTabData>();
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

        private void CreateTabs()
        {

            var tabsParent = CreateRectTransform(m_CraftingMenuEditor.DisplayPanel.MainContent);
            m_TabControl = tabsParent.gameObject.AddComponent<TabControl>();

            tabsParent.name = "Tab Control";
            tabsParent.anchorMin = new Vector2(0.5f, 1);
            tabsParent.anchorMax = new Vector2(0.5f, 1);
            tabsParent.pivot = new Vector2(0.5f, 1);
            tabsParent.anchoredPosition = new Vector2(0, 0);

            tabsParent.sizeDelta = new Vector2(500, 100);

            var layoutGroup = tabsParent.gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;

            m_TabControl.m_Content = tabsParent;
            m_CraftingMenu.m_CraftingRecipeGrid.m_TabControl = m_TabControl;

            AddTab();
            AddTab();
            AddTab();

            m_CraftingMenuEditor.Refresh();

        }

        private void DeleteTabs()
        {
            DestroyGameObject(m_TabControl.m_Content);
            RemoveComponent(m_TabControl);

            m_CraftingMenuEditor.Refresh();
        }
    }
}