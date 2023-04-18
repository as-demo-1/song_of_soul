/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.Shared.Game;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.DropsAndPickups;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using UnityEngine;

    /// <summary>
    /// Drop a quantity of items selected int the quantity picker panel.
    /// </summary>
    [System.Serializable]
    public class QuantityDropItemAction : ItemActionWithQuantityPickerPanel
    {
        [Tooltip("The pickup item prefab, it must have a ItemPickup component.")]
        [SerializeField] protected GameObject m_PickUpItemPrefab;
        [Tooltip("Remove the item that is dropped.")]
        [SerializeField] protected bool m_RemoveOnDrop;
        [Tooltip("The radius where the item should be dropped around the item user.")]
        [SerializeField] protected float m_DropRadius = 2f;
        [Tooltip("The center of the random drop radius.")]
        [SerializeField] protected Vector3 m_CenterOffset;

        public GameObject PickUpItemPrefab {
            get => m_PickUpItemPrefab;
            set => m_PickUpItemPrefab = value;
        }

        protected GameObject m_PickUpGameObject;
        public GameObject PickUpGameObject => m_PickUpGameObject;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public QuantityDropItemAction()
        {
            m_Name = "Drop";
        }

        /// <summary>
        /// Check if the action can be invoked.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <returns>True if the action can be invoked.</returns>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            return true;
        }

        /// <summary>
        /// Invoke the action.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            if (itemInfo.Amount == 1) {
                InvokeWithAwaitedValue(itemInfo, itemUser, 1);
                return;
            }

            base.InvokeActionInternal(itemInfo, itemUser);
        }

        /// <summary>
        /// Setup the quantity picker before it is used.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <param name="itemUser">The item user (can be null).</param>
        /// <param name="quantityPickerPanel">The quantity picker panel.</param>
        protected override void SetupQuantityPickerSettings(ItemInfo itemInfo, ItemUser itemUser, QuantityPickerPanel quantityPickerPanel)
        {
            quantityPickerPanel.QuantityPicker.MinQuantity = 1;
            quantityPickerPanel.QuantityPicker.MaxQuantity = itemInfo.Amount;
        }

        /// <summary>
        /// Invoke with the action with the awaited value. 
        /// </summary>
        /// <param name="itemInfo">The itemInfo.</param>
        /// <param name="itemUser">The item user.</param>
        /// <param name="awaitedValue">The value that was waited for.</param>
        protected override void InvokeWithAwaitedValue(ItemInfo itemInfo, ItemUser itemUser, int awaitedValue)
        {
            if (awaitedValue <= 0) { return; }

            if (m_PickUpItemPrefab == null) {
                Debug.LogWarning("Item Pickup Prefab is null on the Drop Item Action.");
                return;
            }

            var gameObject = itemUser?.gameObject ?? itemInfo.Inventory?.gameObject;

            if (gameObject == null) {
                Debug.LogWarning("The game object where the Item Pickup should spwaned to is null.");
                return;
            }

            itemInfo = (awaitedValue, itemInfo);

            if (m_RemoveOnDrop) {
                itemInfo.ItemCollection?.RemoveItem(itemInfo);
            }

            m_PickUpGameObject = DropItemAction.DropItem(itemInfo, m_PickUpItemPrefab, 
                gameObject.transform.position + m_CenterOffset + new Vector3(
                    Random.value * m_DropRadius - m_DropRadius / 2f,
                    Random.value * m_DropRadius,
                    Random.value * m_DropRadius - m_DropRadius / 2f));
        }
    }
}