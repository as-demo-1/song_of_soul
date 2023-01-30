using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCatIdle : PlayerAction
{
    public PlayerCatIdle(PlayerController playerController) : base(playerController) { }
    public override void StateStart(EPlayerState oldState)
    {
        playerController.playerToCat.toCat();
    }
    public override void StateUpdate()
    {
        playerController.CheckHorizontalMove(0.4f);
    }
}