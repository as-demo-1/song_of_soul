using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 视线trigger，主要是因为如果用distanceToPlayer那个trigger
 * 由于判断跳跃距离的方法是怪物的x-玩家的x
 * 这样的话如果玩家在怪物的头顶且在distanceToPlayer内
 * 怪物就会在下面一直跳，看过去就很蠢
 * 所以写了这个trigger先用着
 */
public class SightCheckTrigger : EnemyFSMBaseTrigger
{
    public float sightLength = 10;
    public float startLength = 0;
    public bool targetInRange=true;//为true是该trigger检测目标是否在范围内，为false时检测目标是否不在范围内
    public LayerMask layer;
    RaycastHit2D hit;
    Vector2 face;
    Vector2 pos;
    Vector2 box;
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        box=fsm_Manager.GetComponent<Collider2D>().bounds.size;
    }

    public override bool IsTriggerReachInFixUpdate(EnemyFSMManager fsm_Manager)
    {
        face = fsm_Manager.transform.right * fsm_Manager.transform.localScale.x;
        pos = fsm_Manager.GetComponent<Collider2D>().bounds.center;
        pos.y-=box.y/2;
        for (int i = 0; i < 10; i++)
        {
            hit = Physics2D.Raycast(pos + face.normalized * startLength, face.normalized, sightLength - startLength, layer);
            pos.y += box.y / 10;
            Debug.DrawLine(pos, pos + face.normalized * sightLength);
            if (hit.collider != null) return targetInRange;
        }        
        return !targetInRange;
    }
}
