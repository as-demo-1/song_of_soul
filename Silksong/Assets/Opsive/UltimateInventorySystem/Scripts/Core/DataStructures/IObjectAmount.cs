/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core.DataStructures
{
    /// <summary>
    /// The base interface for an object with an amount.
    /// </summary>
    public interface IObjectAmount
    {
        /// <summary>
        /// The amount of the object.
        /// </summary>
        int Amount { get; }
    }

    /// <summary>
    /// The generic interface for an object with an amount.
    /// </summary>
    public interface IObjectAmount<T> : IObjectAmount where T : class
    {
        /// <summary>
        /// The object of the amount.
        /// </summary>
        T Object { get; }
    }
}