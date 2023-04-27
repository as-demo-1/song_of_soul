using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 添加States时务必加上序号。
/// </summary>
public enum EnemyStates
{
    StateCombinationNode=99,
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
    Enemy_Smash_Down_State=12,
    Boss_Shoot_State=13,
    Enemy_Turn_State = 14,
    Enemy_Meet_State = 15,
    Enemy_Circle_State = 16,
    Enemy_Die_State=17,
    LittleMonster_Die=18,
    BigMonster_Die=19,

    Boss_ShootLoop_State = 20,
    Boss_ShootEnd_State = 21,
    Boss_Fireball_State = 22,
    Boss_FireballLoop_State = 23,
    Boss_FireballEnd_State = 24,
    Boss_Monsters_State = 25,
    Boss_MonstersLoop_State = 26,
    Boss_MonstersEnd_State = 27,
    Boss_Start_State = 28,
    Boss_Violin_State = 29,
    Boss_ViolinLoop_State = 30,
    Boss_ViolinEnd_State = 31,
    Boss_Piano_State = 32,
    Boss_PianoLoop_State = 33,
    Boss_PianoEnd_State = 34,
    Boss_Clarinet_State = 35,
    Boss_ClarinetLoop_State = 36,
    Boss_ClarinetEnd_State = 37,
    Boss_Drum_State = 38,
    Boss_DrumLoop_State = 39,
    Boss_DrumEnd_State = 40,
    Boss_Attack_State = 41,
    Boss_Second_State = 42,
    Boss_SecondPatrol_State = 43,
    Enemy_LightingChain_State=44,
    Enemy_Tailafter_State=45,
    Enemy_LaserMove_State =46,
    Enemy_ReturnBack_State=47,
    Enemy_PrickIdle_State=48,
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
    TargetTurnTrigger=7,
    RandomTrigger = 8,
    TouchLayerTrigger=9,
    InTurnTrigger = 10,
    ArrivedPos_Trigger=11,
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
