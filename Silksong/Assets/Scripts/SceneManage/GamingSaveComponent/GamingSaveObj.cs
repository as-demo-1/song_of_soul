using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GuidComponent))]
public abstract class GamingSaveObj<T>: MonoBehaviour
{
    public bool ban;
    public string GUID => GetComponent<GuidComponent>().GetGuid().ToString();

    public abstract T loadGamingData(out bool ifError);

    public abstract void saveGamingData(T data);



}
