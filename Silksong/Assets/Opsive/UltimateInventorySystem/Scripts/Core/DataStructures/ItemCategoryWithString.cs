/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.DataStructures
{
    using System;
    using UnityEngine;

    [Serializable]
    public struct ItemCategoryWithString
    {
        [Tooltip("The Item Category.")]
        [SerializeField] private DynamicItemCategory m_Category;
        [Tooltip("The name linked to that category (not the category name).")]
        [SerializeField] private string m_Name;

        public DynamicItemCategory Category => m_Category;
        public string Name => m_Name;

        /// <summary>
        /// Simple constructor.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="name">The name.</param>
        public ItemCategoryWithString(DynamicItemCategory category, string name)
        {
            m_Category = category;
            m_Name = name;
        }
    }
}