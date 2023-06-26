/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.Shared.Events;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using UnityEngine;

    /// <summary>
    /// Simple item action used to drop items.
    /// </summary>
    [System.Serializable]
    public class UseItemActionSetAttribute : ItemAction
    {
        [Tooltip("The name of the attribute which holds the Item Action.")]
        [SerializeField] protected string m_AttributeName = "ItemActionSet";
        [Tooltip("Drop One item instead of the item amount specified by the item info.")]
        [SerializeField] protected int m_ActionIndex;
        [Tooltip("Drop One item instead of the item amount specified by the item info.")]
        [SerializeField] protected bool m_UseOne = true;
        [Tooltip("Remove the item that is dropped.")]
        [SerializeField] protected bool m_RemoveOnUse;
        [Tooltip("Remove the item that is dropped.")]
        [SerializeField] protected string m_CooldownAttributeName = "Cooldown";
        [Tooltip("Remove the item that is dropped.")]
        [SerializeField] protected string m_CooldownItemUserDataName = "ItemCooldown";

        public int ActionIndex {
            get => m_ActionIndex;
            set => m_ActionIndex = value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UseItemActionSetAttribute()
        {
            m_Name = "Use";
        }

        /// <summary>
        /// Check if the action can be invoked.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if the action can be invoked.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            if (itemInfo.Item == null || itemInfo.Amount == 0) { return false; }

            if (IsItemCooldownInCooldown(itemInfo, itemUser)) {
                return false;
            }

            var itemActionAttribute = itemInfo.Item.GetAttribute<Attribute<ItemActionSet>>(m_AttributeName);
            if (itemActionAttribute == null) { return false; }
            
            var actionSet = itemActionAttribute.GetValue();
            if (actionSet == null) { return false; }
            
            actionSet.ItemActionCollection.Initialize(false);
            
            if (m_ActionIndex < 0 || m_ActionIndex >= actionSet.ItemActionCollection.Count) { return false; }
            
            return true;
        }

        protected virtual bool IsItemCooldownInCooldown(ItemInfo itemInfo, ItemUser itemUser)
        {
            var cooldownAttribute = itemInfo.Item.GetAttribute<Attribute<float>>(m_CooldownAttributeName);
            if (cooldownAttribute == null) { return false; }

            if (!itemUser.TryGetData(m_CooldownItemUserDataName, itemInfo.Item, out float nextTime)) { return false; }

            return nextTime > Time.time;
        }

        /// <summary>
        /// Invoke the action.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            if (m_UseOne) { itemInfo = (1, itemInfo); }

            var attribute = itemInfo.Item.GetAttribute<Attribute<ItemActionSet>>(m_AttributeName);
            var actionSet = attribute.GetValue();

            actionSet.ItemActionCollection[m_ActionIndex].InvokeAction(itemInfo, itemUser);

            if (m_RemoveOnUse) {
                itemInfo.ItemCollection?.RemoveItem(itemInfo);
            }

            SetItemCooldown(itemInfo, itemUser);
        }

        private void SetItemCooldown(ItemInfo itemInfo, ItemUser itemUser)
        {
            var cooldownAttribute = itemInfo.Item.GetAttribute<Attribute<float>>(m_CooldownAttributeName);
            if (cooldownAttribute == null) { return; }

            var cooldown = cooldownAttribute.GetValue();
            var nextCooldownTime = cooldown + Time.time;
            itemUser.SetData<float>(m_CooldownItemUserDataName, itemInfo.Item, nextCooldownTime);
            
            EventHandler.ExecuteEvent<ItemAction, ItemInfo, float>(itemUser.gameObject, 
                EventNames.c_CharacterGameObject_UsedItemActionWithCooldown_ItemAction_ItemInfo_Float,
                this,itemInfo,cooldown);
        }
    }
}