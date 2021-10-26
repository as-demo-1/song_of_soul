using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EnemyStates
{
    Enemy_Any_State,
    Enemy_Idle_State,
    Enemy_Patrol_State,
    Enemy_Hitted_State,
    Enemy_Attack_State,
    Enemy_Climb_State,
    Enemy_Pursuit_State,
    Enemy_Wander_State,
    Enemy_Shoot_State,
    Enemy_Bump_State,

}

public enum EnemyTriggers
{
    WaitTimeTrigger,
    HitWallTrigger,
    PlayerDistanceTrigger,
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
    PlayerDistanceTrigger,
}

public enum PlayerStates
{
    Player_Idle_State,
    Player_WalkStart_State,
    Player_WalkingLoop_State,
    Player_WalkStop_State,
    Player_TurnAround_State,
    Player_JumpUp_State,
    Player_JumpAirLoop_State,
    Player_Land_State,
}

public enum PlayerTriggers
{
    HorizontalInputTrigger,
    HorizontalInputAndHaveExitTimeTrigger,
    NonHorizontalInputTrigger,
    MaxExitTimeTrigger,
    TurnAroundTrigger,
    AirSpeedNegativeTrigger,
    AirSpeedPositiveTrigger,
    LandTrigger,
}
