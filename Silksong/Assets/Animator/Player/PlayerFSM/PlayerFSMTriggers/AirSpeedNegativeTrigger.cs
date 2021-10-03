using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSpeedNegativeTrigger : PlayerFSMBaseTrigger
{
    public override bool IsTriggerReach(FSMManager<PlayerStates, PlayerTriggers> fsm_Manager)
    {
        return fsm_Manager.rigidbody.velocity.y <= 0 && !fsm_Manager.playerController.IsGrounded;
    }
}
