/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.DataStructures
{
    /// <summary>
    /// Ite mDefinitions Amounts is an array of item definition amounts.
    /// </summary>
    [System.Serializable]
    public class ItemDefinitionAmounts : ObjectAmounts<ItemDefinition, ItemDefinitionAmount>
    {
        public ItemDefinitionAmounts() : base()
        { }

        public ItemDefinitionAmounts(int arrayStartSize) : base(arrayStartSize)
        { }

        public ItemDefinitionAmounts(ItemDefinitionAmount[] array) : base(array)
        { }

        public static implicit operator ItemDefinitionAmount[](ItemDefinitionAmounts x) => x?.m_Array;
        public static implicit operator ItemDefinitionAmounts(ItemDefinitionAmount[] x) => new ItemDefinitionAmounts(x);
    }
}