/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers;
    using UnityEngine;

    public abstract class InventoryPanelOpener : PanelOpener
    {
        [Tooltip("Find the inventory with this identifier index, 0 is empty.")]
        [SerializeField] protected uint m_ClientInventoryIdentifierIndex;
        [Tooltip("The inventory opening the panel.")]
        [SerializeField] protected Inventory m_ClientInventory;

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force Initialize.</param>
        protected override void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }

            if (m_ClientInventory == null && m_ClientInventoryIdentifierIndex != 0) {
                var identifier = InventorySystemManager.GetInventoryIdentifier(m_ClientInventoryIdentifierIndex);
                if (identifier == null) {
                    Debug.LogError($"The inventory identifier with index {m_ClientInventoryIdentifierIndex}, does not exist");
                    return;
                }

                m_ClientInventory = identifier.Inventory;
            }

            base.Initialize(force);
        }

        /// <summary>
        /// Open the menu using the client inventory field.
        /// </summary>
        public override void Open()
        {
            Open(m_ClientInventory);
        }

        /// <summary>
        /// The inventory trying to open the menu.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        public abstract void Open(Inventory inventory);
    }

    /// <summary>
    /// Base class of that open menus.
    /// </summary>
    public abstract class InventoryPanelOpener<T> : InventoryPanelOpener where T : InventoryPanelBinding
    {
        [Tooltip("The Inventory Panel Binding menu.")]
        [SerializeField] protected T m_Menu;

        /// <summary>
        /// Get the correct menu for the display panel
        /// </summary>
        /// <param name="force">Force initialize.</param>
        protected override void Initialize(bool force)
        {
            if (m_IsInitialized && !force) { return; }
            
            if (m_Menu != null && !force) {
                m_DisplayPanel = m_Menu.DisplayPanel;
                m_IsInitialized = true;
                return;
            }

            base.Initialize(force);

            if (m_DisplayPanel == null) {
                Debug.LogError($"The Display Panel with the name '{m_PanelName}' for the menu of type '{typeof(T).Name}' was not found", gameObject);
                return;
            }

            m_Menu = m_DisplayPanel.GetComponent<T>();
        }


    }
}