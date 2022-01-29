using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonLight : Trigger2DBase
{
    public float g;
    private PlayerController playerController;

    protected override void exitEvent()
    {
        if(playerController.gravityLock==false)
        {
            playerController.RB.gravityScale = playerController.playerInfo.normalGravityScale;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        if (canWork && targetLayer.Contains(collision.gameObject))
        {
            if (playerController.gravityLock == false)
            {
                playerController.RB.gravityScale = g;
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (canWork && targetLayer.Contains(collision.gameObject))
        {
            // enterEvent();
            playerController = collision.GetComponent<PlayerController>();
        }
    }
}
