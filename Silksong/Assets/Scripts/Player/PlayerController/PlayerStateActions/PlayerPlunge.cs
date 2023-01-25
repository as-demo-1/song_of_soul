using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlunge : PlayerAction
{
    public PlayerPlunge(PlayerController playerController) : base(playerController) { }

    private float plungeStartPositionY;
    private float plungeDistance;

    public int plungeStrength;

    public override void StateStart(EPlayerState oldState)
    {

        // 竖直下落
        playerController.setRigidGravityScale(0f);
        playerController.gravityLock = true;
        playerController.setRigidVelocity(new Vector2(0, -1 * Constants.PlayerPlungeSpeed));


        plungeStrength = 0;

        plungeDistance = 0.0f;
        plungeStartPositionY = playerController.transform.position.y;
    }

    public override void StateUpdate()
    {
        float positionY = playerController.transform.position.y;
        plungeDistance = plungeStartPositionY - positionY;

        // 更新Strength
        int i = plungeStrength;
        while (i < playerController.plungeStrengthArr.Length - 1 && plungeDistance > playerController.plungeStrengthArr[i + 1])
        {
            // Debug.Log(plungeStrength);
            i++;
        }
        plungeStrength = i;

        // 在 DestructiblePlatform 组件更新 willBreakGround，在此更新animator param
        // playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.WillBreakGroundParamHash, willBreakGround);
    }

    public override void StateEnd(EPlayerState newState)
    {
        playerController.gravityLock = false;
        playerController.setRigidGravityScaleToNormal();


        if (plungeStrength == 3)
        {
            playerController.plunge.Play();
        }

        Debug.Log("Landed! Plunge strength:" + plungeStrength + "Distance:" + plungeDistance);
        plungeStrength = 0;
        plungeDistance = 0.0f;


    }

}
