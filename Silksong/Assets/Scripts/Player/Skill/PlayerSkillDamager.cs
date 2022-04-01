using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillDamager : DamagerBase
{
    protected override void makeDamage(DamageableBase Damageable)
    {
        makeDamageEvent.Invoke(this, Damageable);
    }
}
