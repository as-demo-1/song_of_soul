using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 这个文件夹下统一了对应组件的触发器，包括碰撞进入和退出
/// </summary>
public class Glue : Trigger2DBase
{
    protected PlayerController playerController;
    public float downSpeedRate;
    protected override void enterEvent()
    {
        playerController.playerToCat.toHuman();
        playerController.playerCharacter.gluedCount++;
        playerController.PlayerHorizontalMoveControl.SpeedRate = downSpeedRate;
        playerController.PlayerHorizontalMoveControl.GroundAccelerationFactor = downSpeedRate;
        playerController.playerStatusDic.SetPlayerStatusFlag(EPlayerStatus.CanJump, false, PlayerStatusDic.PlayerStatusFlag.WayOfChangingFlag.OverrideBuffFlag);
        playerController.playerStatusDic.SetPlayerStatusFlag(EPlayerStatus.CanToCat, false, PlayerStatusDic.PlayerStatusFlag.WayOfChangingFlag.OverrideBuffFlag);
    }

    protected override void exitEvent()
    {
        if (--playerController.playerCharacter.gluedCount==0)
        {
            playerController.PlayerHorizontalMoveControl.SpeedRate = 1;
            playerController.PlayerHorizontalMoveControl.GroundAccelerationFactor = 1;
            playerController.playerStatusDic.SetPlayerStatusFlag(EPlayerStatus.CanJump, true, PlayerStatusDic.PlayerStatusFlag.WayOfChangingFlag.OverrideBuffFlag);
            playerController.playerStatusDic.SetPlayerStatusFlag(EPlayerStatus.CanToCat, true, PlayerStatusDic.PlayerStatusFlag.WayOfChangingFlag.OverrideBuffFlag);
        }
      
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
