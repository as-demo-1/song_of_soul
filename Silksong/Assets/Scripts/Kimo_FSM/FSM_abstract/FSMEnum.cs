using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EnemyStates
{
    Enemy_Any_State,
    Enemy_Idle_State,
    Enemy_Patrol_State,
    Enemy_Hitted_State
}

public enum EnemyTriggers
{
    WaitTimeTrigger,
    HitWallTrigger,
    InDetectionAreaTrigger,
    AnimationPlayOverTrigger,
    OnHittedTrigger
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
    Player_Run_State
}

public enum PlayerTriggers
{
    W_Key_Down,
    A_Key_Down,
    S_Key_Down,
    D_Key_Down
}
