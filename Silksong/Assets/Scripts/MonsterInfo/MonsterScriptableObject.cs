using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterScriptableObject : ScriptableObject
{
    [Tooltip("怪物id   id")]
    [SerializeField] private string _id;
    [Tooltip("怪物名称  item name")]
    [SerializeField] private string _nameSid;
    [Tooltip("怪物说明  item desc")]
    [SerializeField] private string _descSid;
    [Tooltip("怪物生命值 the hp of monster")]
    [SerializeField] private string _health;
    [Tooltip("怪物伤害  this monster can damage num")]
    [SerializeField] private string _damage;
    [Tooltip("速度   speed")]
    [SerializeField] private string _speed;
    [Tooltip("掉落1  first drop")]
    [SerializeField] private string _drop1 = "0";
    [Tooltip("掉落2  second drop")]
    [SerializeField] private string _drop2 = "0";
    [Tooltip("掉落3  third drop")]
    [SerializeField] private string _drop3 = "0";
    [Tooltip("掉落1数量  first drop num")]
    [SerializeField] private int _drop1_num = default;
    [Tooltip("掉落2数量  second drop num")]
    [SerializeField] private int _drop2_num = default;
    [Tooltip("掉落3数量  third drop num")]
    [SerializeField] private int _drop3_num = default;

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

    public void Crt(string id, string nameSid, string descSid, string health, string damage, string speed, string drop1, string drop2, string drop3)
    {
        _id = id;
        _nameSid = nameSid;
        _descSid = descSid;
        _health = health;
        _damage = damage;
        _speed = speed;
        if (drop1 != "0")
        {
            string[] tmp = drop1.Split('~');
            _drop1 = tmp[0];
            try
            {
                _drop1_num = int.Parse(tmp[1]);
            }
            catch (System.Exception ex)
            {
            }
        }
        if (drop2 != "0")
        {
            string[] tmp = drop2.Split('~');
            _drop2 = tmp[0];
            try
            {
                _drop2_num = int.Parse(tmp[1]);
            }
            catch (System.Exception ex)
            {

            }
        }
        if (drop3 != "0")
        {
            string[] tmp = drop3.Split('~');
            _drop3 = tmp[0];
            try
            {
                _drop3_num = int.Parse(tmp[1]);
            }
            catch (System.Exception ex)
            {

            }
        }
    }

    public void Upd(string nameSid, string descSid, string health, string damage, string speed, string drop1, string drop2, string drop3)
    {
        _nameSid = nameSid;
        _descSid = descSid;
        _health = health;
        _damage = damage;
        _speed = speed;
        if (drop1 != "0")
        {
            string[] tmp = drop1.Split('~');
            _drop1 = tmp[0];
            try
            {
                _drop1_num = int.Parse(tmp[1]);
            }
            catch (System.Exception ex)
            {
            }
        }
        if (drop2 != "0")
        {
            string[] tmp = drop2.Split('~');
            _drop2 = tmp[0];
            try
            {
                _drop2_num = int.Parse(tmp[1]);
            }
            catch (System.Exception ex)
            {

            }
        }
        if (drop3 != "0")
        {
            string[] tmp = drop3.Split('~');
            _drop3 = tmp[0];
            try
            {
                _drop3_num = int.Parse(tmp[1]);
            }
            catch (System.Exception ex)
            {

            }
        }
    }
}
