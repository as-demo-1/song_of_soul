using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains all the variables that will be serialized and saved to a file.<br/>
/// Can be considered as a save file structure or format.
/// </summary>
[Serializable]
public class Save
{
    // This will change according to whatever data that needs to be stored
    public int _healthMax;
    public uint _goldAmount;
    public uint _weaponLevel;
    // The variables need to be public, else we would have to write trivial getter/setter functions.
    public List<string> _finishedObjectGuid = new List<string>();
    public Dictionary<string, int> _sceneInterative = new Dictionary<string, int>();
    public List<string> _bossGUID = new List<string>();
    public List<SerializedItemStack> _itemStacks = new List<SerializedItemStack>();
    public List<SerializedItemStack> _storeStacks = new List<SerializedItemStack>();
    public List<string> _destructivePlatformGuid = new List<string>();

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }
}