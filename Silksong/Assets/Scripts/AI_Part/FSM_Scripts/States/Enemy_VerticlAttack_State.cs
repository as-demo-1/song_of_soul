using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_VerticlAttack_State : EnemyFSMBaseState
{
    public Vector2 boxSize;
    public LayerMask layer;
    public float moveSpeed=4;
    Vector2 CheckStartPos;
    Vector2 TargetPlatform;
    RaycastHit2D hit;
    float minY;
    float maxY;
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        BoxCheck(enemyFSM);
        enemyFSM.rigidbody2d.velocity=(TargetPlatform-(Vector2)enemyFSM.transform.position).normalized*moveSpeed;
        base.EnterState(enemyFSM);
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
    }
    public void BoxCheck(EnemyFSMManager fsm_Manager)
    {
        Vector2 upRay = CheckStartPos;
        Vector2 downRay = CheckStartPos;
        Vector2 dir = fsm_Manager.transform.up;
        //float dy = fsm_Manager.transform.localScale.x / Mathf.Abs(fsm_Manager.transform.localScale.x);
        while (upRay.y < maxY || downRay.y > minY)
        {
            if (upRay.y < maxY)
            {
                hit = Physics2D.Raycast(upRay, dir, boxSize.x, layer);
                if (hit.collider != null)
                {
                    TargetPlatform.y = hit.point.y;
                    TargetPlatform.x = fsm_Manager.transform.position.x;
                    return;
                }
                else
                {
                    downRay.y += 0.1f;
                }
            }
            if (downRay.y < minY)
            {
                hit = Physics2D.Raycast(downRay, dir, boxSize.x, layer);
                if (hit.collider != null)
                {
                    TargetPlatform.y = hit.point.y;
                    TargetPlatform.x = fsm_Manager.transform.position.x;
                    return;
                }
                else
                {
                    downRay.y -= 0.1f;
                }
            }
        }
    }

}
