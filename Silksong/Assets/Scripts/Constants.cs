using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    // store all the Constants and mark the usage 
    public const float GroundAccelerationFactor = 1.0f;
    public const float GroundDeccelerationFactor = 1.0f;
    public const float GroundAccelerationTimeReduceFactor = 1.0f;
    public const float GroundDeccelerationTimeReduceFactor = 1.0f;

    public const float AirAccelerationFactor = 1.0f;
    public const float AirDeccelerationFactor = 1.0f;
    public const float AirAccelerationTimeReduceFactor = 1.5f;
    public const float AirDeccelerationTimeReduceFactor = 1.5f;


    public const int BufferFrameTime = 5;//输入缓存帧
    public const int IsGroundedBufferFrame = 10;
    public const int VlunerableAfterDamageTime = 1;//
    public const float JumpUpSlowDownTime=0.3f;//
    public const float JumpUpStopTime = 0.05f;//
    public const float SprintCd=0.3f;//从冲刺结束后算起
    public const float SprintTime = 0.438f;//此值应与实际动画的时长相同

    public const float BreakMoonPointCd = 3f;
    public const float BreakMoonAfterDistance=2f;//碎月缓冲距离

    public const float PlayerBaseHealTime = 2f;

    public const float PlayerMoveSpeed = 5f;
    public const float PlayerCatMoveSpeed = 6f;

    public const float PlayerCatToHumanTime = 0.33f;//此值应与实际动画的时长相同
    public const float PlayerHumanToCatTime = 2f;

    #region player collider settings
    public const float playerBoxColliderOffsetY=-0.1f;
    public const float playerBoxColliderWidth = 0.24f;
    public const float playerBoxColliderHeight = 1.6f;

    public const float playerCatBoxColliderOffsetY = 0.1f;
    public const float playerCatBoxColliderWidth = 0.5f;
    public const float playerCatBoxColliderHeight = 0.75f;

    public const float playerGroundCheckColliderOffsetY = -0.85f;
    public const float playerCatGroundCheckColliderOffsetY = -0.225f;
    public const float playerGroundColliderXSizeSmall = 0.01f;

    #endregion


    #region 玩家有关属性
    public const int playerInitialMaxHp=5;
    public const int playerInitialMaxMana = 100;
    public const int playerInitialMoney =0;

    public const int playerAttackGainSoul = 10;
    public const int playerHealCostMana = 33;
    public const int playerHealBaseValue = 1;


    #endregion

    public const float monsterBeatBackTime = 0.15f;



}
