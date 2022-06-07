using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTableScriptableObject : ScriptableObject
{
    [Tooltip("物品id   id")]
    [SerializeField] private string _id;
    [Tooltip("物品名称  item name")]
    [SerializeField] private string _nameSid;
    [Tooltip("物品说明  item desc")]
    [SerializeField] private string _descSid;
    [Tooltip("效果id   the id of buff")]
    [SerializeField] private string _buffId;
    [Tooltip("效果数值  buff effect")]
    [SerializeField] private string _buffVal;
    [Tooltip("分组  type id")]
    [SerializeField] private string _typeID;
    [Tooltip("图标 icon asset")]
    [SerializeField] private Sprite _icon = default;

    public string ID => _id;
    public string NameSid => _nameSid;
    public string DescSid => _descSid;
    public string BuffID => _buffId;
    public string BuffVal => _buffVal;
    public string TypeID => _typeID;
    public Sprite Icon => _icon;

    public void Crt(string id, string nameSid, string descSid, string buffId, string buffVal)
    {
        _id = id;
        _nameSid = nameSid;
        _descSid = descSid;
        _buffId = buffId;
        _buffVal = buffVal;
        _typeID = id.Substring(1, 2);
    }

    public void Upd(string nameSid, string descSid, string buffId, string buffVal)
    {
        _nameSid = nameSid;
        _descSid = descSid;
        _buffId = buffId;
        _buffVal = buffVal;
    }
}
