/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// The inventory panel binding.
    /// </summary>
    public abstract class InventoryPanelBinding : DisplayPanelBinding
    {
        [Tooltip("Should the inventory on the panel owner be used to bind this panel?")]
        [SerializeField] protected bool m_BindToPanelOwnerInventory = true;
        [Tooltip("Bind the inventory with a specific Identifier ID.")]
        [SerializeField] protected uint m_BindToInventoryByIdentifier = 0;
        [Tooltip("Set the Inventory to bind to this panel.")]
        [SerializeField] protected Inventory m_Inventory;

        public Inventory Inventory {
            get => m_Inventory;
            internal set => m_Inventory = value;
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="display">The bound display panel.</param>
        /// <param name="force"></param>
        public override void Initialize(DisplayPanel display, bool force)
        {
            var wasInitialized = m_IsInitialized;
            if (wasInitialized && !force) { return; }
            base.Initialize(display, force);

            OnInitializeBeforeInventoryBind();

            BindInventory();
        }

        /// <summary>
        /// On Inititalize before the inventory is bound.
        /// </summary>
        protected virtual void OnInitializeBeforeInventoryBind()
        { }

        /// <summary>
        /// Bind the inventory.
        /// </summary>
        public void BindInventory()
        {
            if (m_Inventory != null) {
                BindInventory(m_Inventory);
                return;
            }

            if (m_BindToPanelOwnerInventory) {
                BindInventory(m_DisplayPanel.Manager.PanelOwner.GetCachedComponent<Core.InventoryCollections.Inventory>());
                return;
            }

            if (m_BindToInventoryByIdentifier != 0) {
                var identifier = InventorySystemManager.GetInventoryIdentifier(m_BindToInventoryByIdentifier);
                if (identifier == null) {
                    Debug.LogWarning($"The Inventory Identifier with ID '{m_BindToInventoryByIdentifier}' could not be found", gameObject);
                    return;
                }
                BindInventory(identifier.Inventory);
            }
        }

        /// <summary>
        /// Bind the inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        public void BindInventory(Inventory inventory)
        {
            m_Inventory = inventory;
            if (m_Inventory == null) {
                Debug.LogWarning("You are binding a Null Inventory, Please make sure the Display Panel Manager, Panel Owner field is set to your main Inventory.");
            }

            OnInventoryBound();
        }

        /// <summary>
        /// The inventory was bound.
        /// </summary>
        protected abstract void OnInventoryBound();
    }
}