using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Boss_Fireball_State : EnemyFSMBaseState
{
    public string shootType;
    private ShootSystem ShotPoint;
    private Vector2 dir;
    public override void InitState(EnemyFSMManager fSM_Manager)
    {
        base.InitState(fSM_Manager);
        fsmManager = fSM_Manager;
        ShotPoint = fSM_Manager.gameObject.transform.GetChild(1).gameObject.GetComponent<ShootSystem>();
        animationEvents.AddListener(()=> { ShotPoint.Shoot(shootType); });

    }

    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
        dir = enemyFSM.getTargetDir(true).normalized;
        enemyFSM.rigidbody2d.DORotate(Mathf.Asin(dir.y), 0.5f);
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        enemyFSM.rigidbody2d.DORotate(-Mathf.Asin(dir.y), 1f);
    }
    public override void invokeAnimationEvent()
    {
        for (int i = 0; i < ShotPoint.shootModes.Count; i++)
        {
            ShootSystem.shootParam p = ShotPoint.shootModes[i];
            p.target = fsmManager.player.gameObject;
            ShotPoint.shootModes[i] = p;
        }
        base.invokeAnimationEvent();
       // Debug.Log("");
    }

}
