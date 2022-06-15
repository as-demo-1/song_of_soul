using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 数值改变类护符
/// </summary>

[System.Serializable]
public class CharmEffect
{
    public List<float> floatParams = new List<float>();
    public List<int> intParams = new List<int>();
    public EffectType effectType;
    public EffectTrigger effectTrigger;
    
    [SerializeField]
    [Header("参数备注")]
    [TextArea(2, 4)]
    private string paramNote;

}

[CreateAssetMenu(fileName = "MutiEffectCharm", menuName = "Charm/MutiEffectCharm")]
public class MutiEffectCharmSO : CharmSO
{

    public List<CharmEffect> charmEffects = new List<CharmEffect>();

    public override void OnDisEquip()
    {
        //if (EffectType == EffectType.ATTACKSPEED)
        //{
        //    CharmListSO.CharmAttackSpeed -= param[0];
        //}
        //else if (EffectType == EffectType.EXTRAHEALTH)
        //{
        //    CharmListSO.CharmExtraHealth -= (int)param[0];
        //}
        //else if (EffectType == EffectType.RANGE)
        //{
        //    CharmListSO.CharmAttackRange -= param[0];
        //}
        //else if (EffectType == EffectType.SPEED)
        //{
        //    CharmListSO.CharmMoveSpeed -= param[0];
        //}
        //else if (EffectType == EffectType.ENERGY)
        //{
        //    if (EffectTrigger == EffectTrigger.BLOCK)
        //    {
        //        CharmListSO.CharmBlockGainSoul -= (int)param[0];
        //    }
        //    else if (EffectTrigger == EffectTrigger.ATTACK)
        //    {
        //        CharmListSO.CharmAttackGainSoul -= (int)param[0];
        //    }
        //    else if (EffectTrigger == EffectTrigger.HURT)
        //    {
        //        CharmListSO.CharmHurtGainSoul -= (int)param[0];
        //    }
        //}
    }

    public override void OnEquip()
    {
        //if (EffectType == EffectType.ATTACKSPEED)
        //{
        //    CharmListSO.CharmAttackSpeed += param[0];
        //}
        //else if (EffectType == EffectType.EXTRAHEALTH)
        //{
        //    CharmListSO.CharmExtraHealth += (int)param[0];
        //}
        //else if (EffectType == EffectType.RANGE)
        //{
        //    CharmListSO.CharmAttackRange += param[0];
        //}
        //else if (EffectType == EffectType.SPEED)
        //{
        //    CharmListSO.CharmMoveSpeed += param[0];
        //}
        //else if (EffectType == EffectType.ENERGY)
        //{
        //    if (EffectTrigger == EffectTrigger.BLOCK)
        //    {
        //        CharmListSO.CharmBlockGainSoul += (int)param[0];
        //    }
        //    else if (EffectTrigger == EffectTrigger.ATTACK)
        //    {
        //        CharmListSO.CharmAttackGainSoul += (int)param[0];
        //    }
        //    else if (EffectTrigger == EffectTrigger.HURT)
        //    {
        //        CharmListSO.CharmHurtGainSoul += (int)param[0];
        //    }
        //}
    }
}
