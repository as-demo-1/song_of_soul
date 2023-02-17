using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// 通过SO存储所有的护符列表，并且存储玩家的获得情况与装备情况
/// </summary> 作者：次元

[CreateAssetMenu(fileName = "CharmList", menuName = "Charm/CharmList")]
public class CharmListSO : ScriptableObject
{
    [TableList(ShowIndexLabels = true)]
    [SerializeField] public List<Charm> Charms = new List<Charm>();

    private BuffManager bm;
    public void Init(BuffManager _bm)
    {
        bm = _bm;
        foreach (var charm in Charms)
        {
            charm.InitCharm(bm);
        }
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
                //charm.OnEquip();
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
                //charm.OnEquip();
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
        foreach (var charm in Charms)
        {
            if (charm.HasEquiped && name.Equals(charm.CharmName))
            {
                charm.HasEquiped = false;
                //charm.OnDisEquip();
                return true;
            }
        }
        return false;
    }
    public bool DisEquipCharm(CharmSO _charmSO)
    {
        foreach (var charm in Charms)
        {
            if (charm.HasEquiped && _charmSO.Equals(charm))
            {
                charm.HasEquiped = false;
                //charm.OnDisEquip();
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
                //charm.OnEquip();
            }
        }
    }
}




