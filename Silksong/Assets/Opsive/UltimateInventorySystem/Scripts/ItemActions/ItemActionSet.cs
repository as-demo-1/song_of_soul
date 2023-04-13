/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEngine;

    /// <summary>
    /// A scriptable object that contains item actions that could by invoked on items of a certain category.
    /// </summary>
    [CreateAssetMenu(fileName = "MyItemActionSet", menuName = "Ultimate Inventory System/Item Actions/Item Action Set", order = 51)]
    public class ItemActionSet : ScriptableObject, IDatabaseSwitcher
    {
        [Tooltip("The item category for which the item action can be invoked.")]
        [SerializeField] protected DynamicItemCategory m_ItemCategory;
        [Tooltip("The item categories for which the item action cannot be invoked even if they are part of the category.")]
        [SerializeField] protected DynamicItemCategoryArray m_ExceptionCategories;
        [Tooltip("A collection of item actions.")]
        [SerializeField] protected ItemActionCollection m_ItemActionCollection;

        public ItemCategory ItemCategory {
            get { return m_ItemCategory; }
            set { m_ItemCategory = value; }
        }

        //After
        public ItemCategory[] ExceptionCategories {
            get { return m_ExceptionCategories; }
            set { m_ExceptionCategories = value; }
        }


        public ItemActionCollection ItemActionCollection {
            get => m_ItemActionCollection;
            internal set => m_ItemActionCollection = value;
        }

        /// <summary>
        /// Initialize on awake.
        /// </summary>
        private void Awake()
        {
            m_ItemActionCollection?.Initialize(true);
        }

        /// <summary>
        /// Returns true if the item matches the category.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if there is a match.</returns>
        public bool MatchItem(Item item)
        {
            if (ItemCategory != null && !ItemCategory.InherentlyContains(item)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            return database.Contains(m_ItemCategory);
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            m_ItemCategory = database.FindSimilar(m_ItemCategory);

            return null;
        }
    }
}

