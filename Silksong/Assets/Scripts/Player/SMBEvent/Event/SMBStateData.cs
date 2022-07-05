using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SMBStateData : SMBEventTimeStamp
{
    public EPlayerState newState;
    public override void EventActive(MonoBehaviour mono)
    {
      //  Debug.Log("event " + this.GetType());
      if(newState!=0)
        PlayerController.Instance.playerAnimatorStatesControl.ChangePlayerState(newState);
    }
}
