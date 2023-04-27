/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Equipping
{
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Input;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.ItemObjectBehaviours;
    using System;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// Usable Equipped Items Handler is used to use equipped items which have an Item Object Behaviour Handler.
    /// </summary>
    public class UsableEquippedItemsHandler : MonoBehaviour
    {
        /// <summary>
        /// Hot bar input.
        /// </summary>
        [Serializable]
        public class UsableItemObjectInput : SimpleInput
        {
            public int ItemObjectIndex;
            public int ActionIndex;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="index">The hot bar slot index affected by this input.</param>
            public UsableItemObjectInput(int itemObjectIndex, int actionIndex)
            {
                ItemObjectIndex = itemObjectIndex;
                ActionIndex = actionIndex;
            }
        }
        
        [Tooltip("The item user.")]
        [SerializeField] protected ItemUser m_ItemUser;
        [Tooltip("The input for usable Item Objects.")]
        [SerializeField] protected UsableItemObjectInput[] m_Inputs;

        protected PlayerInput m_PlayerInput;

        protected IEquipper m_Equipper;
        public IEquipper Equipper {
            get => m_Equipper;
            set => m_Equipper = value;
        }

        /// <summary>
        /// Awake.
        /// </summary>
        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize.
        /// </summary>
        protected virtual void Initialize()
        {
            if (m_Equipper == null) { m_Equipper = GetComponent<IEquipper>(); }

            if (m_ItemUser == null) {
                m_ItemUser = GetComponent<ItemUser>();
                if (m_ItemUser == null) {
                    return;
                }
            }

            m_PlayerInput = m_ItemUser.gameObject.GetCachedComponent<PlayerInput>();
            if (m_PlayerInput == null) {
                enabled = false;
                return;
            }

            EventHandler.RegisterEvent<bool>(m_PlayerInput.gameObject, EventNames.c_CharacterGameObject_OnEnableGameplayInput_Bool, HandleEnableGameplayInput);
        }

        private void HandleEnableGameplayInput(bool enable)
        {
            enabled = enable;
        }

        private void Update()
        {
            for (int i = 0; i < m_Inputs.Length; i++) {
                if (m_Inputs[i].CheckInput(m_PlayerInput)) {
                    UseItem(m_Inputs[i].ItemObjectIndex, m_Inputs[i].ActionIndex);
                }
            }
        }

        /// <summary>
        /// Use an item directly without using input.
        /// </summary>
        /// <param name="itemObjectIndex">The index of the usable item object to use.</param>
        /// <param name="itemActionIndex">The item action index to use.</param>
        public void UseItem(int itemObjectIndex, int itemActionIndex)
        {
            if (itemObjectIndex < 0 || itemObjectIndex >= m_Equipper.Slots.Length) {
                return;
            }

            var slot = m_Equipper.Slots[itemObjectIndex];

            var itemObject = slot.ItemObject;

            if (itemObject == null) { return; }

            var itemObjectBehaviourHandler = itemObject.gameObject.GetCachedComponent<IItemObjectBehaviourHandler>();

            UseItem(itemObjectBehaviourHandler, itemActionIndex);
        }

        /// <summary>
        /// Use an item directly without using input.
        /// </summary>
        /// <param name="itemObjectBehaviourHandler">The usable item object to use.</param>
        /// <param name="itemActionIndex">The item action index to use.</param>
        public virtual void UseItem(IItemObjectBehaviourHandler itemObjectBehaviourHandler, int itemActionIndex)
        {
            if (itemObjectBehaviourHandler == null) { return; }
            itemObjectBehaviourHandler.UseItem(m_ItemUser, itemActionIndex);
        }
        
        /// <summary>
        /// Unregister input on destroy.
        /// </summary>
        private void OnDestroy()
        {
            if(m_PlayerInput == null){ return; }
            EventHandler.UnregisterEvent<bool>(m_PlayerInput.gameObject, EventNames.c_CharacterGameObject_OnEnableGameplayInput_Bool, HandleEnableGameplayInput);
        }
    }
}