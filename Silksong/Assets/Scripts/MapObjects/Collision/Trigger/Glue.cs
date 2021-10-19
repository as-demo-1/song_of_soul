using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glue : Trigger2DBase
{
    protected PlayerController playerController;
    public float downSpeedRate;
    protected override void enterEvent()
    {
        playerController.SpeedRate = downSpeedRate;
        playerController.canJump = false;
    }

    protected override void exitEvent()
    {
        playerController.SpeedRate = 1;
        playerController.canJump = true;
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
