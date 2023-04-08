using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHurt : PlayerAction
{
    public PlayerHurt(PlayerController playerController) : base(playerController) { }

    public override void StateStart(EPlayerState oldState)
    {
        playerController.setRigidVelocity(Vector2.zero);
        PlayerAnimatorParamsMapping.SetControl(false);
        playerController.hurt.Play();
    }
    public override void StateEnd(EPlayerState newState)
    {
        PlayerAnimatorParamsMapping.SetControl(true);
  
    }
    public override void StateUpdate()
    {

    }

}