/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.DataContainers
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Storage;
    using System;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// A scriptable object used to map item categories to item views.
    /// </summary>
    [CreateAssetMenu(fileName = "CategoryItemViewSet", menuName = "Ultimate Inventory System/UI/Category Item View Set", order = 1)]
    public class CategoryItemViewSet : ScriptableObject, IDatabaseSwitcher
    {
        [FormerlySerializedAs("m_EmptyItemBoxUI")]
        [Tooltip("The item view when the item is null.")]
        [SerializeField] protected GameObject m_EmptyItemView;

        [FormerlySerializedAs("m_CategoriesItem")]
        [FormerlySerializedAs("m_CategoriesItemBoxes")]
        [Tooltip("The item view mapped to an itemCategory.")]
        [SerializeField] protected CategoryItemViews[] m_CategoriesItemViews;

        public CategoryItemViews[] CategoriesItemViews {
            get => m_CategoriesItemViews;
            set => m_CategoriesItemViews = value;
        }

        /// <summary>
        /// Return the prefab that matches the item category best.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The prefab containing an item view.</returns>
        public GameObject FindItemViewPrefabForItem(Item item)
        {
            if (item == null) {
                if (m_EmptyItemView == null) {
                    Debug.LogWarning($"The Empty Item View must be specified.");
                }

                return m_EmptyItemView;
            }

            var selectedCategoryItemViews = new CategoryItemViews();
            for (int i = 0; i < m_CategoriesItemViews.Length; i++) {
                var category = m_CategoriesItemViews[i].Category;

                if (category == null) {
                    if (selectedCategoryItemViews.Category == null) {
                        selectedCategoryItemViews = m_CategoriesItemViews[i];
                    }
                    continue;
                }

                if (category.InherentlyContains(item) == false) { continue; }

                if (selectedCategoryItemViews.Category != null
                    && selectedCategoryItemViews.Category.InherentlyContains(category) == false) { continue; }

                selectedCategoryItemViews = m_CategoriesItemViews[i];
            }

            if (selectedCategoryItemViews.ItemViewPrefab == null) {
                Debug.LogWarning($"Missing an itemBoxUi type for {item?.Category?.name} category.");
            }

            return selectedCategoryItemViews.ItemViewPrefab;
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            for (int i = 0; i < m_CategoriesItemViews.Length; i++) {
                var category = m_CategoriesItemViews[i].Category;
                if (database.Contains(category)) { continue; }

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

            for (int i = 0; i < m_CategoriesItemViews.Length; i++) {
                var category = m_CategoriesItemViews[i].Category;
                if (database.Contains(category)) { continue; }

                category = database.FindSimilar(category);

                m_CategoriesItemViews[i] =
                    new CategoryItemViews(category,
                        m_CategoriesItemViews[i].ItemViewPrefab);

            }

            return null;
        }
    }

    /// <summary>
    /// An struct with an ItemCategory and a Game Object.
    /// </summary>
    [Serializable]
    public struct CategoryItemViews
    {
        [Tooltip("The item category.")]
        [SerializeField] private DynamicItemCategory m_Category;

        [FormerlySerializedAs("m_ItemBoxPrefab")]
        [Tooltip("The item view prefab.")]
        [SerializeField] private GameObject m_ItemViewPrefab;

        public ItemCategory Category => m_Category;
        public GameObject ItemViewPrefab => m_ItemViewPrefab;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="itemViewPrefab">The prefab.</param>
        public CategoryItemViews(ItemCategory category, GameObject itemViewPrefab)
        {
            m_Category = category;
            m_ItemViewPrefab = itemViewPrefab;
        }
    }
}
