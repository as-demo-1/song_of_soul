using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 受伤回能量护符
/// </summary>

[CreateAssetMenu(fileName = "HurtEnergyCharm", menuName = "Charm/HurtEnergyCharmSO")]
public class HurtEnergyCharmSO : CharmSO
{
    /// <summary>
    /// 回能量值
    /// </summary>
    [Tooltip("回能")]
    [SerializeField]
    int energy;
    
    public override void OnDisEquip()
    {
        // 发送改变玩家数值请求
        CharmListSO.CharmHurtGainSoul -= energy;
        HasEquiped = false;
    }

    public override void OnEquip()
    {
        // 发送改变玩家数值请求
        CharmListSO.CharmHurtGainSoul += energy;
    }
}
