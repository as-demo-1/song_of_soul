/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.UltimateInventorySystem.UI.Monitors;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class InventoryMonitorDesigner : UIDesignerCreateEditTabContent<
        InventoryMonitor,
        InventoryMonitorDesignerCreator,
        InventoryMonitorDesignerEditor>
    {
        public override string Title => "Inventory Monitor";
        public override string Description => "Create an Inventory Monitor.";

    }

    public class InventoryMonitorDesignerCreator : UIDesignerCreator<InventoryMonitor>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/inventory-monitor/";
        public override string Title => "Inventory Monitor Builder";
        public override string Description => "Inventory Monitors pop up items that are added to an inventory to keep the player notified.\n" +
                                              "You can enable/disable the Inventory Monitor component at any time to stop listening to the Inventory.";


        protected override void CreateOptionsContent(VisualElement container)
        { }

        protected override InventoryMonitor BuildInternal()
        {
            var rectParent = m_ParentTransform.value as RectTransform;
            var inventoryMonitor = UIDesignerManager.InstantiateSchemaPrefab<InventoryMonitor>(UIDesignerSchema.InventoryMonitor,
                rectParent);

            inventoryMonitor.m_ItemViewPrefab = UIDesignerSchema.ItemViewForInventoryMonitor.gameObject;

            return inventoryMonitor;
        }
    }

    public class InventoryMonitorDesignerEditor : UIDesignerEditor<InventoryMonitor>
    {
        protected override bool RequireDisplayPanel => false;
    }
}