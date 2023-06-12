using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaUrchin_Shoot_State : EnemyFSMBaseState
{
    SeaUrchin seaUrchin;
    public float interval; 
    public string prickState,prickBackState;
    public string nextState;
    public int lockPointNum;
    public LayerMask wallLayer;
    public float waitTime;

    float angleInterval;
    List<Vector2> lockPoints_Up, lockPoints_Down;
    public Vector2[] dirs = new Vector2[]
    {
        new Vector2(1,1),
        new Vector2(1,-1),
        new Vector2(-1,-1),
        new Vector2(-1,1),
    };
    int index;
    float t;
    bool startShoot;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        seaUrchin = enemyFSM as SeaUrchin;
        lockPoints_Up = new List<Vector2>();
        lockPoints_Down = new List<Vector2>();
        angleInterval = 360 / lockPointNum;
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        index = 0;
        t = 0;
        if (!seaUrchin.ifInWater)
        {
            startShoot = true;
        }
        else
        {
            enemyFSM.StartCoroutine(LockScene(enemyFSM));
        }
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        if (startShoot)
        {
            if (index < seaUrchin.pricks.Count)
            {
                t += Time.deltaTime;
                if (t > interval)
                {
                    seaUrchin.pricks[index].ChangeState(prickState);
                    t = 0;
                    index++;
                }
            }
            else if (index == seaUrchin.pricks.Count)
            {
                t += Time.deltaTime;
                if (t > 5)
                {
                    enemyFSM.ChangeState(nextState);
                    index++;
                }
            }
        }
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        foreach(var prick in seaUrchin.pricks)
        {
            prick.ChangeState(prickBackState);
        }
        startShoot=false;

        if (seaUrchin.ifInWater)
        {
            for (int i=0;i< seaUrchin.chains.Count;i++)
            {
                int n = i;
                seaUrchin.chains[i].Leave(()=>
                {
                    //Debug.Log(n);
                    seaUrchin.hooks[n].TakeBack_Rope();
                }
                );
            }
        }
    }

    IEnumerator LockScene(EnemyFSMManager enemyFSM)
    {
        for(int i = 0; i < 4; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(enemyFSM.transform.position, dirs[i], 100, wallLayer);
            seaUrchin.hooks[i].Shoot(hit.point);
        }
        while (seaUrchin.hooks[0].grappleRope.waveSize > 0)
        {
            yield return null;
        }
        LockingScene(enemyFSM);
        yield return new WaitForSeconds(waitTime);
        startShoot = true;
    }
    public void LockingScene(EnemyFSMManager enemyFSM)
    {
        lockPoints_Up.Clear(); lockPoints_Down.Clear();
        RaycastHit2D hit2D;
        for (int i = 0; i < lockPointNum; i++)
        {
            float angle = Random.Range(i * angleInterval, (i + 1) * angleInterval);
            //Debug.Log(angle);
            hit2D = Physics2D.Raycast(enemyFSM.transform.position, Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.up, 100, wallLayer);
            if (i < lockPointNum / 2)
                lockPoints_Up.Add(hit2D.point);
            else
                lockPoints_Down.Add(hit2D.point);
        }
        for (int i = 0; i < lockPoints_Up.Count; i++)
        {
            int j = Random.Range(0, lockPoints_Down.Count);

            seaUrchin.chains[i].gameObject.SetActive(true);
            seaUrchin.chains[i].LockDown(lockPoints_Up[i], lockPoints_Down[i], 5);
        }
    }
}
