using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalInputAndHaveExitTimeTrigger : PlayerFSMBaseTrigger
{
    public override bool IsTriggerReach(FSMManager<PlayerStates, PlayerTriggers> fsm_Manager)
    {
        return (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.D)) && IsExitTimeReached(fsm_Manager);
    }
}
