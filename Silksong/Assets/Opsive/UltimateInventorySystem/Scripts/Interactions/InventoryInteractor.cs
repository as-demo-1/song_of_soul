/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Interactions
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.Input;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// Character interactor lets a character interact with interactable components. 
    /// </summary>
    public class InventoryInteractor : MonoBehaviour, IInteractorWithInventory
    {
        [Tooltip("The inventory that will be set as an interactor.")]
        [SerializeField] protected Inventory m_Inventory;
        [Tooltip("The interactable indicator is enable when the character can interact with something.")]
        [SerializeField] protected GameObject m_InteractableIndicator;
        [Tooltip("If false the pickup wil be triggered by the inventory input, if true the interactable will be interacted with automatically.")]
        [SerializeField] protected bool m_AutoInteract = false;
        [Tooltip("Interaction Input.")]
        [SerializeField] protected SimpleInput m_Input = new SimpleInput("Action", InputType.ButtonDown);

        protected PlayerInput m_PlayerInput;
        protected ResizableArray<IInteractable> m_Interactables;

        protected IInteractable m_SelectedInteractable;

        protected virtual bool AutoInteract => m_AutoInteract;

        public Inventory Inventory {
            get => m_Inventory;
            set => SetInventory(value);
        }

        public IInteractable SelectedInteractable => m_SelectedInteractable;
        public IReadOnlyList<IInteractable> Interactables => m_Interactables;

        /// <summary>
        /// Set the interactables.
        /// </summary>
        private void Start()
        {
            if (m_Inventory == null) { m_Inventory = GetComponent<Inventory>(); }

            SetInventory(m_Inventory);
            
            m_Interactables = new ResizableArray<IInteractable>();
            if (m_InteractableIndicator != null) {
                m_InteractableIndicator.SetActive(false);
            }
            
            EventHandler.RegisterEvent<bool>(gameObject, EventNames.c_CharacterGameObject_OnEnableGameplayInput_Bool, HandleEnableGameplayInput);
        }
        
        /// <summary>
        /// Handle the gameplay input being enabled/disabled.
        /// </summary>
        /// <param name="enable">is the gameplay input enabled?</param>
        private void HandleEnableGameplayInput(bool enable)
        {
            enabled = enable;
        }

        /// <summary>
        /// Set the Inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        protected void SetInventory(Inventory inventory)
        {
            m_Inventory = inventory;
            if (m_Inventory == null) {
                m_PlayerInput = null;
                return;
            }
            m_PlayerInput = m_Inventory.gameObject.GetCachedComponent<PlayerInput>();
        }

        /// <summary>
        /// Check for the input.
        /// </summary>
        private void Update()
        {
            if(m_PlayerInput == null){ return; }

            if (m_Interactables.Count > 0 && (m_Interactables[0] == null || m_Interactables[0].gameObject.activeInHierarchy == false)) {
                RemoveInteractable(m_Interactables[0]);
                return;
            }
            
            if (m_Input.CheckInput(m_PlayerInput)) {
                Interact();
            }
        }

        /// <summary>
        /// Interact with the first selected interactable.
        /// </summary>
        public void Interact()
        {
            if (m_Interactables.Count > 0) {
                InteractWith(m_Interactables[0]);
            }
        }

        /// <summary>
        /// Add an interactable.
        /// </summary>
        /// <param name="interactable">An interactable.</param>
        public void AddInteractable(IInteractable interactable)
        {
            if(interactable == null) { return; }
            if (m_Interactables.Contains(interactable)) { return; }

            m_Interactables.Add(interactable);

            if (AutoInteract) {
                InteractWith(interactable);
                return;
            }

            if (m_Interactables.Count > 0) {
                if (m_InteractableIndicator != null) {
                    m_InteractableIndicator.SetActive(true);
                }
            }

            if (m_SelectedInteractable == null) { SetSelectedInteractable(interactable); }
        }

        /// <summary>
        /// Remove an interactable.
        /// </summary>
        /// <param name="interactable">The interactable.</param>
        public void RemoveInteractable(IInteractable interactable)
        {
            m_Interactables.Remove(interactable);
            if (m_Interactables.Count == 0) {
                if (m_InteractableIndicator != null) {
                    m_InteractableIndicator.SetActive(false);
                }
            }

            if (m_SelectedInteractable == interactable) { RemoveSelectedInteractable(); }
        }

        /// <summary>
        /// Interact with the interactable.
        /// </summary>
        /// <param name="interactable">The interacable.</param>
        protected virtual void InteractWith(IInteractable interactable)
        {
            interactable.Interact(this);
        }

        /// <summary>
        /// Set the selected interactable.
        /// </summary>
        /// <param name="interactable">The interactable.</param>
        protected void SetSelectedInteractable(IInteractable interactable)
        {
            m_SelectedInteractable = interactable;
            interactable.Select(this);
        }

        /// <summary>
        /// Remove the selected interactable.
        /// </summary>
        protected void RemoveSelectedInteractable()
        {
            if (m_SelectedInteractable != null) {
                m_SelectedInteractable.Deselect(this);
            }

            if (m_Interactables.Count != 0) {
                SetSelectedInteractable(m_Interactables[0]);
            } else {
                m_SelectedInteractable = null;
            }
        }

        /// <summary>
        /// Unregister input on destroy.
        /// </summary>
        private void OnDestroy()
        {
            EventHandler.UnregisterEvent<bool>(gameObject, EventNames.c_CharacterGameObject_OnEnableGameplayInput_Bool, HandleEnableGameplayInput);
        }

       
    }
}