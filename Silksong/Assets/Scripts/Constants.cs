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

    public const int BufferFrameTime = 5;
    public const int VlunerableAfterDamageTime = 1;

    #region 玩家有关属性
    public const int playerInitialMaxHp=5;
    public const int playerInitialMaxSoul = 100;
    public const int playerInitialMoney =0;

    public const int playerAttackGainSoul = 10;
    public const int playerHealCostSoul = 33;


    #endregion

}
