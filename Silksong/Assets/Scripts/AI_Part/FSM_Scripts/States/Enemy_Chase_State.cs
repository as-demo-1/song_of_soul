using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Chase_State :EnemyFSMBaseState
{
    public float chaseSpeed;
    public bool isFaceWithSpeed;
    public bool isFlying = false;
    private Vector3 v;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        if (isFlying)
        {
            enemyFSM.rigidbody2d.gravityScale = 0;
        }
    }
    public override void Act_State(EnemyFSMManager fSM_Manager)
    {
        v = fSM_Manager.getTargetDir(isFaceWithSpeed);
        v=v.normalized;
        if (!isFlying)
        {
            if (v.x > 0)
                v = new Vector3(1, 0, 0);
            else
                v = new Vector3(-1, 0, 0);
        }
        fSM_Manager.transform.Translate(v * chaseSpeed * Time.deltaTime);
       /* if (isFaceWithSpeed)
            fSM_Manager.faceWithSpeed();*/
    }

    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
    }

}
