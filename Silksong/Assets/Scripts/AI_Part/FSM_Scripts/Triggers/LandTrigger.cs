using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 着陆的Trigger
 * 没什么特别的，主要是想让动画播完后再转换状态
 * 但是没有复合Trigger，所以在这里面加了一个
*/
public class LandTrigger : EnemyFSMBaseTrigger
{
    public LayerMask layerMask;
    public bool fall;
    Collider2D collider;
    //float preSpeedY=1;
    //float times = 0;
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        collider = fsm_Manager.GetComponent<Collider2D>();
    }
    public override bool IsTriggerReachInFixUpdate(EnemyFSMManager enemyFSM)
    {
        float speedY = Vector3.Project(enemyFSM.rigidbody2d.velocity,enemyFSM.transform.up).sqrMagnitude;
        if (collider.IsTouchingLayers(layerMask) && speedY <= 0.1)
        {
            enemyFSM.rigidbody2d.velocity = Vector2.zero;
            if(enemyFSM.animator.GetCurrentAnimatorStateInfo(0).normalizedTime>0.99)
                return true;
        }
        return false;
    }
}
