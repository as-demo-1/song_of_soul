using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 通过SO存储所有的护符列表，并且存储玩家的获得情况与装备情况
/// </summary> 作者：次元

[CreateAssetMenu(fileName = "CharmList", menuName = "Charm/CharmList")]
public class CharmListSO : ScriptableObject
{
    [SerializeField] public List<CharmSO> Charms = new List<CharmSO>();


    [SerializeField]
    [Tooltip("攻击回能加成")]
    private int charmAttackGainSoul;
    /// <summary>
    /// 护符攻击回能改变值
    /// </summary>
    public int CharmAttackGainSoul { get => charmAttackGainSoul; set => charmAttackGainSoul = value; }

    [SerializeField]
    [Tooltip("受伤回能加成")]
    private int charmHurtGainSoul;
    /// <summary>
    /// 护符受伤回能改变值
    /// </summary>
    public int CharmHurtGainSoul { get => charmHurtGainSoul; set => charmHurtGainSoul = value; }

    [SerializeField]
    [Tooltip("格挡回能加成")]
    private int charmBlockGainSoul;
    /// <summary>
    /// 护符格挡回能改变值
    /// </summary>
    public int CharmBlockGainSoul { get => charmBlockGainSoul; set => charmBlockGainSoul = value; }


    [Tooltip("临时血量")]
    /// <summary>
    /// 护符提供的临时血量
    /// </summary>
    public int CharmExtraHealth;

    [Tooltip("攻击范围加成")]
    /// <summary>
    /// 护符提供的攻击范围
    /// </summary>
    public float CharmAttackRange;

    [Tooltip("攻击速度加成")]
    /// <summary>
    /// 护符提供的攻击速度
    /// </summary>
    public float CharmAttackSpeed;

    [Tooltip("移动速度加成")]
    /// <summary>
    /// 护符提供的移动速度
    /// </summary>
    public float CharmMoveSpeed;

    [Tooltip("攻击伤害加成")]
    /// <summary>
    /// 护符提供的攻击伤害
    /// </summary>
    public float CharmAttackDamage;

    public void InitRef()
    {
        //ActiveAllEquipedCharms();
    }

    /// <summary>
    /// 获得护符，拾取、购买或剧情获得时调用此方法
    /// </summary>
    /// <param name="name"> 护符名称 </param>
    /// <returns></returns>
    public bool CollectCharm(string name)
    {
        foreach (var item in Charms)
        {
            if (name.Equals(item.CharmName))
            {
                item.HasCollected = true;
                return true;
            }
        }
        Debug.Log("无法找到指定的护符，请检查护符名字");
        return false;
    }

    /// <summary>
    /// 装备护符
    /// </summary>
    /// <param name="name"> 护符名称 </param>
    /// <returns></returns>
    public bool EquipCharm(string name)
    {
        foreach (var charm in Charms)
        {
            if (!charm.HasEquiped && name.Equals(charm.CharmName))
            {
                charm.HasEquiped = true;
                charm.OnEquip();
                return true;
            }
        }
        return false; //没有找到匹配的护符名称
    }
    public bool EquipCharm(CharmSO _charmSO)
    {
        foreach (var charm in Charms)
        {
            if (!charm.HasEquiped && _charmSO.Equals(charm))
            {
                charm.HasEquiped = true;
                charm.OnEquip();
                return true;
            }
        }
        return false; //没有找到匹配的护符名称
    }

    /// <summary>
    /// 卸载护符
    /// </summary>
    /// <param name="name">护符名称</param>
    /// <returns></returns>
    public bool DisEquipCharm(string name)
    {
        foreach (CharmSO charm in Charms)
        {
            if (charm.HasEquiped && name.Equals(charm.CharmName))
            {
                charm.HasEquiped = false;
                charm.OnDisEquip();
                return true;
            }
        }
        return false;
    }
    public bool DisEquipCharm(CharmSO _charmSO)
    {
        foreach (CharmSO charm in Charms)
        {
            if (charm.HasEquiped && _charmSO.Equals(charm))
            {
                charm.HasEquiped = false;
                charm.OnDisEquip();
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




