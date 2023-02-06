using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 护符对应的Buff效果
/// </summary> 作者：次元

public abstract class CharmBuff
{
    protected string _buffId;    // 效果ID
    protected string _buffVal;   // 效果参数，多个参数用&分隔
    protected int _valCount;     // 效果参数的数量
    protected bool _isActive;    // buff效果是否激活

    protected BuffManager _manager;

    public string BuffId => _buffId;
    public string BuffVal => _buffVal;
    public int ValueCount => _valCount;
    public bool IsActive => _isActive;

    public CharmBuff(BuffManager buffManager) { _manager = buffManager; }

    /// <summary>
    /// 初始化未激活的Buff
    /// </summary>
    /// <param name="buffVal"></param>
    public virtual void InitBuff(string buffVal) { }

    /// <summary>
    /// 增加效果层数
    /// </summary>
    /// <param name="buffVal"></param>
    public virtual void AddEffect(string buffVal) { }

    /// <summary>
    /// 减少效果层数
    /// </summary>
    /// <param name="buffVal"></param>
    public virtual void DisEffect(string buffVal) { }
}

/// <summary>
/// 50000001
/// 增加攻击力
/// </summary>
public class AttackBuff: CharmBuff
{
    private int intBuffVal = default;

    public AttackBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;       
    }
    public override void InitBuff(string buffVal) 
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal -= val;
        if (intBuffVal <0)
        {
            intBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("AttackBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal += val;
        Debug.Log("player attack add " + intBuffVal);
        Debug.Log("AttackBuff OnEffect");
    }
}

/// <summary>
/// 50000002
/// 增加攻击回能
/// </summary>
public class AttackSoulBuff : CharmBuff
{
    private int intBuffVal = default;

    public AttackSoulBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal -= val;
        if (intBuffVal < 0)
        {
            intBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("AttackPowerBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal += val;
        Debug.Log("player AttackPowerBuff " + intBuffVal);
        Debug.Log("AttackPowerBuff OnEffect");
    }
}

/// <summary>
/// 50000003
/// 增加受伤回能
/// </summary>
public class HurtSoulBuff : CharmBuff
{
    private int intBuffVal = default;

    public HurtSoulBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal -= val;
        if (intBuffVal < 0)
        {
            intBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("HurtPowerBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal += val;
        Debug.Log("player HurtPowerBuff " + intBuffVal);
        Debug.Log("HurtPowerBuff OnEffect");
    }
}

/// <summary>
/// 50000004
/// 击中弹道回能
/// </summary>
public class BlockSoulBuff : CharmBuff
{
    private int intBuffVal = default;

    public BlockSoulBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal -= val;
        if (intBuffVal < 0)
        {
            intBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("HurtPowerBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal += val;
        Debug.Log("player HurtPowerBuff " + intBuffVal);
        Debug.Log("HurtPowerBuff OnEffect");
    }
}

/// <summary>
/// 50000005
/// 增加额外生命值
/// </summary>
public class ExtraHealthBuff : CharmBuff
{
    private int intBuffVal = default;

    public ExtraHealthBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal -= val;
        if (intBuffVal < 0)
        {
            intBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("ExtraHealthBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal += val;
        Debug.Log("player ExtraHealthBuff " + intBuffVal);
        Debug.Log("ExtraHealthBuff OnEffect");
    }
}

/// <summary>
/// 50000006
/// 增加额外生命值，减少本体生命值
/// </summary>
public class ExtraHealthDecBuff : CharmBuff
{
    private int intBuffValX = default;
    private int intBuffValY = default;
    public ExtraHealthDecBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 2;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffValX -= val;
        if (intBuffValX < 0)
        {
            intBuffValX = 0;
            this._isActive = false;
        }
        Debug.Log("ExtraHealthLossBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffValX += val;
        Debug.Log("player ExtraHealthLossBuff " + intBuffValX);
        Debug.Log("ExtraHealthLossBuff OnEffect");
    }
}

/// <summary>
/// 50000007
/// 增加攻击速度
/// </summary>
public class AttackSpeedBuff: CharmBuff
{
    private float floatBuffVal = default;

    public AttackSpeedBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        float.TryParse(buffVal, out float val);
        floatBuffVal -= val;
        if (floatBuffVal < 0)
        {
            floatBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("AttackSpeedBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        floatBuffVal += val;
        Debug.Log("player AttackSpeedBuff " + floatBuffVal);
        Debug.Log("AttackSpeedBuff OnEffect");
    }
}

/// <summary>
/// 50000008
/// 增加移动速度
/// </summary>
public class MoveSpeedBuff : CharmBuff
{
    private int intBuffVal = default;

    public MoveSpeedBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal -= val;
        if (intBuffVal < 0)
        {
            intBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("AttackBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        intBuffVal += val;
        Debug.Log("player attack add " + intBuffVal);
        Debug.Log("AttackBuff OnEffect");
    }
}

/// <summary>
/// 50000009
/// 减少冲刺冷却时间
/// </summary>
public class DashCDBuff : CharmBuff
{
    private float floatBuffVal = default;

    public DashCDBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        float.TryParse(buffVal, out float val);
        floatBuffVal -= val;
        if (floatBuffVal < 0)
        {
            floatBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("AttackSpeedBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        floatBuffVal += val;
        Debug.Log("player AttackSpeedBuff " + floatBuffVal);
        Debug.Log("AttackSpeedBuff OnEffect");
    }
}

/// <summary>
/// 50000010
/// 增加攻击范围
/// </summary>
public class AttackRangeBuff : CharmBuff
{
    private float floatBuffVal = default;

    public AttackRangeBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        float.TryParse(buffVal, out float val);
        floatBuffVal -= val;
        if (floatBuffVal < 0)
        {
            floatBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("AttackSpeedBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        floatBuffVal += val;
        Debug.Log("player AttackSpeedBuff " + floatBuffVal);
        Debug.Log("AttackSpeedBuff OnEffect");
    }
}

/// <summary>
/// 50000011
/// 加快回血速度
/// </summary>
public class HealSpeedBuff : CharmBuff
{
    private float floatBuffVal = default;

    public HealSpeedBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        float.TryParse(buffVal, out float val);
        floatBuffVal -= val;
        if (floatBuffVal < 0)
        {
            floatBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("AttackSpeedBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        floatBuffVal += val;
        Debug.Log("player AttackSpeedBuff " + floatBuffVal);
        Debug.Log("AttackSpeedBuff OnEffect");
    }
}

/// <summary>
/// 50000012
/// 加快回血速度，增加回血量
/// </summary>
public class HealSpeedIncBuff : CharmBuff
{
    private float floatBuffVal = default;
    private int intBuffVal = default;
    public HealSpeedIncBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        float.TryParse(buffVal, out float val);
        floatBuffVal -= val;
        if (floatBuffVal < 0)
        {
            floatBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("AttackSpeedBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        floatBuffVal += val;
        Debug.Log("player AttackSpeedBuff " + floatBuffVal);
        Debug.Log("AttackSpeedBuff OnEffect");
    }
}

/// <summary>
/// 50000014
/// 脱战回血
/// </summary>
public class LeaveBattleBuff : CharmBuff
{
    private float floatBuffVal = default;

    public LeaveBattleBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 1;
    }
    public override void InitBuff(string buffVal)
    {
        AddEffect(buffVal);
        this._isActive = true;
    }

    public override void DisEffect(string buffVal)
    {
        float.TryParse(buffVal, out float val);
        floatBuffVal -= val;
        if (floatBuffVal < 0)
        {
            floatBuffVal = 0;
            this._isActive = false;
        }
        Debug.Log("AttackSpeedBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        int.TryParse(buffVal, out int val);
        floatBuffVal += val;
        Debug.Log("player AttackSpeedBuff " + floatBuffVal);
        Debug.Log("AttackSpeedBuff OnEffect");
    }
}

/// <summary>
/// 50000015
/// 自动收集金币buff
/// </summary>
public class CollectCoinBuff : CharmBuff
{
    //private int intBuffVal = default;
    private GameObject coinCollecter;
    public CollectCoinBuff(BuffManager buffManager) : base(buffManager)
    {
        _manager = buffManager;
        _valCount = 0;
    }
    public override void InitBuff(string buffVal)
    {
        this._isActive = true;
        coinCollecter = UnityEngine.Object.Instantiate(_manager.coinCollecterPrefab, _manager.playerCharacter.transform);
        AddEffect(buffVal);
    }

    public override void DisEffect(string buffVal)
    {
        //int.TryParse(buffVal, out int val);
        //intBuffVal -= val;
        //if (intBuffVal < 0)
        //{
        //    intBuffVal = 0;
        //    this._isActive = false;
        //}
        if (coinCollecter!=null)
        {
            coinCollecter.SetActive(false);
            this._isActive = false;
        }
        Debug.Log("AttackBuff DisEffect");
    }

    public override void AddEffect(string buffVal)
    {
        //int.TryParse(buffVal, out int val);
        //intBuffVal += val;
        //Debug.Log("player attack add " + intBuffVal);
        if (coinCollecter!=null)
        {
            coinCollecter.SetActive(true);
        }
        Debug.Log("AttackBuff OnEffect");
    }
}