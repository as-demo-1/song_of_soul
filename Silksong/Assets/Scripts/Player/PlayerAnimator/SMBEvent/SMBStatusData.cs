using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBStatusData : SMBEventTimeStamp
{
    public PlayerStatus playerStatus;
    public bool statusIsActive;  
    public PlayerStatusDic.PlayerStatusFlag.WayOfChangingFlag wayOfChangingFlag;
    public override void EventActive()
    {
        PlayerController.Instance.PlayerAnimatorStatesControl.PlayerStatusDic.SetPlayerStatusFlag(playerStatus, statusIsActive, wayOfChangingFlag);
    }
}
