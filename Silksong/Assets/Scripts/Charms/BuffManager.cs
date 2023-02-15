using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

public enum BuffProperty
{
    /// <summary>
    /// 固定攻击力
    /// </summary>
    ATTACK,
    
    
    /// <summary>
    /// 攻击距离
    /// </summary>
    ATTACK_RANG,
    
    /// <summary>
    /// 攻击回能
    /// </summary>
    ATTACK_MANA,
    
    ATTACK_SPEED,
    
    ATTACK_RANGE,
    
    /// <summary>
    /// 受伤回能
    /// </summary>
    HURT_MANA,
    
    /// <summary>
    /// 格挡回能
    /// </summary>
    BLOCK_MANA,
    
    /// <summary>
    /// 额外生命值
    /// </summary>
    EXTRA_HEALTH,
    
    /// <summary>
    /// 最大生命值
    /// </summary>
    [Tooltip("最大生命值")]
    MAX_HEALTH,
    
    MOVE_SPEED,
    
    /// <summary>
    /// 冲刺冷却
    /// </summary>
    SPRINT_CD,
    

    
    /// <summary>
    /// 回血速度
    /// </summary>
    HEAL_SPEED,
    
    /// <summary>
    /// 回血量
    /// </summary>
    HEAL_AMOUNT,
    
    /// <summary>
    /// 脱战回血
    /// </summary>
    LEAVE_HEAL,
    
    COLLECT_COIN,
    
    MAX_MANA,
    
    /// <summary>
    /// 血怒效果
    /// </summary>
    BLOOD_ANGER,
}

public enum PcProperty
{
    /// <summary>
    /// 百分比攻击力
    /// </summary>
    ATTACK_PC,
}
public class BuffManager : SerializedMonoBehaviour
{
    [InlineEditor]
    public CharmListSO charmListSO = default;

    //public PlayerCharacter playerCharacter;
    public PlayerController playerController;

    //public Dictionary<string, CharmBuff> charmBuffDict = new Dictionary<string, CharmBuff>();
    //public List<CharmBuff> activeBuffList = new List<CharmBuff>();

    public GameObject coinCollecterPrefab;

    public Dictionary<BuffProperty, float> BuffPropDic = new Dictionary<BuffProperty, float>();

    public void Init()
    {
        charmListSO.Init(this);
    }



    /// <summary>
    /// 添加一个buff效果
    /// </summary>
    /// <param name="_property">属性种类</param>
    /// <param name="_val">属性变化参数</param>
    public void AddBuff(BuffProperty _property, float _val)
    {
        if (_property.Equals(BuffProperty.COLLECT_COIN))
        {
            // 生成金币收集器
            return;
        }
        if (BuffPropDic.ContainsKey(_property))
        {
            BuffPropDic[_property] += _val;
        }
        else
        {
            BuffPropDic.Add(_property, _val);
        }  
    }

    /// <summary>
    /// 关闭一个buff效果
    /// </summary>
    /// <param name="_property">属性种类</param>
    /// <param name="_val">属性变化参数</param>
    public void DecreaseBuff(BuffProperty _property, float _val)
    {
        if (_property.Equals(BuffProperty.COLLECT_COIN))
        {
            // 销毁金币收集器
            return;
        }
        
        if (!BuffPropDic.ContainsKey(_property))
        {
            Debug.LogError("buff"+_property.ToString() +" not active");
            return;
        }
        
        
        BuffPropDic[_property] -= _val;
  
    }

    /// <summary>
    /// 获取增益属性
    /// </summary>
    /// <param name="_buffProperty">属性种类</param>
    /// <returns></returns>
    public float GetBuffProperty(BuffProperty _buffProperty)
    {
        if (BuffPropDic.ContainsKey(_buffProperty))
        {
            return BuffPropDic[_buffProperty];
        }
        else
        {
            return 0.0f;
        }
    }
    
}
