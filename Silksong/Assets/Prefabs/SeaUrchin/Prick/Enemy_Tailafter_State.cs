using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Tailafter_State : EnemyFSMBaseState
{
    [LabelText("初始速度")]
    public float moveSpeed=1;
    [LabelText("旋转速度")]
    public float rotateSpeed=1;
    [LabelText("加速度")]
    public float accelerated=1;
    Rigidbody2D rb;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        rb=enemyFSM.GetComponent<Rigidbody2D>();
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        if(enemyFSM.player==null)
            enemyFSM.player = GameObject.FindGameObjectWithTag("Player");
        rb.velocity = enemyFSM.transform.up * moveSpeed;
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        rb.velocity = Vector2.zero;
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        float currentSpeed=rb.velocity.magnitude;
        currentSpeed += accelerated * Time.fixedDeltaTime;
        Vector2 dir = enemyFSM.getTargetDir();
        if (Vector2.Angle(dir, enemyFSM.transform.up) > 1f)
        {
            enemyFSM.transform.up=Vector2.MoveTowards(enemyFSM.transform.up,dir, rotateSpeed*Time.fixedDeltaTime).normalized;
        }//
        rb.velocity = enemyFSM.transform.up * currentSpeed;
    }
}
