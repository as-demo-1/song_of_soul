/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange.Shops
{
    using Opsive.Shared.Events;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// A Shop Binding component that can add and/or remove items from the shop inventory when bought or sold.
    /// </summary>
    public class ShopAddRemoveBinding : MonoBehaviour, IShopBuyCondition
    {
        [Tooltip("The shop to bind to.")]
        [SerializeField] protected ShopBase m_Shop;
        [Tooltip("Remove the item bought from the shop.")]
        [SerializeField] protected bool m_RemoveOnBuy;
        [Tooltip("Add the item sold to the shop.")]
        [SerializeField] protected bool m_AddOnSell;

        /// <summary>
        /// Register the event listener.
        /// </summary>
        private void Awake()
        {
            if (m_Shop == null) {
                m_Shop = GetComponent<ShopBase>();
                if (m_Shop == null) {
                    Debug.LogWarning("The Shop reference is missing from te ShopAddRemoveBinding component.");
                    return;
                }
            }

            EventHandler.RegisterEvent<Inventory, ItemInfo>(m_Shop.gameObject,
                EventNames.c_ShopGameObject_OnBuyComplete_BuyerInventory_ItemInfo,
                OnBuyComplete);
            EventHandler.RegisterEvent<Inventory, ItemInfo>(m_Shop.gameObject,
                EventNames.c_ShopGameObject_OnSellComplete_SellerInventory_ItemInfo,
                OnSellComplete);
        }

        /// <summary>
        /// An item was bought, it can be removed from the shop inventory.
        /// </summary>
        /// <param name="buyerInventory">The buyer inventory.</param>
        /// <param name="itemInfo">The item info.</param>
        private void OnBuyComplete(Inventory buyerInventory, ItemInfo itemInfo)
        {
            if (m_RemoveOnBuy) {
                m_Shop.Inventory.RemoveItem(itemInfo);
            }
        }

        /// <summary>
        /// An item was sold to the inventory, it can be added to the shop inventory.
        /// </summary>
        /// <param name="sellerInventory">The seller inventory.</param>
        /// <param name="itemInfo">The item info.</param>
        private void OnSellComplete(Inventory sellerInventory, ItemInfo itemInfo)
        {
            if (m_AddOnSell) {
                m_Shop.Inventory.AddItem(itemInfo);
            }
        }

        /// <summary>
        /// Can the buyer buy an item from the shop. 
        /// </summary>
        /// <param name="shop">The shop.</param>
        /// <param name="buyerInventory">The buyer.</param>
        /// <param name="currencyOwner">The currency owner.</param>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>True if the item can be bought.</returns>
        public bool CanBuy(ShopBase shop, Inventory buyerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo)
        {
            if (m_RemoveOnBuy == false) { return true; }
            return shop.Inventory.HasItem(itemInfo);
        }
    }
}