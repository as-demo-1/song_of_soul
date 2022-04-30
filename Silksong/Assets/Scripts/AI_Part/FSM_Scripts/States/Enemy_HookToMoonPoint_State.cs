using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 一开始打算自己写摆动逻辑的，但是写到后面发现总会偏离轨道
 * 如果强制让他在轨道上运动，又会慢慢的损失速度，摆两下就停了
 * 最后用铰链来做，但是铰链的问题在于有时候会瞬移，但是效果上还是很不错的毕竟是自带的
 * 如果月亮碎片最后设计的都很近的话瞬移就会很频繁
 * 那就别用铰链了，就改用代码，所以先注释代码，等场景做出来看看效果
 */
public class Enemy_HookToMoonPoint_State : EnemyFSMBaseState
{
    public float minDisToPoint;//摆动半径
    BreakMoonPoint targetPoint;//绕着转的那个月亮碎片
    float baseG;
    HingeJoint2D hingeJoint;
    //public float rotateSpeed;
    //float centripetalF;
    //float power;
    //float baseH;
    //Vector2 rotateVelocity;
    //Vector2 dir;
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        hingeJoint=enemyFSM.transform.GetComponent<HingeJoint2D>();
        base.InitState(enemyFSM);
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        hingeJoint.enabled=true;
        targetPoint = FindMoonPoinTrigger.moonPointDic[enemyFSM];
        hingeJoint.connectedAnchor = targetPoint.transform.position;
        baseG = enemyFSM.rigidbody2d.gravityScale;
        //baseH = enemyFSM.transform.position.y;
        //power =enemyFSM.rigidbody2d.velocity.sqrMagnitude/2;
        //enemyFSM.rigidbody2d.velocity = Vector2.zero;
        //Rotate(enemyFSM);
        targetPoint.atBreakMoonPoint();
        base.EnterState(enemyFSM);
    }
    //public void Rotate(EnemyFSMManager enemyFSM)
    //{
    //    enemyFSM.StartCoroutine(Rotating(enemyFSM));
    //    //enemyFSM.StartCoroutine(CircleRotating(enemyFSM));
    //}
    //public IEnumerator Rotating(EnemyFSMManager enemyFSM)
    //{       
    //    while (true)
    //    {
    //        dir = (targetPoint.transform.position - enemyFSM.transform.position).normalized;       
    //        rotateSpeed = enemyFSM.rigidbody2d.velocity.sqrMagnitude;
    //        centripetalF = enemyFSM.rigidbody2d.mass * rotateSpeed  / minDisToPoint;         
    //        Vector2 G = -Vector3.Project(Physics2D.gravity * baseG, dir);
    //        //float h = Mathf.Abs(targetPoint.transform.position.y - baseH);
    //        //float v = Mathf.Sqrt(Mathf.Abs(2 * Physics2D.gravity.y * baseG * h));
    //        //enemyFSM.rigidbody2d.velocity = enemyFSM.rigidbody2d.velocity.normalized * v;
    //        if (enemyFSM.transform.position.y < targetPoint.transform.position.y)
    //        {
    //            enemyFSM.rigidbody2d.AddForce(G+centripetalF * dir);
    //            Debug.DrawLine(enemyFSM.transform.position, (Vector2)enemyFSM.transform.position+ G + centripetalF * dir- baseG * Physics2D.gravity);
    //            //Debug.DrawLine(enemyFSM.transform.position, (Vector2)enemyFSM.transform.position + baseG * Physics2D.gravity);
    //            Debug.DrawLine(enemyFSM.transform.position, (Vector2)enemyFSM.transform.position + enemyFSM.rigidbody2d.velocity, Color.red);
    //            enemyFSM.transform.position = (Vector2)targetPoint.transform.position - dir * minDisToPoint;
    //        }
    //        yield return new WaitForFixedUpdate();
    //    }
    //}
    //IEnumerator CircleRotating(EnemyFSMManager enemyFSM)
    //{
    //    while (ifRotating)
    //    {
    //        dir = (targetPoint.transform.position - enemyFSM.transform.position).normalized;
    //        enemyFSM.rigidbody2d.AddForce(dir *centripetalF);
    //        enemyFSM.rigidbody2d.AddForce(-baseG * Physics2D.gravity);
    //        rotateVelocity.x = dir.y;
    //        rotateVelocity.y = -dir.x;
    //        enemyFSM.rigidbody2d.velocity = rotateVelocity.normalized * rotateSpeed;
    //        yield return new WaitForFixedUpdate();
    //    }
    //}

    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        //要跳到下一个月亮碎片前先加速
        float h = Mathf.Abs(targetPoint.transform.position.y - enemyFSM.transform.position.y);
        float v = Mathf.Sqrt(Mathf.Abs(2*Physics2D.gravity.y * baseG * h));
        enemyFSM.rigidbody2d.velocity = enemyFSM.rigidbody2d.velocity.normalized * v*1.4f;
        FindMoonPoinTrigger.preMoonPointDic[enemyFSM] = targetPoint;
        base.ExitState(enemyFSM);
    }
}
