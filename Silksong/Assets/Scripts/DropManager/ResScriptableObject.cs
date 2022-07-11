using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResScriptableObject : ScriptableObject
{
    [Tooltip("资源id   id")]
    [SerializeField] private string _id;
    [Tooltip("资源名称  name")]
    [SerializeField] private string _nameSid;
    [Tooltip("资源说明  desc")]
    [SerializeField] private string _descSid;

    public string ID => _id;
    public string NameSid => _nameSid;
    public string DescSid => _descSid;

    public void Crt(string id, string nameSid, string descSid)
    {
        _id = id;
        _nameSid = nameSid;
        _descSid = descSid;
    }

    public void Upd(string nameSid, string descSid)
    {
        _nameSid = nameSid;
        _descSid = descSid;
    }
}
