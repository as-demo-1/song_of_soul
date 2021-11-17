using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Pursuit_State : EnemyFSMBaseState
{
    public float avoidForce = 2;//推力的大小
    public float MAX_SEE_AHEAD = 2.0f;//视野范围
    public float maxSpeed = 2;
    public float maxForce = 2;
    //private GameObject target;
    public override void InitState(EnemyFSMManager fSMManager)
    {
        base.InitState(fSMManager);
        fsmManager = fSMManager;
        stateType = EnemyStates.Enemy_Pursuit_State;
    }
    public override void Act_State(EnemyFSMManager fSM_Manager)
    {
        base.Act_State(fSM_Manager);
        fsmManager.rigidbody2d.AddForce(Project4(fsmManager));
        Force(fsmManager);
    }
    public void Force(EnemyFSMManager fsmManager)
    {

        Vector2 desiredVelocity = (fsmManager as EnemyFSMManager).getTargetDir(true).normalized * maxSpeed;

        Vector2 steeringForce = (desiredVelocity - fsmManager.rigidbody2d.velocity);
        if (steeringForce.magnitude > maxForce) steeringForce = steeringForce.normalized * maxForce;
      //  Debug.DrawLine(fsmManager.transform.position, (Vector2)fsmManager.transform.position + steeringForce, Color.green);
        fsmManager.rigidbody2d.AddForce(steeringForce);
    }
    public Vector2 Project4(EnemyFSMManager fsmManager)
    {
        Vector2 steeringForce = Vector2.zero;
        Vector2 toward = fsmManager.rigidbody2d.velocity.normalized;
        Vector2 vetical = new Vector2(-toward.y, toward.x);
        Vector2 pos = fsmManager.transform.position;
        Vector2 pointA = pos - (toward + vetical) * fsmManager.GetComponent<Collider2D>().bounds.extents.magnitude;
        Vector2 pointB = pos + toward * MAX_SEE_AHEAD + vetical * fsmManager.GetComponent<Collider2D>().bounds.extents.magnitude;
        Collider2D wall = Physics2D.OverlapArea(pointA, pointB, 1<<LayerMask.NameToLayer("Ground"));
       // Debug.DrawLine(pointA, pointB, Color.red);
        if (wall)
        {
            Vector2 ahead = pos + fsmManager.rigidbody2d.velocity.normalized * MAX_SEE_AHEAD;
            steeringForce = (ahead - (Vector2)wall.transform.position).normalized;
            steeringForce *= avoidForce;
        }
       // Debug.DrawLine(pos, pos + steeringForce);
        return steeringForce;
    }
}
