using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdle : PlayerAction
{
    public PlayerIdle(PlayerController playerController) : base(playerController) { }

    public override void StateStart(EPlayerState oldState)
    {
        playerController.setRigidGravityScaleToNormal();
    }

    public override void StateUpdate()
    {
        playerController.CheckAddItem();
        playerController.CheckHorizontalMove(0.4f);
    }
}
