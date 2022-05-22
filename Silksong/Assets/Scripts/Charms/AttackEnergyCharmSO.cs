using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 攻击回能量护符
/// </summary>

[CreateAssetMenu(fileName = "AttackEnergyCharm", menuName = "Charm/AttackEnergyCharm")]
public class AttackEnergyCharmSO : CharmSO
{
    /// <summary>
    /// 攻击时回能量
    /// </summary>
    [Tooltip("攻击回能")]
    [SerializeField]
    int energy;
    
    public override void OnDisEquip()
    {
        // 发送改变玩家数值请求
        CharmListSO.CharmAttackGainSoul -= energy;
        HasEquiped = false;
    }

    public override void OnEquip()
    {
        // 发送改变玩家数值请求
        CharmListSO.CharmAttackGainSoul += energy;
    }
}
