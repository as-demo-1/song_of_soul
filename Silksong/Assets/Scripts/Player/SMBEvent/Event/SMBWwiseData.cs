using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBWwiseData : SMBEventTimeStamp
{
    public string EventName;
    public override void EventActive(MonoBehaviour mono)
    {
        Debug.Log(EventName +"  "+ mono.name);
        AkSoundEngine.PostEvent(EventName,mono.gameObject);
    }
}
