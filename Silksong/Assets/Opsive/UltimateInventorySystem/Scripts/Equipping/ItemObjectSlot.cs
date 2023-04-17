/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Equipping
{
    using Opsive.UltimateInventorySystem.Core;
    using System;
    using UnityEngine;

    /// <summary>
    /// The item object slot is used to define a slot for an item object. 
    /// </summary>
    [Serializable]
    public class ItemObjectSlot
    {
        [Tooltip("The item object slot name.")]
        [SerializeField] protected string m_Name;
        [Tooltip("The item category.")]
        [SerializeField] protected DynamicItemCategory m_Category;
        [Tooltip("Is the slot used to equip skinned meshes.")]
        [SerializeField] protected bool m_IsSkinnedEquipment;
        [Tooltip("The transform that will be the parent of the spawned item object.")]
        [SerializeField] protected Transform m_Transform;
        [Tooltip("The item object that is equipped in this slot.")]
        [SerializeField] protected ItemObject m_ItemObject;
        [Tooltip("The game objects which should be hidden when the slot is used.")]
        [SerializeField] protected GameObject[] m_HideWhenWearingEquipment;

        public string Name { get => m_Name; internal set => m_Name = value; }
        public virtual bool IsSkinnedEquipment { get => m_IsSkinnedEquipment; internal set => m_IsSkinnedEquipment = value; }
        public virtual Transform Transform { get => m_Transform; internal set => m_Transform = value; }
        public virtual ItemCategory Category { get => m_Category; internal set => m_Category = value; }
        public virtual ItemObject ItemObject { get => m_ItemObject; internal set => m_ItemObject = value; }
        public virtual GameObject[] HideWhenWearingEquipment { get => m_HideWhenWearingEquipment; internal set => m_HideWhenWearingEquipment = value; }

        /// <summary>
        /// Create the item object slot.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="category">The Item Category.</param>
        /// <param name="isSkinnedEquipment">Is the slot used for skinned item objects.</param>
        /// <param name="transform">The transform parent which will contain the item Object.</param>
        /// <param name="itemObject">The item object.</param>
        public ItemObjectSlot(string name, ItemCategory category, bool isSkinnedEquipment, Transform transform, ItemObject itemObject)
        {
            m_Name = name;
            m_Category = category;
            m_IsSkinnedEquipment = isSkinnedEquipment;
            m_Transform = transform;
            m_ItemObject = itemObject;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The slot name.</param>
        /// <param name="category">The item Category.</param>
        /// <param name="other">Another item object slot.</param>
        public ItemObjectSlot(string name, ItemCategory category, ItemObjectSlot other)
        {
            m_Name = name;
            m_IsSkinnedEquipment = other.IsSkinnedEquipment;
            m_Transform = other.Transform;
            m_Category = category;
            m_ItemObject = other.ItemObject;
            m_HideWhenWearingEquipment = other.m_HideWhenWearingEquipment;
        }

        /// <summary>
        /// Set the itemObject in the slot.
        /// </summary>
        /// <param name="itemObject">The item object slot.</param>
        public virtual void SetItemObject(ItemObject itemObject)
        {
            m_ItemObject = itemObject;
            HideDefaultGameObjects(itemObject != null);
        }

        /// <summary>
        /// Hide and show default game objects.
        /// </summary>
        /// <param name="hide">Hide or show.</param>
        public virtual void HideDefaultGameObjects(bool hide)
        {
            if (m_HideWhenWearingEquipment == null) { return; }
            for (int i = 0; i < m_HideWhenWearingEquipment.Length; i++) {
                if (m_HideWhenWearingEquipment[i] == null) { continue; }
                m_HideWhenWearingEquipment[i].SetActive(!hide);
            }
        }
    }
}