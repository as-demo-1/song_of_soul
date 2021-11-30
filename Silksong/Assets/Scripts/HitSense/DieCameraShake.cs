using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(HpDamable))]
public class DieCameraShake : CinemachineImpulseSource
{
  public void dieCameraShake(DamagerBase damager,DamageableBase damageable)
    {
        // GenerateImpulse(GetComponent<DamageableBase>().damageDirection.normalized);
        //GenerateImpulse(Vector2.zero);
         GenerateImpulse();
       // GenerateImpulse(Vector2.down * 5);
       // GenerateImpulse(Vector2.down * -5);
        //Debug.Log("die shacke");
    }

    private void Awake()
    {
        GetComponent<HpDamable>().onDieEvent.AddListener(dieCameraShake);
    }
}
