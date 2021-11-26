using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
/// <summary>
/// 
/// damageable的抽象基类 
/// </summary>作者：青瓜
public abstract class DamageableBase : MonoBehaviour
{
    public bool invulnerable;//是否无敌

    [HideInInspector]
    public Vector2 damageDirection;//伤害来源的方向

    public bool playerAttackCanGainSoul;//玩家攻击时能得到魂

    public abstract void takeDamage(DamagerBase damager);//受到伤害时调用

    [Serializable]
    public class DamageEvent : UnityEvent<DamagerBase, DamageableBase>
    { }

    public DamageEvent takeDamageEvent;

    protected virtual void Awake()//在awake时检测刚体 完成设定
    {
        setRigidbody2D();
    }

    protected void setRigidbody2D()//ontriggerEnter的触发需要至少一方有刚体  这里保证damable有刚体
    {
        if(GetComponent<Rigidbody2D>()==null)//如果没有手动设定的刚体 则添加一个static刚体
        {
            gameObject.AddComponent<Rigidbody2D>();
            gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;//默认添加static刚体 用于机关、墙壁  Dynamic刚体需要手动添加
        }
        gameObject.GetComponent<Rigidbody2D>().sleepMode = RigidbodySleepMode2D.NeverSleep;//如果刚体进入睡眠状态就无法响应碰撞
        
    }

}
