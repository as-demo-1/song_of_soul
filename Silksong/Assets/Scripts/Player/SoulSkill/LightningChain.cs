using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningChain : SoulSkill
{
    public float range = 1.0f;
    [Range(0,1)]
    public float extraSpeedPercent = 0;
    public float baseDamage = 1;
    [Range(0,1)]
    public float extraDamagePercent = 0;

    void Start()
    {
        _playerInfomation = GetComponentInParent<PlayerInfomation>();
        RefreshAtk(_playerInfomation.atk);
    }

    private void OnEnable()
    {
        base.OnEnable();
        SpeedUp(true);
    }

    private void OnDisable()
    {
        base.OnDisable();
        SpeedUp(false);
    }

    public void TriggerAddElectricMarkEvent()
    {
        EventCenter<BattleEventType>.Instance.TiggerEvent(BattleEventType.LightningAddElectricMarkEvent, this);
    }
    
    public override bool AtkPerTarget(Hittable target)
    {
        if (!IsAtkSuccess(target)) return false;
        target.GetDamage(Damage());
        return true;
    }

    public bool AddElectricMark(Hittable target)
    {
        if (!IsAddElectricMarkSuccess(target)) return false;
        target.GetBuff(BuffType.ElectricMark);
        return true;
    }

    protected override bool IsAtkSuccess(Hittable target)
    {
        float dis = (GetComponentInParent<Transform>().position - target.transform.position).magnitude;
        return (GetComponentInParent<Transform>().position - target.transform.position).magnitude <= _atkDistance;
    }

    // TODO:实现挂载闪电标记的逻辑
    private bool IsAddElectricMarkSuccess(Hittable target)
    {
        return true;
    }

    public void SpeedUp(bool needApply)
    {
        if(needApply) _playerInfomation.SpeedUp(extraSpeedPercent);
        else
        {
            _playerInfomation.SpeedUpReset();
        }
    }

    private int Damage()
    {
        var damage = baseDamage * (1.0f + extraDamagePercent * ElectricMark.counter);
        return (int)damage;
    }
}
