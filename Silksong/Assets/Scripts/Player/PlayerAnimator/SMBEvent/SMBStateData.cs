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
