using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D_Key_Down : PlayerFSMBaseTrigger
{
    public override bool IsTriggerReach(FSMManager<PlayerStates, PlayerTriggers> fsm_Manager)
    {
        return Input.GetKeyDown(KeyCode.D);
    }
}
