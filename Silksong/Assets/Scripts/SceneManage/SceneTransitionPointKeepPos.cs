using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionPointKeepPos : SceneTransitionPoint
{
    private Vector3 playerRelativePos;
    public bool recordXPosWhenTrue;
    public Vector3 PlayerRelativePos
    {
        get { return playerRelativePos; }
    }
    

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (canWork && targetLayer.Contains(collision.gameObject))
        {
            Vector3 pos = collision.transform.position - transform.position;
            if(recordXPosWhenTrue)
            {
                pos.y = 0;
            }
            else
            {
                pos.x = 0;
            }
            playerRelativePos = pos;
            enterEvent();
        }

    }
}
