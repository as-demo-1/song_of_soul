/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.Shared.Input;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Input;
    using System.Collections.Generic;
    using UnityEngine;

    public interface IItemUserAction
    { }

    /// <summary>
    /// The component that is used as the Item User.
    /// </summary>
    public class ItemUser : MonoBehaviour
    {
        [Tooltip("The component used to get input from the player to control UI and use items.")]
        [SerializeField] protected PlayerInput m_InventoryInput;

        public PlayerInput InventoryInput {
            get => m_InventoryInput;
            set => m_InventoryInput = value;
        }

        protected Dictionary<string, TypeDictionary> m_ItemActionsData;

        /// <summary>
        /// Initialize the component.
        /// </summary>
        protected virtual void Awake()
        {
            if (m_InventoryInput == null) { m_InventoryInput = GetComponent<PlayerInput>(); }

            m_ItemActionsData = new Dictionary<string, TypeDictionary>();
        }

        /// <summary>
        /// Try getting the data for an item action.
        /// </summary>
        /// <param name="itemAction">The item action.</param>
        /// <param name="data">The stored data for that item action.</param>
        /// <returns>True if the item action data exists.</returns>
        public bool TryGetData<T>(string itemAction, out T data)
        {
            return TryGetData(itemAction, 0, out data);
        }

        /// <summary>
        /// Try getting the data for an item action.
        /// </summary>
        /// <param name="itemAction">The item action.</param>
        /// <param name="data">The stored data for that item action.</param>
        /// <returns>True if the item action data exists.</returns>
        public bool TryGetData<T>(string itemAction, Item item, out T data)
        {
            return TryGetData(itemAction, item.ID, out data);
        }

        public virtual bool TryGetData<T>(string itemAction, uint id, out T data)
        {
            var result = m_ItemActionsData.TryGetValue(itemAction, out var typeDictionary);
            if (result == false) {
                data = default;
                return false;
            }

            return typeDictionary.TryGet<T>(id, out data);
        }

        /// <summary>
        /// Set the item action data.
        /// </summary>
        /// <param name="itemAction">The item action.</param>
        /// <param name="data">The data for the item action.</param>
        public void SetData<T>(string itemAction, T data)
        {
            SetData(itemAction, 0, data);
        }
        
        /// <summary>
        /// Set the item action data.
        /// </summary>
        /// <param name="itemAction">The item action.</param>
        /// <param name="data">The data for the item action.</param>
        public void SetData<T>(string itemAction, Item item, T data)
        {
            SetData(itemAction, item.ID, data);
        }

        private void SetData<T>(string itemAction, uint id, T data)
        {
            var result = m_ItemActionsData.TryGetValue(itemAction, out var typeDictionary);
            if (result == false) {
                typeDictionary = new TypeDictionary();
                m_ItemActionsData[itemAction] = typeDictionary;
            }

            typeDictionary.Set(data, id);
        }
    }
}