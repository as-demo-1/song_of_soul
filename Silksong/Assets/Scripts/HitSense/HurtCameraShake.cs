using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(DamageableBase))]
public class HurtCameraShake : CinemachineImpulseSource
{
    private void Awake()
    {
        GetComponent<DamageableBase>().takeDamageEvent.AddListener(hurtCameraShake);
    }
    public void hurtCameraShake(DamagerBase damager, DamageableBase damageable)
    {

        // GenerateImpulse(GetComponent<DamageableBase>().damageDirection.normalized);
        //GenerateImpulse(Vector2.zero);
        GenerateImpulse();
        // GenerateImpulse(Vector2.down * 5);
        // GenerateImpulse(Vector2.down * -5);
        //Debug.Log("die shacke");
    }
}
