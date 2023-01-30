using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToCat : PlayerAction
{
    public PlayerToCat(PlayerController playerController) : base(playerController) { }

    public override void StateUpdate()
    {
        playerController.CheckHorizontalMove(0.4f);
    }
}
