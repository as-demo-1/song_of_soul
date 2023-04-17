/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.DataStructures
{
    /// <summary>
    /// Item Amounts is an array of item amounts.
    /// </summary>
    [System.Serializable]
    public class ItemAmounts : ObjectAmounts<Item, ItemAmount>
    {
        public ItemAmounts() : base()
        { }

        public ItemAmounts(int arrayStartSize) : base(arrayStartSize)
        { }

        public ItemAmounts(ItemAmount[] array) : base(array)
        { }

        public static implicit operator ItemAmount[](ItemAmounts x) => x?.m_Array;
        public static implicit operator ItemAmounts(ItemAmount[] x) => new ItemAmounts(x);
    }
}