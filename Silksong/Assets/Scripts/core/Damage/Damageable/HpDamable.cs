using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
/// <summary>
/// 
/// 拥有生命值及相关方法的damable 
/// </summary>作者：青瓜
public class HpDamable :Damable
{
    [SerializeField]
    private int maxHp ;//最大生命值
    public int MaxHp
    {
        get { return maxHp; }
        set { maxHp = value; }
    }

    [SerializeField]
    private int currentHp;//当前hp
    public int CurrentHp
    {
        get { return currentHp; }
    }
    

    public bool resetHealthOnSceneReload;

    [Serializable]
    public class dieEvent : UnityEvent<DamagerBase, DamageableBase>
    { }

    //[Serializable]
    public class setHpEvent : UnityEvent<HpDamable>
    { }

    public dieEvent onDieEvent;

    public setHpEvent onHpChange=new setHpEvent();

    public AudioCue dieAudio;//在audiomanager中有绑定怪物hpdamable默认《受击》音效的效果


    public override void takeDamage(DamagerBase damager)
    {
        if ( currentHp <= 0)
        {
           // return;
        }

        base.takeDamage(damager);
        addHp(-damager.getDamage(this),damager);

    }


    public void setCurrentHp(int hp,DamagerBase damager=null)
    {
        currentHp = Mathf.Clamp(hp,0,MaxHp);
        onHpChange.Invoke(this);
        if (currentHp == 0)
        {
            die(damager);
        }
    }

    public void addHp(int number,DamagerBase damager)//如受到伤害 number<0
    {
        setCurrentHp(currentHp + number,damager);
    }

    protected virtual void die(DamagerBase damager)
    {
        onDieEvent.Invoke(damager,this);

        if(gameObject.tag!="Player")//reborn player for  test
        Destroy(gameObject);//未完善

        Debug.Log(gameObject.name+" die");
        if (dieAudio)
        {
            dieAudio.PlayAudioCue();
        }

    }



}
