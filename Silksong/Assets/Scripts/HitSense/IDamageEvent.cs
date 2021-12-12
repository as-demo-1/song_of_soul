using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageEvent
{
    void damageEvent(DamagerBase damager, DamageableBase damageable);

}
