using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glue : Trigger2DBase
{
    protected PlayerController playerController;
    public float downSpeedRate;
    protected override void enterEvent()
    {
        playerController.PlayerHorizontalMoveControl.SpeedRate = downSpeedRate;
        playerController.PlayerHorizontalMoveControl.GroundAccelerationFactor = downSpeedRate;
        playerController.PlayerAnimatorStatesControl.PlayerStatusDic.SetPlayerStatusFlag(EPlayerStatus.CanJump, false, PlayerStatusDic.PlayerStatusFlag.WayOfChangingFlag.AndBuffFlag);
    }

    protected override void exitEvent()
    {
        playerController.PlayerHorizontalMoveControl.SpeedRate = 1;
        playerController.PlayerHorizontalMoveControl.GroundAccelerationFactor = 1;
        playerController.PlayerAnimatorStatesControl.PlayerStatusDic.SetPlayerStatusFlag(EPlayerStatus.CanJump, true, PlayerStatusDic.PlayerStatusFlag.WayOfChangingFlag.OverrideBuffFlags);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (canWork && targetLayer.Contains(collision.gameObject))
        {
            playerController = collision.gameObject.GetComponent<PlayerController>();
            enterEvent();
        }
    }

}
