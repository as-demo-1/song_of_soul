using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 碰撞检测的抽象类 
/// </summary>作者：青瓜
public abstract class Collider2DBase : MonoBehaviour
{
    public LayerMask targetLayer;//触发该collider的layer
    public bool canWork;
    public bool isOneTime;//是否只触发一次

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (canWork && targetLayer.Contains(collision.gameObject) )
        {
            enterEvent();
        }
    }

    protected virtual void enterEvent()
    {

    }

    protected virtual void OnCollisionExit2D(Collision2D collision)
    {
        if (canWork && targetLayer.Contains(collision.gameObject))
        {
            exitEvent();
            if(isOneTime)
            canWork = false;
        }
    }

    protected virtual void exitEvent()
    {

    }

}

