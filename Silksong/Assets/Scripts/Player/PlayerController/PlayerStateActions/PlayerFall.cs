using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFall : PlayerAction
{

    public PlayerFall(PlayerController playerController) : base(playerController) { }
    public override void StateUpdate()
    {
        playerController.CheckFlipPlayer(1f);
        playerController.CheckHorizontalMove(0.5f);
        checkMaxFallSpeed();
    }

    public void checkMaxFallSpeed()
    {
        if (playerController.getRigidVelocity().y < -Constants.PlayerMaxFallSpeed)
        {
            playerController.setRigidVelocity(new Vector2(playerController.getRigidVelocity().x, -Constants.PlayerMaxFallSpeed));
        }
    }
}
