/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.ItemViewSlotContainers
{
    using Opsive.UltimateInventorySystem.UI.Item;
    using UnityEngine;

    public interface IItemViewSlotContainerBinding
    {
        ItemViewSlotsContainerBase ItemViewSlotsContainer { get; }

        void BindItemViewSlotContainer();

        void BindItemViewSlotContainer(ItemViewSlotsContainerBase itemViewSlotsContainer);

        void UnbindItemViewSlotContainer();
    }

    /// <summary>
    /// Bas class to for object bound to an item view slot container.
    /// </summary>
    public abstract class ItemViewSlotsContainerBinding : MonoBehaviour, IItemViewSlotContainerBinding
    {

        protected ItemViewSlotsContainerBase m_ItemViewSlotsContainer;

        protected bool m_IsInitialized;

        public ItemViewSlotsContainerBase ItemViewSlotsContainer => m_ItemViewSlotsContainer;

        /// <summary>
        /// Initialize.
        /// </summary>
        protected virtual void Awake()
        {
            Initialize(false);
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        /// <param name="force">Force Initialize.</param>
        public virtual void Initialize(bool force)
        {
            if (m_IsInitialized && force == false) { return; }

            m_IsInitialized = true;
        }

        /// <summary>
        /// Bind the item view slots container.
        /// </summary>
        public virtual void BindItemViewSlotContainer()
        {
            BindItemViewSlotContainer(m_ItemViewSlotsContainer);
        }

        /// <summary>
        /// Bind an item view slots container.
        /// </summary>
        /// <param name="container">The container.</param>
        public virtual void BindItemViewSlotContainer(ItemViewSlotsContainerBase container)
        {
            Initialize(false);
            if (m_ItemViewSlotsContainer == container) { return; }

            UnbindItemViewSlotContainer();

            m_ItemViewSlotsContainer = container;

            OnBindItemViewSlotContainer();

        }

        /// <summary>
        /// Item View Slot Container was bound.
        /// </summary>
        protected abstract void OnBindItemViewSlotContainer();

        /// <summary>
        /// Unbind the item view slot container.
        /// </summary>
        public virtual void UnbindItemViewSlotContainer()
        {
            Initialize(false);
            if (m_ItemViewSlotsContainer == null) { return; }

            OnUnbindItemViewSlotContainer();
            m_ItemViewSlotsContainer = null;
        }

        /// <summary>
        /// The Item View Slot Container was unbound.
        /// </summary>
        protected abstract void OnUnbindItemViewSlotContainer();
    }

    /// <summary>
    /// base class for Item View Slots Container Inventory Grid Bindings.
    /// </summary>
    public abstract class ItemViewSlotsContainerInventoryGridBinding : ItemViewSlotsContainerBinding
    {

        protected InventoryGrid m_InventoryGrid;

        /// <summary>
        /// Bind an inventory grid.
        /// </summary>
        /// <param name="container">The inventory grid.</param>
        public override void BindItemViewSlotContainer(ItemViewSlotsContainerBase container)
        {
            Initialize(false);
            if (m_ItemViewSlotsContainer == container) { return; }

            UnbindItemViewSlotContainer();

            var inventoryGrid = container as InventoryGrid;
            if (inventoryGrid == null) { return; }

            m_ItemViewSlotsContainer = container;
            m_InventoryGrid = inventoryGrid;

            OnBindItemViewSlotContainer();
        }

        /// <summary>
        /// Unbind the inventory.
        /// </summary>
        public override void UnbindItemViewSlotContainer()
        {
            Initialize(false);
            if (m_ItemViewSlotsContainer == null) { return; }

            OnUnbindItemViewSlotContainer();
            m_ItemViewSlotsContainer = null;
            m_InventoryGrid = null;
        }
    }
}