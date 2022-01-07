using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 为了魂灵墙重写的Trigger2D基类
/// </summary>作者：煎饼狗子
public class Trigger2DSoulWall : Trigger2DBase
{
    protected Collider2D coll;
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        coll = collision;
        if (canWork && targetLayer.Contains(collision.gameObject))
        {
            enterEvent();
        }
    }
    protected override void enterEvent()
    {
        bool state = coll.gameObject.GetComponent<PlayerState>().isSoulJump;
        Rigidbody2D rb = coll.gameObject.GetComponent<Rigidbody2D>();
        if(!state)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
        else
        {
            rb.velocity += new Vector2(0f, 10f);
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        if (canWork && targetLayer.Contains(collision.gameObject))
        {
            exitEvent();
            if (isOneTime)
                canWork = false;//
        }
    }

    protected override void exitEvent()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
    }
}
