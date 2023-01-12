using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MonsterInfoManager
{
    // 存放所有数据的字典
    private Dictionary<string, MonsterInfor> _monsterDic = new Dictionary<string, MonsterInfor>();

    private MonsterInfoManager()
    {
        string path = "ScriptableObjects/monsters";
        var all = Resources.LoadAll<MonsterScriptableObject>(path);

        foreach (var monster in all)
        {
            _monsterDic[monster.ID] = new MonsterInfor(monster);
        }
    }

    public void Init()
    {
    }

    public MonsterInfor GetOneMonsterByID(string id)
    {
        if (_monsterDic.ContainsKey(id))
        {
            return _monsterDic[id];
        }
        else
        {
            Debug.LogError("id为" + id + "的怪物不存在，请检查怪物表以及输入id");
            return default;
        }
    }

    private static MonsterInfoManager _instance;
    public static MonsterInfoManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MonsterInfoManager();
            }

            return _instance;
        }
    }
}

public class MonsterInfor
{
    private string _id;             // 物品id   id
    private string _nameSid;        // 物品名称  item name
    private string _descSid;        // 物品说明  item desc
    private string _health;         // 怪物生命值 the hp of monster
    private string _damage;         // 怪物伤害  this monster can damage num
    private string _speed;          // 速度   speed
    private string _drop1;          // 掉落1  first drop
    private string _drop2;          // 掉落2  second drop
    private string _drop3;          // 掉落3  third drop
    private int _drop1_num;        // 掉落1数量  first drop num
    private int _drop2_num;        // 掉落2数量  second drop num
    private int _drop3_num;        // 掉落3数量  third drop num

    public string ID => _id;
    public string NameSid => _nameSid;
    public string DescSid => _descSid;
    public string Health => _health;
    public string Damage => _damage;
    public string Speed => _speed;
    public string Drop1 => _drop1;
    public string Drop2 => _drop2;
    public string Drop3 => _drop3;
    public int Drop1Num => _drop1_num;
    public int Drop2Num => _drop2_num;
    public int Drop3Num => _drop3_num;

    public MonsterInfor(MonsterScriptableObject monster)
    {
        _id = monster.ID;
        _nameSid = monster.NameSid;
        _descSid = monster.DescSid;
        _health = monster.Health;
        _damage = monster.Damage;
        _speed = monster.Speed;
        _drop1 = monster.Drop1;
        _drop2 = monster.Drop2;
        _drop3 = monster.Drop3;
        _drop1_num = monster.Drop1Num;
        _drop2_num = monster.Drop2Num;
        _drop3_num = monster.Drop3Num;
    }
}
