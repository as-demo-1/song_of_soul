using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 竖直跳跃怪的跳跃状态
 * 有两种情况，一种是移动时随机跳跃
 * 第二种是攻击时会反复横跳，并射出子弹。
 */
public class Enemy_VerticalJump_State : EnemyFSMBaseState
{
    public float jumpSpeed;//跳跃的速率
    public string shootType="normal";
    public bool ifAttack;//是否是攻击跳跃状态
    private ShootSystem ShotPoint;
    //public GameObject bullet;//攻击跳跃时射出的子弹
    public LayerMask layer;
    Vector2 velocity;//计算后得到的跳跃速度
    Vector2 jumpDirect;//跳跃方向
    Vector2 startPos;//起跳的位置,其实就是当前位置
    float baseDirect;//一开始到目标跳板的距离，当超过该值的一半时怪物上下翻转
    bool ifFlip;//已经翻转过就不用再翻转了
    public override void InitState(EnemyFSMManager enemyFSM)
    {
        base.InitState(enemyFSM);
        ShotPoint = enemyFSM.gameObject.transform.GetChild(1).gameObject.GetComponent<ShootSystem>();
    }
    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        jumpDirect = CheckPlatformTrigger.jumpDirectDic[enemyFSM];
        ifFlip = false;
        startPos=enemyFSM.transform.position;
        baseDirect = jumpDirect.sqrMagnitude;
        velocity = jumpDirect.normalized * jumpSpeed;
        enemyFSM.rigidbody2d.velocity = velocity;
        base.EnterState(enemyFSM);
    }
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        if (!ifFlip)
        {
            float currentDir = ((Vector2)enemyFSM.transform.position - startPos).sqrMagnitude;
            if (currentDir >= baseDirect / 3)//跳过三分之一位置时翻转，如果是攻击跳跃则射出子弹
            {
                enemyFSM.transform.up = -enemyFSM.transform.up;
                ifFlip =true;
                if(ifAttack)ShotPoint.Shoot(shootType);
                //enemyFSM.transform.localScale = scale;
            }
        }
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
        enemyFSM.rigidbody2d.velocity = Vector2.zero;
    }
}
