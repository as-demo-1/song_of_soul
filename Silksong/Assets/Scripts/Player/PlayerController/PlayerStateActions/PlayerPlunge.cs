using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlunge : PlayerAction
{
    public PlayerPlunge(PlayerController playerController) : base(playerController) 
    {
        groudCheck = playerController.groundCheckCollider;
        waterCheck = playerController.underWaterCheckCollider;
    }

    private float plungeStartPositionY;
    private float plungeDistance;

    public int plungeStrength;
    CapsuleCollider2D groudCheck;
    Collider2D waterCheck;

    public override void StateStart(EPlayerState oldState)
    {

        // 竖直下落
        playerController.setRigidGravityScale(0f);
        playerController.gravityLock = true;
        playerController.setRigidVelocity(new Vector2(0, -1 * Constants.PlayerPlungeSpeed));


        plungeStrength = 0;

        plungeDistance = 0.0f;
        plungeStartPositionY = playerController.transform.position.y;
        PlayerAnimatorParamsMapping.SetControl(false);
 
        groudCheck.offset = new Vector2(0, Constants.PlungeingGroundCheckBoxYOff);
        groudCheck.size = new Vector2(Constants.playerBoxColliderWidth, Constants.PlungeingGroudCheckBoxYSize);

        waterCheck.offset = new Vector2(Constants.plungeWaterCheckColliderOffsetX, Constants.plungeWaterCheckColliderOffsetY);
    }

    public override void StateUpdate()
    {
        float positionY = playerController.transform.position.y;
        plungeDistance = plungeStartPositionY - positionY;

        // 更新Strength
        int i = plungeStrength;
        while (i < playerController.plungeStrengthArr.Length - 1 && plungeDistance > playerController.plungeStrengthArr[i + 1])
        {
            i++;
        }
        plungeStrength = i;
    }

    public override void StateEnd(EPlayerState newState)
    {

        //Debug.Log("Landed! Plunge strength:" + plungeStrength + "Distance:" + plungeDistance);
        playerController.gravityLock = false;
        playerController.setRigidGravityScaleToNormal();
        int strength = plungeStrength;
        plungeStrength = 0;
        plungeDistance = 0.0f;

        PlayerAnimatorParamsMapping.SetControl(true);
        groudCheck.offset = new Vector2(Constants.playerGroundCheckColliderOffsetX, Constants.playerGroundCheckColliderOffsetY);
        groudCheck.size = new Vector2(Constants.playerGroundCheckColliderSizeX, Constants.playerGroundCheckColliderSizeY);

        waterCheck.offset = new Vector2(Constants.playerWaterCheckColliderOffsetX, Constants.playerWaterCheckColliderOffsetY);

        if (newState == EPlayerState.Hurt) return;


        if (strength == 3)
        {
            playerController.plunge.Play();
        }

  
        RaycastHit2D raycastHit = Physics2D.Raycast(playerController.transform.position, Vector2.down, 10, 1 << LayerMask.NameToLayer("Ground"));
        float downDistance = raycastHit.distance - Constants.playerBoxColliderHeight / 2;
        playerController.transform.position = new Vector2(playerController.transform.position.x, playerController.transform.position.y - downDistance);

        
    }

}
