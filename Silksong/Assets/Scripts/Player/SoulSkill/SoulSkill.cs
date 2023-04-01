using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public enum SkillName
{
    FlameGeyser,
    ShadowBlade,

    [Tooltip("闪电链")]
    LightningChain,

    ArcaneBlast,
    IceStorm
}

[Serializable]
public abstract class SoulSkill : Hitter
{
    public SkillName skillName;
    public int constPerSec = 1;
    public int constPerAttack = 0;

    //protected PlayerInfomation _playerInfomation;
    protected PlayerCharacter _playerCharacter;
    protected PlayerController _playerController;

    /// <summary>
    /// 技能开启事件
    /// </summary>
    public UnityEvent SkillStart;
    
    /// <summary>
    /// 技能结束事件
    /// </summary>
    public UnityEvent SkillEnd;

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
