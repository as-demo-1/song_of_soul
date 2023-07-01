using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.Events;
using Cinemachine.Utility;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

public class HpDamable :Damable
{
    [SerializeField]
    private int maxHp ;
    public int MaxHp
    {
        get { return maxHp; }
        set { maxHp = value; }
    }

    [SerializeField]
    private int currentHp;
    public int CurrentHp
    {
        get { return currentHp; }
    }

    private int tempHp;
    public int TempHp
    {
        get { return tempHp; }
    }
    //同步当前血量以及临时血量

    public bool notDestroyWhenDie;

    public bool resetHealthOnSceneReload;

    //[Serializable]
    public class setHpEvent : UnityEvent<HpDamable>
    { }

    public DamageEvent onDieEvent;

    public setHpEvent onHpChange=new setHpEvent();

    public ParticleSystem hurt = default;

    private Dictionary<BuffType, Buff> _buffs = new Dictionary<BuffType, Buff>();

    public GameObject electricMarkPref = default; // 闪电标记

    private uint buffindex;



    public override void takeDamage(DamagerBase damager)
    {

        if ( currentHp <= 0)
        {
           // return;
           //这里应该是如果低于0执行的是寄了的方法而不是受击方法
        }
        if (hurt)
        {
            Destroy(Instantiate(hurt, transform).gameObject, 1.0f);
        }

        base.takeDamage(damager);

        countDamageNumber(damager);
    }

    private void countDamageNumber(DamagerBase damager)
    {
        int damage = damager.getDamage(this);
        int damageForTempHp = Mathf.Clamp(damage, 0, tempHp);
        int damageForCurrentHp = damage - damageForTempHp;

        addTempHp(-damageForTempHp,damager);
        addCurrentHp(-damageForCurrentHp, damager);
        //在这里进行血量统计和更新
    }

   /* public void takeDamage(int number)
    {
        if (currentHp <= 0)
        {
            // return;
        }
        if (hurt)
        {
            Destroy(Instantiate(hurt, transform), 3.0f);
        }
        takeDamageEvent.Invoke(null, this);

        setCurrentHp(currentHp - number);
    }
   */
    public void setCurrentHp(int val,DamagerBase damager=null)
    {
        currentHp = Mathf.Clamp(val,0,MaxHp);

        onHpChange.Invoke(this);

        if (isDead())
        {
            die(damager);//果然死亡被单独拿出来了
        }
    }

    /// <summary>
    /// use this to change currentHp,number can be negative to reduce currentHp
    /// 加血方法，number是加血量，负数时扣血
    /// 不确定为啥加血会在damable，可能是把血量相关的都放在这里了
    /// </summary>
    /// <param name="number"></param>
    /// <param name="damager"></param>
    public void addCurrentHp(int number, DamagerBase damager)
    {
        if (number == 0) return;
        setCurrentHp(currentHp + number, damager);

    }

    public void setTempHp(int val, DamagerBase damager=null)
    {
        if (val< 0) val = 0;
        tempHp = val;

        onHpChange.Invoke(this);

        if (isDead())
        {
            die(damager);
        }
    }

    /// <summary>
    /// use this to change tempHp,number can be negative to reduce tempHp
    /// 所以temphp这个临时血量的功能是啥来着，需要确认
    /// </summary>
    /// <param name="number"></param>
    /// <param name="damager"></param>
    public void addTempHp(int number, DamagerBase damager)
    {
        if (number == 0) return;
        setTempHp(tempHp + number,damager);
    }

    /// <summary>
    /// 查询似了没，没啥好说的
    /// </summary>
    /// <returns></returns>
    public bool isDead()
    {
        return CurrentHp <= 0 && TempHp <= 0;
    }

    /// <summary>
    /// 似了的话确认是否摧毁玩家对象，并且存档
    /// </summary>
    /// <param name="damager"></param>
    protected virtual void die(DamagerBase damager)
    {
        onDieEvent.Invoke(damager, this);
        damager.killDamableEvent.Invoke(damager, this);

        if (!notDestroyWhenDie)
            Destroy(gameObject);

        Debug.Log(gameObject.name + " die");

        GamingSaveObj<bool> gamingSave;
        if (TryGetComponent(out gamingSave) && !gamingSave.ban)
        {
            gamingSave.saveGamingData(true);
        }

    }

    /// <summary>
    /// 这个getbuff应该是老方法，里面switch明显是为后续开发留口子了，但是只有electricmask留在这里
    /// </summary>
    /// <param name="buffType"></param>
    public void GetBuff(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.ElectricMark:
                ElectricMark eBuff = (ElectricMark)_buffs[BuffType.ElectricMark];
                eBuff.AddOneLayer();
                buffindex = ElectricMark.GetCurrentIndex();
                ElectricMark.AddTarget(buffindex, this);
                ElectricMark.counter++;
                break;
            default:
                break;
        }
    }
    
    /// <summary>
    /// 同上，取消buff也只有electricmask
    /// </summary>
    /// <param name="buffType"></param>
    public void RemoveBuff(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.ElectricMark:
                ElectricMark eBuff = (ElectricMark)_buffs[BuffType.ElectricMark];
                eBuff.ResetLayer();
                eBuff.HidePerformance();
                ElectricMark.RemoveTarget(buffindex);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 这个判断buff赋予条件
    /// </summary>
    /// <param name="buffType"></param>
    /// <returns></returns>
    public bool CanGetBuff(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.ElectricMark:
                if (_buffs.ContainsKey(BuffType.ElectricMark))
                {
                    ElectricMark eBuff = (ElectricMark)_buffs[BuffType.ElectricMark];
                    return eBuff.GetLayerNum() < 1;
                }
                else
                {
                    _buffs.Add(BuffType.ElectricMark, new ElectricMark());
                    return false;
                }
            default:
                return false;
        }
    }

    /// <summary>
    /// 判断现有buff状态
    /// </summary>
    /// <param name="buffType"></param>
    /// <returns></returns>
    public bool HaveBuff(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.ElectricMark:
                if (_buffs.ContainsKey(BuffType.ElectricMark))
                {
                    ElectricMark eBuff = (ElectricMark)_buffs[BuffType.ElectricMark];
                    return eBuff.GetLayerNum() > 0;
                }
                return false;
            default:
                return false;
        }
    }

}
