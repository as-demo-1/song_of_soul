using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNormalAttack : PlayerAction
{
    public PlayerNormalAttack(PlayerController playerController) : base(playerController) { }

    public override void StateStart(EPlayerState oldState)
    {

        playerController.CheckFlipPlayer(1f);



    }
    public override void StateUpdate()
    {
        playerController.CheckHorizontalMove(0.5f);
        playerController.checkMaxFallSpeed();
    }
}
