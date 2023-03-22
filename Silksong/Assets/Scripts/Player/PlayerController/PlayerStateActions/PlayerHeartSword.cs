using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeartSword : PlayerAction
{
    public PlayerHeartSword(PlayerController playerController) : base(playerController) { }

    public override void StateStart(EPlayerState oldState)
    {

        playerController.CheckFlipPlayer(1f);
        playerController.setRigidVelocity(Vector2.zero);
    }
    public override void StateUpdate()
    {

    }
}

