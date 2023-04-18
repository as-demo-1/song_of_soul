/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Exchange.Shops
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// Generic class for a shop with any type of currency.
    /// </summary>
    /// <typeparam name="CurrencyT">The currency type</typeparam>
    public abstract class ShopBase : MonoBehaviour, IShop
    {
        [Tooltip("Buy price attribute name on the items.")]
        [SerializeField] private string m_BuyAttributeName = "BuyPrice";
        [Tooltip("Buy price modifier. 0 => no modification.")]
        [SerializeField] protected float m_BuyModifier;
        [Tooltip("Sell price attribute name on the items.")]
        [SerializeField] private string m_SellAttributeName = "SellPrice";
        [Tooltip("Sell price modifier. 0 => no modification.")]
        [SerializeField] protected float m_SellModifier;

        [Tooltip("If true the item won't be added by the shop, but instead the shop will send an event with a callback to add the item externally.")]
        [SerializeField] protected bool m_AddItemWithCallback = false;
        [Tooltip("If true the item won't be removed by the shop, but instead the shop will send an event with a callback to remove the item externally.")]
        [SerializeField] protected bool m_RemoveItemWithCallback = false;

        [Tooltip("The shop inventory, containing the items that are on sale.")]
        [SerializeField] protected Inventory m_Inventory;

        protected IShopBuyCondition[] m_BuyConditions;
        protected IShopSellCondition[] m_SellConditions;

        public Inventory Inventory {
            get { return m_Inventory; }
            set => m_Inventory = value;
        }

        //Feel free to override these attribute names to whatever you want.
        public virtual string BuyPriceAttributeName => m_BuyAttributeName;
        public virtual string SellPriceAttributeName => m_SellAttributeName;

        /// <summary>
        /// Initialize on awake.
        /// </summary>
        protected virtual void Awake()
        {
            m_BuyConditions = GetComponents<IShopBuyCondition>();
            m_SellConditions = GetComponents<IShopSellCondition>();
        }

        /// <summary>
        /// Buy an item from the shop.
        /// </summary>
        /// <param name="buyerInventory">The inventory of the buyer.</param>
        /// <param name="currencyOwner">The currency owner of the buyer.</param>
        /// <param name="itemInfo">The item to buy.</param>
        /// <returns>The process can fail returning false.</returns>
        public abstract bool BuyItem(Inventory buyerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo);

        /// <summary>
        /// Sell an item to the shop.
        /// </summary>
        /// <param name="sellerInventory">The inventory of the seller.</param>
        /// <param name="currencyOwner">The currency owner of the seller.</param>
        /// <param name="itemInfo">The item being sold.</param>
        /// <returns>The process can fail returning false.</returns>
        public abstract bool SellItem(Inventory sellerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo);

        /// <summary>
        /// Is the item specified being sold by the shop. 
        /// </summary>
        /// <param name="itemInfo">The item.</param>
        /// <returns>True if buyable.</returns>
        public virtual bool IsItemBuyable(ItemInfo itemInfo)
        {
            if (m_Inventory == null) { return true; }

            var item = itemInfo.Item;
            if (item == null) { return false; }

            var itemCollection = itemInfo.ItemCollection;
            if (itemCollection == null) { itemCollection = m_Inventory.MainItemCollection; }

            return itemCollection.HasItem((1, item)) && item.HasAttribute(BuyPriceAttributeName);
        }

        /// <summary>
        /// Can the buyer actually buy the item.
        /// </summary>
        /// <param name="buyerInventory">The inventory of the buyer.</param>
        /// <param name="currencyOwner">The currency owner of the buyer.</param>
        /// <param name="itemInfo">The item.</param>
        /// <returns>True only if buyable.</returns>
        public virtual bool CanBuyerBuyItem(Inventory buyerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo)
        {
            for (int i = 0; i < m_BuyConditions.Length; i++) {
                if (m_BuyConditions[i].CanBuy(this, buyerInventory, currencyOwner, itemInfo)) { continue; }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the specific buy price modifier for the buyer.
        /// </summary>
        /// <param name="buyer">The buyer inventory.</param>
        /// <returns>The modifier.</returns>
        public virtual float GetBuyModifierForBuyer(Inventory buyer)
        {
            return 1 + m_BuyModifier;
        }

        /// <summary>
        /// Can the shop buy the item specified from a seller. 
        /// </summary>
        /// <param name="itemInfo">The itemInfo.</param>
        /// <returns>True if sellable.</returns>
        public virtual bool IsItemSellable(ItemInfo itemInfo)
        {
            var item = itemInfo.Item;
            if (item == null) { return false; }

            return item.HasAttribute(SellPriceAttributeName);
        }

        /// <summary>
        /// Can then seller sell the item to the shop.
        /// </summary>
        /// <param name="sellerInventory">The seller inventory.</param>
        /// <param name="currencyOwner">The currency of the seller.</param>
        /// <param name="itemInfo">The item.</param>
        /// <returns>True if it can be sold.</returns>
        public virtual bool CanSellerSellItem(Inventory sellerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo)
        {
            for (int i = 0; i < m_SellConditions.Length; i++) {
                if (m_SellConditions[i].CanSell(this, sellerInventory, currencyOwner, itemInfo)) { continue; }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the specific sell price modifier for the seller.
        /// </summary>
        /// <param name="seller">The seller inventory.</param>
        /// <returns>The modifier.</returns>
        public virtual float GetSellModifierForSeller(Inventory seller)
        {
            return 1 + m_SellModifier;
        }

    }

    public interface IShopSellCondition
    {
        bool CanSell(ShopBase shop, Inventory sellerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo);
    }

    public interface IShopBuyCondition
    {
        bool CanBuy(ShopBase shop, Inventory buyerInventory, ICurrencyOwner currencyOwner, ItemInfo itemInfo);
    }
}