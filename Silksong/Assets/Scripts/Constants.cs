using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    // store all the Constants and mark the usage 
    #region about player move
    public const float PlayerMoveSpeed = 5f;
    public const float PlayerCatToFastMoveDistance = 5f;
    public const float PlayerCatMoveSpeed = 6f;
    public const float PlayerCatFastMoveSpeed = 7.5f;

    public const float GroundAccelerationFactor = 1.0f;
    public const float GroundDeccelerationFactor = 1.0f;
    public const float GroundAccelerationTimeReduceFactor = 1.0f;
    public const float GroundDeccelerationTimeReduceFactor = 1.0f;

    public const float AirAccelerationFactor = 1.0f;
    public const float AirDeccelerationFactor = 1.0f;
    public const float AirAccelerationTimeReduceFactor = 1.5f;
    public const float AirDeccelerationTimeReduceFactor = 1.5f;
    #endregion

    #region about player sprint
    public const float PlayerSprintDistance = 4f;
    public const int PlayerMaxAirSprintCount = 1;
    public const float SprintCd = 0.3f;//从冲刺结束后算起
    public const float SprintTime = 0.248f;//此值应与实际动画的时长相同
    #endregion

    #region about player input
    public const int BufferFrameTime = 5;//输入缓存帧
    public const int IsGroundedBufferFrame = 10;
    #endregion

    #region about player climb
    public const float PlayerClimbIdleFallSpeed = 2f;
    public const float PlayerClimbJumpFixedLength = 1.5f;
    public const float PlayerClimbJumpFixedHeight = 1.5f;
    public const float PlayerClimbJumpMaxHeight = 4.1f;
    public const float PlayerClimbJumpFixedTime = 0.2f;
    public const float PlayerClimbJumpTotalTime = 0.5f;
    public const float PlayerClimbJumpNormalSlowDownTime = 0.3f;//
    public const float PlayerClimbJumpCanMoveTime = 0.1f;
    #endregion

    #region about player attack
    public const float AttackingMoveSpeed = 1f;
    #endregion

    #region about player jump
    public const float PlayerJumpUpSpeed = 12f;
    public const float PlayerJumpMinHeight = 0f;
    public const float PlayerJumpMaxHeight = 4.1f;

    public const float PlayerCatJumpHeight = 5.1f;
    public const float PlayerCatJumpUpSpeed = 15f;

    public const float PlayerMaxFallSpeed = 20f;
    public const float PlayerNormalGravityScale = 6f;
    public const int PlayerMaxJumpCount = 1;
    public const int PlayerMaxDoubleJumpCount = 2;
    public const float JumpUpSlowDownTime = 0.3f;//
    public const float JumpUpStopTime = 0.05f;//
    #endregion

    #region about player break moon
    public const float BreakMoonPointCd = 3f;
    public const float BreakMoonAfterDistance=2f;//碎月缓冲距离
    public const float BreakMoonPrePareTime = 0.278f;
    public const float BreakMoonAvgSpeed = 12f;
    #endregion

    public const float PlayerBaseHealTime = 2f;

    public const float PlayerCatToHumanExtraJumpHeight = 2.9f;

    public const int VlunerableAfterDamageTime = 1;//

    #region player collider settings
    public const float playerBoxColliderOffsetY=0f;
    public const float playerBoxColliderWidth = 0.24f;
    public const float playerBoxColliderHeight = 1.6f;

    public const float playerCatBoxColliderOffsetY = 0.2f;
    public const float playerCatBoxColliderWidth = 0.5f;
    public const float playerCatBoxColliderHeight = 0.75f;

    public const float playerGroundCheckColliderOffsetY = -0.80f;
    public const float playerCatGroundCheckColliderOffsetY = -0.13f;
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

    public const float lookUpDownDistance = 10.0f;

    public const float monsterBeatBackTime = 0.15f;



}
