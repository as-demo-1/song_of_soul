/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.InventoryCollections
{
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEngine;

    /// <summary>
    /// A scriptable object for a group ItemCollection restriction.
    /// </summary>
    [CreateAssetMenu(fileName = "GroupItemRestriction", menuName = "Ultimate Inventory System/Inventory/Group Item Restriction", order = 2)]
    public class GroupItemRestrictionObject : ItemRestrictionObject, IDatabaseSwitcher
    {
        [Tooltip("The group restriction.")]
        [SerializeField] protected GroupItemRestriction m_Restriction;

        public override IItemRestriction OriginalRestriction => m_Restriction;
        public override IItemRestriction DuplicateRestriction => new GroupItemRestriction(m_Restriction);

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            if (m_Restriction.ItemCategories == null) {
                m_Restriction.ItemCategories = new ItemCategory[0];
                return true;
            }

            for (int i = 0; i < m_Restriction.ItemCategories.Length; i++) {
                var category = m_Restriction.ItemCategories[i];
                if (category == null || database.Contains(category)) { continue; }

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

            for (int i = 0; i < m_Restriction.ItemCategories.Length; i++) {
                var category = m_Restriction.ItemCategories[i];
                if (database.Contains(category)) { continue; }

                category = database.FindSimilar(category);
                m_Restriction.ItemCategories[i] = category;
            }

            return null;
        }
    }
}