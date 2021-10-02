using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EnemyStates
{
    Enemy_Any_State,
    Enemy_Idle_State,
    Enemy_Patrol_State,
    Enemy_Hitted_State,
    Enemy_Test_State,
}

public enum EnemyTriggers
{
    WaitTimeTrigger,
    HitWallTrigger,
    InDetectionAreaTrigger,
    AnimationPlayOverTrigger,
    OnHittedTrigger,
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
    InDetectionAreaTrigger
}

public enum PlayerStates
{
    Player_Idle_State,
    Player_WalkStart_State,
    Player_WalkingLoop_State,
    Player_WalkStop_State,
}

public enum PlayerTriggers
{
    HorizontalInputTrigger,
    HorizontalInputAndHaveExitTimeTrigger,
    NonHorizontalInputTrigger,
    MaxExitTimeTrigger,
}
