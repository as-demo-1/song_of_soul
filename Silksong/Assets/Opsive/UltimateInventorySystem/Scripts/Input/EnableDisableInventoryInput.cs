/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Input
{
    using System;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// Enable or disable the Inventory input when enabling/disabling this component.
    /// </summary>
    public class EnableDisableInventoryInput : MonoBehaviour
    {
        [Serializable]
        public enum EnableDisable
        {
            None,
            Enable,
            Disable
        }

        [Tooltip("The Inventory ID with the input to enable/disable.")]
        [SerializeField] protected uint m_InventoryID = 1;
        [Tooltip("Should the input be disabled or enabled when this component is enabled.")]
        [SerializeField] protected EnableDisable m_OnComponentEnable;
        [Tooltip("Should the input be disabled or enabled when this component is disabled.")]
        [SerializeField] protected EnableDisable m_OnComponentDisable;

        protected virtual void OnEnable()
        {
            if (TryGetInventory(out var inventory) == false) {
                return;
            }

            if (m_OnComponentEnable == EnableDisable.Enable) {
                EnableInput(inventory, true);
            }else if (m_OnComponentEnable == EnableDisable.Disable) {
                EnableInput(inventory, false);
            }
        }

        protected virtual void EnableInput(Inventory inventory, bool enableInput)
        {
            EventHandler.ExecuteEvent<bool>(inventory.gameObject, 
                EventNames.c_CharacterGameObject_OnEnableGameplayInput_Bool,
                enableInput);
        }

        protected virtual void OnDisable()
        {
            if (TryGetInventory(out var inventory) == false) {
                return;
            }
            
            if (m_OnComponentDisable == EnableDisable.Enable) {
                EnableInput(inventory, true);
            }else if (m_OnComponentDisable == EnableDisable.Disable) {
                EnableInput(inventory, false);
            }
        }

        protected virtual bool TryGetInventory(out Inventory inventory)
        {
            inventory = null;
            
            var inventoryIdentifier = InventorySystemManager.GetInventoryIdentifier(m_InventoryID);
            if (inventoryIdentifier == null) { return false; }

            inventory = inventoryIdentifier.Inventory;
            if (inventory == null) { return false; }

            return true;
        }
    }
}