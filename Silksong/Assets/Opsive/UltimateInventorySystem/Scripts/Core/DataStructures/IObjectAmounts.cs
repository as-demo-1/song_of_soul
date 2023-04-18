/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.DataStructures
{
    /// <summary>
    /// Interface used by an array of object-amount tuple
    /// </summary>
    /// <typeparam name="T">T must be a IObjectAmount.</typeparam>
    public interface IObjectAmounts
    {
        /// <summary>
        /// Removes the item at the index provided from the array by shifting the content and reducing the count.
        /// </summary>
        /// <param name="index">The index of the item that should be removed.</param>
        void RemoveAt(int index);

        /// <summary>
        /// Returns a readable format of the array. 
        /// </summary>
        /// <returns>The names and amounts of the objects in the list.</returns>
        string ToString();
    }

    /// <summary>
    /// Interface used by an array of object-amount tuple
    /// </summary>
    /// <typeparam name="T">T must be a IObjectAmount.</typeparam>
    public interface IObjectAmounts<T> : IObjectAmounts where T : IObjectAmount
    {
        /// <summary>
        /// The array of object amounts.
        /// </summary>
        T[] Array { get; }

        /// <summary>
        /// Add an item to the array.
        /// </summary>
        /// <param name="item">The item that will be added to the array.</param>
        /// <return>The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection.</return>
        int Add(T item);
    }
}