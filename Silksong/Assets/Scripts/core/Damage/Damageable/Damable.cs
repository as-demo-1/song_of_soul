<<<<<<< HEAD

using UnityEngine;
public class Damable : DamageableBase
{
    protected override void Awake()
    {
        base.Awake();     
    }
    public override void takeDamage(DamagerBase damager)
    {
        damageDirection = damager.transform.position - transform.position;
        takeDamageEvent.Invoke(damager,this);

        if(takeDamageAudio)
        {
            takeDamageAudio.PlayAudioCue();
        }

        if(takeDamageSfxSO)
        {
            //Debug.Log("creat hitted sfx");
            Vector2 hittedPosition = Vector2.zero;

            hittedPosition=GetComponent<Collider2D>().bounds.ClosestPoint(damager.transform.position);

            SfxManager.Instance.creatHittedSfx(hittedPosition, hittedPosition-(Vector2)transform.position ,takeDamageSfxSO);
        }

    }

=======
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 作者：青瓜
/// 最简单的damable
/// </summary>
/// 
public class Damable : DamageableBase
{
    public override void takeDamage(DamagerBase damager)
    {
        hittedEffect();//或者是event.invoke()
    }

    protected virtual void hittedEffect()//受击效果 或有必要以事件形式触发
    {
        //Debug.Log(gameObject.name + " is hitted");
    }
>>>>>>> 30f6fd9d (damage test)

}
