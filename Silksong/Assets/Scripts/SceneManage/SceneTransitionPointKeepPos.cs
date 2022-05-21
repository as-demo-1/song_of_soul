using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionPointKeepPos : SceneTransitionPoint
{
    private Vector3 playerRelativePos;
    private bool recordXPosWhenTrue;
    public Vector3 PlayerRelativePos
    {
        get { return playerRelativePos; }
    }
    private void Start()
    {
        Vector2 collider = GetComponent<BoxCollider2D>().size;
        recordXPosWhenTrue = collider.x > collider.y ? true:false;
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
