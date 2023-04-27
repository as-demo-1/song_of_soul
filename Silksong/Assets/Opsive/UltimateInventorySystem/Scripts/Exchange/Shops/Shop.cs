/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange.Shops
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// Shop where the currency is a currencyCollection.
    /// </summary>
    public class Shop : ShopGeneric<CurrencyCollection>
    {
        /// <summary>
        /// Initialize on awake.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            if (m_InternalCurrency == null) {
                m_InternalCurrency = new CurrencyCollection();
            }
            m_InternalCurrency.Initialize(null, false);
        }

        /// <summary>
        /// Try to get the buy value specific for the buyer. The buy value is set to the referenced parameter. 
        /// </summary>
        /// <param name="buyer">The inventory of the buyer.</param>
        /// <param name="itemInfo">The item.</param>
        /// <param name="buyValue">Reference to the buy value.</param>
        /// <returns>True if buyable by buyer.</returns>
        public override bool TryGetBuyValueForBuyer(Inventory buyer, ItemInfo itemInfo, ref CurrencyCollection buyValue)
        {
            if (itemInfo.Item == null) { return false; }

            if (itemInfo.Item.TryGetAttributeValue(BuyPriceAttributeName, out CurrencyAmounts itemBuyValue) == false) {
                Debug.LogWarning($"Attribute name does not exist on item {BuyPriceAttributeName}.");
                return false;
            }

            if (buyValue == null) { return false; }
            buyValue.SetCurrency((CurrencyAmount[])itemBuyValue, GetBuyModifierForBuyer(buyer) * itemInfo.Amount);
            return true;
        }

        /// <summary>
        /// Try to get the buy value specific for the buyer. The buy value is set to the referenced parameter. 
        /// </summary>
        /// <param name="seller">The seller inventory.</param>
        /// <param name="itemInfo">The item.</param>
        /// <param name="sellValue">Reference to the sell value.</param>
        /// <returns>True if sellable by seller.</returns>
        public override bool TryGetSellValueForSeller(Inventory seller, ItemInfo itemInfo, ref CurrencyCollection sellValue)
        {
            if (itemInfo.Item == null) { return false; }

            if (itemInfo.Item.TryGetAttributeValue(SellPriceAttributeName, out CurrencyAmounts itemSellValue) == false) {
                Debug.LogWarning($"Attribute name does not exist on item {SellPriceAttributeName}.");
                return false;
            }

            if (sellValue == null) { return false; }
            sellValue.SetCurrency((CurrencyAmount[])itemSellValue, GetSellModifierForSeller(seller) * itemInfo.Amount);
            return true;
        }
    }
}

