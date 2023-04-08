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
        playerController.checkMaxFallSpeed();
    }


}
