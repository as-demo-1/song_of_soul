using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Die_State :EnemyFSMBaseState
{
    public float delayTime;
    protected WaitTimeTrigger trigger;
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        trigger = new WaitTimeTrigger();
        trigger.maxTime = delayTime;
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        if (trigger.IsTriggerReachInUpdate(enemyFSM))
            GameObject.Destroy(enemyFSM.gameObject);
    }
}

public class LittleMonster_Die:Enemy_Die_State
{
    public GameObject bigMonster;
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        if (trigger.IsTriggerReachInUpdate(enemyFSM))
        {
            GameObject.Instantiate(bigMonster, enemyFSM.transform.position, bigMonster.transform.rotation, GameObject.Find("MonsterParent").transform);
            GameObject.Destroy(enemyFSM.gameObject);
        }
    }
}

public class BigMonster_Die : Enemy_Die_State
{
    public GameObject littleMonster;
    public float maxForce,minForce;
    public float maxAngle;
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        if (trigger.IsTriggerReachInUpdate(enemyFSM))
        {
            var tem= GameObject.Instantiate(littleMonster, enemyFSM.transform.position, littleMonster.transform.rotation, GameObject.Find("MonsterParent").transform);
            tem.GetComponent<Rigidbody2D>().AddForce(getRandomForce());
            tem = GameObject.Instantiate(littleMonster, enemyFSM.transform.position, littleMonster.transform.rotation, GameObject.Find("MonsterParent").transform);
            tem.GetComponent<Rigidbody2D>().AddForce(getRandomForce());
            GameObject.Destroy(enemyFSM.gameObject);
        }
    }

    public Vector2 getRandomForce()
    {
        float angle = Random.Range(90.0f - maxAngle, 90.0f + maxAngle);
        float force = Random.Range(minForce, maxForce);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized*force;
    }
}
