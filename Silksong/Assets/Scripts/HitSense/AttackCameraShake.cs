using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(DamagerBase))]
public class AttackCameraShake : CinemachineImpulseSource
{
    public LayerMask targetLayer;
    private void Awake()
    {
        GetComponent<DamagerBase>().makeDamageEvent.AddListener(attackCameraShake);
    }
public void attackCameraShake(DamagerBase damager, DamageableBase damageable)
    {
        if (targetLayer.Contains(damageable.gameObject)) 
        {
            // GenerateImpulse(GetComponent<DamageableBase>().damageDirection.normalized);
            //GenerateImpulse(Vector2.zero);
            GenerateImpulse();
            // GenerateImpulse(Vector2.down * 5);
            // GenerateImpulse(Vector2.down * -5);
            //Debug.Log("die shacke");
        }

    }
}
