/// ---------------------------------------------
/// Ultimate Inventory System.
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Crafting
{
    using Opsive.Shared.Events;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;

    /// <summary>
    /// Base processor used to craft recipes
    /// </summary>
    public abstract class CraftingProcessor
    {

        /// <summary>
        /// Automatically select the items that will be used as ingredients in the crafting process from the inventory.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <param name="resultSelectedItemInfos">Reference to an array of item infos, can be resized up.</param>
        /// <param name="selectedIngredientsItemInfos">Output of a wrapper containing the result array.</param>
        /// <returns>True if all the requested ingredients were found in the inventory.</returns>
        public abstract bool TryAutoSelectIngredients(CraftingRecipe recipe, IInventory inventory, int quantity,
            ref ItemInfo[] resultSelectedItemInfos, out ListSlice<ItemInfo> selectedIngredientsItemInfos);

        /// <summary>
        /// Validate that the recipe was correctly set.
        /// </summary>
        /// <param name="recipe">The recipe to validate.</param>
        /// <returns>True if the recipe is valid.</returns>
        protected virtual bool ValidateRecipe(CraftingRecipe recipe)
        {
            if (recipe?.Ingredients?.ItemDefinitionAmounts?.Array == null) { return false; }
            if (recipe?.Ingredients?.ItemAmounts?.Array == null) { return false; }
            if (recipe?.Ingredients?.ItemCategoryAmounts?.Array == null) { return false; }
            if (recipe?.DefaultOutput?.ItemAmounts?.Array == null) { return false; }

            return true;
        }

        /// <summary>
        /// Check if the parameters are valid to craft an item.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <returns>True if you can craft.</returns>
        public bool CanCraft(CraftingRecipe recipe, IInventory inventory, int quantity = 1)
        {
            if (ValidateRecipe(recipe) == false) { return false; }

            var pooledArray = GenericObjectPool.Get<ItemInfo[]>();
            var result = TryAutoSelectIngredients(recipe, inventory, quantity, ref pooledArray,
                out var selectedIngredients);

            if (result) {
                result = CanCraftInternal(recipe, inventory, selectedIngredients, quantity);
            }

            GenericObjectPool.Return(pooledArray);

            if (inventory != null && inventory.gameObject != null) {
                EventHandler.ExecuteEvent(inventory.gameObject, EventNames.c_InventoryGameObject_OnCanCraft_CraftingRecipe_Bool, recipe, result);
            }

            return result;
        }

        /// <summary>
        /// Check if the parameters are valid to craft an item.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="selectedIngredients">The item infos selected.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <returns>True if you can craft.</returns>
        public bool CanCraft(CraftingRecipe recipe, IInventory inventory, ListSlice<ItemInfo> selectedIngredients,
            int quantity = 1)
        {
            if (ValidateRecipe(recipe) == false) { return false; }
            if (selectedIngredients.IsNull) { return false; }
            var result = CanCraftInternal(recipe, inventory, selectedIngredients, quantity);

            if (inventory != null && inventory.gameObject != null) {
                EventHandler.ExecuteEvent(inventory.gameObject, EventNames.c_InventoryGameObject_OnCanCraft_CraftingRecipe_Bool, recipe, result);
            }

            return result;
        }

        /// <summary>
        /// Check if the parameters are valid to craft an item.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="selectedIngredients">The item infos selected.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <returns>True if you can craft.</returns>
        protected abstract bool CanCraftInternal(CraftingRecipe recipe, IInventory inventory,
            ListSlice<ItemInfo> selectedIngredients, int quantity);

        /// <summary>
        /// Craft the items.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <returns>Returns a crafting result.</returns>
        public CraftingResult Craft(CraftingRecipe recipe, IInventory inventory, int quantity = 1)
        {
            var pooledArray = GenericObjectPool.Get<ItemInfo[]>();
            TryAutoSelectIngredients(recipe, inventory, quantity, ref pooledArray,
                out var selectedIngredients);

            var result = Craft(recipe, inventory, selectedIngredients, quantity);

            GenericObjectPool.Return(pooledArray);
            return result;
        }

        /// <summary>
        /// Craft the items.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="selectedIngredients">The items selected to craft the recipe.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <returns>Returns a crafting result.</returns>
        public CraftingResult Craft(CraftingRecipe recipe, IInventory inventory, ListSlice<ItemInfo> selectedIngredients,
            int quantity = 1)
        {
            var craftingResult = selectedIngredients.IsNull
                ? new CraftingResult(null, false)
                : CraftInternal(recipe, inventory, selectedIngredients, quantity);

            if (inventory != null && inventory.gameObject != null) {
                EventHandler.ExecuteEvent(inventory.gameObject, EventNames.c_InventoryGameObject_OnCraft_CraftingRecipe_CraftingResult, recipe, craftingResult);
            }

            return craftingResult;
        }

        /// <summary>
        /// Craft the items.
        /// </summary>
        /// <param name="recipe">The recipe.</param>
        /// <param name="inventory">The inventory containing the items.</param>
        /// <param name="selectedIngredients">The item infos selected.</param>
        /// <param name="quantity">The quantity to craft.</param>
        /// <returns>True if you can craft.</returns>
        protected abstract CraftingResult CraftInternal(CraftingRecipe recipe, IInventory inventory,
            ListSlice<ItemInfo> selectedIngredients, int quantity);
    }
}