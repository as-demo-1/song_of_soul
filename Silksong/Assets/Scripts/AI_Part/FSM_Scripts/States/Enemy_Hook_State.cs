using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * ³ö¹³µÄ×´Ì¬
 */
public class Enemy_Hook_State : EnemyFSMBaseState
{
    Hook hook;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        hook=enemyFSM.transform.GetChild(0).GetComponent<Hook>();
        base.InitState(enemyFSM);
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
        base.Act_State(enemyFSM);
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        hook.gameObject.SetActive(true);
        Debug.Log("shoot");
        hook.Shoot(enemyFSM.transform.position,enemyFSM.getTargetDir(true));
    }
}
