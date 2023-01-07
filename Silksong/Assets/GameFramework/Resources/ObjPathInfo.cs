using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ObjPathInfo : ISerializationCallbackReceiver
{
    [NonSerialized]
    public ObjType  objType;
    public string objName;
    public string objPath;

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
        objType = (ObjType)System.Enum.Parse(typeof(ObjType),objName);
    }
}

public class ObjPathList
{
    public List<ObjPathInfo> pathInfoList;
}