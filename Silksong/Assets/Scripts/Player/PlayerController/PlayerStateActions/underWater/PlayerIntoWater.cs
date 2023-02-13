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
        PlayerAnimatorParamsMapping.SetControl(false);
        playerController.setRigidGravityScale(0);
        playerController.gravityLock = true;
        inPos = playerController.transform.position;

        if (oldState == EPlayerState.Jump)
        {
           // Debug.Log("endJump");
            (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).EndJump();
        
        }

        if (oldState == EPlayerState.ClimbJump)
        {
            // Debug.Log("endJump");
            (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.ClimbJump] as PlayerClimbJump).EndJump();

        }
            


        float v = playerController.getRigidVelocity().magnitude;
        if (v < 1)
        {
            Vector2 speed = playerController.getRigidVelocity();
            speed += new Vector2(0.5f, 0.5f);
            Debug.Log(speed);
            playerController.setRigidVelocity(speed* 2 );
        }

        playerController.playerToCat.toHuman();

        PlayerJump pj = (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump);
        pj.ClearAllJumpCount();
        pj.justResetDoubleJump();

        PlayerSprint ps = (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Sprint] as PlayerSprint);
        ps.resetAirSprintLeftCount();
    }

    public override void StateUpdate()
    {
        if(Vector2.Distance(inPos,playerController.transform.position)>Constants.PlayerMinIntoWaterDistance )
        {
            //Debug.Log("slow down");
            playerController.setRigidLinearDrag(Constants.PlayerIntoWaterLinearDarg);
        }

  
    }

    public override void StateEnd(EPlayerState newState)
    {
        PlayerAnimatorParamsMapping.SetControl(true);
        playerController.setRigidLinearDrag(0);
        playerController.gravityLock = false;
        if ((int)newState >= 300)
        {
            playerController.setRigidVelocity(Vector2.zero);
        }
        else
        {
            playerController.setRigidGravityScaleToNormal();
        }

    }
}

