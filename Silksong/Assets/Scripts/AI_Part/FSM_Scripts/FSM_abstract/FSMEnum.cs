using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 添加States时务必加上序号。
/// </summary>
public enum EnemyStates
{
    EnemySubFSMManager = 10,
    Enemy_Any_State =0,
    Enemy_Idle_State=1,
    Enemy_Patrol_State=2,
    Enemy_Hitted_State=3,
    Enemy_Attack_State=4,
    Enemy_Climb_State=5,
    Enemy_Pursuit_State=6,
    Enemy_Wander_State=7,
    Enemy_Shoot_State=8,
    Enemy_Bump_State=9,
    Enemy_Chase_State=11,
}
/// <summary>
/// 添加Trigger时务必加上序号。
/// </summary>
public enum EnemyTriggers
{
    WaitTimeTrigger=0,
    HitWallTrigger=1,
    PlayerDistanceTrigger=2,
    AnimationPlayOverTrigger=3,
    OnHittedTrigger=4,
    SelfHPTrigger=5,
    NearPlatformBorderTrigger=6,

}




public enum NPCStates
{
    NPC_Idle_State,
    NPC_Run_State
}

public enum NPCTriggers
{
    WaitTimeTrigger,
    HitWallTrigger,
    PlayerDistanceTrigger,
}

public enum PlayerStates
{
    Player_Idle_State,
    Player_Run_State
}

public enum PlayerTriggers
{
    W_Key_Down,
    A_Key_Down,
    S_Key_Down,
    D_Key_Down
}
