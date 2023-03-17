/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.ItemActions
{
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Demo.CharacterControl;
    using Opsive.UltimateInventorySystem.ItemActions;

    /// <summary>
    /// Demo Item action used to consume an item.
    /// </summary>
    [System.Serializable]
    public class DemoConsumeItemAction : ItemAction
    {
        protected int m_HealAmount;

        public int HealAmount => m_HealAmount;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DemoConsumeItemAction()
        {
            m_Name = "Consume";
        }

        /// <summary>
        /// Can the item action be invoked.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if it can be invoked.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            var item = itemInfo.Item;
            var inventory = itemInfo.Inventory;
            var character = itemUser.GetComponent<Character>();
            return item.GetAttribute<Attribute<int>>("HealAmount") != null
                   && character != null
                   && inventory.MainItemCollection.HasItem((1, item))
                   && character.CharacterDamageable.CurrentHp != character.CharacterDamageable.MaxHp;
        }

        /// <summary>
        /// Consume the item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            var item = itemInfo.Item;
            var inventory = itemInfo.Inventory;
            var character = itemUser.GetComponent<Character>();
            inventory.MainItemCollection.RemoveItem(item);
            m_HealAmount = item.GetAttribute<Attribute<int>>("HealAmount").GetValue();
            character.CharacterDamageable.Heal(m_HealAmount);
        }
    }
}