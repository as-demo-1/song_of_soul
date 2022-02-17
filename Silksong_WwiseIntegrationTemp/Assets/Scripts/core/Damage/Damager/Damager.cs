using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// 仅造成伤害的damager
/// </summary>
/// 
public class Damager : DamagerBase
{

    protected override void makeDamage(DamageableBase Damageable)
    {
        makeDamageEvent.Invoke(this, Damageable);
    }



}
