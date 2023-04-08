using UnityEngine;
public enum BuffProperty
{
    /// <summary>
    /// 固定攻击力
    /// </summary>
    ATTACK,
    /// <summary>
    /// 百分比攻击力
    /// </summary>
    ATTACK_PC,
    
    /// <summary>
    /// 攻击范围
    /// </summary>
    ATTACK_RANG,
    
    /// <summary>
    /// 百分比攻击范围
    /// </summary>
    ATTACK_RANG_PC,
    
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
    
    /// <summary>
    /// 闪电链
    /// </summary>
    LIGHTNING,
    
    /// <summary>
    /// 烈焰斩
    /// </summary>
    FIRE,
    
    /// <summary>
    /// 影
    /// </summary>
    SHADOW,
    
    /// <summary>
    /// 冲击波
    /// </summary>
    WAVE,
    
    /// <summary>
    /// 冰霜
    /// </summary>
    ICE,
    
}