/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange.Shops
{
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// Shop where the currency is an integer.
    /// </summary>
    public class ShopInt : ShopGeneric<int>
    {

        /// <summary>
        /// Try to get the buy value specific for the buyer. The buy value is set to the referenced parameter. 
        /// </summary>
        /// <param name="buyer">The inventory of the buyer.</param>
        /// <param name="itemInfo">The item.</param>
        /// <param name="buyValue">Reference to the buy value.</param>
        /// <returns>True if buyable by buyer.</returns>
        public override bool TryGetBuyValueForBuyer(Inventory buyer, ItemInfo itemInfo, ref int buyValue)
        {
            var itemBuyValue = itemInfo.Item.GetAttribute<Attribute<int>>(BuyPriceAttributeName).GetValue();
            buyValue = Mathf.FloorToInt(itemBuyValue * itemInfo.Amount * GetBuyModifierForBuyer(buyer));
            return true;
        }

        /// <summary>
        /// Try to get the buy value specific for the buyer. The buy value is set to the referenced parameter. 
        /// </summary>
        /// <param name="seller">The seller inventory.</param>
        /// <param name="itemInfo">The item.</param>
        /// <param name="sellValue">Reference to the sell value.</param>
        /// <returns>True if sellable by seller.</returns>
        public override bool TryGetSellValueForSeller(Inventory seller, ItemInfo itemInfo, ref int sellValue)
        {
            var itemSellValue = itemInfo.Item.GetAttribute<Attribute<int>>(SellPriceAttributeName).GetValue();
            sellValue = Mathf.FloorToInt(itemSellValue * itemInfo.Amount * GetSellModifierForSeller(seller));
            return true;
        }
    }
}
