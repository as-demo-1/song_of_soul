using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Boss_ViolinLoop_State : EnemyFSMBaseState
{
    public string shootType;
    private ShootSystem[] ShotPoint;
    private Vector2 dir;
    public override void InitState(EnemyFSMManager fSM_Manager)
    {
        base.InitState(fSM_Manager);
        fsmManager = fSM_Manager;
        
        GameObject violin = fsmManager.gameObject.transform.GetChild(1).gameObject;
        ShotPoint = new ShootSystem[violin.transform.childCount - 1];
        for (int i = 0; i < violin.transform.childCount - 1; ++i)
        {
            ShotPoint[i] = violin.transform.GetChild(i).gameObject.GetComponent<ShootSystem>();
            //animationEvents.AddListener(() => { ShotPoint[i].Shoot(shootType); });
        }
        
        /*
        ShotPoint = fSM_Manager.gameObject.transform.GetChild(1).GetChild(0).gameObject.GetComponent<ShootSystem>();
        animationEvents.AddListener(() => { ShotPoint.Shoot(shootType); });
        */

    }

    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
        dir = enemyFSM.getTargetDir(true).normalized;
        enemyFSM.rigidbody2d.DORotate(Mathf.Asin(dir.y), 0.5f);
        invokeAnimationEvent();
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        enemyFSM.rigidbody2d.DORotate(-Mathf.Asin(dir.y), 1f);
    }
    public override void invokeAnimationEvent()
    {
        
        for (int i = 0; i < ShotPoint.Length; i++)
        {
            for (int j = 0; j < ShotPoint[i].shootModes.Count; ++j)
            {
                ShootSystem.shootParam p = ShotPoint[i].shootModes[j];
                p.target = fsmManager.player.gameObject;
                ShotPoint[i].shootModes[j] = p;
                ShotPoint[i].Shoot(shootType);
            }
        }
        base.invokeAnimationEvent();
        
        /*
        for (int i = 0; i < ShotPoint.shootModes.Count; i++)
        {
            ShootSystem.shootParam p = ShotPoint.shootModes[i];
            p.target = fsmManager.player.gameObject;
            ShotPoint.shootModes[i] = p;
        }
        base.invokeAnimationEvent();
        */
    }

}
