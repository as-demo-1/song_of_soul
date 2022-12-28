using System;
using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public enum BuffType
{
    ElectricMark
}

public class Buff
{
    public BuffType buffType;
}

public class ElectricMark : Buff
{
    public ElectricMark()
    {
        buffType = BuffType.ElectricMark;
    }

    public static uint counter = 0;
}


[RequireComponent(typeof(Collider2D), typeof(Animator))]
public class Hittable : MonoBehaviour
{
    public int m_maxHP;
    public int m_currentHP;
    // face left is -1, face right is 1
    public int face = -1;
    public bool lockHP;

    private Animator _animator;
    public Hittable(int maxHp = 100, int currentHp = Int32.MaxValue)
    {
        m_maxHP = maxHp;
        m_currentHP = Mathf.Min(maxHp, currentHp);
    }
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        EventCenter<BattleEventType>.Instance.AddEventListener(BattleEventType.PlayerNormalAtk, BeHitAction);
        EventCenter<BattleEventType>.Instance.AddEventListener(BattleEventType.LightningChainAtk, BeHitAction);
    }

    

    public void BeHitAction(object hitter)
    {
        Debug.LogError("monster be hit");
        Hitter hitter_ = (Hitter)hitter;
        switch (hitter_.m_eventType)
        {
            case BattleEventType.PlayerNormalAtk:
                TickNormalAtcEffect((PlayerNormalAtk)hitter);
                break;
            case BattleEventType.LightningChainAtk:
                TickLightningChainEffect((LightningChain)hitter);
                break;
            default:
                break;
        }
    }
    
    private string beHitTrigger = "beHit";
    void TickNormalAtcEffect(PlayerNormalAtk playerNormalAtk)
    {
        if (playerNormalAtk.AtkPerTarget(this))
        {
            _animator.SetTrigger(beHitTrigger);
        }
    }
    void TickLightningChainEffect(LightningChain lightningChain)
    {
        if (lightningChain.AtkPerTarget(this))
        {
            _animator.SetTrigger(beHitTrigger);
        }
    }

    public void GetDamage(int atk)
    {
        if(lockHP) return;
        if (m_currentHP <= 0)
        {
            
        }
        else
        {
            m_currentHP -= atk;
        }       
    }

    public void GetRepel(float backDistance)
    {
        Vector3 position = transform.position;
        transform.position = new Vector3(position.x + face * backDistance, position.y, position.z);
    }
}
