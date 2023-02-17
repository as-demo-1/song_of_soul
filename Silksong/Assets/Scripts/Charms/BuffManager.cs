using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;




public class BuffManager : SerializedMonoBehaviour
{
    [InlineEditor]
    public CharmListSO charmListSO = default;

    //public PlayerCharacter playerCharacter;
    public PlayerController playerController;

    //public Dictionary<string, CharmBuff> charmBuffDict = new Dictionary<string, CharmBuff>();
    //public List<CharmBuff> activeBuffList = new List<CharmBuff>();

    public GameObject coinCollecterPrefab;

    /// <summary>
    /// 固定增益属性buff
    /// </summary>
    public Dictionary<BuffProperty, float> BuffPropDic = new Dictionary<BuffProperty, float>();

    // TODO: 不应该从buff系统直接引用玩家的组件
    /// <summary>
    /// 引用角色的攻击判定框
    /// </summary>
    [SerializeField]
    private List<TwoTargetDamager> atkDamagers = new List<TwoTargetDamager>();

    /// <summary>
    /// 需要触发回调的buff
    /// </summary>
    private Dictionary<BuffProperty, Action> buffActionDic = new Dictionary<BuffProperty, Action>();

    public void Init()
    {
        buffActionDic[BuffProperty.COLLECT_COIN] = GenCoinCollect;
        buffActionDic[BuffProperty.ATTACK] = SetAtkDamage;
        buffActionDic[BuffProperty.ATTACK_PC] = SetAtkDamage;
        buffActionDic[BuffProperty.ATTACK_RANG] = SetAtkRange;
        buffActionDic[BuffProperty.ATTACK_RANG_PC] = SetAtkRange;
        
        charmListSO.Init(this);
    }



    /// <summary>
    /// 添加一个buff效果
    /// </summary>
    /// <param name="_property">属性种类</param>
    /// <param name="_val">属性变化参数</param>
    public void AddBuff(BuffProperty _property, float _val)
    {
        if (BuffPropDic.ContainsKey(_property))
        {
            BuffPropDic[_property] += _val;
        }
        else
        {
            BuffPropDic.Add(_property, _val);
        }  
        
        if (buffActionDic.ContainsKey(_property))
        {
            // 触发特殊buff的回调
            buffActionDic[_property].Invoke();
        }
    }

    /// <summary>
    /// 关闭一个buff效果
    /// </summary>
    /// <param name="_property">属性种类</param>
    /// <param name="_val">属性变化参数</param>
    public void DecreaseBuff(BuffProperty _property, float _val)
    {
        if (!BuffPropDic.ContainsKey(_property))
        {
            Debug.LogError("buff"+_property.ToString() +" not active");
            return;
        }

        BuffPropDic[_property] -= _val;
        
        if (buffActionDic.ContainsKey(_property))
        {
            // 触发特殊buff的回调
            buffActionDic[_property].Invoke();
        }
  
    }

    /// <summary>
    /// 获取增益属性
    /// </summary>
    /// <param name="_buffProperty">属性种类</param>
    /// <returns></returns>
    public float GetBuffProperty(BuffProperty _buffProperty)
    {
        return BuffPropDic.ContainsKey(_buffProperty) ? BuffPropDic[_buffProperty] : 0.0f;
    }



    private void SetAtkDamage()
    {
        foreach (var atkDamager in atkDamagers)
        {
            atkDamager.damage = 
                (int) (Constants.AttackDamage * (1.0f + GetBuffProperty(BuffProperty.ATTACK_PC)) 
                                                  + GetBuffProperty(BuffProperty.ATTACK));
        }
    }

    public void SetAtkRange()
    {
        foreach (var atkDamager in atkDamagers)
        {
            float size = (Constants.AttackRange * (1.0f + GetBuffProperty(BuffProperty.ATTACK_RANG_PC))
                          + GetBuffProperty(BuffProperty.ATTACK_RANG));
            atkDamager.transform.localScale = new Vector3(size, size, 1.0f);
        }
    }
    
    public void GenCoinCollect()
    {}
}
