using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBStateData : SMBEventTimeStamp
{
    public PlayerState newState;
    public override void EventActive()
    {
        if (newState != PlayerController.Instance.PlayerAnimatorStatesControl.CurrentPlayerState)
            //PlayerController.Instance.PlayerAnimatorStatesManager.ChangePlayerStatus(statusIsActive ? newStatus : PlayerStatus.None);
            PlayerController.Instance.PlayerAnimatorStatesControl.ChangePlayerState(newState);
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
