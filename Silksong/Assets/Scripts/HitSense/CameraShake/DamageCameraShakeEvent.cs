using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DamageCameraShakeEvent : damageEventSetter
{
    //[SerializeField] private Vector2 shakeForce;
    [SerializeField] private float shakeForce;
    public override void damageEvent(DamagerBase damager, DamageableBase damageable)
    {
        CameraShakeManager.Instance.cameraShake(shakeForce);
    }
}
