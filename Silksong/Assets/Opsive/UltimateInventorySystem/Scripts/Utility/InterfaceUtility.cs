/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Utility
{
    /// <summary>
    /// Utility class for interfaces.
    /// </summary>
    public static class InterfaceUtility
    {
        /// <summary>
        /// Check null on interface which could be a mono behaviour.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>True if null.</returns>
        public static bool IsNull<T>(T obj) where T : class
        {
            return obj == null || obj.Equals(null);
        }

        /// <summary>
        /// Check not null on interface which could be a mono behaviour.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>True if not null.</returns>
        public static bool IsNotNull<T>(T obj) where T : class
        {
            return obj != null && !obj.Equals(null);
        }
    }
}