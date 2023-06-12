using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_LaserMove_State : EnemyFSMBaseState
{
     
    public Vector2 moveDir,movePos,faceDir ;
    public float maxDis,moveSpeed;
    public float laserLength=10;
    LineRenderer laser;
    Transform parent;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        parent = enemyFSM.transform.parent;
        laser = enemyFSM.GetComponent<LineRenderer>();
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        enemyFSM.transform.up = faceDir;
        enemyFSM.transform.localPosition = movePos;
        enemyFSM.rigidbody2d.velocity = moveDir * moveSpeed;
        
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
    }//
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        laser.SetPosition(0,Vector2.zero);
        laser.SetPosition(1, Vector2.up * laserLength);
    }
    public override void ExitState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        base.ExitState(fSM_Manager);
        laser.SetPosition(0, Vector2.zero);
        laser.SetPosition(1, Vector2.zero);
    }
}
