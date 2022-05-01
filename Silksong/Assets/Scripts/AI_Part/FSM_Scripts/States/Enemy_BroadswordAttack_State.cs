using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 大剑怪的攻击逻辑，在攻击结束的时候生成一个裂地效果
 * 裂地效果也可以绑定在攻击动画的某个阶段
 * 但是现在没有动画所以先这样吧
 */
public class Enemy_BroadswordAttack_State : Enemy_Attack_State
{
    public GameObject crack;
    protected RaycastHit2D hit;
    public LayerMask layerMask;

    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        fsmManager.rigidbody2d.velocity = Vector2.zero;
        hit = Physics2D.Raycast(enemyFSM.transform.position, -enemyFSM.transform.up, 10, layerMask);
        GameObject.Instantiate(crack).transform.position = hit.point;
    }
}
