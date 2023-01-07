using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjSaveSystem
{
    static List<string> saveList;
    public static Action whenSaveMap;
    public static void AddNewMapObject(string guid)
    {
        if(saveList == null)saveList = new List<string>();
        if(!saveList.Contains(guid))
            saveList.Add(guid);
    }

    public static void RemoveMapObject(string guid)
    {
        if(saveList != null&&saveList.Contains(guid))saveList.Remove(guid); 
    }

    public static bool ContainsObject(string guid)
    {
        if(saveList!=null&&saveList.Contains(guid))return true;
        else return false;  
    }

    public static void SaveAllMapObject()
    {
        SaveSystem _saveSystem = GameManager.Instance.saveSystem;
        whenSaveMap?.Invoke();
        for (int i = 0; i < saveList.Count; i++)
        {
            if (!_saveSystem.ContainDestructiblePlatformGUID(saveList[i]))
            {
                _saveSystem.AddDestructiblePlatformGUID(saveList[i]);
            }
        }
        
    }
}
