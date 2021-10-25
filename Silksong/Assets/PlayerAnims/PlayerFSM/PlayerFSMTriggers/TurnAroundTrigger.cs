using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAroundTrigger : PlayerFSMBaseTrigger
{
    public override bool IsTriggerReach(FSMManager<PlayerStates, PlayerTriggers> fsm_Manager)
    {
        return Input.GetAxisRaw("Horizontal") == 1 & !fsm_Manager.playerController.playerInfo.playerFacingRight ||
            Input.GetAxisRaw("Horizontal") == -1 & fsm_Manager.playerController.playerInfo.playerFacingRight;
    }
}
