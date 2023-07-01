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
//shakeforce即为抖动大小，这里仅作方法调取和传参。