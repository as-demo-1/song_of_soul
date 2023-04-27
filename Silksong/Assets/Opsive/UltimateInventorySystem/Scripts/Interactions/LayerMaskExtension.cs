/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Interactions
{
    using UnityEngine;

    /// <summary>
    /// An extension that checks if a layer mask contains a layer.
    /// </summary>
    public static class LayerMaskExtension
    {
        /// <summary>
        /// Check if the layer mask contains a layer.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <param name="layer">The layer.</param>
        /// <returns>True if the layer is contained.</returns>
        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }

        /// <summary>
        /// Check if the layer mask contains the game objects layer.
        /// </summary>
        /// <param name="mask">The layer mask.</param>
        /// <param name="go">The game object.</param>
        /// <returns>True if the game object is contained.</returns>
        public static bool Contains(this LayerMask mask, GameObject go)
        {
            return mask == (mask | (1 << go.layer));
        }
    }
}