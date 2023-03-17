/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Exchange.Shops;
    using Opsive.UltimateInventorySystem.UI;
    using Opsive.UltimateInventorySystem.UI.Menus.Shop;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public enum ShopDisplayPanelOption
    {
        Basic,
        MainMenu,
    }

    public class ShopDesigner : UIDesignerCreateEditTabContent<
        ShopMenu,
        ShopDesignerCreator,
        ShopDesignerEditor>
    {
        public override string Title => "Shop";
        public override string Description => "Create a shop menu.";
    }

    public class ShopDesignerCreator : UIDesignerCreator<ShopMenu>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/shop/";
        protected EnumField m_PanelOption;

        protected override void CreateOptionsContent(VisualElement container)
        {
            m_PanelOption = new EnumField("Panel Option", ShopDisplayPanelOption.Basic);
            m_PanelOption.RegisterValueChangedCallback(evt => { Refresh(); });
            container.Add(m_PanelOption);
        }

        public override bool BuildCondition(bool logWarnings)
        {
            var result = base.BuildCondition(logWarnings);
            if (result == false) { return false; }

            var panelOption = (ShopDisplayPanelOption)m_PanelOption.value;
            if (panelOption == ShopDisplayPanelOption.MainMenu) {
                var rectParent = m_ParentTransform.value as RectTransform;
                var mainMenu = rectParent.gameObject.GetComponentInParent<MainMenu>(true);
                if (mainMenu == null || mainMenu.DisplayPanel.MainContent != rectParent) {
                    m_ConditionHelpBox.SetMessage("The parent transform must be the main menu main content when creating a main menu inner panel.");
                    return false;
                }
            }

            return true;
        }

        protected override ShopMenu BuildInternal()
        {
            var rectParent = m_ParentTransform.value as RectTransform;

            var panelOption = (ShopDisplayPanelOption)m_PanelOption.value;
            var shopMenu = UIDesignerManager.InstantiateSchemaPrefab<ShopMenu>(UIDesignerSchema.ShopMenu, rectParent);

            if (panelOption == ShopDisplayPanelOption.MainMenu) {
                shopMenu.DisplayPanel.m_IsMenuPanel = false;
                shopMenu.DisplayPanel.m_SetDisableOnClose = false;
                UIDesignerManager.GetTab<MainMenuDesigner>().AddInnerPanel("Shop", shopMenu.DisplayPanel);
            } else {
                shopMenu.DisplayPanel.m_IsMenuPanel = true;
                shopMenu.DisplayPanel.m_SetDisableOnClose = true;
            }

            return shopMenu;
        }
    }

    public class ShopDesignerEditor : UIDesignerEditor<ShopMenu>
    {
        private ShopOptions m_ShopOptions;

        private ShopInventoryGridOptions m_ShopInventoryGridOptions;

        private ShopCurrencyDisplaysOptions m_ShopCurrencyDisplaysOptions;

        private ShopQuantityPickerPanelOptions m_ShopQuantityPickerPanelOptions;

        public ShopDesignerEditor()
        {
            m_ShopOptions = new ShopOptions(this);

            m_ShopInventoryGridOptions = new ShopInventoryGridOptions(this);

            m_ShopCurrencyDisplaysOptions = new ShopCurrencyDisplaysOptions(this);

            m_ShopQuantityPickerPanelOptions = new ShopQuantityPickerPanelOptions(this);
        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();

            m_ShopOptions.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_ShopOptions);

            m_ShopInventoryGridOptions.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_ShopInventoryGridOptions);

            m_ShopCurrencyDisplaysOptions.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_ShopCurrencyDisplaysOptions);

            m_ShopQuantityPickerPanelOptions.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_ShopQuantityPickerPanelOptions);
        }
    }

    public abstract class ShopMenuEditorOption : UIDesignerBoxBase
    {
        protected ShopDesignerEditor m_ShopMenuEditor;

        protected ShopMenu m_ShopMenu;
        protected VisualElement m_Container;

        protected ShopMenuEditorOption(ShopDesignerEditor menuEditor)
        {
            m_ShopMenuEditor = menuEditor;
            m_Container = new VisualElement();
            Add(m_Container);
        }

        public virtual void Refresh(ShopMenu shopMenu)
        {
            m_ShopMenu = shopMenu;
            m_Container.Clear();
        }
    }

    public class ShopOptions : ShopMenuEditorOption
    {
        public override string Title => "Shop";

        public override string Description =>
            "The Shop component can be added directly in the menu. Multiple Shop components can use the same UI.\n" +
            "Assign the Shop to the Shop Menu in code before opening the Shop Menu in order to use the same shop menu for multiple Shop components.";

        private Shop m_Shop;
        private ShopAddRemoveBinding m_ShopAddRemoveBinding;

        private CreateSelectDeleteContainer m_CreateSelectDeleteShop;
        private ComponentSelectionButton m_SelectShopInventory;
        private CreateSelectDeleteContainer m_CreateSelectDeleteAddRemoveBinding;

        public ShopOptions(ShopDesignerEditor menuEditor) : base(menuEditor)
        {
            m_CreateSelectDeleteShop = new CreateSelectDeleteContainer("Shop", AddRemoveShop, AddRemoveShop, () => m_Shop);

            m_SelectShopInventory = new ComponentSelectionButton("Select Shop Inventory", () => m_Shop.Inventory);

            m_CreateSelectDeleteAddRemoveBinding = new CreateSelectDeleteContainer("  Shop Add Remove Binding",
                AddRemoveBinding,
                AddRemoveBinding,
                () => m_ShopAddRemoveBinding);
        }

        public override void Refresh(ShopMenu shopMenu)
        {
            base.Refresh(shopMenu);
            m_Shop = m_ShopMenu.GetComponent<Shop>();

            m_CreateSelectDeleteShop.Refresh();

            m_Container.Add(m_CreateSelectDeleteShop);

            if (m_CreateSelectDeleteShop.HasTarget == false) {
                return;
            }

            m_Container.Add(m_SelectShopInventory);

            m_ShopAddRemoveBinding = m_Shop.GetComponent<ShopAddRemoveBinding>();

            m_Container.Add(new SubTitleLabel("Shop Add Remove Binding"));

            m_Container.Add(new SubDescriptionLabel(
                "You may use an Shop Add Remove Binding component to dynamically add and remove items sold and bought."));

            m_CreateSelectDeleteAddRemoveBinding.Refresh();

            m_Container.Add(m_CreateSelectDeleteAddRemoveBinding);
        }

        private void AddRemoveShop()
        {
            if (m_Shop != null) {
                //remove
                RemoveComponent(m_Shop.Inventory);
                RemoveComponent(m_Shop);
                return;
            }

            var shop = m_ShopMenu.gameObject.AddComponent<Shop>();
            var inventory = m_ShopMenu.gameObject.AddComponent<Inventory>();

            shop.Inventory = inventory;
            Refresh(m_ShopMenu);
        }

        private void AddRemoveBinding()
        {
            if (m_ShopAddRemoveBinding != null) {
                //remove
                RemoveComponent(m_ShopAddRemoveBinding);
                return;
            }

            m_ShopMenu.gameObject.AddComponent<ShopAddRemoveBinding>();

            Refresh(m_ShopMenu);
        }
    }

    public class ShopInventoryGridOptions : ShopMenuEditorOption
    {
        public override string Title => "Inventory Grid";

        public override string Description =>
            "Use the Inventory Grid tab to edit the Shop Menu Inventory Grid.\n" +
            "Assign the Shop to the Shop Menu in code before opening the Shop Menu in order to use the same shop menu for multiple Shop components.";

        private CreateSelectDeleteContainer m_CreateSelectDeleteInventoryGrid;
        private Button m_EditButton;

        public ShopInventoryGridOptions(ShopDesignerEditor menuEditor) : base(menuEditor)
        {
            m_CreateSelectDeleteInventoryGrid = new CreateSelectDeleteContainer("Inventory Grid",
                CreateInventoryGrid,
                RemoveInventoryGrid,
                () => m_ShopMenu.InventoryGrid);

            m_EditButton = new SubMenuButton("Edit Inventory Grid", EditInventoryGrid);
        }

        private void RemoveInventoryGrid()
        {
            Debug.LogWarning("The Inventory Grid cannot be removed.");
        }

        public override void Refresh(ShopMenu shopMenu)
        {
            base.Refresh(shopMenu);

            m_CreateSelectDeleteInventoryGrid.Refresh();
            m_Container.Add(m_CreateSelectDeleteInventoryGrid);

            if (m_CreateSelectDeleteInventoryGrid.HasTarget == false) {
                return;
            }

            m_Container.Add(m_EditButton);

        }

        private void CreateInventoryGrid()
        {
            var inventoryGridDesigner = UIDesignerManager.GetTab<InventoryGridDesigner>();
            var inventoryGrid = inventoryGridDesigner.DesignerCreator.CreateShopInventory(m_ShopMenu.DisplayPanel.MainContent);

            m_ShopMenu.m_InventoryGrid = inventoryGrid;
        }

        private void EditInventoryGrid()
        {
            var inventoryGridDesigner = UIDesignerManager.GetTab<InventoryGridDesigner>();
            UIDesignerManager.ChangeTab(inventoryGridDesigner);
            inventoryGridDesigner.DesignerEditor.SetTarget(m_ShopMenu.InventoryGrid);
        }
    }

    public class ShopCurrencyDisplaysOptions : ShopMenuEditorOption
    {
        public override string Title => "Currency Displays";

        public override string Description =>
            "Multi Currency Views are used to display amounts of currency.\n" +
            "It is only required to use have a Multi Currency View for the Total Currency Amount.\n" +
            "Use the Currency tab to add a Currency Owner Monitor to show the player currency.";

        private CreateSelectDeleteContainer m_CreateSelectDeleteCurrency;
        private Button m_EditButton;

        public ShopCurrencyDisplaysOptions(ShopDesignerEditor menuEditor) : base(menuEditor)
        {
            m_CreateSelectDeleteCurrency = new CreateSelectDeleteContainer(
                "Total Price UI",
                CreateCurrencyDisplay,
                RemoveCurrencyDisplay,
                () => m_ShopMenu.m_TotalPrice);

            m_EditButton = new SubMenuButton("Edit Total Price UI", EditCurrencyDisplay);
        }

        private void RemoveCurrencyDisplay()
        {
            Debug.LogWarning("Cannot remove Multi Currency View");
        }

        public override void Refresh(ShopMenu shopMenu)
        {
            base.Refresh(shopMenu);

            m_CreateSelectDeleteCurrency.Refresh();
            m_Container.Add(m_CreateSelectDeleteCurrency);

            if (m_CreateSelectDeleteCurrency.HasTarget == false) {
                return;
            }

            m_Container.Add(m_EditButton);
        }

        private void EditCurrencyDisplay()
        {
            UIDesignerManager.GetTab<CurrencyDesigner>().DesignerCreator.CreateMultiCurrencyView(
                m_ShopMenuEditor.Target.GetComponent<RectTransform>(), CurrencyCreatorOptions.MultiCurrencyView);
        }

        private void CreateCurrencyDisplay()
        {
            UIDesignerManager.ChangeTab(UIDesignerManager.GetTab<CurrencyDesigner>());
        }
    }

    public class ShopQuantityPickerPanelOptions : ShopMenuEditorOption
    {
        public override string Title => "Quantity Picker Panel";

        public override string Description =>
            "The Quantity Picker Panel is used to choose an amount to buy and or sell.";

        private CreateSelectDeleteContainer m_CreateSelectDeleteQuantityPicker;

        public ShopQuantityPickerPanelOptions(ShopDesignerEditor menuEditor) : base(menuEditor)
        {
            m_CreateSelectDeleteQuantityPicker = new CreateSelectDeleteContainer("Quantity Picker Panel",
                CreateQuantityPickerPanel,
                RemoveQuantityPickerPanel,
                () => m_ShopMenu.m_QuantityPickerPanel);
        }

        public override void Refresh(ShopMenu shopMenu)
        {
            base.Refresh(shopMenu);

            m_CreateSelectDeleteQuantityPicker.Refresh();

            m_Container.Add(m_CreateSelectDeleteQuantityPicker);

            if (m_CreateSelectDeleteQuantityPicker.HasTarget == false) {
                return;
            }
        }

        private void CreateQuantityPickerPanel()
        {
            var quantityPickerPanel = UIDesignerManager.InstantiateSchemaPrefab<QuantityPickerPanel>(UIDesignerSchema.QuantityPickerPanel,
                m_ShopMenu.DisplayPanel.MainContent);

            m_ShopMenu.m_QuantityPickerPanel = quantityPickerPanel;
        }

        private void RemoveQuantityPickerPanel()
        {
            Debug.LogWarning("The Quantity Picker Panel cannot be removed.");
        }
    }
}