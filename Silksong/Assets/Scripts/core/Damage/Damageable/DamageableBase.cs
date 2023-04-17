using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

[Serializable]
public class DamageEvent : UnityEvent<DamagerBase, DamageableBase>
{ }

public abstract class DamageableBase : MonoBehaviour
{
    public bool invulnerable;

    [HideInInspector]
    public Vector2 damageDirection;

    public bool playerAttackCanGainSoul;

    public float beatBackRate;

    public abstract void takeDamage(DamagerBase damager);


    public DamageEvent takeDamageEvent = new DamageEvent();

    public AudioCue takeDamageAudio;

    public SfxSO takeDamageSfxSO;


    protected virtual void Awake()
    {
        setRigidbody2D();
    }

    protected void setRigidbody2D()
    {
        if(GetComponent<Rigidbody2D>()==null)
        {
            gameObject.AddComponent<Rigidbody2D>();
            gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }
        gameObject.GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

}
