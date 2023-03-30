using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBEffectData : SMBEventTimeStamp
{
    public bool enableEffect;
    public string chlidEffectName;
    public override void EventActive(MonoBehaviour mono)
    {
        //Debug.Log(newState);
        GameObject effect = mono.transform.Find(chlidEffectName).gameObject;
        if(effect)
        {
            effect.SetActive(enableEffect);
        }
        else
        {
            Debug.Log("no this chlid effect");
        }
    }
}
