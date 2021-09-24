using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Chase_State :EnemyFSMBaseState
{
    public GameObject target;
    public float chaseSpeed;

    private Vector3 v;

    public override void Act_State(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        v = target.transform.position - fSM_Manager.transform.position;
        if (v.x > 0)
        {
            v = new Vector3(1, 0, 0);
            fSM_Manager.transform.localScale = new Vector3(1, 1, 1);
        }
        else 
        {
            v = new Vector3(-1, 0, 0);
            fSM_Manager.transform.localScale = new Vector3(-1, 1, 1);
        }
        fSM_Manager.GetComponent<Rigidbody2D>().velocity = v * chaseSpeed;

            
    }
}
