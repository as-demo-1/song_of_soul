using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBStatusData : SMBTimeStamp
{
    public PlayerStatus currentStatus;
    public bool statusIsActive;
    public override void Behaviour()
    {
        Debug.Log("CurrentStatus :" + currentStatus);
    }
}

public abstract class SMBTimeStamp : SMBEvent
{
    [Range(0, 1)]
    public float timeNormalized;
    public override bool Next(float previousTimeNormalized)
    {
        return timeNormalized <= previousTimeNormalized;
    }
    public override bool IsActive(float previousTimeNormalized, float currentTimeNormalized)
    {
        return timeNormalized > previousTimeNormalized && timeNormalized <= currentTimeNormalized;
    }
    public override float FieldToCompare => timeNormalized;
}
