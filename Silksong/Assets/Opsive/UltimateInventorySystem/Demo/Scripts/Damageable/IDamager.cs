/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Damageable
{
    /// <summary>
    /// The damager interface.
    /// </summary>
    public interface IDamager
    {
        /// <summary>
        /// Damage the damageable.
        /// </summary>
        /// <param name="damageable">The damageable to damage.</param>
        void Damage(IDamageable damageable);
    }
}