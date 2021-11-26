using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Shoot_State : EnemyFSMBaseState
{
    public GameObject bullet;
    //public float range;
    public float shotCD;
   // public float bulletExistTime;子弹存在时间在bulletCollision中设置
    public float bulletSpeed;
    private float time = 0;
    private Transform shotPosition;//子弹发射的位置 发射子弹的小怪需要有这个子物体

    public override void Act_State(EnemyFSMManager fSM_Manager)
    {
        //Debug.Log("shoot "+fsmManager.gameObject.name);
        base.Act_State(fSM_Manager);
        time += Time.deltaTime;
        if (time >= shotCD)
        {
            Shot();
            time = 0;
        }
    }


    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        base.EnterState(fSM_Manager);

    }
    public override void ExitState(EnemyFSMManager fSM_Manager)
    {
        base.ExitState(fSM_Manager);

    }


    public override void InitState(EnemyFSMManager fSM_Manager)
    {
        base.InitState(fSM_Manager);
        fsmManager = fSM_Manager ;
        stateType = EnemyStates.Enemy_Shoot_State;
        shotPosition = fsmManager.transform.Find("shotPosition");
    }

    private void Shot()
    {
       // Debug.Log("发射");
        Vector3 move = (fsmManager as EnemyFSMManager).getTargetDir(true).normalized;

        GameObject shot = UnityEngine.Object.Instantiate(bullet);
        shot.transform.position = shotPosition.position;
        shot.GetComponent<Rigidbody2D>().velocity = move * bulletSpeed;
    }

}
