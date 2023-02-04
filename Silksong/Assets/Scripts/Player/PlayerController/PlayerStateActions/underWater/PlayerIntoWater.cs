using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIntoWater : PlayerAction
{
    public PlayerIntoWater(PlayerController playerController) : base(playerController) { }
    public Vector2 inPos;
    public override void StateStart(EPlayerState oldState)//need a min into water distance to make sure player into water corretly/totaly
    {
        base.StateStart(oldState);
        PlayerAnimatorParamsMapping.HasControl = false;
        playerController.setRigidGravityScale(0);
        playerController.gravityLock = true;
        inPos = playerController.transform.position;

        if (oldState == EPlayerState.Jump)
        {
           // Debug.Log("endJump");
            (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).EndJump();
        }
         

        float v = playerController.getRigidVelocity().magnitude;
        if (v < 1)
        {
            Vector2 speed = playerController.getRigidVelocity();
            Debug.Log(speed);
            playerController.setRigidVelocity(speed + new Vector2(0.5f, 0.5f));
            playerController.setRigidVelocity(playerController.getRigidVelocity() * 2 / v);
            Debug.Log(playerController.getRigidVelocity());
        }
        }

    public override void StateUpdate()
    {
        if(Vector2.Distance(inPos,playerController.transform.position)>Constants.PlayerMinIntoWaterDistance )
        {
            Debug.Log("slow down");
            playerController.setRigidLinearDrag(Constants.PlayerIntoWaterLinearDarg);
        }

  
    }

    public override void StateEnd(EPlayerState newState)
    {
        base.StateEnd(newState);
        PlayerAnimatorParamsMapping.HasControl = true;
        playerController.setRigidLinearDrag(0);
        playerController.gravityLock = false;
        playerController.setRigidVelocity(Vector2.zero);
    }
}

