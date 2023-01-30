using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCatToHumanExtraJump : PlayerAction
{
    public PlayerCatToHumanExtraJump(PlayerController playerController) : base(playerController) { }
    public override void StateUpdate()
    {
        playerController.CheckHorizontalMove(0.4f);
    }
}
