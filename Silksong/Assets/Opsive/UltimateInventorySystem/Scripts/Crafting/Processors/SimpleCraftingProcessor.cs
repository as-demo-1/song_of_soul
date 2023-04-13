/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Crafting.Processors
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using EventHandler = Opsive.Shared.Events.EventHandler;

    /// <summary>
    /// Comparer used to sort currencyAmounts by the root the currencies.
    /// </summary>
    public class ItemCategoryGenerationComparer : IComparer<ItemCategoryAmount>
    {

        /// <summary>
        /// Compare the root of the currencies.
        /// </summary>
        /// <param name="x">Currency amount lhs.</param>
        /// <param name="y">Currency amount rhs.</param>
        /// <returns>The comparison value.</returns>
        public int Compare(ItemCategoryAmount x, ItemCategoryAmount y)
        {
            return x.ItemCategory.InherentlyContains(y.ItemCategory, false) ? 1 :
                y.ItemCategory.InherentlyContains(x.ItemCategory, false) ? -1 : 0;
        }
    }

    /// <summary>
    /// Simple crafting solution using the recipe itemDefinitions.
    /// </summary>
    [Serializable]
    public class SimpleCraftingProcessor : CraftingProcessor
    {
        [UnityEngine.Serialization.FormerlySerializedAs("m_RemoveIngredientsExternally")]
        [Tooltip("Use crafting processor callback to remove ingredient items from the inventory.")]
        [SerializeField] protected bool m_ExternallyRemoveIngredients;

        /// <summary>
        /// Constructor.
        /// </summary>
        public SimpleCraftingProcessor()
        {
            m_ExternallyRemoveIngredients = false;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="externallyRemoveIngredients">Choose whether to remove the items externally or from the main collection.</param>
        public SimpleCraftingProcessor(bool externallyRemoveIngredients)
        {
            m_ExternallyRemoveIngredients = externallyRemoveIngredients;
        }

        #region Static functions

        protected static readonly ItemCategoryGenerationComparer s_ItemCategoryGenerationComparer = new ItemCategoryGenerationComparer();

        /// <summary>
        /// Try to select a list of ItemAmounts from an inventory with conditions and of equal size amount as the ingredientList
        /// </summary>
        /// <param name="ingredientList">The ingredient list.</param>
        /// <param name="inventory">The inventory.</param>
        /// <param name="quantity">A multiplier of ingredient amount.</param>
        /// <param name="selectedItemInfos">Reference to the selected ItemAmounts.</param>
        /// <param name="selectedItemAmountsCount">Reference to the selected ItemAmounts count.</param>
        /// <param name="getAmount">Function to get an ingredientAmount.</param>
        /// <param name="condition">Function used as condition to decide whether an item should be selected or not.</param>
        /// <typeparam name="T">The type of ingredient.</typeparam>
        /// <returns>True if the selected amount size matches the ingredient list amount size.</returns>
        private static bool TryAutoSelectIngredientGeneric<T>(ListSlice<T> ingredientList,
            IInventory inventory, int quantity, ref ItemInfo[] selectedItemInfos, ref int selectedItemAmountsCount,
            Func<T, int> getAmount, Func<ItemInfo, T, bool> condition)
        {
            var pooledArray = GenericObjectPool.Get<ItemInfo[]>();

            for (var i = 0; i < ingredientList.Count; i++) {
                var ingredientAmount = ingredientList[i];

                var neededAmount = quantity * getAmount.Invoke(ingredientAmount);

                var filteredItemAmountsSlice = inventory.GetItemInfos(ref pooledArray, ingredientAmount, condition);//  filterInventoryItems(ingredientAmount, inventory);
                for (var j = 0; j < filteredItemAmountsSlice.Count; j++) {
                    var itemInfo = filteredItemAmountsSlice[j];

                    var haveAmount = itemInfo.Amount;
                    var foundMatch = false;

                    for (int k = 0; k < selectedItemAmountsCount; k++) {
                        var itemInfoToIgnore = selectedItemInfos[k];
                        if (itemInfo.Item != itemInfoToIgnore.Item) { continue; }
                        if (itemInfo.ItemStack != itemInfoToIgnore.ItemStack) { continue; }

                        foundMatch = true;
                        haveAmount = Mathf.Max(0, haveAmount - itemInfoToIgnore.Amount);
                        var additionalIgnoreAmount = Mathf.Clamp(haveAmount, 0, neededAmount);
                        selectedItemInfos[k] = (itemInfoToIgnore.Amount + additionalIgnoreAmount, itemInfoToIgnore);
                    }

                    var selectedAmount = Mathf.Clamp(haveAmount, 0, neededAmount);

                    if (foundMatch == false) {
                        TypeUtility.ResizeIfNecessary(ref selectedItemInfos, selectedItemAmountsCount + 1);
                        selectedItemInfos[selectedItemAmountsCount] = (selectedAmount, itemInfo);
                        selectedItemAmountsCount++;
                    }

                    neededAmount -= selectedAmount;
                    if (neededAmount <= 0) { break; }
                }

                if (neededAmount <= 0) { continue; }

                GenericObjectPool.Return(pooledArray);
                return false;
            }

            GenericObjectPool.Return(pooledArray);
            return true;
        }

        /// <summary>
        /// Check if the Items selected are valid for the ingredient list.
        /// </summary>
        /// <param name="ingredientList">The ingredient list.</param>
        /// <param name="quantity">A multiplier of ingredient amount.</param>
        /// <param name="selectedIngredients">The Item infos to compare against the ingredients.</param>
        /// <param name="itemAmountsToIgnore">The Item amounts that were already compared and taken into account.</param>
        /// <param name="itemAmountsToIgnoreCount">The ignored ItemAmounts count.</param>
        /// <param name="getAmount">Function to get an ingredientAmount.</param>
        /// <param name="condition">Function used as condition to decide whether an item should be selected or not.</param>
        /// <typeparam name="T">The type of ingredient.</typeparam>
        /// <returns>True if the ItemAmount size matches the ingredient list amount size.</returns>
        private static bool CheckIfEnoughIngredientGeneric<T>(ListSlice<T> ingredientList,
            int quantity, ListSlice<ItemInfo> selectedIngredients,
            ref ItemInfo[] itemAmountsToIgnore, ref int itemAmountsToIgnoreCount,
            Func<T, int> getAmount, Func<T, Item, bool> condition)
        {
            for (var i = 0; i < ingredientList.Count; i++) {
                var ingredientAmount = ingredientList[i];

                var neededAmount = quantity * getAmount.Invoke(ingredientAmount);

                for (var j = 0; j < selectedIngredients.Count; j++) {
                    var itemInfo = selectedIngredients[j];
                    if (!condition.Invoke(ingredientAmount, itemInfo.Item)) { continue; }

                    var haveAmount = itemInfo.Amount;
                    var foundMatch = false;

                    for (int k = 0; k < itemAmountsToIgnoreCount; k++) {
                        var itemAmountToIgnore = itemAmountsToIgnore[k];

                        if (itemInfo.Item != itemAmountToIgnore.Item) { continue; }
                        if (itemInfo.ItemStack != itemAmountToIgnore.ItemStack) { continue; }

                        foundMatch = true;

                        haveAmount = Mathf.Max(0, haveAmount - itemAmountToIgnore.Amount);
                        var additionalIgnoreAmount = Mathf.Clamp(haveAmount, 0, neededAmount);
                        itemAmountsToIgnore[k] = (itemAmountToIgnore.Amount + additionalIgnoreAmount, itemAmountToIgnore);
                    }

                    var selectedAmount = Mathf.Clamp(haveAmount, 0, neededAmount);

                    if (foundMatch == false) {
                        TypeUtility.ResizeIfNecessary(ref itemAmountsToIgnore, itemAmountsToIgnoreCount + 1);
                        itemAmountsToIgnore[itemAmountsToIgnoreCount] = (selectedAmount, itemInfo);
                        itemAmountsToIgnoreCount++;
                    }

                    neededAmount -= selectedAmount;
                    if (neededAmount <= 0) { break; }
                }

                if (neededAmount > 0) {
                    return false;
                }
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Automatically select the items that will be used as ingredients in the crafting process from the inventory.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <param name="resultSelectedItemInfos">Reference to an array of item amounts, can be resized up.</param>
        /// <param name="selectedIngredientsItemInfos">Output of a wrapper containing the result array.</param>
        /// <returns>True if all the requested ingredients were found in the inventory.</returns>
        public override bool TryAutoSelectIngredients(CraftingRecipe recipe, IInventory inventory, int quantity,
            ref ItemInfo[] resultSelectedItemInfos, out ListSlice<ItemInfo> selectedIngredientsItemInfos)
        {
            var count = 0;

            var result = TryAutoSelectItemIngredients(recipe.Ingredients.ItemAmounts.Array, inventory, quantity,
                ref resultSelectedItemInfos, ref count);

            if (result == false) {
                selectedIngredientsItemInfos = (resultSelectedItemInfos, 0, count);
                return false;
            }

            result = TryAutoSelectItemDefinitionIngredients(recipe.Ingredients.ItemDefinitionAmounts.Array, inventory, quantity,
                ref resultSelectedItemInfos, ref count);

            if (result == false) {
                selectedIngredientsItemInfos = (resultSelectedItemInfos, 0, count);
                return false;
            }

            var pooledArray2 = GenericObjectPool.Get<ItemCategoryAmount[]>();

            var itemCategoryAmountIngredientSorted = ListSlice.CopyTo(recipe.Ingredients.ItemCategoryAmounts.Array, ref pooledArray2);

            Array.Sort(pooledArray2, 0, itemCategoryAmountIngredientSorted.Count, s_ItemCategoryGenerationComparer);

            result = TryAutoSelectItemCategoryIngredients(
                itemCategoryAmountIngredientSorted,
                inventory, quantity, ref resultSelectedItemInfos, ref count);

            selectedIngredientsItemInfos = (resultSelectedItemInfos, 0, count);
            GenericObjectPool.Return(pooledArray2);
            return result;
        }

        /// <summary>
        /// Try to select a list of ItemAmounts from an inventory with equivalent items from the ingredientList
        /// </summary>
        /// <param name="itemAmountIngredients">The ingredients.</param>
        /// <param name="inventory">The inventory.</param>
        /// <param name="quantity">A multiplier of ingredient amount.</param>
        /// <param name="selectedItemInfos">Reference to the selected ItemAmounts.</param>
        /// <param name="selectedItemAmountsCount">Reference to the selected ItemAmounts count.</param>
        /// <returns>True if the equivalent items were selected from the inventory.</returns>
        protected virtual bool TryAutoSelectItemIngredients(ListSlice<ItemAmount> itemAmountIngredients,
            IInventory inventory, int quantity, ref ItemInfo[] selectedItemInfos, ref int selectedItemAmountsCount)
        {
            return TryAutoSelectIngredientGeneric(itemAmountIngredients,
                inventory, quantity, ref selectedItemInfos, ref selectedItemAmountsCount,
                xItemAmount => xItemAmount.Amount,
                (xInventoryItemAmount, xItemAmount) => xItemAmount.Item.ValueEquivalentTo(xInventoryItemAmount.Item));
        }

        /// <summary>
        /// Try to select a list of ItemAmounts from an inventory with equivalent itemDefinitions from the ingredientList
        /// </summary>
        /// <param name="itemDefinitionAmountIngredients">The itemDefinition amounts.</param>
        /// <param name="inventory">The inventory.</param>
        /// <param name="quantity">A multiplier of ingredient amount.</param>
        /// <param name="selectedItemInfos">Reference to the selected ItemAmounts.</param>
        /// <param name="selectedItemAmountsCount">Reference to the selected ItemAmounts count.</param>
        /// <returns>True if the equivalent items were selected from the inventory.</returns>
        protected virtual bool TryAutoSelectItemDefinitionIngredients(ListSlice<ItemDefinitionAmount> itemDefinitionAmountIngredients,
            IInventory inventory, int quantity, ref ItemInfo[] selectedItemInfos, ref int selectedItemAmountsCount)
        {
            return TryAutoSelectIngredientGeneric(itemDefinitionAmountIngredients,
                inventory, quantity, ref selectedItemInfos, ref selectedItemAmountsCount,
                xItemDefAmount => xItemDefAmount.Amount,
                (xInventoryItemAmount, xItemDefAmount) => xItemDefAmount.ItemDefinition == xInventoryItemAmount.Item.ItemDefinition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemCategoryAmountIngredients">The itemCategory amounts.</param>
        /// <param name="inventory">The inventory.</param>
        /// <param name="quantity">A multiplier of ingredient amount.</param>
        /// <param name="selectedItemInfos">Reference to the selected ItemAmounts.</param>
        /// <param name="selectedItemAmountsCount">Reference to the selected ItemAmounts count.</param>
        /// <returns>True if the equivalent items were selected from the inventory.</returns>
        protected virtual bool TryAutoSelectItemCategoryIngredients(ListSlice<ItemCategoryAmount> itemCategoryAmountIngredients,
            IInventory inventory, int quantity, ref ItemInfo[] selectedItemInfos, ref int selectedItemAmountsCount)
        {
            return TryAutoSelectIngredientGeneric(itemCategoryAmountIngredients,
                inventory, quantity, ref selectedItemInfos, ref selectedItemAmountsCount,
                xItemCatAmount => xItemCatAmount.Amount,
                (xInventoryItemAmount, xItemCatAmount) => xItemCatAmount.ItemCategory.InherentlyContains(xInventoryItemAmount.Item));
        }

        /// <summary>
        /// Check if the parameters are valid to craft an item.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="selectedIngredients">The item infos selected.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <returns>True if you can craft.</returns>
        protected override bool CanCraftInternal(CraftingRecipe recipe, IInventory inventory,
            ListSlice<ItemInfo> selectedIngredients, int quantity)
        {
            var pooledArray = GenericObjectPool.Get<ItemInfo[]>();
            var ignoreCount = 0;

            var result = CheckIfEnoughItemIngredients(recipe.Ingredients.ItemAmounts.Array, quantity,
                selectedIngredients, ref pooledArray, ref ignoreCount);

            if (result == false) {
                GenericObjectPool.Return(pooledArray);
                return false;
            }

            result = CheckIfEnoughItemDefinitionIngredients(recipe.Ingredients.ItemDefinitionAmounts.Array, quantity,
                selectedIngredients, ref pooledArray, ref ignoreCount);

            if (result == false) {
                GenericObjectPool.Return(pooledArray);
                return false;
            }

            var pooledArray2 = GenericObjectPool.Get<ItemCategoryAmount[]>();

            var itemCategoryAmountIngredientSorted = ListSlice.CopyTo(recipe.Ingredients.ItemCategoryAmounts.Array, ref pooledArray2);

            Array.Sort(pooledArray2, 0, itemCategoryAmountIngredientSorted.Count, s_ItemCategoryGenerationComparer);

            result = CheckIfEnoughItemCategoryIngredients(
                itemCategoryAmountIngredientSorted,
                quantity, selectedIngredients, ref pooledArray, ref ignoreCount);

            if (result == false) {
                GenericObjectPool.Return(pooledArray2);
                GenericObjectPool.Return(pooledArray);
                return false;
            }

            result = inventory.HasItemList((pooledArray, 0, ignoreCount));

            GenericObjectPool.Return(pooledArray2);
            GenericObjectPool.Return(pooledArray);
            return result;
        }

        /// <summary>
        /// Check if the Items selected are valid for the ingredient list.
        /// </summary>
        /// <param name="itemAmountIngredients">The Item amounts ingredients.</param>
        /// <param name="quantity">A multiplier of ingredient amount.</param>
        /// <param name="selectedIngredients">The Item infos to compare against the ingredients.</param>
        /// <param name="itemAmountsToIgnore">The ItemAmounts that were already compared and taken into account.</param>
        /// <param name="itemAmountsToIgnoreCount">The ignored ItemAmounts count.</param>
        /// <returns>True if the ItemAmounts have enough valid ingredients.</returns>
        protected virtual bool CheckIfEnoughItemIngredients(ListSlice<ItemAmount> itemAmountIngredients,
            int quantity, ListSlice<ItemInfo> selectedIngredients,
            ref ItemInfo[] itemAmountsToIgnore, ref int itemAmountsToIgnoreCount)
        {
            return CheckIfEnoughIngredientGeneric(itemAmountIngredients,
                quantity, selectedIngredients,
                ref itemAmountsToIgnore, ref itemAmountsToIgnoreCount,
                x_ItemAmount => x_ItemAmount.Amount,
                (x_ItemAmount, x_item) => x_ItemAmount.Item.ValueEquivalentTo(x_item));
        }

        /// <summary>
        /// Check if the Items selected are valid for the ingredient list.
        /// </summary>
        /// <param name="itemDefinitionAmountIngredients">The itemDefinition ingredients.</param>
        /// <param name="quantity">A multiplier of ingredient amount.</param>
        /// <param name="selectedIngredients">The Item infos to compare against the ingredients.</param>
        /// <param name="itemAmountsToIgnore">The ItemAmounts that were already compared and taken into account.</param>
        /// <param name="itemAmountsToIgnoreCount">The ignored ItemAmounts count.</param>
        /// <returns>True if the ItemAmounts have enough valid ingredients.</returns>
        protected virtual bool CheckIfEnoughItemDefinitionIngredients(ListSlice<ItemDefinitionAmount> itemDefinitionAmountIngredients,
            int quantity, ListSlice<ItemInfo> selectedIngredients,
            ref ItemInfo[] itemAmountsToIgnore, ref int itemAmountsToIgnoreCount)
        {
            return CheckIfEnoughIngredientGeneric(itemDefinitionAmountIngredients,
                quantity, selectedIngredients,
                ref itemAmountsToIgnore, ref itemAmountsToIgnoreCount,
                x_ItemDefAmount => x_ItemDefAmount.Amount,
                (x_ItemDefAmount, x_item) => x_ItemDefAmount.ItemDefinition == x_item.ItemDefinition);
        }

        /// <summary>
        /// Check if the Items selected are valid for the ingredient list.
        /// </summary>
        /// <param name="itemCategoryAmountIngredients">The itemCategoryAmountIngredients.</param>
        /// <param name="quantity">A multiplier of ingredient amount.</param>
        /// <param name="selectedIngredients">The Item infos to compare against the ingredients.</param>
        /// <param name="itemAmountsToIgnore">The ItemAmounts that were already compared and taken into account.</param>
        /// <param name="itemAmountsToIgnoreCount">The ignored ItemAmounts count.</param>
        /// <returns>True if the ItemAmounts have enough valid ingredients.</returns>
        protected virtual bool CheckIfEnoughItemCategoryIngredients(ListSlice<ItemCategoryAmount> itemCategoryAmountIngredients,
            int quantity, ListSlice<ItemInfo> selectedIngredients,
            ref ItemInfo[] itemAmountsToIgnore, ref int itemAmountsToIgnoreCount)
        {
            return CheckIfEnoughIngredientGeneric(itemCategoryAmountIngredients,
                quantity, selectedIngredients,
                ref itemAmountsToIgnore, ref itemAmountsToIgnoreCount,
                x_ItemCatAmount => x_ItemCatAmount.Amount,
                (x_ItemCatAmount, x_item) => x_ItemCatAmount.ItemCategory.InherentlyContains(x_item));
        }

        /// <summary>
        /// Craft the items.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="selectedIngredients">The item infos selected.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <returns>True if you can craft.</returns>
        protected override CraftingResult CraftInternal(CraftingRecipe recipe, IInventory inventory,
            ListSlice<ItemInfo> selectedIngredients, int quantity)
        {
            if (CanCraftInternal(recipe, inventory, selectedIngredients, quantity) == false) {
                return new CraftingResult(null, false);
            }

            if (RemoveIngredients(inventory, selectedIngredients) == false) {
                return new CraftingResult(null, false);
            }

            var output = CreateCraftingOutput(recipe, inventory, quantity);

            return new CraftingResult(output, true);
        }

        /// <summary>
        /// Creates the crafting output.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory.</param>
        /// <param name="quantity">The quantity.</param>
        /// <returns>The crafting output.</returns>
        protected virtual CraftingOutput CreateCraftingOutput(CraftingRecipe recipe, IInventory inventory, int quantity)
        {
            var resultItemAmounts = new ItemAmount[recipe.DefaultOutput.ItemAmounts.Count];

            for (int i = 0; i < resultItemAmounts.Length; i++) {
                var itemAmount = recipe.DefaultOutput.ItemAmounts[i];
                var itemCopy = new ItemAmount(InventorySystemManager.CreateItem(itemAmount.Item), itemAmount.Amount * quantity);
                resultItemAmounts[i] = itemCopy;
                inventory.AddItem((ItemInfo)itemCopy);
            }

            return new CraftingOutput(resultItemAmounts);
        }

        /// <summary>
        /// Removes the ingredients from the inventory.
        /// </summary>
        /// <param name="inventory">The inventory.</param>
        /// <param name="selectedIngredients">The ingredients.</param>
        /// <param name="ItemAmountsCount">The count of ingredients.</param>
        /// <returns>Returns false if an ingredient could not be removed.</returns>
        protected virtual bool RemoveIngredients(IInventory inventory, ListSlice<ItemInfo> selectedIngredients)
        {
            if (m_ExternallyRemoveIngredients) {
                var success = false;

                EventHandler.ExecuteEvent<CraftingProcessor, ListSlice<ItemInfo>, Action<bool>>(inventory.gameObject,
                    EventNames.c_InventoryGameObject_OnCraftRemoveItem_CraftingProecessor_ItemInfoListSlice_ActionBoolSucces,
                    this, selectedIngredients, (result) =>
                    {
                        success = result;
                    });

                return success;
            }

            for (var i = 0; i < selectedIngredients.Count; i++) {
                var itemAmount = selectedIngredients[i];
                if (inventory.MainItemCollection.RemoveItem(itemAmount.Item, itemAmount.Amount).Amount == itemAmount.Amount) { continue; }
                return false;
            }

            return true;
        }
    }
}
