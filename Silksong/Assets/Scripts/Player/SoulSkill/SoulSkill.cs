using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum SkillName
{
    FlameGeyser,
    ShadowBlade,
    LightningChain,
    ArcaneBlast,
    IceStorm
}

public abstract class SoulSkill : Hitter
{
    public SkillName skillName;
    public int constPerSec = 1;
    public int constPerAttack = 0;

    protected PlayerInfomation _playerInfomation;

    protected void Start()
    {
        MonoManager.Instance.AddUpdateEvent(Timer.Instance.TimerUpdate);
    }

    protected void OnEnable()
    {
        // 使用定时器定时结算灵魂状态的const
        Timer.Instance.StartTickActionLoop("TickSoulStatus", 0, 1, TickSoulStatus);
    }

    protected void OnDisable()
    {
        Timer.Instance.EndTickActionLoop("TickSoulStatus");
    }

    protected void TickSoulStatus()
    {
        _playerInfomation.CostMana(constPerSec);
        Debug.LogError("soul skill ticking");
    }
}
