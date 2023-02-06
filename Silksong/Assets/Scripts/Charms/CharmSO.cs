using UnityEngine;

[System.Serializable]
/// <summary>
/// 护符类，用于保存各种护符属性和效果
/// </summary>
public abstract class CharmSO: ScriptableObject
{
    /// <summary>
    /// 护符名称
    /// </summary>
    [SerializeField] string charmName = "护符";
    public string CharmName { get => charmName; }

    /// <summary>
    /// 是否已获得该护符
    /// </summary>
    [Tooltip("已收集该护符")]
    [SerializeField] 
    bool hasCollected;
    public bool HasCollected { get => hasCollected; set => hasCollected = value; }

    /// <summary>
    /// 是否已装备该护符
    /// </summary>
    [Tooltip("已装备该护符")]
    [SerializeField] bool hasEquiped;
    public bool HasEquiped { get => hasEquiped; set => hasEquiped = value; }


    /// <summary>
    /// 护符品质
    /// </summary>
    [Tooltip("护符品质")] 
    [SerializeField] 
    public CharmQuality CharmQuality;

    /// <summary>
    /// 护符效果描述
    /// </summary>
    [Tooltip("护符效果文字描述")]
    [SerializeField]
    [TextArea(2,2)]
    public string effectText;

    /// <summary>
    /// 护符图片
    /// </summary>
    [SerializeField]
    public Sprite charmImage;

    /// <summary>
    /// 是否是易碎护符
    /// </summary>
    [SerializeField]
    [Tooltip("是否为易碎护符")]
    private bool isFragile;

    /// <summary>
    /// 引用护符列表，护符触发效果需要改变列表中的变量
    /// </summary>
    [SerializeField]
    protected CharmListSO CharmListSO = default;

    /// <summary>
    /// 护符实际效果
    /// </summary>
    //private CharmEffect charmEffect;
    //public CharmEffect CharmEffect { get => charmEffect; }
    
    [SerializeField]
    protected EffectTrigger EffectTrigger;

    [SerializeField]
    protected EffectType EffectType;

    /// <summary>
    /// 效果参数
    /// </summary>
    //[Tooltip("效果参数")]
    //[SerializeField]
    //private float[] effectPram;

    // 可以选用效果SO来配置护符的效果
    //[SerializeField] CharmListSO CharmListSO;

    public abstract void OnEquip();
    public abstract void OnDisEquip();
    //public abstract void OnPlayerDead();
}

/// <summary>
/// 护符品质
/// </summary>
public enum CharmQuality
{
    BLUE, PURPLE, ORANGE
}

public enum EffectTrigger
{
    [Tooltip("攻击命中")]
    ATTACK, 
    [Tooltip("受伤")]
    HURT,
    [Tooltip("格挡")]
    BLOCK,
    [Tooltip("低生命值")]
    HEALTHLOWER,
    [Tooltip("默认")]
    DEFAULT,

}

public enum EffectType
{
    [Tooltip("回复能量")]
    ENERGY,
    [Tooltip("基础生命值")]
    HEALTH,
    [Tooltip("临时血量")]
    EXTRAHEALTH,
    [Tooltip("加攻速")]
    ATTACKSPEED,
    [Tooltip("加移速")]
    SPEED,
    [Tooltip("加攻击范围")]
    RANGE,
    [Tooltip("加速回血")]
    HEALSPEED,
    [Tooltip("回复血量")]
    HEAL,

}
