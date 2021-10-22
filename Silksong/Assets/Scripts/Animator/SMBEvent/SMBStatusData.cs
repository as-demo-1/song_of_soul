using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBStatusData : SMBEventTimeStamp
{
    public PlayerStatus newStatus;
    public bool statusIsActive;
    public override void Behaviour()
    {
        if (newStatus != PlayerController.Instance.PlayerAnimatorStatesManager.CurrentPlayerStatus)
            PlayerController.Instance.PlayerAnimatorStatesManager.ChangePlayerStatus(statusIsActive ? newStatus : PlayerStatus.None);
    }
}

public abstract class SMBEventTimeStamp : SMBEvent
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
    public override bool NotEndYet(float currentTimeNormalized)
    {
        return this.timeNormalized <= currentTimeNormalized;
    }
    public override float FieldToCompare => timeNormalized;
}
