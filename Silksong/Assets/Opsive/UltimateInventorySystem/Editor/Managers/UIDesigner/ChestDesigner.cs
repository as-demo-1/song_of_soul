/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.UltimateInventorySystem.UI.Menus.Chest;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class ChestDesigner : UIDesignerCreateEditTabContent<
        ChestMenu,
        ChestDesignerCreator,
        ChestDesignerEditor>
    {
        public override string Title => "Chest";
        public override string Description => "Create a chest menu.";
    }

    public class ChestDesignerCreator : UIDesignerCreator<ChestMenu>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/ui-designer/chest/";

        protected override void CreateOptionsContent(VisualElement container)
        { }

        protected override ChestMenu BuildInternal()
        {
            var rectParent = m_ParentTransform.value as RectTransform;

            var chestMenu = UIDesignerManager.InstantiateSchemaPrefab<ChestMenu>(UIDesignerSchema.ChestMenu, rectParent);

            chestMenu.DisplayPanel.m_IsMenuPanel = true;
            chestMenu.DisplayPanel.m_SetDisableOnClose = true;

            return chestMenu;
        }
    }

    public class ChestDesignerEditor : UIDesignerEditor<ChestMenu>
    {
        private ChestInventoryGridOptions m_InventoryGridOptions;

        private ChestQuantityPickerPanelOptions m_QuantityPickerPanelOptions;

        public ChestDesignerEditor()
        {
            m_InventoryGridOptions = new ChestInventoryGridOptions(this);

            m_QuantityPickerPanelOptions = new ChestQuantityPickerPanelOptions(this);
        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();

            m_InventoryGridOptions.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_InventoryGridOptions);

            m_QuantityPickerPanelOptions.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_QuantityPickerPanelOptions);
        }
    }

    public abstract class ChestMenuEditorOption : UIDesignerBoxBase
    {
        protected ChestDesignerEditor m_ChestMenuEditor;

        protected ChestMenu m_ChestMenu;
        protected VisualElement m_Container;

        protected ChestMenuEditorOption(ChestDesignerEditor menuEditor)
        {
            m_ChestMenuEditor = menuEditor;
            m_Container = new VisualElement();
            Add(m_Container);
        }

        public virtual void Refresh(ChestMenu chestMenu)
        {
            m_ChestMenu = chestMenu;
            m_Container.Clear();
        }
    }

    public class ChestInventoryGridOptions : ChestMenuEditorOption
    {
        public override string Title => "Inventory Grid";

        public override string Description =>
            "Use the Inventory Grid tab to edit the Chest Inventory Grid.\n" +
            "Assign the Chest Inventory to the Chest Menu in code before opening the Chest Menu in order to use the same Chest Menu for multiple Inventory components.";

        private CreateSelectDeleteContainer m_CreateSelectDeleteInventoryGrid;
        private Button m_EditButton;

        public ChestInventoryGridOptions(ChestDesignerEditor menuEditor) : base(menuEditor)
        {
            m_CreateSelectDeleteInventoryGrid = new CreateSelectDeleteContainer("Chest Inventory Grid",
                CreateInventoryGrid,
                RemoveInventoryGrid,
                () => m_ChestMenu.InventoryGrid);

            m_EditButton = new SubMenuButton("Edit Chest Inventory Grid", EditInventoryGrid);
        }

        private void RemoveInventoryGrid()
        {
            Debug.LogWarning("The Inventory Grid cannot be removed.");
        }

        public override void Refresh(ChestMenu chestMenu)
        {
            base.Refresh(chestMenu);

            m_CreateSelectDeleteInventoryGrid.Refresh();
            m_Container.Add(m_CreateSelectDeleteInventoryGrid);

            if (m_CreateSelectDeleteInventoryGrid.HasTarget) {
                m_Container.Add(m_EditButton);
            }

        }

        private void CreateInventoryGrid()
        {
            var inventoryGridDesigner = UIDesignerManager.GetTab<InventoryGridDesigner>();
            var inventoryGrid = inventoryGridDesigner.DesignerCreator.CreateShopInventory(m_ChestMenu.DisplayPanel.MainContent);


            m_ChestMenu.m_InventoryGrid = inventoryGrid;

        }

        private void EditInventoryGrid()
        {
            var inventoryGridDesigner = UIDesignerManager.GetTab<InventoryGridDesigner>();
            UIDesignerManager.ChangeTab(inventoryGridDesigner);

            inventoryGridDesigner.DesignerEditor.SetTarget(m_ChestMenu.InventoryGrid);

        }
    }

    public class ChestQuantityPickerPanelOptions : ChestMenuEditorOption
    {
        public override string Title => "Quantity Picker Panel";

        public override string Description =>
            "The Quantity Picker Panel is used to choose an amount to take from the chest.";

        private CreateSelectDeleteContainer m_CreateSelectDeleteQuantityPicker;

        public ChestQuantityPickerPanelOptions(ChestDesignerEditor menuEditor) : base(menuEditor)
        {
            m_CreateSelectDeleteQuantityPicker = new CreateSelectDeleteContainer("Quantity Picker Panel",
                CreateQuantityPickerPanel,
                RemoveQuantityPickerPanel,
                () => m_ChestMenu.m_QuantityPickerPanel);
        }

        public override void Refresh(ChestMenu chestMenu)
        {
            base.Refresh(chestMenu);

            m_CreateSelectDeleteQuantityPicker.Refresh();

            m_Container.Add(m_CreateSelectDeleteQuantityPicker);

            if (m_CreateSelectDeleteQuantityPicker.HasTarget == false) {
                return;
            }
        }

        private void CreateQuantityPickerPanel()
        {
            var quantityPickerPanel = UIDesignerManager.InstantiateSchemaPrefab<QuantityPickerPanel>(UIDesignerSchema.QuantityPickerPanel,
                m_ChestMenu.DisplayPanel.MainContent);

            m_ChestMenu.m_QuantityPickerPanel = quantityPickerPanel;
        }

        private void RemoveQuantityPickerPanel()
        {
            Debug.LogWarning("The Quantity Picker Panel cannot be removed.");
        }
    }
}