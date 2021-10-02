using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalInputTrigger : PlayerFSMBaseTrigger
{
    public override bool IsTriggerReach(FSMManager<PlayerStates, PlayerTriggers> fsm_Manager)
    {
        return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D);
    }
}
