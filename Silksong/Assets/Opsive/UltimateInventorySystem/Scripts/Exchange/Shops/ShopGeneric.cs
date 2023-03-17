/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange.Shops
{
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// Generic class for a shop with any type of currency.
    /// </summary>
    /// <typeparam name="CurrencyT">The currency type</typeparam>
    public abstract class ShopGeneric<CurrencyT> : ShopBase, IShop
    {
        [Tooltip("The default Item Collection where the items should be added when bought.")]
        [SerializeField] protected ItemCollectionID m_DefaultItemCollectionToAddOnBuy = new ItemCollectionID("", ItemCollectionPurpose.Main);
        [Tooltip("The ItemCollections where the items should be added depending on their Item Category.")]
        [SerializeField] protected List<ItemCategoryWithString> m_ItemCollectionToAddOnBuyByCategory;
        
        protected CurrencyT m_InternalCurrency;

        /// <summary>
        /// Get the Item Collection where the item should be added when buying an item.
        /// </summary>
        /// <param name="buyerInventory">The buyer inventory.</param>
        /// <param name="itemToBuy">The item to buy.</param>
        /// <returns>The item collection where the item should be added.</returns>
        public ItemCollection GetItemCollectionToAddItemTo(Inventory buyerInventory, ItemInfo itemToBuy)
        {
            if (m_ItemCollectionToAddOnBuyByCategory == null || m_ItemCollectionToAddOnBuyByCategory.Count == 0) {
                return buyerInventory.GetItemCollection(m_DefaultItemCollectionToAddOnBuy);
            }

            for (int i = 0; i < m_ItemCollectionToAddOnBuyByCategory.Count; i++) {
                var categoryWithItemCollectionToAddOnBuy = m_ItemCollectionToAddOnBuyByCategory[i];
                
                if(categoryWithItemCollectionToAddOnBuy.Category.HasValue == false){ continue; }
                
                if(categoryWithItemCollectionToAddOnBuy.Category.Value.InherentlyContains(itemToBuy.Item) == false){ continue; }

                var itemCollection = buyerInventory.GetItemCollection(categoryWithItemCollectionToAddOnBuy.Name);
                if (itemCollection == null) {
                    Debug.LogWarning($"The Item Collection with name '{categoryWithItemCollectionToAddOnBuy.Name}' could not be found.");
                    continue;
                }

                return itemCollection;
            }
            
            return buyerInventory.GetItemCollection(m_DefaultItemCollectionToAddOnBuy);
        }

        /// <summary>
        /// Buy an item from the shop.
        /// </summary>
        /// <param name="buyerInventory">The inventory of the buyer.</param>
        /// <param name="currencyOwner">The currency owner of the buyer.</param>
        /// <param name="itemInfo">The item to buy.</param>
        /// <returns>The process can fail returning false.</returns>
        public override bool BuyItem(Inventory buyerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo)
        {
            if (currencyOwner is ICurrencyOwner<CurrencyT> currencyOwnerCurrencyT) {
                return BuyItemInternal(buyerInventory, currencyOwnerCurrencyT, itemInfo);
            }

            return false;
        }

        /// <summary>
        /// Buy an item from the shop.
        /// </summary>
        /// <param name="buyerInventory">The inventory of the buyer.</param>
        /// <param name="currencyOwner">The currency owner of the buyer.</param>
        /// <param name="itemInfo">The item to buy.</param>
        /// <returns>The process can fail returning false.</returns>
        protected virtual bool BuyItemInternal(Inventory buyerInventory, ICurrencyOwner<CurrencyT> currencyOwner, ItemInfo itemInfo)
        {
            if (buyerInventory == null) {
                return false;
            }
            if (itemInfo.Item == null) {
                return false;
            }
            if (currencyOwner == null) {
                return false;
            }
            if (itemInfo.Amount <= 0) {
                return false;
            }

            // Check if the shop sells the item.
            if (IsItemBuyable(itemInfo) == false) {
                return false;
            }

            if (CanBuyerBuyItem(buyerInventory, currencyOwner, itemInfo) == false) {
                return false;
            }

            // Retrieve the money from buyer.
            if (TryGetBuyValueForBuyer(buyerInventory, itemInfo, ref m_InternalCurrency)) {
                currencyOwner.RemoveCurrency(m_InternalCurrency);
            } else { return false; }

            // Give the item to the buyer.
            if (m_AddItemWithCallback == false) {
                var itemCollection = GetItemCollectionToAddItemTo(buyerInventory, itemInfo);
                
                if (itemInfo.Item.IsMutable && itemInfo.Item.IsUnique) {
                    for (int i = 0; i < itemInfo.Amount; i++) {
                        itemCollection.AddItem(Item.Create(itemInfo.Item));
                    }
                } else {
                    itemCollection.AddItem(itemInfo);
                }

                OnBuySuccess(buyerInventory, itemInfo);

                return true;
            }

            var success = false;
            EventHandler.ExecuteEvent<ShopGeneric<CurrencyT>, ItemInfo, Action<bool>>(buyerInventory.gameObject,
                EventNames.c_InventoryGameObject_OnBuyAddItem_Shop_ItemInfo_ActionBoolSucces,
                this, itemInfo, (result) =>
                {
                    if (result == false) { return; }

                    success = true;

                    OnBuySuccess(buyerInventory, itemInfo);
                });
            return success;

        }

        /// <summary>
        /// The item was bought successfully.
        /// </summary>
        /// <param name="buyerInventory">The item buyer.</param>
        /// <param name="itemInfo">The item bought.</param>
        protected virtual void OnBuySuccess(Inventory buyerInventory, ItemInfo itemInfo)
        {
            EventHandler.ExecuteEvent<ShopGeneric<CurrencyT>, ItemInfo>(buyerInventory.gameObject,
                EventNames.c_InventoryGameObject_OnBuyComplete_Shop_ItemInfo,
                this, itemInfo);
            EventHandler.ExecuteEvent<Inventory, ItemInfo>(gameObject,
                EventNames.c_ShopGameObject_OnBuyComplete_BuyerInventory_ItemInfo,
                buyerInventory, itemInfo);
        }

        /// <summary>
        /// Sell an item to the shop.
        /// </summary>
        /// <param name="sellerInventory">The inventory of the seller.</param>
        /// <param name="currencyOwner">The currency owner of the seller.</param>
        /// <param name="itemInfo">The item being sold.</param>
        /// <returns>The process can fail returning false.</returns>
        public override bool SellItem(Inventory sellerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo)
        {
            if (currencyOwner is ICurrencyOwner<CurrencyT> currencyOwnerCurrencyT) {
                return SellItemInternal(sellerInventory, currencyOwnerCurrencyT, itemInfo);
            }

            return false;
        }

        /// <summary>
        /// Sell an item to the shop.
        /// </summary>
        /// <param name="sellerInventory">The inventory of the seller.</param>
        /// <param name="currencyOwner">The currency owner of the seller.</param>
        /// <param name="itemInfo">The item being sold.</param>
        /// <returns>The process can fail returning false.</returns>
        protected virtual bool SellItemInternal(Inventory sellerInventory, ICurrencyOwner<CurrencyT> currencyOwner, ItemInfo itemInfo)
        {
            if (sellerInventory == null) {
                return false;
            }
            if (itemInfo.Item == null) {
                return false;
            }
            if (currencyOwner == null) {
                return false;
            }
            if (itemInfo.Amount <= 0) {
                return false;
            }

            // Check if the shop sells the item.
            if (IsItemSellable(itemInfo) == false) {
                return false;
            }

            if (CanSellerSellItem(sellerInventory, currencyOwner, itemInfo) == false) {
                return false;
            }

            var itemCollection = itemInfo.ItemCollection;
            if (itemCollection == null || !ReferenceEquals(itemCollection.Inventory, sellerInventory)) {
                itemCollection = sellerInventory.MainItemCollection;
            }

            // Take the item from the seller.
            if (m_RemoveItemWithCallback == false) {
                if (itemCollection.RemoveItem(itemInfo).Amount != itemInfo.Amount) {
                    Debug.LogWarning("Items could not be the removed from seller inventory.");
                    return false;
                }

                return SellRemovedItemComplete(sellerInventory, currencyOwner, itemInfo);
            }

            var success = false;
            EventHandler.ExecuteEvent<ShopBase, ItemInfo, Action<bool>>(sellerInventory.gameObject,
                EventNames.c_InventoryGameObject_OnSellRemoveItem_ShopBase_ItemInfo_ActionBoolSucces,
                this, itemInfo, (result) =>
                {
                    if (result == false) { return; }
                    success = SellRemovedItemComplete(sellerInventory, currencyOwner, itemInfo);
                });

            return success;
        }

        /// <summary>
        /// The items were removed and you can now add the currency to the seller inventory
        /// </summary>
        /// <param name="sellerInventory">The inventory of the seller.</param>
        /// <param name="currencyOwner">The currency owner of the seller.</param>
        /// <param name="itemInfo">The item being sold.</param>
        /// <returns>True if the currency was added correctly.</returns>
        protected bool SellRemovedItemComplete(Inventory sellerInventory, ICurrencyOwner<CurrencyT> currencyOwner, ItemInfo itemInfo)
        {

            // Give the currency to the seller.
            if (TryGetSellValueForSeller(sellerInventory, itemInfo, ref m_InternalCurrency)) {
                currencyOwner.AddCurrency(m_InternalCurrency);
            } else { return false; }

            EventHandler.ExecuteEvent<ShopGeneric<CurrencyT>, ItemInfo>(sellerInventory.gameObject,
                EventNames.c_InventoryGameObject_OnSellComplete_Shop_ItemInfo,
                this, itemInfo);
            EventHandler.ExecuteEvent<Inventory, ItemInfo>(gameObject,
                EventNames.c_ShopGameObject_OnSellComplete_SellerInventory_ItemInfo,
                sellerInventory, itemInfo);

            return true;

        }

        /// <summary>
        /// Can the buyer actually buy the item.
        /// </summary>
        /// <param name="buyerInventory">The inventory of the buyer.</param>
        /// <param name="currencyOwner">The currency owner of the buyer.</param>
        /// <param name="itemInfo">The item.</param>
        /// <returns>True only if buyable.</returns>
        public override bool CanBuyerBuyItem(Inventory buyerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo)
        {
            var itemCollection = GetItemCollectionToAddItemTo(buyerInventory, itemInfo);
            if (itemCollection == null || itemCollection.CanAddItem(itemInfo).HasValue == false) {
                return false;
            }
            
            if (base.CanBuyerBuyItem(buyerInventory, currencyOwner, itemInfo) == false) { return false; }
            if (currencyOwner is CurrencyOwnerGeneric<CurrencyT> currencyOwnerCurrencyT) {
                return CanBuyerBuyItemInternal(buyerInventory, currencyOwnerCurrencyT, itemInfo);
            }

            return false;
        }

        /// <summary>
        /// Can the buyer actually buy the item.
        /// </summary>
        /// <param name="buyer">The inventory of the buyer.</param>
        /// <param name="currencyOwner">The currency owner of the buyer.</param>
        /// <param name="itemInfo">The item.</param>
        /// <returns>True only if buyable.</returns>
        protected virtual bool CanBuyerBuyItemInternal(Inventory buyer, ICurrencyOwner<CurrencyT> currencyOwner, ItemInfo itemInfo)
        {
            if (TryGetBuyValueForBuyer(buyer, itemInfo, ref m_InternalCurrency)) {
                return currencyOwner.HasCurrency(m_InternalCurrency);
            }

            return false;
        }

        /// <summary>
        /// Try to get the buy value specific for the buyer. The buy value is set to the referenced parameter. 
        /// </summary>
        /// <param name="buyer">The inventory of the buyer.</param>
        /// <param name="item">The item.</param>
        /// <param name="buyValue">Reference to the buy value.</param>
        /// <returns>True if buyable by buyer.</returns>
        public abstract bool TryGetBuyValueForBuyer(Inventory buyer, ItemInfo itemInfo, ref CurrencyT buyValue);

        /// <summary>
        /// Can then seller sell the item to the shop.
        /// </summary>
        /// <param name="sellerInventory">The seller inventory.</param>
        /// <param name="currencyOwner">The currency of the seller.</param>
        /// <param name="itemInfo">The item.</param>
        /// <returns>True if it can be sold.</returns>
        public override bool CanSellerSellItem(Inventory sellerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo)
        {
            if (base.CanSellerSellItem(sellerInventory, currencyOwner, itemInfo) == false) { return false; }
            if (currencyOwner is CurrencyOwnerGeneric<CurrencyT> currencyOwnerCurrencyT) {
                return CanSellerSellItemInternal(sellerInventory, currencyOwnerCurrencyT, itemInfo);
            }

            return false;
        }

        /// <summary>
        /// Can then seller sell the item to the shop.
        /// </summary>
        /// <param name="seller">The seller inventory.</param>
        /// <param name="currencyOwner">The currency of the seller.</param>
        /// <param name="itemInfo">The item.</param>
        /// <returns>True if it can be sold.</returns>
        protected virtual bool CanSellerSellItemInternal(Inventory seller, ICurrencyOwner<CurrencyT> currencyOwner, ItemInfo itemInfo)
        {
            var itemCollection = itemInfo.ItemCollection;
            if (itemCollection == null || !ReferenceEquals(itemCollection.Inventory, seller)) {
                itemCollection = seller.MainItemCollection;
            }

            if (itemCollection.HasItem(itemInfo) == false) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Try to get the buy value specific for the buyer. The buy value is set to the referenced parameter. 
        /// </summary>
        /// <param name="seller">The seller inventory.</param>
        /// <param name="itemInfo">The item.</param>
        /// <param name="sellValue">Reference to the sell value.</param>
        /// <returns>True if sellable by seller.</returns>
        public abstract bool TryGetSellValueForSeller(Inventory seller, ItemInfo itemInfo, ref CurrencyT sellValue);
    }
}


