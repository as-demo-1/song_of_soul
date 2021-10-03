using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_TurnAround_State : PlayerFSMBaseState
{
    public override void EnterState(FSMManager<PlayerStates, PlayerTriggers> fSM_Manager)
    {
        MovementScript.Flip(fSM_Manager.playerController.SpriteRenderer, ref fSM_Manager.playerController.playerFacingRight);
        Debug.Log("Player_TurnAround_State");
    }
}
