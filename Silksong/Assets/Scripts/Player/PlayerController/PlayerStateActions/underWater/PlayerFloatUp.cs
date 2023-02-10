using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFloatUp : PlayerAction
{
    public PlayerFloatUp(PlayerController playerController) : base(playerController) { }

    public override void StateStart(EPlayerState oldState)
    {
        playerController.gravityLock = true;
        playerController.setRigidVelocity(new Vector2(0,4f));
    }

    public override void StateUpdate()
    {
        playerController.CheckHorizontalMove(0.5f);
        playerController.CheckFlipPlayer();
        playerController.checkWaterSurface();
    }

    public override void StateEnd(EPlayerState newState)
    {
        playerController.gravityLock =false;
        playerController.setRigidVelocity(new Vector2(0, 0));

        playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.WaterSurfaceParamHash, false);
    }
}

