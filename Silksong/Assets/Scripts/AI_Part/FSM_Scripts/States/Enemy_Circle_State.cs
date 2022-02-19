using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Enemy_Circle_State :Enemy_Idle_State
{
    public float angleSpeed;
    public float radio;
    [Tooltip("0为绕自身当前位置，1为绕玩家")]
    public int centreOfCircle=0;
    public bool lock_x=false,lock_y=false;
    public bool isFaceToPlayer = true;
    private Vector2 centre =new Vector2(99999,99999);
    private float timer;
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        enemyFSM.getTargetDir(isFaceToPlayer);
        if (DOTween.IsTweening(enemyFSM.rigidbody2d))
            return;
        GetCentre(enemyFSM);
        var endPos = GetTargetPos();
        var pos = Vector2.Lerp(enemyFSM.transform.position, endPos, 0.3f);
        if (lock_x)
            enemyFSM.rigidbody2d.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        if (lock_y)
            enemyFSM.rigidbody2d.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        enemyFSM.rigidbody2d.MovePosition(pos);
    }
    
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        enemyFSM.rigidbody2d.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void GetCentre(EnemyFSMManager fsm)
    {
        if(centreOfCircle==0)
        {
            if(centre.Equals(new Vector2(99999,99999)))
            {
                centre = fsm.transform.position;
            }
               
        }else if(centreOfCircle==1)
        {
            centre = fsm.player.transform.position;
        }
    }
    private Vector2 GetTargetPos()
    {
        timer += Time.deltaTime;
        var angle = angleSpeed * timer;
        return (centre + radio * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
        
    }
}
