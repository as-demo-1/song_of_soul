using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 通过SO存储所有的护符列表，并且存储玩家的获得情况与装备情况
/// </summary> 作者：次元

[CreateAssetMenu(fileName = "CharmList", menuName = "Charm/CharmList")]
public class CharmListSO : ScriptableObject
{
    [SerializeField] private List<CharmSO> Charms = new List<CharmSO>();

    
    private int charmAttackGainSoul;
    /// <summary>
    /// 护符攻击回能改变值
    /// </summary>
    public int CharmAttackGainSoul { get => charmAttackGainSoul; set => charmAttackGainSoul = value; }
    
    private int charmHurtGainSoul;
    /// <summary>
    /// 护符受伤回能改变值
    /// </summary>
    public int CharmHurtGainSoul { get => charmHurtGainSoul; set => charmHurtGainSoul = value; }



    /// <summary>
    /// 获得护符
    /// </summary>
    /// <param name="name"> 护符名称 </param>
    /// <returns></returns>
    public bool CollectCharm(string name)
    {
        foreach (var item in Charms)
        {
            if (name.Equals(item.Name))
            {
                item.HasCollected = true;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 装备护符
    /// </summary>
    /// <param name="name"> 护符名称 </param>
    /// <returns></returns>
    public bool EquipCharm(string name)
    {
        foreach (var item in Charms)
        {
            if (name.Equals(item.Name))
            {
                item.HasEquiped = true;
                item.OnEquip();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 激活所有护符，在游戏开始后激活所有已装备的护符
    /// </summary>
    public void ActiveAllEquipedCharms()
    {
        foreach (var charm in Charms)
        {
            if (charm.HasEquiped)
            {
                charm.OnEquip();
            }
        }
    }
}




