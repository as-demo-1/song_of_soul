using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
[Serializable]
public class Enemy_Idle_State : EnemyFSMBaseState
{
    public bool isflying=false;
    public float flyingMinHight;
    public override void Act_State(EnemyFSMManager fSM_Manager)
    {
        base.Act_State(fSM_Manager);
    }
    public override void FixAct_State(EnemyFSMManager enemyFSM)
    {
        base.FixAct_State(enemyFSM);
    }
    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        base.EnterState(fSM_Manager);
        fSM_Manager.getTargetDir(true);
        fSM_Manager.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        fsmManager = fSM_Manager;
        if (isflying)
        {
            fsmManager.rigidbody2d.gravityScale = 0;
            RaycastHit2D tem = Physics2D.Raycast(fSM_Manager.transform.position, Vector2.down,300,LayerMask.GetMask("Ground"));
            Debug.Log(tem.distance);
            if (tem.distance < flyingMinHight)
            {
                Vector3 t = fsmManager.transform.position + new Vector3(0, flyingMinHight - tem.distance, 0);
                DOTweenModulePhysics2D.DOMove(fSM_Manager.rigidbody2d, t, 1f);
            }
        }
    }
    public override void InitState(EnemyFSMManager fSM_Manager)
    {
        base.InitState(fSM_Manager);
    }
}
