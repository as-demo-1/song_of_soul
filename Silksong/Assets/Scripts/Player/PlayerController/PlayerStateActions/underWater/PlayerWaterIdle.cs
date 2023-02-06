using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerWaterIdle : PlayerAction
{
    public PlayerWaterIdle(PlayerController playerController) : base(playerController) { }

    public override void StateStart(EPlayerState oldState)
    {
        //Debug.Log("into waterIdle");
        playerController.setRigidVelocity(Vector2.zero);
    }

    public override void StateUpdate()
    {
        playerController.CheckAddItem();
    }
}

