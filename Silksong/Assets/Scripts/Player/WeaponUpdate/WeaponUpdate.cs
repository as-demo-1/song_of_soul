using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


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

        InventoryManager inventoryManager=new InventoryManager();
        inventoryManager._currentInventory = playerInventory;
        foreach (RequiredMaterial i in requiredItem.requiredMaterial)
        {
            ItemSO item = inventoryManager.SearchItem(i.id);
            if (playerInventory.Count(item) < i.amonut)
                return false;
            
        }

        foreach (RequiredMaterial i in requiredItem.requiredMaterial)
        {
            ItemSO item = inventoryManager.SearchItem(i.id);
            inventoryManager.RemoveItem(i.id,i.amonut);//¿Û³ý²ÄÁÏ
        }
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
