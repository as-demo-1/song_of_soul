using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBWwiseData : SMBEventTimeStamp
{
    public string EventName;
    public override void EventActive()
    {
        AkSoundEngine.PostEvent(EventName,PlayerController.Instance.gameObject);

    }
}
