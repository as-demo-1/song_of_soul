/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// An item slot set lets you define a set of item slots.
    /// </summary>
    [CreateAssetMenu(fileName = "MyItemSlotSet", menuName = "Ultimate Inventory System/Inventory/Item Slot Set", order = 1)]
    public class ItemSlotSet : ScriptableObject, IDatabaseSwitcher
    {
        [Tooltip("The item slots.")]
        [SerializeField] protected ItemSlot[] m_ItemSlots;

        public IReadOnlyList<ItemSlot> ItemSlots => m_ItemSlots;
        internal ItemSlot[] ItemSlotsArray {
            get => m_ItemSlots;
            set => m_ItemSlots = value;
        }

        /// <summary>
        /// Get the slot at the index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item slot.</returns>
        public ItemSlot? GetSlot(int index)
        {
            if (index < 0 || index >= m_ItemSlots.Length) {
                return null;
            }
            return m_ItemSlots[index];
        }

        /// <summary>
        /// Get the slot by slot name.
        /// </summary>
        /// <param name="slotName">The index.</param>
        /// <returns>The item slot.</returns>
        public ItemSlot? GetSlot(string slotName)
        {
            for (int i = 0; i < m_ItemSlots.Length; i++) {
                var itemSlot = m_ItemSlots[i];

                if (itemSlot.Name == slotName) {
                    return itemSlot;
                }
            }

            return null;
        }

        /// <summary>
        /// Return the index for the slot name.
        /// </summary>
        /// <param name="slotName">The slot name.</param>
        /// <returns>The index (-1 if not found).</returns>
        public int GetIndexOf(string slotName)
        {
            for (int i = 0; i < m_ItemSlots.Length; i++) {
                var itemSlot = m_ItemSlots[i];

                if (itemSlot.Name == slotName) {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            if (m_ItemSlots == null) {
                m_ItemSlots = new ItemSlot[0];
            }

            for (int i = 0; i < m_ItemSlots.Length; i++) {
                var itemSlot = m_ItemSlots[i];
                if (database.Contains(itemSlot.Category)) { continue; }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            if (m_ItemSlots == null) {
                m_ItemSlots = new ItemSlot[0];
            }

            for (int i = 0; i < m_ItemSlots.Length; i++) {
                var itemSlot = m_ItemSlots[i];
                if (database.Contains(itemSlot.Category)) { continue; }

                m_ItemSlots[i] =
                    new ItemSlot(itemSlot.Name, database.FindSimilar(itemSlot.Category));

            }

            return null;
        }
    }

    /// <summary>
    /// The Item slot struct.
    /// </summary>
    [Serializable]
    public struct ItemSlot
    {
        [Tooltip("The item slot name.")]
        [SerializeField] private string m_Name;
        [Tooltip("The item category.")]
        [SerializeField] private DynamicItemCategory m_Category;

        public string Name => m_Name;
        public ItemCategory Category => m_Category;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="category">The item Category.</param>
        /// <param name="sizeLimit">The size limit.</param>
        public ItemSlot(string name, ItemCategory category)
        {
            m_Name = name;
            m_Category = category;
        }
    }

}

