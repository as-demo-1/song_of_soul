/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Utility
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System.Collections.Generic;

    /// <summary>
    /// A static class used to compare amounts of different lists of structs.
    /// </summary>
    public static class AmountUtility
    {

        /// <summary>
        /// Converts a list of ItemStacks into an array of itemAmounts.
        /// </summary>
        /// <param name="itemStacks">The itemStacks.</param>
        /// <returns>The item amounts.</returns>
        public static ItemAmount[] ToItemAmountArray(this IReadOnlyList<ItemStack> itemStacks)
        {
            var itemAmounts = new ItemAmount[itemStacks.Count];
            for (int i = 0; i < itemAmounts.Length; i++) { itemAmounts[i] = itemStacks[i].ItemAmount; }
            return itemAmounts;
        }

        /// <summary>
        /// Converts a list of ItemStacks into an array of itemAmounts.
        /// </summary>
        /// <param name="itemStacks">The itemStacks.</param>
        /// <returns>The item amounts.</returns>
        public static ItemAmount[] ToItemAmountArray(this ListSlice<ItemStack> itemStacks)
        {
            var itemAmounts = new ItemAmount[itemStacks.Count];
            for (int i = 0; i < itemAmounts.Length; i++) { itemAmounts[i] = itemStacks[i].ItemAmount; }
            return itemAmounts;
        }

        /// <summary>
        /// Converts a list of ItemStacks into an array of itemInfos.
        /// </summary>
        /// <param name="itemStacks">The itemStacks.</param>
        /// <returns>The item infos.</returns>
        public static ItemInfo[] ToItemInfoArray(this IReadOnlyList<ItemStack> itemStacks)
        {
            var itemAmounts = new ItemInfo[itemStacks.Count];
            for (int i = 0; i < itemAmounts.Length; i++) { itemAmounts[i] = (ItemInfo)itemStacks[i]; }
            return itemAmounts;
        }

        /// <summary>
        /// Converts a list of ItemStacks into an array of itemInfos.
        /// </summary>
        /// <param name="itemStacks">The itemStacks.</param>
        /// <returns>The item infos.</returns>
        public static ItemInfo[] ToItemInfoArray(this ListSlice<ItemStack> itemStacks)
        {
            var itemAmounts = new ItemInfo[itemStacks.Count];
            for (int i = 0; i < itemAmounts.Length; i++) { itemAmounts[i] = (ItemInfo)itemStacks[i]; }
            return itemAmounts;
        }

        /// <summary>
        /// Get the total amount of items.
        /// </summary>
        /// <param name="amounts">List of ItemAmounts.</param>
        /// <param name="listLength">The length of the list.</param>
        /// <returns>The count.</returns>
        public static int GetTotalAmount(IReadOnlyList<ItemAmount> amounts, int listLength = -1)
        {
            if (listLength < 0) { listLength = amounts.Count; }

            var totalAmount = 0;
            for (var i = 0; i < listLength; i++) {
                var amount = amounts[i];
                totalAmount += amount.Amount;
            }

            return totalAmount;
        }

        /// <summary>
        /// Get the total amount of itemDefinitions.
        /// </summary>
        /// <param name="amounts">List of ItemAmounts.</param>
        /// <param name="listLength">The length of the list.</param>
        /// <returns>The count.</returns>
        public static int GetTotalAmount(IReadOnlyList<ItemDefinitionAmount> amounts, int listLength = -1)
        {
            if (listLength < 0) { listLength = amounts.Count; }

            var totalAmount = 0;
            for (var i = 0; i < listLength; i++) {
                var amount = amounts[i];
                totalAmount += amount.Amount;
            }

            return totalAmount;
        }

        /// <summary>
        /// Get the total amount of itemCategories.
        /// </summary>
        /// <param name="amounts">List of ItemAmounts.</param>
        /// <param name="listLength">The length of the list.</param>
        /// <returns>The count.</returns>
        public static int GetTotalAmount(IReadOnlyList<ItemCategoryAmount> amounts, int listLength = -1)
        {
            if (listLength < 0) { listLength = amounts.Count; }

            var totalAmount = 0;
            for (var i = 0; i < listLength; i++) {
                var amount = amounts[i];
                totalAmount += amount.Amount;
            }

            return totalAmount;
        }

        /// <summary>
        /// Check if the amounts of item match the amounts of item definition.
        /// </summary>
        /// <param name="ItemAmounts">The item amounts.</param>
        /// <param name="itemDefinitionAmounts">The iteDefinition amounts.</param>
        /// <param name="exactAmountMatch">if false if will check if there are more ItemDefinitions than items.</param>
        /// <returns>True if the amounts match.</returns>
        public static bool AmountsMatch(IReadOnlyList<ItemAmount> ItemAmounts, IReadOnlyList<ItemDefinitionAmount> itemDefinitionAmounts, bool exactAmountMatch = true)
        {
            foreach (var itemDefAmount in itemDefinitionAmounts) {
                int neededAmount = itemDefAmount.Amount;
                foreach (var ItemAmount in ItemAmounts) {
                    if (itemDefAmount.ItemDefinition == ItemAmount.Item.ItemDefinition) {
                        neededAmount -= ItemAmount.Amount;
                        if (neededAmount <= 0) {
                            if (exactAmountMatch && neededAmount < 0) {
                                //too many items
                                return false;
                            }
                            break;
                        }
                    }
                }
                if (neededAmount > 0) {
                    //not enough
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if the amounts of item match the amounts of item category.
        /// </summary>
        /// <param name="ItemAmounts">The item amounts.</param>
        /// <param name="itemCategoryAmounts">The itemCategory amounts.</param>
        /// <param name="exactAmountMatch">if false if will check if there are more ItemCategories than items.</param>
        /// <returns>True if the amounts match.</returns>
        public static bool AmountsMatch(IReadOnlyList<ItemAmount> ItemAmounts, IReadOnlyList<ItemCategoryAmount> itemCategoryAmounts, bool exactAmountMatch = true)
        {
            foreach (var itemCategoryAmount in itemCategoryAmounts) {
                int neededAmount = itemCategoryAmount.Amount;
                foreach (var ItemAmount in ItemAmounts) {
                    if (itemCategoryAmount.ItemCategory == ItemAmount.Item.ItemDefinition.Category) {
                        neededAmount -= ItemAmount.Amount;
                        if (neededAmount <= 0) {
                            if (exactAmountMatch && neededAmount < 0) {
                                //too many items
                                return false;
                            }
                            break;
                        }
                    }
                }
                if (neededAmount > 0) {
                    //not enough
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check if the amounts of item definition match the amounts of item categories.
        /// </summary>
        /// <param name="itemDefinitionAmounts">The item definition amounts.</param>
        /// <param name="itemCategoryAmounts">The item Categories amounts.</param>
        /// <param name="exactAmountMatch">if false if will check if there are more ItemCategories than item Definitions.</param>
        /// <returns>True if the amounts match.</returns>
        public static bool AmountsMatch(IReadOnlyList<ItemDefinitionAmount> itemDefinitionAmounts, IReadOnlyList<ItemCategoryAmount> itemCategoryAmounts, bool exactAmountMatch = true)
        {
            foreach (var itemCategoryAmount in itemCategoryAmounts) {
                int neededAmount = itemCategoryAmount.Amount;
                foreach (var itemDefinitionAmount in itemDefinitionAmounts) {
                    if (itemCategoryAmount.ItemCategory == itemDefinitionAmount.ItemDefinition.Category) {
                        neededAmount -= itemDefinitionAmount.Amount;
                        if (neededAmount <= 0) {
                            if (exactAmountMatch && neededAmount < 0) {
                                //too many items
                                return false;
                            }
                            break;
                        }
                    }
                }
                if (neededAmount > 0) {
                    //not enough
                    return false;
                }
            }
            return true;
        }
    }

}
