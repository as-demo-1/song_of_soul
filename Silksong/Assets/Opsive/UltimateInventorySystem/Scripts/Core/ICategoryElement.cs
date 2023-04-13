/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using UnityEngine;

    /// <summary>
    /// The interface for a category element.
    /// </summary>
    /// <typeparam name="Tc">The category type.</typeparam>
    /// <typeparam name="Te">The element type.</typeparam>
    public interface ICategoryElement<Tc, Te>
        where Tc : ObjectCategoryBase<Tc, Te>
        where Te : ScriptableObject, ICategoryElement<Tc, Te>
    {
        /// <summary>
        /// Get the category.
        /// </summary>
        Tc Category { get; }
    }
}