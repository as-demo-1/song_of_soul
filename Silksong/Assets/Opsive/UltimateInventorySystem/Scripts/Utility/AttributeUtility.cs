/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Utility
{
    using System.Collections.Generic;
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.DataStructures;

    /// <summary>
    /// A static class used to solve common functions related to attributes.
    /// </summary>
    public static class AttributeUtility
    {
        /// <summary>
        /// Get the numeric value of an attribute whether it is int or float.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="item">The item with the attribute.</param>
        /// <returns>The numeric value.</returns>
        private static float GetNumericValue(string attributeName, Item item)
        {
            float stat = 0;
            if (item.TryGetAttributeValue<int>(attributeName, out var intAttributeValue)) {
                stat += intAttributeValue;
            }
            if (item.TryGetAttributeValue<float>(attributeName, out var floatAttributeValue)) {
                stat += floatAttributeValue;
            }

            return stat;
        }
        
        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static int GetIntSum(string attributeName, IReadOnlyList<ItemInfo> itemList)
        {
            return (int)GetFloatSum(attributeName, itemList);
        }

        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static float GetFloatSum(string attributeName, IReadOnlyList<ItemInfo> itemList)
        {
            var stat = 0f;
            for (int i = 0; i < itemList.Count; i++) {
                if (itemList[i].Item == null) { continue; }
                var item = itemList[i].Item;
                stat += GetNumericValue(attributeName, item);
            }
            return stat;
        }
        
        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static int GetIntSum(string attributeName, ListSlice<ItemInfo> itemList)
        {
            return (int)GetFloatSum(attributeName, itemList);
        }

        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static float GetFloatSum(string attributeName, ListSlice<ItemInfo> itemList)
        {
            var stat = 0f;
            for (int i = 0; i < itemList.Count; i++) {
                if (itemList[i].Item == null) { continue; }
                var item = itemList[i].Item;
                stat += GetNumericValue(attributeName, item);
            }
            return stat;
        }
        
        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static int GetIntSum(string attributeName, ListSlice<ItemStack> itemList)
        {
            return (int)GetFloatSum(attributeName, itemList);
        }

        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static float GetFloatSum(string attributeName, ListSlice<ItemStack> itemList)
        {
            var stat = 0f;
            for (int i = 0; i < itemList.Count; i++) {
                if (itemList[i].Item == null) { continue; }
                var item = itemList[i].Item;
                stat += GetNumericValue(attributeName, item);
            }
            return stat;
        }
        
        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static int GetIntSum(string attributeName, IReadOnlyList<ItemStack> itemList)
        {
            return (int)GetFloatSum(attributeName, itemList);
        }

        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static float GetFloatSum(string attributeName, IReadOnlyList<ItemStack> itemList)
        {
            var stat = 0f;
            for (int i = 0; i < itemList.Count; i++) {
                if (itemList[i].Item == null) { continue; }
                var item = itemList[i].Item;
                stat += GetNumericValue(attributeName, item);
            }
            return stat;
        }
        
        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static int GetIntSum(string attributeName, ListSlice<Item> itemList)
        {
            return (int)GetFloatSum(attributeName, itemList);
        }

        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static float GetFloatSum(string attributeName, ListSlice<Item> itemList)
        {
            var stat = 0f;
            for (int i = 0; i < itemList.Count; i++) {
                if (itemList[i] == null) { continue; }
                var item = itemList[i];
                stat += GetNumericValue(attributeName, item);
            }
            return stat;
        }

        

        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static int GetIntSum(string attributeName, IReadOnlyList<Item> itemList)
        {
            return (int)GetFloatSum(attributeName, itemList);
        }

        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static float GetFloatSum(string attributeName, IReadOnlyList<Item> itemList)
        {
            var stat = 0f;
            for (int i = 0; i < itemList.Count; i++) {
                if (itemList[i] == null) { continue; }
                var item = itemList[i];
                stat += GetNumericValue(attributeName, item);
            }
            return stat;
        }
        
        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static int GetIntSum(string attributeName, ListSlice<ItemDefinition> itemList)
        {
            return (int)GetFloatSum(attributeName, itemList);
        }

        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static float GetFloatSum(string attributeName, ListSlice<ItemDefinition> itemList)
        {
            var stat = 0f;
            for (int i = 0; i < itemList.Count; i++) {
                if (itemList[i] == null) { continue; }
                var item = itemList[i].DefaultItem;
                stat += GetNumericValue(attributeName, item);
            }
            return stat;
        }
        
        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static int GetIntSum(string attributeName, IReadOnlyList<ItemDefinition> itemList)
        {
            return (int)GetFloatSum(attributeName, itemList);
        }

        /// <summary>
        /// Get the Equipment stats by retrieving the total value of the attribute.
        /// </summary>
        /// <param name="attributeName">The attribute name.</param>
        /// <param name="itemList">The item list to loop through to get the attribute values from</param>
        /// <returns>The total amount for the attribute.</returns>
        public static float GetFloatSum(string attributeName, IReadOnlyList<ItemDefinition> itemList)
        {
            var stat = 0f;
            for (int i = 0; i < itemList.Count; i++) {
                if (itemList[i] == null) { continue; }
                var item = itemList[i].DefaultItem;
                stat += GetNumericValue(attributeName, item);
            }
            return stat;
        }
    }
}