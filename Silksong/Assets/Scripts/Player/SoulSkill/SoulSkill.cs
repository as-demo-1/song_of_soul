using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public abstract class SoulSkill : Hitter
{
    public EPlayerAttackType skillName;
    public int constPerSec = 1;
    public int constPerAttack = 0;

    //protected PlayerInfomation _playerInfomation;
    protected PlayerCharacter _playerCharacter;

    //protected SoulSkill()
    //{
    //    _playerCharacter = GetComponentInParent<PlayerCharacter>();
    //    //_playerInfomation = GetComponentInParent<PlayerInfomation>();
    //}
    
    protected void Start()
    {
        //Debug.LogError("START TIMER UPDATE");
        _playerCharacter = GetComponentInParent<PlayerCharacter>();
        MonoManager.Instance.AddUpdateEvent(Timer.Instance.TimerUpdate);
    }

    protected void OnEnable()
    {
        //Debug.LogError("test!!!!!!!!!!!");
        // 使用定时器定时结算灵魂状态的const
        Timer.Instance.StartTickActionLoop("TickSoulStatus", 0, 10, TickSoulStatus);
    }

    protected void OnDisable()
    {
        Timer.Instance.EndTickActionLoop("TickSoulStatus");
    }

    private int debugCnt = 0;
    protected void TickSoulStatus()
    {
        //_playerInfomation.CostMana(constPerSec);
        _playerCharacter.CostMana(constPerSec);
        //Debug.LogError("!!!!!!!!! soul skill ticking" + debugCnt);
        debugCnt++;
    }
}
