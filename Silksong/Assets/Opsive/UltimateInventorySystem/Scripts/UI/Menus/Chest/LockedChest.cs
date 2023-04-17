/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Menus.Chest
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEngine;
    using UnityEngine.Events;

    public class LockedChest : Chest, IDatabaseSwitcher
    {
        [Tooltip("The chest key item definition.")]
        [SerializeField] protected DynamicItemDefinition m_ChestKey;
        [Tooltip("If true the chest key item will be removed from its inventory when the chest is unlocked.")]
        [SerializeField] protected bool m_RemoveKeyOnUnlock;

        [Tooltip("The amount of second the text will hte displayed for.")]
        [SerializeField] protected GameObject[] m_DisableWhenUnlocked;
        [Tooltip("Event called when trying to open without a key.")]
        [SerializeField] protected UnityEvent m_OnTryOpenNoKey;
        [Tooltip("Event called when trying to open with a key.")]
        [SerializeField] protected UnityEvent m_OnTryOpenHasKey;

        protected bool m_Unlocked = false;

        /// <summary>
        /// Use the interactor to reference its inventory in the chest menu.
        /// </summary>
        /// <param name="interactor">The interactor.</param>
        public override void Open(Inventory clientInventory)
        {
            if (!m_Unlocked) {
                var chestKeyItem = clientInventory.GetItemInfo(m_ChestKey, false);
                if (chestKeyItem.HasValue) {

                    if (m_RemoveKeyOnUnlock) {
                        clientInventory.RemoveItem((1, chestKeyItem.Value));
                    }

                    m_Unlocked = true;

                    for (int i = 0; i < m_DisableWhenUnlocked.Length; i++) {
                        m_DisableWhenUnlocked[i].SetActive(false);
                    }

                    OnTryOpenSuccess();

                } else { OnTryOpenFailed(); }
            }

            if (m_Unlocked) {
                base.Open(clientInventory);
            }
        }

        /// <summary>
        /// Handle a try open success.
        /// </summary>
        protected virtual void OnTryOpenSuccess()
        {
            m_OnTryOpenHasKey.Invoke();
        }

        /// <summary>
        /// Handle a try open failed.
        /// </summary>
        protected virtual void OnTryOpenFailed()
        {
            m_OnTryOpenNoKey.Invoke();
        }

        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IDatabaseSwitcher.IsComponentValidForDatabase(InventorySystemDatabase database)
        {
            if (database == null) { return false; }

            return database.Contains(m_ChestKey);
        }

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects IDatabaseSwitcher.ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database)
        {
            if (database == null) { return null; }

            m_ChestKey = database.FindSimilar(m_ChestKey);

            return null;
        }
    }
}