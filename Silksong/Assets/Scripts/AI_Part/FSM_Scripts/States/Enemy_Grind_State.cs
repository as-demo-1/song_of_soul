using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Grind_State:EnemyFSMBaseState
{
    public float grindWidth;
    public int grindSpeed;
    public int grindCount;
    public AnimationCurve curve;

    private int forwardOrBack=1;
    private float leftSide;
    private int currentCount;

    public override void EnterState(EnemyFSMManager enemyFSM)
    {
        base.EnterState(enemyFSM);
        leftSide = enemyFSM.player.transform.position.x - grindWidth/2;
        currentCount = grindCount;

}
    public override void Act_State(EnemyFSMManager enemyFSM)
    {
        base.Act_State(enemyFSM);
        
    }
    public override void ExitState(EnemyFSMManager enemyFSM)
    {
        base.ExitState(enemyFSM);
    }
}
