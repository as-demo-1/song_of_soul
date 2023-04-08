using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GamingSave", order = 2)]
public class GamingSaveSO : ScriptableObject
{
    //public HashSet<string> destroyedGameObjs=new HashSet<string>();   hashSet and dictionary can not be saved when reStart game in editor.
    List<string> destroyedGameObjs = new List<string>();//if public,it will be saved to disk with SO
    Dictionary<string, int> intGamingDic = new Dictionary<string, int>();
    Dictionary<string, bool> boolGamingDic = new Dictionary<string, bool>();

    public void clearAllData()
    {
        destroyedGameObjs.Clear();
        intGamingDic.Clear();
        boolGamingDic.Clear();
    }
    public bool addDestroyedGameObj(string guid)
    {
        destroyedGameObjs.Add(guid);
        return true;
    }
    public bool checkDestroyedGameObj(string guid)
    {
        return destroyedGameObjs.Contains(guid);
    }

    public bool addIntGamingData(int v,string guid)
    {
        if (intGamingDic.ContainsKey(guid)) intGamingDic[guid] = v;
        else intGamingDic.Add(guid, v);
        return true;
    }

    public int getIntGamingData(string guid,out bool ifError)
    {
        ifError = false;
        if (intGamingDic.ContainsKey(guid) == false)
        {
            ifError = true;
            return 0;
            
        }
            return intGamingDic[guid];
    }

    public bool addBoolGamingData(bool v, string guid)
    {
        if (boolGamingDic.ContainsKey(guid)) boolGamingDic[guid] = v;
        else boolGamingDic.Add(guid, v);
        return true;
    }

    public bool getBoolGamingData(string guid,out bool ifError)
    {
        ifError = false;
        if (boolGamingDic.ContainsKey(guid) == false)
        {
            ifError = true;
            return false;
        }
        return boolGamingDic[guid];
    }

}


#if UNITY_EDITOR

[CustomEditor(typeof(GamingSaveSO))]
public class GamingSaveEdior : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GamingSaveSO config = target as GamingSaveSO;
       
        if (GUILayout.Button("clear all data"))
        {
            config.clearAllData();
        }
    }
}
#endif
