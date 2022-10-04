using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerHeal : PlayerAction
{
    public PlayerHeal(PlayerController playerController) : base(playerController) { }
    private float healTotalTime;
    private float healTimer;
    private float healStartMana;
    public override void StateStart(EPlayerState oldState)
    {
        healTotalTime = Constants.PlayerBaseHealTime;
        healTimer = 0;
        healStartMana = playerController.playerCharacter.Mana;
    }
    public override void StateUpdate()
    {
        playerController.CheckHorizontalMove(0.4f);
        healProcess();
    }
    public void healProcess()
    {
        healTimer += Time.deltaTime;
        float rate = healTimer / healTotalTime;
        playerController.playerCharacter.Mana = (int)Mathf.Lerp(healStartMana, (healStartMana - Constants.playerHealCostMana), rate);
        if (rate >= 1)
        {
            playerController.playerCharacter.playerDamable.addHp(Constants.playerHealBaseValue, null);
            playerController.PlayerAnimator.Play("Idle");
        }
    }

}

public class PlayerHurt : PlayerAction
{
    public PlayerHurt(PlayerController playerController) : base(playerController) { }
    public override void StateEnd(EPlayerState newState)
    {
        PlayerAnimatorParamsMapping.SetControl(true);
    }
    public override void StateUpdate()
    {
        playerController.CheckHorizontalMove(0.4f);
    }
    public override void StateStart(EPlayerState oldState)
    {
        PlayerAnimatorParamsMapping.SetControl(false);
    }
}

public class PlayerCastSkill : PlayerAction
{
    public PlayerCastSkill(PlayerController playerController) : base(playerController)
    {
        playerSkillManager = playerController.gameObject.GetComponent<PlayerSkillManager>();
    }

    private PlayerSkillManager playerSkillManager;

    public override void StateStart(EPlayerState oldState)
    {
        playerSkillManager.Cast();
    }



}

public class PlayerPlunge : PlayerAction
{
    public PlayerPlunge(PlayerController playerController) : base(playerController) { }

    private float plungeStartPositionY;
    private float plungeDistance;

    public int plungeStrength;


    public override void StateStart(EPlayerState oldState)
    {
        // Debug.Log("start plunging");

        // ��ֱ����
        playerController.setRigidGravityScale(0f);
        playerController.gravityLock = true;
        playerController.setRigidVelocity(new Vector2(0, -1 * playerController.playerInfo.plungeSpeed));


        plungeStrength = 0;

        plungeDistance = 0.0f;
        plungeStartPositionY = playerController.transform.position.y;
    }

    public override void StateUpdate()
    {
        float positionY = playerController.transform.position.y;
        plungeDistance = plungeStartPositionY - positionY;

        // ����Strength
        int i = plungeStrength;
        while (i < playerController.plungeStrengthArr.Length - 1 && plungeDistance > playerController.plungeStrengthArr[i + 1])
        {
            // Debug.Log(plungeStrength);
            i++;
        }
        plungeStrength = i;

        // �� DestructiblePlatform ������� willBreakGround���ڴ˸���animator param
        // playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.WillBreakGroundParamHash, willBreakGround);
    }

    public override void StateEnd(EPlayerState newState)
    {
        playerController.gravityLock = false;
        playerController.setRigidGravityScaleToNormal();

        // Debug.Log("Landed! Plunge strength:" + plungeStrength + "Distance:" + plungeDistance);
        plungeStrength = 0;
        plungeDistance = 0.0f;
    }

}

public class PlayerClimbJump : PlayerAction
{
    public PlayerClimbJump(PlayerController playerController) : base(playerController) { }

    private bool canMove = false;//�ܷ�ˮƽ�ƶ�
    private float fixedJumpAcce = 0;//ˮƽ���ٶ�


    public override void StateStart(EPlayerState oldState)
    {
        playerController.Flip();
        int x = playerController.playerInfo.playerFacingRight ? 1 : -1;
        Vector2 speed;
        speed.x = 2 * Constants.PlayerClimbJumpFixedLength / Constants.PlayerClimbJumpFixedTime * x;
        fixedJumpAcce = speed.x / Constants.PlayerClimbJumpFixedTime;
        float jumpSpeed = Constants.PlayerClimbJumpMaxHeight / (Constants.PlayerClimbJumpTotalTime - Constants.PlayerClimbJumpNormalSlowDownTime * 0.5f);
        speed.y = jumpSpeed;
        canMove = false;
        playerController.setRigidVelocity(speed);
        playerController.setRigidGravityScale(0);

        playerController.StopCoroutine(IEClimbJumping());
        playerController.StartCoroutine(IEClimbJumping());
    }

    public override void StateUpdate()
    {
        if (canMove)
        {
            playerController.CheckFlipPlayer(1f);
            playerController.CheckHorizontalMove(0.5f);
        }
    }

    public IEnumerator IEClimbJumping()
    {
        float timer = 0;
        bool hasActiveSlowDown = false;//player active stop
        bool hasNormalSlowDown = false;//passive stop
        bool isFixedJumping = true;
        bool isContinueJumping = false;
        float jumpStartYPos = playerController.transform.position.y;



        while (true)
        {
            yield return null;
            if (playerController.getRigidVelocity().y < 0.01f)//��Ծ�������̽���
            {
                playerController.setRigidGravityScaleToNormal();
                break;
            }
            timer += Time.deltaTime;
            float jumpHeight = playerController.transform.position.y - jumpStartYPos;

            if (timer >= Constants.PlayerClimbJumpFixedTime)
            {
                if (isFixedJumping)
                {
                    isFixedJumping = false;
                    isContinueJumping = true;
                    canMove = true;
                }

            }
            else if (!canMove)
            {

                Vector2 t = playerController.getRigidVelocity();
                t.x -= Time.deltaTime * fixedJumpAcce;
                playerController.setRigidVelocity(t);

                if (((PlayerInput.Instance.horizontal.Value == 1) ? fixedJumpAcce < 0 : fixedJumpAcce > 0) && timer > Constants.PlayerClimbJumpCanMoveTime)
                {
                    canMove = true;
                }

            }

            if (!hasActiveSlowDown && PlayerInput.Instance.jump.Held == false)
            {
                hasActiveSlowDown = true;
                if (isFixedJumping)
                {
                    float speed = 2f * (Constants.PlayerClimbJumpFixedHeight - jumpHeight) / (Constants.PlayerClimbJumpFixedTime - timer);
                    playerController.setRigidVelocity(new Vector2(playerController.getRigidVelocity().x, speed));
                    (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).stopJumpInTime(Constants.PlayerClimbJumpFixedTime - timer);

                }
                if (isContinueJumping)
                {
                    (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).stopJumpInTime(Constants.JumpUpStopTime);
                }

            }
            if (!hasNormalSlowDown && !hasActiveSlowDown && timer > Constants.PlayerClimbJumpTotalTime - Constants.PlayerClimbJumpNormalSlowDownTime)//
            {
                hasNormalSlowDown = true;
                (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).stopJumpInTime(Constants.PlayerClimbJumpNormalSlowDownTime);
            }

        }


    }

}

public class PlayerClimbIdle : PlayerAction
{
    public PlayerClimbIdle(PlayerController playerController) : base(playerController) { }

    public override void StateStart(EPlayerState oldState)
    {
        playerController.setRigidGravityScale(0);
        playerController.setRigidVelocity(new Vector2(0, -Constants.PlayerClimbIdleFallSpeed));
        (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).resetJumpCount();
        (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Sprint] as PlayerSprint).resetAirSprintLeftCount();
    }
    public override void StateEnd(EPlayerState newState)
    {
        playerController.setRigidGravityScale(playerController.playerInfo.normalGravityScale);
        (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).CurrentJumpCountLeft--;
    }


}

public class PlayerNormalAttack : PlayerAction
{
    public PlayerNormalAttack(PlayerController playerController) : base(playerController) { }

    public override void StateStart(EPlayerState oldState)
    {
        playerController.CheckFlipPlayer(1f);
    }
    public override void StateUpdate()
    {
        playerController.CheckHorizontalMove(0.5f);
    }
}

public class PlayerSprint : PlayerAction
{
    public PlayerSprint(PlayerController playerController) : base(playerController) { }

    private bool sprintReady;
    public bool SprintReady
    {
        get { return sprintReady; }
        set
        {
            sprintReady = value;
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SprintReadyParamHash, sprintReady);
        }
    }

    private int airSprintLeftCount;
    public int AirSprintLeftCount
    {
        get { return airSprintLeftCount; }
        set
        {
            airSprintLeftCount = value;
            playerController.PlayerAnimator.SetInteger(playerController.animatorParamsMapping.AirSprintLeftCountParamHash, airSprintLeftCount);
        }
    }

    public void resetAirSprintLeftCount()
    {
        AirSprintLeftCount = Constants.PlayerMaxAirSprintCount;
    }

    public override void StateStart(EPlayerState oldState)
    {
        playerController.setRigidGravityScale(0);
        if (oldState == EPlayerState.ClimbIdle)
        {
            playerController.Flip();
        }
        else
        {
            playerController.CheckFlipPlayer(0.5f);
        }

        int x = playerController.playerInfo.playerFacingRight ? 1 : -1;

        playerController.setRigidVelocity(new Vector2(playerController.playerInfo.sprintSpeed * x, 0));

        if (playerController.isGroundedBuffer() == false)
            AirSprintLeftCount--;

        playerController.gravityLock = true;
    }
    public override void StateEnd(EPlayerState newState)
    {
        playerController.gravityLock = false;

        playerController.setRigidGravityScaleToNormal();
        playerController.setRigidVelocity(Vector2.zero);
        playerController.StartCoroutine(sprintCdCount());
    }

    public IEnumerator sprintCdCount()
    {
        SprintReady = false;
        yield return new WaitForSeconds(Constants.SprintCd);
        SprintReady = true;
    }

}

public class PlayerBreakMoon : PlayerAction
{
    public PlayerBreakMoon(PlayerController playerController) : base(playerController) { }

    public BreakMoonPoint currentTarget;
    public List<BreakMoonPoint> availableTargets = new List<BreakMoonPoint>();

    private float timer;
    private float totalTime;
    private Vector2 totalDistance;
    private Vector2 startPosition;
    private Vector2 toMoonDistance;
    private bool hasBreakTheMoon;
    private bool prepareOver;

    public override void StateStart(EPlayerState oldState)
    {
        // Debug.Log("breakMoonStart");
        startPosition = playerController.transform.position;
        Vector2 target = currentTarget.transform.position;
        toMoonDistance = target - startPosition;
        Vector2 afterDistance = toMoonDistance.normalized * Constants.BreakMoonAfterDistance;
        totalDistance = afterDistance + toMoonDistance;
        /* Debug.Log(afterDistance);
         Debug.Log(toMoonDistance);
         Debug.Log(totalDistance);*/
        totalTime = totalDistance.magnitude / Constants.BreakMoonAvgSpeed;
        // Debug.Log(totalTime);

        timer = 0;
        PlayerAnimatorParamsMapping.SetControl(false);
        playerController.setRigidGravityScale(0);
        playerController.gravityLock = true;
        playerController.setRigidVelocity(Vector2.zero);
        hasBreakTheMoon = false;
        prepareOver = false;

        if (sameSide(currentTarget) == false)
        {
            playerController.Flip();
        }
    }

    public void findCurrentTarget()
    {

        if (availableTargets.Count < 1)
        {
            currentTarget = null;
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.HasBreakMoonPointParamHash, false);
            return;
        }

        BreakMoonPoint temp;
        temp = availableTargets[0];
        for (int i = 1; i < availableTargets.Count; i++)
        {
            BreakMoonPoint t = availableTargets[i];
            if (sameSide(t) == sameSide(temp))
            {
                if (Vector2.Distance(t.transform.position, playerController.transform.position)
                    < Vector2.Distance(temp.transform.position, playerController.transform.position))
                {
                    temp = t;
                }
            }
            else if (sameSide(t))
            {
                temp = t;
            }
        }
        if (currentTarget)
            currentTarget.unPicked();

        currentTarget = temp;
        playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.HasBreakMoonPointParamHash, true);
        currentTarget.bePicked();

    }
    private bool sameSide(BreakMoonPoint b)//�Ƿ�������泯��һ��
    {
        float x = b.transform.position.x - playerController.transform.position.x;
        bool result = playerController.playerInfo.playerFacingRight ? x > 0 : x < 0;
        return result;
    }

    public override void StateEnd(EPlayerState newState)
    {
        //  Debug.Log("end braeakMoon");
        playerController.gravityLock = false;
        playerController.setRigidGravityScaleToNormal();
    }
    public override void StateUpdate()
    {
        timer += Time.deltaTime;
        if (!prepareOver)
        {
            if (timer < Constants.BreakMoonPrePareTime) return;
            else
            {
                timer = 0;
                prepareOver = true;
            }
        }

        if (timer < totalTime)
        {
            float rate = playerController.playerInfo.breakMoonPositionCurve.Evaluate(timer / totalTime);
            //Debug.Log(rate);
            Vector2 s = totalDistance * rate;
            playerController.rigidMovePosition(startPosition + s);

            if (!hasBreakTheMoon && s.magnitude >= toMoonDistance.magnitude)//��������
            {
                //Debug.Log("break");
                hasBreakTheMoon = true;
                PlayerAnimatorParamsMapping.SetControl(true);
                (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Sprint] as PlayerSprint).resetAirSprintLeftCount();
                (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).resetDoubleJump();
                currentTarget.atBreakMoonPoint();
            }
        }
        else
        {
            // Debug.Log("start fall");
            playerController.gravityLock = false;
            playerController.setRigidGravityScaleToNormal();
        }
    }


}

public class PlayerLookUp: PlayerAction
{
    public PlayerLookUp(PlayerController playerController) : base(playerController) { }
    public Vector3 lookPos;
    public override void StateStart(EPlayerState oldState)
    {
        lookPos = new Vector3(0.0f, Constants.lookUpDownDistance, 0.0f);
    }
    public override void StateUpdate()
    {
        playerController.followPoint.transform.localPosition = lookPos;
    }
    public override void StateEnd(EPlayerState newState)
    {
        playerController.followPoint.transform.localPosition = Vector3.zero;
    }
}

public class PlayerLookDown : PlayerAction
{
    public PlayerLookDown(PlayerController playerController) : base(playerController) { }
    public Vector3 lookPos;
    public override void StateStart(EPlayerState oldState)
    {
        lookPos = new Vector3(0.0f, -Constants.lookUpDownDistance, 0.0f);
    }
    public override void StateUpdate()
    {
        playerController.followPoint.transform.localPosition = lookPos;
    }
    public override void StateEnd(EPlayerState newState)
    {
        playerController.followPoint.transform.localPosition = Vector3.zero;
    }
}