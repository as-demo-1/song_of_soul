/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.Shared.Utility;
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Interface for filters and sorters.
    /// </summary>
    public interface IFilterSorter { }

    /// <summary>
    /// Interface for filters and sorters.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public interface IFilterSorter<T> : IFilterSorter
    {
        /// <summary>
        /// Filter the list.
        /// </summary>
        /// <param name="input">The input list.</param>
        /// <param name="outputPooledArray">The reference to an output array.</param>
        /// <returns>The filtered/sorted list.</returns>
        ListSlice<T> Filter(ListSlice<T> input, ref T[] outputPooledArray);
    }

    /// <summary>
    /// Component for filtering and sorting.
    /// </summary>
    public abstract class FilterSorter : MonoBehaviour, IFilterSorter
    { }

    /// <summary>
    /// Component for filtering and sorting.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    public abstract class FilterSorter<T> : FilterSorter, IFilterSorter<T>
    {
        /// <summary>
        /// Filter the list.
        /// </summary>
        /// <param name="input">The input list.</param>
        /// <param name="outputPooledArray">The reference to an output array.</param>
        /// <returns>The filtered/sorted list.</returns>
        public abstract ListSlice<T> Filter(ListSlice<T> input, ref T[] outputPooledArray);

        /// <summary>
        /// Can the list contain the element.
        /// </summary>
        /// <param name="input">The element.</param>
        /// <returns>True if it can contain the element.</returns>
        public abstract bool CanContain(T input);
    }

    /// <summary>
    /// Filter and sorter for Item Infos.
    /// </summary>
    public abstract class ItemInfoFilterSorterBase : FilterSorter<ItemInfo>
    {
        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return GetType().Name;
        }
    }

    /// <summary>
    /// Filter forItem Infos.
    /// </summary>
    public abstract class ItemInfoFilterBase : ItemInfoFilterSorterBase
    {
        /// <summary>
        /// Filter the item info.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>True if the list can contain the item info.</returns>
        public abstract bool Filter(ItemInfo itemInfo);

        /// <summary>
        /// Filter the list.
        /// </summary>
        /// <param name="input">The input list.</param>
        /// <param name="outputPooledArray">The reference to an output array.</param>
        /// <returns>The filtered/sorted list.</returns>
        public override ListSlice<ItemInfo> Filter(ListSlice<ItemInfo> input, ref ItemInfo[] outputPooledArray)
        {
            outputPooledArray.ResizeIfNecessary(ref outputPooledArray, input.Count);

            var count = 0;
            for (int i = 0; i < input.Count; i++) {
                if (Filter(input[i]) == false) { continue; }

                outputPooledArray[count] = input[i];
                count++;
            }

            return (outputPooledArray, 0, count);
        }

        /// <summary>
        /// Filter the item info.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>True if the list can contain the item info.</returns>
        public override bool CanContain(ItemInfo input)
        {
            return Filter(input);
        }
    }

    public abstract class ItemInfoSorterBase : ItemInfoFilterSorterBase
    {
        public abstract Comparer<ItemInfo> Comparer { get; }

        /// <summary>
        /// Virtual awake is overriden.
        /// </summary>
        protected virtual void Awake()
        { }

        /// <summary>
        /// Filter the list.
        /// </summary>
        /// <param name="input">The input list.</param>
        /// <param name="outputPooledArray">The reference to an output array.</param>
        /// <returns>The filtered/sorted list.</returns>
        public override ListSlice<ItemInfo> Filter(ListSlice<ItemInfo> input, ref ItemInfo[] outputPooledArray)
        {
            outputPooledArray.ResizeIfNecessary(ref outputPooledArray, input.Count);
            var count = input.Count;

            for (int i = 0; i < input.Count; i++) {
                outputPooledArray[i] = input[i];
            }

            Array.Sort(outputPooledArray, 0, count, Comparer);

            return (outputPooledArray, 0, count);
        }

        /// <summary>
        /// Filter the item info.
        /// </summary>
        /// <param name="itemInfo">The item info.</param>
        /// <returns>True if the list can contain the item info.</returns>
        public override bool CanContain(ItemInfo input)
        {
            return true;
        }
    }
}