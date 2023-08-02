using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

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
    [ShowInInspector] public List<string> _finishedObjectGuid = new List<string>();
    [ShowInInspector] public Dictionary<string, int> _sceneInterative = new Dictionary<string, int>();
    [ShowInInspector] public List<string> _bossGUID = new List<string>();

    //public List<SerializedItemStack> _itemStacks = new List<SerializedItemStack>();
    //public List<SerializedItemStack> _storeStacks = new List<SerializedItemStack>();

    public List<string> _destroyedGameObjs = new List<string>();// 只保存永久销毁

    public string levelName;// 玩家所在的场景名
    public long timestamp;// 存档时间
    //about player-----------------------------------------------------------------------------
    [ShowInInspector]
    public Dictionary<EPlayerStatus, bool> learnedSkills = new Dictionary<EPlayerStatus, bool>
    {   { EPlayerStatus.CanBreakMoon,false},
        { EPlayerStatus.CanCastSkill,false},
        { EPlayerStatus.CanClimbIdle,false},
        { EPlayerStatus.CanHeal,true},
        { EPlayerStatus.CanJump,true},
        { EPlayerStatus.CanNormalAttack,false},
        { EPlayerStatus.CanPlunge,false},
        { EPlayerStatus.CanRun,true},
        { EPlayerStatus.CanSprint,false},
        { EPlayerStatus.CanToCat,false},
        { EPlayerStatus.CanSing,false},
        { EPlayerStatus.CanSwim,true},
        { EPlayerStatus.CanDive,false},
        { EPlayerStatus.CanWaterSprint,false},
        { EPlayerStatus.CanHeartSword,false},

    };


    public string ToJson()
    {
        string jsonData = JsonConvert.SerializeObject(this, Formatting.Indented);
        return jsonData;
       // return JsonUtility.ToJson(this);
    }

    public Save LoadFromJson(string json)
    {
        // JsonUtility.FromJsonOverwrite(json, this);
        return  JsonConvert.DeserializeObject<Save>(json);
    }
}