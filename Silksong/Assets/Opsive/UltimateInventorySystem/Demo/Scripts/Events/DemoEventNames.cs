/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.Events
{
    /// <summary>
    /// Demo event names, used by the EventHandler.
    /// </summary>
    public static class DemoEventNames
    {
        public const string c_Damageable_OnHpChange_Damageable = "Damageable_HpChange";
        public const string c_Damageable_OnTakeDamage_Damageable_Int = "Damageable_TakeDamage";
        public const string c_Damageable_OnHeal_Damageable_Int = "Damageable_Heal";
        public const string c_Damageable_OnDie_Damageable = "Damageable_Die";

        public const string c_CharacterStats_OnChanged = "CharacterStats_Changed";
    }
}
