/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.ItemActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Demo.CharacterControl;
    using Opsive.UltimateInventorySystem.Demo.Throwables;
    using Opsive.UltimateInventorySystem.ItemActions;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// Demo Item action used to consume an item.
    /// </summary>
    [System.Serializable]
    public class DemoThrowItemAction : ItemAction
    {
        [SerializeField] protected float m_CoolDown = 0.4f;
        [SerializeField] protected string m_ItemUserDataName = "DemoThrow";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DemoThrowItemAction()
        {
            m_Name = "Throw";
        }

        /// <summary>
        /// Initializes the Item Action.
        /// <param name="force">Force the initialization.</param>
        /// </summary>
        public override void Initialize(bool force)
        {
            if (m_Initialized && !force) { return; }

            base.Initialize(force);
        }

        /// <summary>
        /// Can the item action be invoked.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user.</param>
        /// <returns>True if it can be invoked.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            var item = itemInfo.Item;
            if (item == null) { return false; }

            var character = itemUser.GetComponent<Character>();
            var canInvoke = item.GetAttribute<Attribute<GameObject>>("ThrowablePrefab") != null
                            && character != null
                            && itemInfo.ItemCollection.HasItem((1, item));

            if (canInvoke == false) { return false; }

            var nextTime = 0f;
            if (itemUser.TryGetData(m_ItemUserDataName, out float nextTimeData)) {
                nextTime = nextTimeData;
            }

            return Time.time > nextTime;
        }

        /// <summary>
        /// Consume the item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user.</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            itemUser.SetData(m_ItemUserDataName, Time.time + m_CoolDown);

            var item = itemInfo.Item;
            var character = itemUser.GetComponent<Character>();
            itemInfo.ItemCollection.RemoveItem(item);

            var throwablePrefab = item.GetAttribute<Attribute<GameObject>>("ThrowablePrefab").GetValue();

            var characterTransform = character.transform;
            var throwableInstance = ObjectPool.Instantiate(throwablePrefab,
                characterTransform.position + characterTransform.up * 2 + characterTransform.forward * 0.2f,
                Quaternion.identity);

            var throwable = throwableInstance.GetComponent<IThrowable>();

#pragma warning disable 4014
            AnimatedThrow(itemInfo, character, throwable);
#pragma warning restore 4014
        }

        /// <summary>
        /// The asynchronous task for throwing the item.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="character">The character throwing the item.</param>
        /// <param name="throwable">The throwable object.</param>
        /// <returns>The task.</returns>
        protected async Task AnimatedThrow(ItemInfo itemInfo, Character character, IThrowable throwable)
        {
            character.CharacterAnimator.Attack(itemInfo.Item);
            var time = Time.time;
            while (time + 0.2f > Time.time) {
                await Task.Yield();
            }
            throwable.Throw(character.transform.forward);
        }
    }
}