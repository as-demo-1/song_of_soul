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
    private void Awake()
    {
        currentHP = Mathf.Min(maxHP, currentHP);
        currentMana = Mathf.Min(maxMana, currentMana);
        sprintSpeed = Constants.PlayerSprintDistance / Constants.SprintTime;
        gravityUnderWater = normalGravityScale / 5;
    }
    
    public float getMoveSpeed()
    {
        if (isCat)
        {
            if (isFastMoving)
            {
                return Constants.PlayerCatFastMoveSpeed;
            }
            else
            {
                return Constants.PlayerCatMoveSpeed;
            }
        }
        else if(currentState == EPlayerState.NormalAttack)
        {
            return Constants.AttackingMoveSpeed;
        }
        else return Constants.PlayerMoveSpeed;
    }
    public float getJumpUpSpeed()
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
    public float getJumpHeight()
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

    public int getJumpCount()
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
}
