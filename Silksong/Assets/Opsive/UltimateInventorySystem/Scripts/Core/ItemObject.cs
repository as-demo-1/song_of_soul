/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEngine;
    using EventHandler = Shared.Events.EventHandler;

    /// <summary>
    /// The item object component allows you to have a reference to an item within the scene.
    /// </summary>
    public class ItemObject : MonoBehaviour, IDatabaseSwitcher
    {
        [Tooltip("The item.")]
        [SerializeField] protected ItemInfo m_ItemInfo;

        public ItemInfo ItemInfo => m_ItemInfo;
        public Item Item => m_ItemInfo.Item;
        public int Amount => m_ItemInfo.Amount;

        protected bool m_ItemValidated = false;

        /// <summary>
        /// Validate the item.
        /// </summary>
        protected virtual void Awake()
        {
            ValidateItem();
        }

        /// <summary>
        /// Validate that the item is functional.
        /// </summary>
        public void ValidateItem()
        {
            if (m_ItemValidated) { return; }

            if (m_ItemInfo.Item?.ItemDefinition == null) {
                m_ItemInfo = ItemInfo.None;
                return;
            }

            if (Application.isPlaying) {
                var registeredItem = m_ItemInfo.Item;
                InventorySystemManager.ItemRegister.Register(ref registeredItem);
                m_ItemInfo = (registeredItem, m_ItemInfo.Amount, m_ItemInfo.ItemCollection, m_ItemInfo.ItemStack);
            }

            if (m_ItemInfo.Item.IsInitialized == false) { m_ItemInfo.Item.Initialize(false); }
            m_ItemInfo.Item.UpdateAttributes();

            m_ItemValidated = true;
        }

        /// <summary>
        /// Execute a change when the object is enabled.
        /// </summary>
        protected virtual void OnEnable()
        {
            EventHandler.ExecuteEvent(this, EventNames.c_ItemObject_OnItemChanged);
        }

        /// <summary>
        /// Set the item.
        /// </summary>
        private void Start()
        {
            RefreshBinding();
        }
        
        /// <summary>
        /// Rebind the item if it not correctly bound.
        /// </summary>
        public void ForceChangeEvent()
        {
            RefreshBinding();
            EventHandler.ExecuteEvent(this, EventNames.c_ItemObject_OnItemChanged);
        }

        /// <summary>
        /// Rebind the item if it not correctly bound.
        /// </summary>
        public void RefreshBinding()
        {
            SetItem(m_ItemInfo);
        }

        /// <summary>
        /// Set the item and bind it to the component.
        /// </summary>
        /// <param name="item">The item.</param>
        public virtual void SetItem(Item item)
        {
            SetItem((ItemInfo)(1, item));
        }

        /// <summary>
        /// Set the item and bind it to the component.
        /// </summary>
        /// <param name="item">The item.</param>
        public virtual void SetAmount(int amount)
        {
            SetItem((amount, m_ItemInfo));
        }

        /// <summary>
        /// Set the item and bind it to the component.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        public virtual void SetItem(ItemInfo itemInfo)
        {
            var isBoundToItemObject = itemInfo.Item?.IsBoundToItemObject(this) ?? false;

            if (itemInfo == m_ItemInfo && isBoundToItemObject) { return; }

            if (itemInfo.Item != m_ItemInfo.Item || !isBoundToItemObject) {

                if (m_ItemInfo.Item != null) {
                    m_ItemInfo.Item.RemoveBindingToItemObject(this);
                }

                m_ItemInfo = itemInfo;

                if (m_ItemInfo.Item != null) {
                    m_ItemInfo.Item.BindToItemObject(this);
                }
            } else {
                m_ItemInfo = itemInfo;
            }

            if (Application.isPlaying == false) { return; }
            EventHandler.ExecuteEvent(this, EventNames.c_ItemObject_OnItemChanged);
        }

        /// <summary>
        /// Remove the bindings and event on destroy.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (m_ItemInfo.Item != null) {
                m_ItemInfo.Item.RemoveBindingToItemObject(this);
            }
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            if (Item == null) { return true; }

            return database.Contains(Item);
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            SetItem((ItemInfo)(m_ItemInfo.Amount, database.FindSimilar(Item)));

            return null;
        }
    }
}
