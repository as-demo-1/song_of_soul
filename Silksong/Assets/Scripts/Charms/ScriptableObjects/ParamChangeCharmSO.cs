using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 数值改变类护符
/// </summary>

[CreateAssetMenu(fileName = "ParamChangeCharm", menuName = "Charm/ParamChangeCharm")]
public class ParamChangeCharmSO : CharmSO
{
    /// <summary>
    /// 参数列表
    /// </summary>
    [Tooltip("参数列表")]
    [SerializeField]
    List<float> param = new List<float>();

    [SerializeField]
    [Header("参数备注")]
    [TextArea(2, 4)]
    private string paramNote;
    
    public override void OnDisEquip()
    {
        if (EffectType == EffectType.ATTACKSPEED)
        {
            CharmListSO.CharmAttackSpeed -= param[0];
        }
        else if (EffectType == EffectType.EXTRAHEALTH)
        {
            CharmListSO.CharmExtraHealth -= (int)param[0];
        }
        else if (EffectType == EffectType.RANGE)
        {
            CharmListSO.CharmAttackRange -= param[0];
        }
        else if (EffectType == EffectType.SPEED)
        {
            CharmListSO.CharmMoveSpeed -= param[0];
        }
        else if (EffectType == EffectType.ENERGY)
        {
            if (EffectTrigger == EffectTrigger.BLOCK)
            {
                CharmListSO.CharmBlockGainSoul -= (int)param[0];
            }
            else if (EffectTrigger == EffectTrigger.ATTACK)
            {
                CharmListSO.CharmAttackGainSoul -= (int)param[0];
            }
            else if (EffectTrigger == EffectTrigger.HURT)
            {
                CharmListSO.CharmHurtGainSoul -= (int)param[0];
            }
        }
    }

    public override void OnEquip()
    {
        if (EffectType == EffectType.ATTACKSPEED)
        {
            CharmListSO.CharmAttackSpeed += param[0];
        }
        else if (EffectType == EffectType.EXTRAHEALTH)
        {
            CharmListSO.CharmExtraHealth += (int)param[0];
        }
        else if (EffectType == EffectType.RANGE)
        {
            CharmListSO.CharmAttackRange += param[0];
        }
        else if (EffectType == EffectType.SPEED)
        {
            CharmListSO.CharmMoveSpeed += param[0];
        }
        else if (EffectType == EffectType.ENERGY)
        {
            if (EffectTrigger == EffectTrigger.BLOCK)
            {
                CharmListSO.CharmBlockGainSoul += (int)param[0];
            }
            else if (EffectTrigger == EffectTrigger.ATTACK)
            {
                CharmListSO.CharmAttackGainSoul += (int)param[0];
            }
            else if (EffectTrigger == EffectTrigger.HURT)
            {
                CharmListSO.CharmHurtGainSoul += (int)param[0];
            }
        }
    }
}
