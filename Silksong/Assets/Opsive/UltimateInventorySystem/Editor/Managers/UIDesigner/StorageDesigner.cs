/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.UltimateInventorySystem.Demo.UI.Menus.Storage;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class StorageDesigner : UIDesignerCreateEditTabContent<
        StorageMenu,
        StorageDesignerCreator,
        StorageDesignerEditor>
    {
        public override string Title => "Storage";
        public override string Description => "Create a storage menu.";
    }

    public class StorageDesignerCreator : UIDesignerCreator<StorageMenu>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/ui-designer/storage/";

        protected override void CreateOptionsContent(VisualElement container)
        { }

        protected override StorageMenu BuildInternal()
        {
            var rectParent = m_ParentTransform.value as RectTransform;

            var storageMenu = UIDesignerManager.InstantiateSchemaPrefab<StorageMenu>(UIDesignerSchema.StorageMenu, rectParent);

            storageMenu.DisplayPanel.m_IsMenuPanel = true;
            storageMenu.DisplayPanel.m_SetDisableOnClose = true;

            return storageMenu;
        }
    }

    public class StorageDesignerEditor : UIDesignerEditor<StorageMenu>
    {
        private StorageInventoryGridOptions m_ShopInventoryGridOptions;

        private StorageQuantityPickerPanelOptions m_ShopQuantityPickerPanelOptions;

        public StorageDesignerEditor()
        {
            m_ShopInventoryGridOptions = new StorageInventoryGridOptions(this);

            m_ShopQuantityPickerPanelOptions = new StorageQuantityPickerPanelOptions(this);
        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();

            m_ShopInventoryGridOptions.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_ShopInventoryGridOptions);

            m_ShopQuantityPickerPanelOptions.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_ShopQuantityPickerPanelOptions);
        }
    }

    public abstract class StorageMenuEditorOption : UIDesignerBoxBase
    {
        protected StorageDesignerEditor m_StorageMenuEditor;

        protected StorageMenu m_StorageMenu;
        protected VisualElement m_Container;

        protected StorageMenuEditorOption(StorageDesignerEditor menuEditor)
        {
            m_StorageMenuEditor = menuEditor;
            m_Container = new VisualElement();
            Add(m_Container);
        }

        public virtual void Refresh(StorageMenu storageMenu)
        {
            m_StorageMenu = storageMenu;
            m_Container.Clear();
        }
    }

    public class StorageInventoryGridOptions : StorageMenuEditorOption
    {
        public override string Title => "Inventory Grid";

        public override string Description =>
            "Use the Inventory Grid tab to edit the Storage and Client(Player) Inventory Grids.\n" +
            "Assign the Storage Inventory to the Storage Menu in code before opening the Storage Menu in order to use the same Storage Menu for multiple Inventory components.";

        private CreateSelectDeleteContainer m_CreateSelectDeleteClientInventoryGrid;
        private CreateSelectDeleteContainer m_CreateSelectDeleteStorageInventoryGrid;
        private Button m_EditStorageButton;
        private Button m_EditClientButton;

        public StorageInventoryGridOptions(StorageDesignerEditor menuEditor) : base(menuEditor)
        {
            m_CreateSelectDeleteStorageInventoryGrid = new CreateSelectDeleteContainer("Storage Inventory Grid",
                () => CreateInventoryGrid(true),
                RemoveInventoryGrid,
                () => m_StorageMenu.StorageInventoryGrid);

            m_EditStorageButton = new SubMenuButton("Edit Storage Inventory Grid", () => EditInventoryGrid(true));

            m_CreateSelectDeleteClientInventoryGrid = new CreateSelectDeleteContainer("Client Inventory Grid",
                () => CreateInventoryGrid(false),
                RemoveInventoryGrid,
                () => m_StorageMenu.ClientInventoryGrid);

            m_EditClientButton = new SubMenuButton("Edit Client Inventory Grid", () => EditInventoryGrid(false));
        }

        private void RemoveInventoryGrid()
        {
            Debug.LogWarning("The Inventory Grid cannot be removed.");
        }

        public override void Refresh(StorageMenu storageMenu)
        {
            base.Refresh(storageMenu);

            m_Container.Add(new SubTitleLabel("Storage Inventory Grid"));

            m_CreateSelectDeleteStorageInventoryGrid.Refresh();
            m_Container.Add(m_CreateSelectDeleteStorageInventoryGrid);

            if (m_CreateSelectDeleteStorageInventoryGrid.HasTarget) {
                m_Container.Add(m_EditStorageButton);
            }

            m_Container.Add(new SubTitleLabel("Client Inventory Grid"));

            m_CreateSelectDeleteClientInventoryGrid.Refresh();
            m_Container.Add(m_CreateSelectDeleteClientInventoryGrid);

            if (m_CreateSelectDeleteClientInventoryGrid.HasTarget) {
                m_Container.Add(m_EditClientButton);
            }

        }

        private void CreateInventoryGrid(bool storage)
        {
            var inventoryGridDesigner = UIDesignerManager.GetTab<InventoryGridDesigner>();
            var inventoryGrid = inventoryGridDesigner.DesignerCreator.CreateShopInventory(m_StorageMenu.DisplayPanel.MainContent);

            if (storage) {
                m_StorageMenu.m_StorageInventoryGrid = inventoryGrid;
            } else {
                m_StorageMenu.m_ClientInventoryGrid = inventoryGrid;
            }
        }

        private void EditInventoryGrid(bool storage)
        {
            var inventoryGridDesigner = UIDesignerManager.GetTab<InventoryGridDesigner>();
            UIDesignerManager.ChangeTab(inventoryGridDesigner);

            if (storage) {
                inventoryGridDesigner.DesignerEditor.SetTarget(m_StorageMenu.StorageInventoryGrid);
            } else {
                inventoryGridDesigner.DesignerEditor.SetTarget(m_StorageMenu.ClientInventoryGrid);
            }

        }
    }

    public class StorageQuantityPickerPanelOptions : StorageMenuEditorOption
    {
        public override string Title => "Quantity Picker Panel";

        public override string Description =>
            "The Quantity Picker Panel is used to choose an amount to exchange one way or the other.";

        private CreateSelectDeleteContainer m_CreateSelectDeleteQuantityPicker;

        public StorageQuantityPickerPanelOptions(StorageDesignerEditor menuEditor) : base(menuEditor)
        {
            m_CreateSelectDeleteQuantityPicker = new CreateSelectDeleteContainer("Quantity Picker Panel",
                CreateQuantityPickerPanel,
                RemoveQuantityPickerPanel,
                () => m_StorageMenu.m_QuantityPickerPanel);
        }

        public override void Refresh(StorageMenu storageMenu)
        {
            base.Refresh(storageMenu);

            m_CreateSelectDeleteQuantityPicker.Refresh();

            m_Container.Add(m_CreateSelectDeleteQuantityPicker);

            if (m_CreateSelectDeleteQuantityPicker.HasTarget == false) {
                return;
            }
        }

        private void CreateQuantityPickerPanel()
        {
            var quantityPickerPanel = UIDesignerManager.InstantiateSchemaPrefab<QuantityPickerPanel>(UIDesignerSchema.QuantityPickerPanel,
                m_StorageMenu.DisplayPanel.MainContent);

            m_StorageMenu.m_QuantityPickerPanel = quantityPickerPanel;
        }

        private void RemoveQuantityPickerPanel()
        {
            Debug.LogWarning("The Quantity Picker Panel cannot be removed.");
        }
    }
}