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


    public bool notDestroyWhenDie;

    public bool resetHealthOnSceneReload;

    //[Serializable]
    public class setHpEvent : UnityEvent<HpDamable>
    { }

    public DamageEvent onDieEvent;

    public setHpEvent onHpChange=new setHpEvent();

    public ParticleSystem hurt = default;

    private uint buffindex;



    public override void takeDamage(DamagerBase damager)
    {

        if ( currentHp <= 0)
        {
           // return;
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
            die(damager);
        }
    }

    /// <summary>
    /// use this to change currentHp,number can be negative to reduce currentHp
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
    /// </summary>
    /// <param name="number"></param>
    /// <param name="damager"></param>
    public void addTempHp(int number, DamagerBase damager)
    {
        if (number == 0) return;
        setTempHp(tempHp + number,damager);
    }

    public bool isDead()
    {
        return CurrentHp <= 0 && TempHp <= 0;
    }

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







}
