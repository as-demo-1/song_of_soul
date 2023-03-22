using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbIdle : PlayerAction
{
    public PlayerClimbIdle(PlayerController playerController) : base(playerController) { }

    public override void StateStart(EPlayerState oldState)
    {
        playerController.setRigidGravityScale(0);
        playerController.setRigidVelocity(new Vector2(0, -Constants.PlayerClimbIdleFallSpeed));
        (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).resetAllJumpCount();
        (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Sprint] as PlayerSprint).resetAirSprintLeftCount();
    }
    public override void StateEnd(EPlayerState newState)
    {
        playerController.setRigidGravityScale(Constants.PlayerNormalGravityScale);
        (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).CurrentJumpCountLeft--;
    }


}
