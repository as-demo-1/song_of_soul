/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.DataStructures
{
    using Opsive.UltimateInventorySystem.Core;

    /// <summary>
    /// ItemCategory Amounts is an array of item category amounts.
    /// </summary>
    [System.Serializable]
    public class ItemCategoryAmounts : ObjectAmounts<ItemCategory, ItemCategoryAmount>
    {
        public ItemCategoryAmounts() : base()
        { }

        public ItemCategoryAmounts(int arrayStartSize) : base(arrayStartSize)
        { }

        public ItemCategoryAmounts(ItemCategoryAmount[] array) : base(array)
        { }

        public static implicit operator ItemCategoryAmount[](ItemCategoryAmounts x) => x?.m_Array;
        public static implicit operator ItemCategoryAmounts(ItemCategoryAmount[] x) => new ItemCategoryAmounts(x);
    }
}