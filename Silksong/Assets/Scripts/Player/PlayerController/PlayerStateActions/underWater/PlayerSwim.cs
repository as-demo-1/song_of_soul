using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSwim : PlayerAction
{
    public PlayerSwim(PlayerController playerController) : base(playerController) { }
    public override void StateStart(EPlayerState oldState)
    {
        playerController.setRigidVelocity(Vector2.zero);
        (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).resetAllJumpCount();
    }
    public override void StateUpdate()
    {

        playerController.CheckAddItem();
        playerController.CheckFlipPlayer(1f);
        playerController.CheckHorizontalMove(0.4f);
    }

    public override void StateEnd(EPlayerState newState)
    {
        if (newState != EPlayerState.Jump)
        {
            PlayerJump pj = (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump);
            pj.ClearAllJumpCount();
            pj.justResetDoubleJump();
        }
 
    }
}