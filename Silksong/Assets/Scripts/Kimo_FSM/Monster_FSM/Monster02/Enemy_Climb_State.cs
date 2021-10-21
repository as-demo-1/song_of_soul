using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Climb_State : EnemyFSMBaseState
{
   // public Vector2 range;
    public float maxSpeed = 2;
    public float force = 10;
    ContactPoint2D[] points = new ContactPoint2D[3];
    //float g; 怪物初始重力为0 仅死亡时获得重力
    public override void EnterState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
       // g = fsmManager.rigidbody2d.gravityScale;
       // fsmManager.rigidbody2d.gravityScale = 0;
    }

    public override void ExitState(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        // fsmManager.rigidbody2d.gravityScale = g;
        fsmManager.rigidbody2d.velocity = Vector2.zero;
    }
    public override void InitState(FSMManager<EnemyStates, EnemyTriggers> fSMManager)
    {
        base.InitState(fSMManager);
        fsmManager = fSMManager;
        stateID = EnemyStates.Enemy_Climb_State;
    }
    public override void Act_State(FSMManager<EnemyStates, EnemyTriggers> fSM_Manager)
    {
        base.Act_State(fSM_Manager);
        Force(fsmManager);
    }
    
    public  void Force(FSMManager<EnemyStates, EnemyTriggers> fsmManager)
    {
        Vector2 steeringForce = Vector2.zero;
        int n = fsmManager.GetComponent<Collider2D>().GetContacts(points);
        if (n > 0)
        {
            //Debug.Log(n);
            Vector2 toward = ((Vector2)fsmManager.transform.position - points[n - 1].point).normalized;
            //Debug.Log(toward.ToString("F4"));
            steeringForce += -toward * force;
          //  fsmManager.transform.right = toward;
            fsmManager.rigidbody2d.velocity = new Vector2(toward.y, -toward.x) * maxSpeed;
            fsmManager.transform.right = fsmManager.rigidbody2d.velocity;
        }
        else
        {
          // Debug.Log("no collision");
        }
       // Debug.Log(steeringForce.ToString("F4"));
       fsmManager.rigidbody2d.AddForce(steeringForce);
    }
}
