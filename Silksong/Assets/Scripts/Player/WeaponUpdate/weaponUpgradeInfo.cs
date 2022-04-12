using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public struct RequiredMaterial
{
    public string id;
    public int amonut;
}
[Serializable]
public class WeaponUpgradeInfo 
{
    public int level;//该次升级时武器的等级
    public int attack;
    public List<RequiredMaterial> requiredMaterial=new List<RequiredMaterial>() ;//该次升级时所需的材料id和对应的数量


}
