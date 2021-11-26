using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Chase_State :EnemyFSMBaseState
{
    public float chaseSpeed;
    public bool isFaceWithSpeed;
    private Vector3 v;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
    }
    public override void Act_State(EnemyFSMManager fSM_Manager)
    {
        v = fSM_Manager.getTargetDir();
        Debug.Log(v.x);
        if (v.x > 0)
            v = new Vector3(1, 0, 0);
        else
            v = new Vector3(-1, 0, 0);
        // fSM_Manager.transform.Translate(v * chaseSpeed * Time.deltaTime);
        fSM_Manager.rigidbody2d.velocity = v*chaseSpeed;

        if (isFaceWithSpeed)
            fSM_Manager.faceWithSpeed();
    }

    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
    }

}
