using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfomation : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    public int maxMana = 100;
    public int currentMana;
    public int atk = 1;
    public int money;
    public float sprintSpeed;
    public float normalGravityScale;
    public float swimSpeed;
    public float gravityUnderWater;
    public float plungeSpeed;
    public AnimationCurve breakMoonPositionCurve;
    
    public bool hasDoubleJump;
    public bool playerFacingRight;
    public bool isCat;
    public bool isFastMoving;

    public EPlayerState currentState; 
    public void Init()
    {
        currentHP = Mathf.Min(maxHP, currentHP);
        currentMana = Mathf.Min(maxMana, currentMana);
        sprintSpeed = Constants.PlayerSprintDistance / Constants.SprintTime;
        gravityUnderWater = normalGravityScale / 5;
    }

    private float _speedUpFactor = 1.0f;
    public float GetMoveSpeed()
    {
        float speed;
        if (isCat)
        {
            if (isFastMoving)
            {
                speed = Constants.PlayerCatFastMoveSpeed;
            }
            else
            {
                speed = Constants.PlayerCatMoveSpeed;
            }
        }
        else if(currentState == EPlayerState.NormalAttack)
        {
            speed = Constants.AttackingMoveSpeed;
        }
        else speed = Constants.PlayerMoveSpeed;

        return speed *= _speedUpFactor;
    }
    public float GetJumpUpSpeed()
    {
        if (isCat)
        {
            return Constants.PlayerCatJumpUpSpeed;
        }
        else
        {
            return Constants.PlayerJumpUpSpeed;
        }
    }
    public float GetJumpHeight()
    {
        if (isCat)
        {
            return Constants.PlayerCatJumpHeight;
        }
        else
        {
            return Constants.PlayerJumpMaxHeight;
        }
    }
    public int GetJumpCount()
    {
        if (hasDoubleJump)
        {
            return Constants.PlayerMaxDoubleJumpCount;
        }
        else
        {
            return Constants.PlayerMaxJumpCount;
        }
    }

    public void CostMana(int cost)
    {
        currentMana -= cost;
    }

    public void SpeedUp(float speedUpPercent)
    {
        Debug.LogError("Speed Up Success");
        _speedUpFactor *= 1.0f + speedUpPercent;
    }

    public void SpeedUpReset()
    {
        _speedUpFactor = 1.0f;
    }
}
