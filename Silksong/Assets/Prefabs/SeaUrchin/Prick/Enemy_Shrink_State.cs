using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Shrink_State : EnemyFSMBaseState
{
    public float minDis;
    public float shrinkSpeed;
    public float rotateSpeed;
    public float maxSpeed;
    public float accelerate;
    Transform parent;
    bool ifRotate;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        parent = enemyFSM.transform.parent;
        
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        enemyFSM.StartCoroutine(Shrink(enemyFSM));
        ifRotate = false;
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
        if (ifRotate)
        {
            enemyFSM.transform.RotateAround(parent.position, Vector3.forward, rotateSpeed * Time.fixedDeltaTime);
        }
    }
    IEnumerator Shrink(EnemyFSMManager enemyFSM)
    {
        enemyFSM.rigidbody2d.velocity = (parent.position - enemyFSM.transform.position).normalized * shrinkSpeed;
        while ( enemyFSM.transform.localPosition.magnitude > minDis )
        {
            yield return null;
        }
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
        ifRotate = true;
        while (rotateSpeed < maxSpeed)
        {
            rotateSpeed += accelerate * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    { 
        base.ExitState(enemyFSM);
    } 
}
