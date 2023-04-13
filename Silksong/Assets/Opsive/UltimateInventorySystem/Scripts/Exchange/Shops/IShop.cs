/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange.Shops
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;

    /// <summary>
    /// Shop interface.
    /// </summary>
    public interface IShop
    {
        /// <summary>
        /// Get the inventory.
        /// </summary>
        Inventory Inventory { get; }

        /// <summary>
        /// Buy an Item.
        /// </summary>
        /// <param name="buyerInventory">The buyer inventory.</param>
        /// <param name="currencyOwner">The currency owner.</param>
        /// <param name="item">The item.</param>
        /// <returns>True if the item was bought.</returns>
        bool BuyItem(Inventory buyerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo);

        /// <summary>
        /// Sell an item.
        /// </summary>
        /// <param name="sellerInventory">The seller inventory.</param>
        /// <param name="currencyOwner">The currency owner.</param>
        /// <param name="item">The item to sell.</param>
        /// <returns>True if the item was sold.</returns>
        bool SellItem(Inventory sellerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo);

        /// <summary>
        /// Is the item buyable.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <returns>True if the item can be bought.</returns>
        bool IsItemBuyable(ItemInfo itemInfo);

        /// <summary>
        /// Check if a potential buyer is allowed to buy item.
        /// </summary>
        /// <param name="buyer">The buyer.</param>
        /// <param name="currencyOwner">The currency owner.</param>
        /// <param name="item">The item.</param>
        /// <returns>True if the buyer can buy the item.</returns>
        bool CanBuyerBuyItem(Inventory buyer, ICurrencyOwner currencyOwner, ItemInfo itemInfo);

        /// <summary>
        /// Get the buy modifier specific to the buyer.
        /// </summary>
        /// <param name="buyer">The buyer.</param>
        /// <returns>The buy modifier.</returns>
        float GetBuyModifierForBuyer(Inventory buyer);

        /// <summary>
        /// Is the item sellable.
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <returns>True if the item can be sold.</returns>
        bool IsItemSellable(ItemInfo itemInfo);

        /// <summary>
        /// Check if the potential seller can sell the item.
        /// </summary>
        /// <param name="sellerInventory">The seller.</param>
        /// <param name="currencyOwner">The sellers currency.</param>
        /// <param name="item">The item.</param>
        /// <returns>True if the seller can sell the item.</returns>
        bool CanSellerSellItem(Inventory sellerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo);

        /// <summary>
        /// Get the sell modifier specific to the seller.
        /// </summary>
        /// <param name="seller">The seller.</param>
        /// <returns>The modifier.</returns>
        float GetSellModifierForSeller(Inventory seller);
    }
}