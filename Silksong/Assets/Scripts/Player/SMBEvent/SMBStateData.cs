using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBStateData : SMBEventTimeStamp
{
    public EPlayerState newState;
    public override void EventActive()
    {
        PlayerController.Instance.PlayerAnimatorStatesControl.ChangePlayerState(newState);
        //if (newState != PlayerController.Instance.PlayerAnimatorStatesControl.CurrentPlayerState)
            //PlayerController.Instance.PlayerAnimatorStatesManager.ChangePlayerStatus(statusIsActive ? newStatus : PlayerStatus.None);
    }
}
