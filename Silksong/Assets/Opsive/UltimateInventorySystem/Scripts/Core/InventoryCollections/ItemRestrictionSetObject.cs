/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System;
    using System.Collections.Generic;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEngine;

    /// <summary>
    /// An Item Restriction Set Object is used to combine a lot of different restrictions for your Item Collections within an Inventory.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemRestrictionSet", menuName = "Ultimate Inventory System/Inventory/Item Restriction Set", order = 51)]
    public class ItemRestrictionSetObject : ItemRestrictionObject, IDatabaseSwitcher
    {
        
        [Tooltip("The group restriction.")]
        [SerializeField] protected ItemRestrictionSet m_Restriction;

        public ItemRestrictionSet ItemRestrictionSet
        {
            get { return m_Restriction; }
            set { m_Restriction = value; }
        }

        public override IItemRestriction OriginalRestriction => m_Restriction;
        public override IItemRestriction DuplicateRestriction => new ItemRestrictionSet(m_Restriction);
        
        public bool IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            return true;
        }

        public ModifiedObjectWithDatabaseObjects ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            return null;
        }
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    public class HideFromItemRestrictionSetAttribute : Attribute
    { }
    
    [Serializable]
    [HideFromItemRestrictionSetAttribute]
    public class ItemRestrictionSet : IItemRestriction
    {
        [SerializeField] protected Serialization[] m_RestrictionListData;
            
        protected List<IItemRestriction> m_RestrictionList;
        protected bool m_Initialized;
        protected IInventory m_Inventory;

        public List<IItemRestriction> RestrictionList { get => m_RestrictionList; set => m_RestrictionList = value; }
        public bool Initialized { get => m_Initialized; set => m_Initialized = value; }

        public ItemRestrictionSet()
        {
            m_RestrictionList = new List<IItemRestriction>();
        }

        /// <summary>
        /// Duplicate the object.
        /// </summary>
        /// <param name="other">The other object to duplicate.</param>
        public ItemRestrictionSet(ItemRestrictionSet other)
        {
            m_RestrictionListData = other.m_RestrictionListData;
        }

        /// <summary>
        /// Initialize the item collection.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="force">Force to initialize.</param>
        public void Initialize(IInventory inventory, bool force)
        {
            if (m_Initialized && !force && inventory == m_Inventory) { return; }

            if (m_Inventory != inventory) {
                m_Inventory = inventory;

                Deserialize();

                for (int i = 0; i < m_RestrictionList.Count; i++) {
                    m_RestrictionList[i].Initialize(inventory, force);
                }
            }

            m_Initialized = true;
        }

        /// <summary>
        /// Deserialize the restrictions.
        /// </summary>
        public void Deserialize()
        {
            m_RestrictionList = new List<IItemRestriction>();

            if (m_RestrictionListData == null) {
                return;
            }

            for (var i = 0; i < m_RestrictionListData.Length; i++) {
                var data = m_RestrictionListData[i];

                if (!(data.DeserializeFields(MemberVisibility.Public) is IItemRestriction itemRestriction)) { continue; }

                m_RestrictionList.Add(itemRestriction);
            }
        }

        /// <summary>
        /// Serialize the restrictions.
        /// </summary>
        public void Serialize()
        {
            m_RestrictionList.RemoveAll(item => item == null);
            m_RestrictionListData = Serialization.Serialize((IList<IItemRestriction>)m_RestrictionList);
        }

        /// <summary>
        /// Can the Item be added to the item collection?
        /// </summary>
        /// <param name="itemInfo">The item to add.</param>
        /// <param name="receivingCollection">The item collection the item is added to.</param>
        /// <returns>The item that can be added.</returns>
        public ItemInfo? CanAddItem(ItemInfo itemInfo, ItemCollection receivingCollection)
        {
            for (int i = 0; i < m_RestrictionList.Count; i++) {
                var result = m_RestrictionList[i].CanAddItem(itemInfo, receivingCollection);
                if (result.HasValue == false) { return null; }

                itemInfo = result.Value;
            }

            return itemInfo;
        }

        /// <summary>
        /// Can the Item be removed.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>The item Info.</returns>
        public ItemInfo? CanRemoveItem(ItemInfo itemInfo)
        {
            for (int i = 0; i < m_RestrictionList.Count; i++) {
                var result = m_RestrictionList[i].CanRemoveItem(itemInfo);
                if (result.HasValue == false) { return null; }

                itemInfo = result.Value;
            }

            return itemInfo;
        }
    }
}