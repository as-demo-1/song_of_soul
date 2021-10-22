using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalInputAndHaveExitTimeTrigger : PlayerFSMBaseTrigger
{
    public override bool IsTriggerReach(FSMManager<PlayerStates, PlayerTriggers> fsm_Manager)
    {
        return Input.GetAxisRaw("Horizontal") != 0 && IsExitTimeReached(fsm_Manager);
    }
}
