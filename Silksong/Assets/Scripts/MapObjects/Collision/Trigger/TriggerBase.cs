using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 碰撞检测的抽象类 不一定正式需要
/// </summary>作者：青瓜
public abstract class TriggerBase : MonoBehaviour
{
    public LayerMask targetLayer;//触发该trigger的layer
    public bool canWork;

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (canWork && targetLayer.Contains(collision.gameObject) )
        {
            enterEvent();
            canWork = false;
        }
    }

    protected abstract void enterEvent();

}
