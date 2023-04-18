/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Damageable
{
    /// <summary>
    /// The damageable interface.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// The max Hp.
        /// </summary>
        int MaxHp { get; }

        /// <summary>
        /// The current Hp.
        /// </summary>
        int CurrentHp { get; }

        /// <summary>
        /// Damage this damageable.
        /// </summary>
        /// <param name="amount">The damage amount.</param>
        void TakeDamage(int amount);

        /// <summary>
        /// Heal by the amount specified.
        /// </summary>
        /// <param name="amount">The heal amount.</param>
        /// <param name="notify">Notify the listeners about the heal?</param>
        void Heal(int amount, bool notify = true);

        /// <summary>
        /// Kill this damageable.
        /// </summary>
        void Die();
    }
}