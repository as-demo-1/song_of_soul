/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Panels.Hotbar
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Input;
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// The component that handles input for the Item Hotbar.
    /// </summary>
    public class ItemHotbarHandler : MonoBehaviour
    {
        [Tooltip("The item hotbar to use when an input is pressed.")]
        [SerializeField] protected ItemHotbar m_ItemHotbar;
        [Tooltip("Input to use Item Actions in the Item Hotbar.")]
        [SerializeField]
        protected IndexedInput[] m_HotbarInput = new IndexedInput[]
        {
            new IndexedInput(0, "Equip First Item", InputType.ButtonDown),
            new IndexedInput(1, "Equip Second Item",  InputType.ButtonDown),
            new IndexedInput(2, "Equip Third Item", InputType.ButtonDown),
            new IndexedInput(3, "Equip Fourth Item", InputType.ButtonDown),
            new IndexedInput(4, "Equip Fifth Item", InputType.ButtonDown),
            new IndexedInput(5, "Equip Sixth Item",  InputType.ButtonDown),
            new IndexedInput(6, "Equip Seventh Item",  InputType.ButtonDown),
            new IndexedInput(7, "Equip Eighth Item", InputType.ButtonDown),
            new IndexedInput(8, "Equip Ninth Item",  InputType.ButtonDown),
            new IndexedInput(9, "Equip Tenth Item",  InputType.ButtonDown),
        };
        protected PlayerInput m_PlayerInput;
        
        /// <summary>
        /// Initialize and listen to events.
        /// </summary>
        private void Start()
        {
            if (m_ItemHotbar == null) { m_ItemHotbar = GetComponent<ItemHotbar>(); }

            m_ItemHotbar.OnBindInventory += OnInventoryBound;
            m_ItemHotbar.OnUnBindInventory += OnInventoryUnbound;
            OnInventoryBound(m_ItemHotbar.Inventory);
            
            if (m_PlayerInput == null) {
                enabled = false;
            }
        }

        /// <summary>
        /// Handle gameplay input being enabled/disabled.
        /// </summary>
        /// <param name="enable">is gameplay input enabled?</param>
        private void HandleEnableGameplayInput(bool enable)
        {
            enabled = enable;
        }

        /// <summary>
        /// Check for the input in update.
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < m_HotbarInput.Length; i++) {
                if ( m_HotbarInput[i].Index < 0 || m_ItemHotbar.SlotCount < m_HotbarInput[i].Index) {
                    continue;
                }
                if (m_HotbarInput[i].CheckInput(m_PlayerInput)) {
                    m_ItemHotbar.UseItem(m_HotbarInput[i].Index);
                }
            }
        }

        /// <summary>
        /// Handle a new inventory being bound.
        /// </summary>
        protected void OnInventoryBound(Inventory inventory)
        {
            if (inventory == null) { return; }

            m_PlayerInput = inventory.gameObject.GetCachedComponent<PlayerInput>();
            EventHandler.RegisterEvent<bool>(m_PlayerInput.gameObject, EventNames.c_CharacterGameObject_OnEnableGameplayInput_Bool, HandleEnableGameplayInput);
            enabled = true;
        }

        /// <summary>
        /// Handle inventor being unbound.
        /// </summary>
        protected void OnInventoryUnbound(Inventory inventory)
        {
            if (m_PlayerInput == null) {
                return;
            }
            EventHandler.UnregisterEvent<bool>(m_PlayerInput.gameObject, EventNames.c_CharacterGameObject_OnEnableGameplayInput_Bool, HandleEnableGameplayInput);
            m_PlayerInput = null;
            enabled = false;
        }
        
        /// <summary>
        /// Unregister input on destroy.
        /// </summary>
        private void OnDestroy()
        {
            if (m_PlayerInput == null) {
                return;
            }
            EventHandler.UnregisterEvent<bool>(m_PlayerInput.gameObject, EventNames.c_CharacterGameObject_OnEnableGameplayInput_Bool, HandleEnableGameplayInput);
        }
    }
}