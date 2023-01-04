using System;
using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using Object = UnityEngine.Object;



[RequireComponent(typeof(Collider2D), typeof(Animator))]
public class Hittable : MonoBehaviour
{
    public int m_maxHP;
    public int m_currentHP;
    // face left is -1, face right is 1
    public int face = -1;
    public bool lockHP;
    private Dictionary<BuffType, Buff> _buffs = new Dictionary<BuffType, Buff>();

    private Animator _animator;
    public Hittable(int maxHp = 100, int currentHp = Int32.MaxValue)
    {
        m_maxHP = maxHp;
        m_currentHP = Mathf.Min(maxHp, currentHp);
    }
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _buffs = new Dictionary<BuffType, Buff>();
        EventCenter<BattleEventType>.Instance.AddEventListener(BattleEventType.PlayerNormalAtk, BeHitAction);
        EventCenter<BattleEventType>.Instance.AddEventListener(BattleEventType.LightningChainAtk, BeHitAction);
        EventCenter<BattleEventType>.Instance.AddEventListener(BattleEventType.LightningAddElectricMarkEvent, BeHitAction);
    }

    public void BeHitAction(object hitter)
    {
        Hitter hitter_ = (Hitter)hitter;
        switch (hitter_.m_eventType)
        {
            case BattleEventType.PlayerNormalAtk:
                TickNormalAtkEffect((PlayerNormalAtk)hitter);
                break;
            case BattleEventType.LightningChainAtk:
                TickLightningChainAtkEffect((LightningChain)hitter);
                break;
            case BattleEventType.LightningAddElectricMarkEvent:
                TickElectricMarkEffect((LightningChain)hitter);
                break;
            default:
                break;
        }
    }
    
    private const string beHitTrigger = "beHit";
    void TickNormalAtkEffect(PlayerNormalAtk playerNormalAtk)
    {
        if (playerNormalAtk.AtkPerTarget(this))
        {
            Debug.LogError("set beHitTrigger");
            _animator.SetTrigger(beHitTrigger);
        }
    }
    void TickLightningChainAtkEffect(LightningChain lightningChain)
    {
        if (lightningChain.AtkPerTarget(this))
        {
            Debug.Log("Light Chain ATK");
            _animator.SetTrigger(beHitTrigger);
        }
    }

    void TickElectricMarkEffect(LightningChain lightningChain)
    {
        Debug.Log("Tick Electric Mark");
        if (lightningChain.AddElectricMark(this))
        {
            ((ElectricMark)_buffs[BuffType.ElectricMark]).ShowPerformance(transform);
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

    private uint buffindex;
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
