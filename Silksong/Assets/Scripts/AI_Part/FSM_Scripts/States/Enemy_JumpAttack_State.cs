using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * ´óµ¶¹ÖÌøÔ¾¹¥»÷×´Ì¬
 */
public class Enemy_JumpAttack_State: Enemy_BroadswordAttack_State
{
    public float verticalSpeed=3;
    public float verticalDistance=5;  
    float horizontalSpeed;
    float xDis;
    Vector2 speed;
    public override void EnterState(EnemyFSMManager fSM_Manager)
    {
        base.EnterState(fSM_Manager);
        //CheckHeight(fSM_Manager);
        float t = fSM_Manager.getTargetDir().x / verticalSpeed;
        horizontalSpeed = Mathf.Abs(fSM_Manager.rigidbody2d.gravityScale * Physics2D.gravity.y * t / 2);
        if (!fSM_Manager.currentFacingLeft())
            speed = verticalSpeed * Vector2.right + horizontalSpeed * Vector2.up;
        else speed = verticalSpeed * Vector2.left + horizontalSpeed * Vector2.up;
        fsmManager.rigidbody2d.velocity = speed;
        speed.y = 0;
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        speed.y = enemyFSM.rigidbody2d.velocity.y;
        fsmManager.rigidbody2d.velocity = speed;
    }
    public override void InitState(EnemyFSMManager fSMManager)
    {
        base.InitState(fSMManager);
        fsmManager = fSMManager;
          
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        fsmManager.rigidbody2d.velocity=Vector2.zero;
        hit = Physics2D.Raycast(enemyFSM.transform.position, -enemyFSM.transform.up, 10, layerMask);
        GameObject.Instantiate(crack).transform.position=hit.point;
    }
    //public void CheckHeight(EnemyFSMManager fSM_Manager)
    //{
    //    float G = fSM_Manager.rigidbody2d.gravityScale * Physics2D.gravity.y;
    //    xDis = fSM_Manager.getTargetDir().x;
    //    float t = xDis / (verticalSpeed*2);
    //    float maxHeight =(G )*t*t/2;
    //    Vector2 boxSize=fSM_Manager.GetComponent<Collider2D>().bounds.extents;
    //    Vector2 position = fSM_Manager.transform.position;
    //    position += fSM_Manager.GetComponent<Collider2D>().offset;
    //    position.y+=boxSize.y;
    //    RaycastHit2D hit;
    //    for (int i = 0; i < 10; i++)
    //    {
    //        Vector2 startPos=position*i*xDis/10;
    //        hit= Physics2D.Raycast(startPos,fsmManager.transform.up,maxHeight,layerMask);
    //        Debug.DrawLine(startPos, startPos + (Vector2)fsmManager.transform.up * maxHeight);
    //        if (hit.collider != null)
    //        {
    //            maxHeight = hit.point.y;
    //            t = Mathf.Sqrt(2 * G * maxHeight);
    //            xDis = verticalSpeed * t * 2;
    //        }
    //    }
    //}
}
