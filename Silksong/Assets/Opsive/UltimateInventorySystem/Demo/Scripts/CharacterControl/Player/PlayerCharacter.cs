/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl.Player
{
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Demo.Damageable;
    using Opsive.UltimateInventorySystem.UI.Panels.Hotbar;
    using UnityEngine;

    /// <summary>
    /// The player character.
    /// </summary>
    public class PlayerCharacter : Character
    {
        [Tooltip("The item hot bar.")]
        [SerializeField] protected ItemHotbar m_ItemHotbar;
        [Tooltip("Moves the items defined in the attached inventory's 'DebugLoadout' named Item Collection to the Main Item Collection.")]
        [SerializeField] protected bool m_DebugLoadout;

        public ItemHotbar ItemHotbar => m_ItemHotbar;
        public bool DebugLoadout { set { m_DebugLoadout = value; } }

        /// <summary>
        /// Register for damageable popup.
        /// </summary>
        protected void Start()
        {
            DamagePopupSpawner.RegisterDamageable(m_CharacterDamageable, DamagePopupSpawner.DamageableType.PLAYER);
            if (m_ItemHotbar == null) { m_ItemHotbar = FindObjectOfType<ItemHotbar>(); }

            if (m_DebugLoadout) {
                var inventory = GetComponent<Inventory>();
                if (inventory == null) { return; }
                inventory.GetItemCollection("DebugLoadout")?.GiveAllItems(inventory.MainItemCollection);
            }
        }

        /// <summary>
        /// Unregister for damageable popup.
        /// </summary>
        protected void OnDestroy()
        {
            DamagePopupSpawner.UnregisterDamageable(m_CharacterDamageable, DamagePopupSpawner.DamageableType.PLAYER);
        }
    }
}
