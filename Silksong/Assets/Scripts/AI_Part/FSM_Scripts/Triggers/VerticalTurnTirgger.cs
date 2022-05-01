using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 用于竖直移动的怪物的边缘/撞墙检测
 */
public class VerticalTurnTirgger : EnemyFSMBaseTrigger
{
    public LayerMask layer;
    public Vector2 face;
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
    }
    public override bool IsTriggerReachInFixUpdate(EnemyFSMManager enemyFSM)
    {
        return Turn(enemyFSM);
    }
    public bool Turn(EnemyFSMManager fsm_Manager)
    {
        Vector2 pos = fsm_Manager.transform.position;
        Vector2 up = fsm_Manager.transform.up;
        face = fsm_Manager.rigidbody2d.velocity.normalized;
        RaycastHit2D ray = Physics2D.Raycast(pos+face, -up, 10f, layer);
        Debug.DrawLine(pos + face, (pos + face - up*10), Color.red);
        RaycastHit2D ray2 = Physics2D.Raycast(pos,face, 1.5f, layer);
        Debug.DrawLine(pos, pos+face*1.5f, Color.blue);
        if (!ray.collider || ray2.collider)
        {
            return true;
        }
        return false;
    }
}
