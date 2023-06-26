/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using System;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class DisplayPanelOptions : UIDesignerBoxBase
    {
        public override string Title => "Display Panel options";
        public override string Description => "Edit common settings on the panel object.";
        public override Func<Component> SelectTargetGetter => () => m_DisplayPanel;

        protected DisplayPanel m_DisplayPanel;
        protected VisualElement m_Container;

        //protected TextField m_PanelName;
        protected ComponentSelectionButton m_SelectInventoryButton;
        //protected ComponentSelectionButton m_SelectPanelButton;

        public DisplayPanel DisplayPanel => m_DisplayPanel;

        public DisplayPanelOptions()
        {
            m_SelectInventoryButton = new ComponentSelectionButton("Select Bound Inventory", GetBoundInventory);

            m_Container = new VisualElement();
            Add(m_Container);
        }

        protected virtual Inventory GetBoundInventory()
        {
            return m_DisplayPanel?.GetComponent<InventoryPanelBinding>()?.Inventory;
        }

        public void Refresh(DisplayPanel displayPanel)
        {
            m_Container.Clear();

            m_DisplayPanel = displayPanel;

            if (m_DisplayPanel == null) {
                m_Container.Add(new Label("DisplayPanel does not found for target."));
                return;
            }


            if (GetBoundInventory() != null) {
                m_Container.Add(m_SelectInventoryButton);
            }
        }
    }
}