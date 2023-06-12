using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Limit : EnemyFSMBaseState
{
    //EnemyStates
    SeaUrchin urchin;
    public int lockPointNum;
    public LayerMask wallLayer;
    float angleInterval;
    List<Vector2> lockPoints_Up,lockPoints_Down;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        lockPoints_Up= new List<Vector2>();
        lockPoints_Down = new List<Vector2>();
        angleInterval = 360 / lockPointNum;
        urchin = enemyFSM.GetComponent<SeaUrchin>();
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        LockingScene(enemyFSM);
    }
    public void LockingScene(EnemyFSMManager enemyFSM)
    {
        lockPoints_Up.Clear(); lockPoints_Down.Clear();
        RaycastHit2D hit2D;
        for(int i = 0; i < lockPointNum; i++)
        {
            float angle = Random.Range(i * angleInterval, (i + 1) * angleInterval);
            Debug.Log(angle);
            hit2D = Physics2D.Raycast(enemyFSM.transform.position,Quaternion.AngleAxis(angle,Vector3.forward)*Vector2.up,100,wallLayer);
            if(i<lockPointNum/2)
                lockPoints_Up.Add(hit2D.point);
            else
                lockPoints_Down.Add(hit2D.point);
        }
        for (int i = 0; i < lockPoints_Up.Count; i++)
        {
            int j = Random.Range(0, lockPoints_Down.Count);
            
            urchin.chains[i].gameObject.SetActive(true);
            urchin.chains[i].LockDown(lockPoints_Up[i], lockPoints_Down[i],5);
        }
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
         
    }
} 