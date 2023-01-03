using System;
using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using UnityEditor;
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
    private Dictionary<BuffType, Buff> _buffs;

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
        EventCenter<BattleEventType>.Instance.AddEventListener(BattleEventType.LightningAddElectricMarkEvent, BeHitAction);
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
            case BattleEventType.LightningAddElectricMarkEvent:
                
            default:
                break;
        }
    }
    
    private const string beHitTrigger = "beHit";
    void TickNormalAtcEffect(PlayerNormalAtk playerNormalAtk)
    {
        if (playerNormalAtk.AtkPerTarget(this))
        {
            Debug.LogError("set beHitTrigger");
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

    void TickElectricMarkEffect(LightningChain lightningChain)
    {
        if (lightningChain.AddElectricMark(this))
        {
            if (!_buffs.ContainsKey(BuffType.ElectricMark))
            {
                _buffs.Add(BuffType.ElectricMark, new ElectricMark());
                ElectricMark.counter++;
            }
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

    public void GetBuff(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.ElectricMark:
                ElectricMark eBuff = (ElectricMark) _buffs[BuffType.ElectricMark];
                if (eBuff.GetLayerNum() == 0)
                {
                    eBuff.AddOneLayer();
                    ElectricMark.targets.Add(this);
                }
                break;
            default:
                break;
        }
    }

    private void InitViableBuff()
    {
        _buffs = new Dictionary<BuffType, Buff>();
        _buffs.Add(BuffType.ElectricMark, new ElectricMark());
    }
}
