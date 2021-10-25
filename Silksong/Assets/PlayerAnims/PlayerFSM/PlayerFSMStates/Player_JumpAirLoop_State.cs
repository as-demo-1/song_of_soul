using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_JumpAirLoop_State : PlayerFSMBaseState
{
    public override void Act_State(FSMManager<PlayerStates, PlayerTriggers> fSM_Manager)
    {
        if (Input.GetAxisRaw("Horizontal") == 1 & !fSM_Manager.playerController.playerInfo.playerFacingRight || 
            Input.GetAxisRaw("Horizontal") == -1 & fSM_Manager.playerController.playerInfo.playerFacingRight)
        {
            MovementScript.Flip(fSM_Manager.playerController.SpriteRenderer, ref fSM_Manager.playerController.playerInfo.playerFacingRight);
        }
    }
}
