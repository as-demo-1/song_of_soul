using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 一个碰撞检测的抽象类 
/// </summary>作者：青瓜
public abstract class Trigger2DBase : MonoBehaviour
{
    public LayerMask targetLayer;//触发该trigger的layer
    [HideInInspector]
    public bool canWork = true;
    [HideInInspector]
    public bool isOneTime;

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (canWork && targetLayer.Contains(collision.gameObject) )
        {
            enterEvent();
        }
    }

    protected virtual void enterEvent()
    {

    }

    protected virtual void OnTriggerExit2D(Collider2D collision)
    {

        if (canWork && targetLayer.Contains(collision.gameObject))
        {
            exitEvent();
            if(isOneTime)
            canWork = false;//
        }
    }

    protected virtual void exitEvent()
    {

    }

}
