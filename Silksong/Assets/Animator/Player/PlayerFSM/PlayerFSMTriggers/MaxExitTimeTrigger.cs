using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxExitTimeTrigger : PlayerFSMBaseTrigger
{
    public override bool IsTriggerReach(FSMManager<PlayerStates, PlayerTriggers> fsm_Manager)
    {
        return IsExitTimeReached(fsm_Manager);
    }
}
