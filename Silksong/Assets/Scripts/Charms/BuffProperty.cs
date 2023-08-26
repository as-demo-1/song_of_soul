using UnityEngine;
public enum BuffProperty
{
    /// <summary>
    /// 固定攻击力
    /// </summary>
    ATTACK = 0,
    /// <summary>
    /// 百分比攻击力
    /// </summary>
    ATTACK_PC = 1,
    
    /// <summary>
    /// 攻击范围
    /// </summary>
    ATTACK_RANG =2,
    
    /// <summary>
    /// 百分比攻击范围
    /// </summary>
    ATTACK_RANG_PC=3,
    
    /// <summary>
    /// 攻击回能
    /// </summary>
    ATTACK_MANA=4,
    
    /// <summary>
    /// 攻击间隔时间
    /// </summary>
    ATTACK_SPEED=5,
    
    /// <summary>
    /// 攻击范围
    /// </summary>
    ATTACK_RANGE=6,
    
    /// <summary>
    /// 受伤回能
    /// </summary>
    HURT_MANA=7,
    
    /// <summary>
    /// 格挡回能
    /// </summary>
    BLOCK_MANA=8,
    
    /// <summary>
    /// 额外生命值
    /// </summary>
    EXTRA_HEALTH=9,
    
    /// <summary>
    /// 最大生命值
    /// </summary>
    [Tooltip("最大生命值")]
    MAX_HEALTH=10,
    
    /// <summary>
    /// 移动速度
    /// </summary>
    MOVE_SPEED=11,
    
    /// <summary>
    /// 冲刺冷却
    /// </summary>
    SPRINT_CD=12,
    

    
    /// <summary>
    /// 回血速度
    /// </summary>
    HEAL_SPEED=13,
    
    /// <summary>
    /// 回血量
    /// </summary>
    HEAL_AMOUNT=14,
    
    /// <summary>
    /// 脱战回血
    /// </summary>
    LEAVE_HEAL=15,
    
    COLLECT_COIN=16,
    
    MAX_MANA=17,
    
    /// <summary>
    /// 血怒效果
    /// </summary>
    BLOOD_ANGER=18,
    
    /// <summary>
    /// 闪电链
    /// </summary>
    LIGHTNING=19,
    
    /// <summary>
    /// 烈焰斩
    /// </summary>
    FIRE=20,
    
    /// <summary>
    /// 影
    /// </summary>
    SHADOW=21,
    
    /// <summary>
    /// 冲击波
    /// </summary>
    WAVE=22,
    
    /// <summary>
    /// 冰霜
    /// </summary>
    ICE=23,
    
    /// <summary>
    /// 增加掉落金币
    /// </summary>
    EXTRA_COIN=24,
    
    /// <summary>
    /// 强力回血，加时间加回血
    /// </summary>
    SUPER_HEAL=25,
    
    /// <summary>
    /// 减最大，加额外血
    /// </summary>
    MAX_DOWN_EXTRA_HEALTH=26,
    
    /// <summary>
    /// 触发血怒的血量
    /// </summary>
    ANGER_HEALTH=27,
    
    /// <summary>
    /// 血怒时的攻击力增加百分比
    /// </summary>
    ANGER_ATK_PC=28,
    
}