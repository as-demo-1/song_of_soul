using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/WeaponUpdate")]
public class WeaponUpdate : ScriptableObject
{
    public int weaponLevel=0;
    public InventorySO playerInventory;
    public WeaponUpgradeInfo[] weaponUpgradeInfoList;
    public bool checkResource()
    {
        WeaponUpgradeInfo requiredItem=null;
        foreach (WeaponUpgradeInfo i in weaponUpgradeInfoList)
        {
            if(i.level==(weaponLevel+1))
            {
                requiredItem = i;
                break;
            }
        }

        if (requiredItem == null)
            return false;

        //foreach (RequiredMaterial i in requiredItem.requiredMaterial)
        //{  
        //    if (playerInventory.Count(i.id) < i.amonut)
        //        return false;
        //    else
        //    {
        //        for (int j = 0; j < i.amonut; j++)
        //            playerInventory.Remove(item);//¿Û³ý²ÄÁÏ
        //    }
        //}
        return true;
    }
    public bool weaponUpdate()
    {
        if (checkResource())
        {
            weaponLevel++;
            return true;
        }
        else
        {
            return false;
        }
    }
}
