using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRun : PlayerAction
{
    public PlayerRun(PlayerController playerController) : base(playerController) { }
    public override void StateStart(EPlayerState oldState)
    {
        playerController.playerToCat.catMoveStart();
    }
    public override void StateUpdate()
    {

        playerController.CheckAddItem();
        playerController.CheckFlipPlayer(1f);
        playerController.CheckHorizontalMove(0.4f);
        playerController.playerToCat.moveDistanceCount();
    }
}

