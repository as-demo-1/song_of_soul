using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandTrigger : PlayerFSMBaseTrigger
{
    public override bool IsTriggerReach(FSMManager<PlayerStates, PlayerTriggers> fsm_Manager)
    {
        Debug.Log("IsGrounded is " + fsm_Manager.playerController.IsGrounded);
        return fsm_Manager.playerController.IsGrounded;
    }
}
