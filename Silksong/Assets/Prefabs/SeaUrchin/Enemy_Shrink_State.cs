using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Shrink_State : EnemyFSMBaseState
{
    public float minDis;
    public float shrinkSpeed;
    public float rotateSpeed;
    Transform parent;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        parent = enemyFSM.transform.parent;
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        enemyFSM.rigidbody2d.velocity = (parent.position - enemyFSM.transform.position).normalized*shrinkSpeed;
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        if (enemyFSM.transform.localPosition.magnitude <= minDis)
        {
            enemyFSM.rigidbody2d.velocity = Vector2.zero;
            enemyFSM.transform.RotateAround(parent.position, Vector3.forward, rotateSpeed * Time.fixedDeltaTime);
        }
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    { 
        base.ExitState(enemyFSM);
    } 
}
