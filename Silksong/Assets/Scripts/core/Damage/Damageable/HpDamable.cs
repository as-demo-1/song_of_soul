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
    

    public bool resetHealthOnSceneReload;

    [Serializable]
    public class dieEvent : UnityEvent<DamagerBase, DamageableBase>
    { }

    //[Serializable]
    public class setHpEvent : UnityEvent<HpDamable>
    { }

    public dieEvent onDieEvent;

    public setHpEvent onHpChange=new setHpEvent();

    public ParticleSystem hurt = default;
    


    public override void takeDamage(DamagerBase damager)
    {

        if ( currentHp <= 0)
        {
           // return;
        }
        if (hurt)
        {
            Destroy(Instantiate(hurt, transform), 3.0f);
        }

        base.takeDamage(damager);
        addHp(-damager.getDamage(this),damager);
        
        
    }

    public void takeDamage(int number)
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


    public void setCurrentHp(int hp,DamagerBase damager=null)
    {
        currentHp = Mathf.Clamp(hp,0,MaxHp);
        onHpChange.Invoke(this);
        if (currentHp == 0)
        {
            die(damager);
        }
    }

    public void addHp(int number,DamagerBase damager)
    {
        setCurrentHp(currentHp + number,damager);
    }

    protected virtual void die(DamagerBase damager)
    {
        onDieEvent.Invoke(damager,this);

        if(gameObject.tag!="Player")
        Destroy(gameObject);

        Debug.Log(gameObject.name+" die");

    }

}
