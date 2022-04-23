using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 不要轻易修改其值 修改后smb的枚举将丢失
public enum EPlayerState
{
    None = 0,
    Idle = 10,
    Run = 20,
    Jump = 30,
    Fall = 40,
    NormalAttack = 50,
    Sprint = 60,
    BreakMoon = 70,
    Heal = 90,
    Hurt = 100,
    CastSkill = 110,
    Swim = 120,
    Plunge = 130,
    ClimbIdle=140,
    ClimbJump=150,

    ToCat = 200,
    CatIdle = 210,
    ToHuman = 220,
    CatToHumanExtraJump = 230,
    SprintInWater = 240,
}
public class PlayerStatesBehaviour
{
    public PlayerController playerController { get; set; }
    public PlayerJump playerJump;
    public PlayerFall playerFall;
    public PlayerSprint playerSprint;
    public PlayerBreakMoon playerBreakMoon;
    public PlayerHeal playerHeal;
    public PlayerSwim playerSwim;
    public PlayerCastSkill playerCastSkill;
    public PlayerPlunge playerPlunge;
    public PlayerClimb playerClimb;
    public PlayerSprintInWater playerSprintInWater;

    public void init()
    {
        playerJump = new PlayerJump(playerController);
        playerFall = new PlayerFall(playerController);
        playerSprint = new PlayerSprint(playerController);
        playerBreakMoon = new PlayerBreakMoon(playerController);
        playerHeal = new PlayerHeal(playerController);
        playerSwim = new PlayerSwim(playerController);
        playerCastSkill = new PlayerCastSkill(playerController);
        playerPlunge = new PlayerPlunge(playerController);
        playerClimb = new PlayerClimb(playerController);
        playerSprintInWater = new PlayerSprintInWater(playerController);
    }

    public PlayerStatesBehaviour(PlayerController playerController)
    {
        this.playerController = playerController;
        init();
    }
    public void StatesEnterBehaviour(EPlayerState newStates,EPlayerState oldState)
    {
        switch (newStates)
        {
            /* case EPlayerState.None:
                   break*/
            case EPlayerState.Idle:
                break;
            case EPlayerState.Run:
                playerController.playerToCat.catMoveStart();
                break;
            case EPlayerState.Jump:
                playerJump.JumpStart();
                break;
            case EPlayerState.Fall:
                break;
            case EPlayerState.NormalAttack:
                playerController.CheckFlipPlayer(1f);
                break;
            case EPlayerState.Sprint:
                playerSprint.SprintStart(oldState);
                break;
            case EPlayerState.BreakMoon:
                playerBreakMoon.breakMoonStart();
                break;
            case EPlayerState.Hurt:
                PlayerAnimatorParamsMapping.SetControl(false);
                break;
            case EPlayerState.Heal:
                playerHeal.healStart();
                break;
            case EPlayerState.Swim:
                playerSwim.SwimStart();
                break;
            case EPlayerState.ToCat:

                break;
            case EPlayerState.ToHuman:
                playerController.playerToCat.colliderToHuman();
                break;
            case EPlayerState.CatIdle:
                playerController.playerToCat.toCat();
                break;
            case EPlayerState.CatToHumanExtraJump:

                break;
            case EPlayerState.CastSkill:
                playerCastSkill.CastSkill();
                break;
            case EPlayerState.Plunge:
                playerPlunge.PlungeStart();
                break;
            case EPlayerState.ClimbIdle:
                playerClimb.climbIdleStart();
                break;
            case EPlayerState.ClimbJump:
                playerClimb.climbJumpStart();
                break;
            case EPlayerState.SprintInWater:
                playerSprintInWater.SprintInWaterStart();
                break;
            default:
                break;
        }
    }
    //active为进入该state时第一帧开始，也就是没有把start从中分开
    public void StatesActiveBehaviour(EPlayerState playerStates)
    {
        switch (playerStates)
        {
            /* case EPlayerState.None:
                 break;*/
            case EPlayerState.Idle:

                playerController.CheckAddItem();
                playerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.Run:

                playerController.CheckAddItem();
                playerController.CheckFlipPlayer(1f);
                playerController.CheckHorizontalMove(0.4f);
                playerController.playerToCat.moveDistanceCount();
                break;
            case EPlayerState.Jump:

                playerController.CheckFlipPlayer(1f);
                playerController.CheckHorizontalMove(0.5f);
                break;
            case EPlayerState.Fall:

                playerController.CheckFlipPlayer(1f);
                playerController.CheckHorizontalMove(0.5f);
                playerFall.checkMaxFallSpeed();
                break;
            case EPlayerState.NormalAttack:
                playerController.CheckHorizontalMove(0.5f);
                break;
            case EPlayerState.Sprint:

                break;
            case EPlayerState.BreakMoon:
                playerBreakMoon.breakingMoon();

                break;
            case EPlayerState.Hurt:
                playerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.Heal:
                playerController.CheckHorizontalMove(0.4f);
                playerHeal.healProcess();
                break;
            case EPlayerState.Swim:
                playerController.SwimUnderWater();
                break;
            case EPlayerState.ToCat:
                playerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.ToHuman:
                playerController.CheckFlipPlayer(1f);
                playerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.CatIdle:
                playerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.CatToHumanExtraJump:
                playerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.Plunge:
                playerPlunge.Plunging();
                break;
            case EPlayerState.ClimbIdle:

                break;
            case EPlayerState.ClimbJump:
                playerClimb.climbJumping();
                break;
            case EPlayerState.SprintInWater:
    
                break;
            default:
                break;
        }
    }
    public void StatesExitBehaviour(EPlayerState exitStates,EPlayerState newState)
    {
        switch (exitStates)
        {

            case EPlayerState.Idle:

                break;
            case EPlayerState.Run:

                break;
            case EPlayerState.Jump:

                break;
            case EPlayerState.Fall:

                break;
            case EPlayerState.NormalAttack:

                break;
            case EPlayerState.Sprint:
                playerSprint.SprintEnd();
                break;
            case EPlayerState.BreakMoon:
                playerBreakMoon.endBreakMoon();

                break;
            case EPlayerState.Hurt:
                PlayerAnimatorParamsMapping.SetControl(true);
                break;
            case EPlayerState.Heal:

                break;
            case EPlayerState.Swim:
                playerSwim.SwimEnd();
                break;
            case EPlayerState.ToCat:

                break;
            case EPlayerState.ToHuman:
                playerJump.EndJump();
                playerController.playerToCat.extraJump();
                playerController.playerToCat.stateToHuman();
                break;
            case EPlayerState.CatIdle:

                break;
            case EPlayerState.CatToHumanExtraJump:

                break;
            case EPlayerState.Plunge:
                playerPlunge.PlungeEnd();
                break;
            case EPlayerState.ClimbIdle:
                playerClimb.climbIdleEnd();
                break;
            case EPlayerState.ClimbJump:

                break;
            case EPlayerState.SprintInWater:
                playerSprintInWater.SprintInWaterEnd();
                break;
            default:
                break;
        }
    }


}


/*public abstract class StatesBehaviour
{
    public abstract void StatesEnterBehaviour(EPlayerState playerStates);
    public abstract void StatesActiveBehaviour(EPlayerState playerStates);
    public abstract void StatesExitBehaviour(EPlayerState playerStates);
}*/

public abstract class PlayerAction
{
    protected PlayerController playerController;
    public PlayerAction(PlayerController playerController)
    {
        this.playerController = playerController;
    }
}

public class PlayerJump : PlayerAction
{
    public PlayerJump(PlayerController playerController) : base(playerController) { }

    private float jumpStartHeight;

    private int currentJumpCountLeft;
    public int CurrentJumpCountLeft
    {
        get { return currentJumpCountLeft; }
        set
        {
            currentJumpCountLeft = value;
            playerController.PlayerAnimator.SetInteger(playerController.animatorParamsMapping.JumpLeftCountParamHash, currentJumpCountLeft);
        }
    }

    public void resetJumpCount() => CurrentJumpCountLeft = playerController.playerInfo.maxJumpCount;

    public void JumpStart()
    {

        playerController.setRigidGravityScale(0);

        if (playerController.isGroundedBuffer() == false)//只有空中跳跃减跳跃次数，地上跳跃次数由IsGround set方法减去
            --CurrentJumpCountLeft;

        playerController.setRigidVelocity(new Vector2(playerController.getRigidVelocity().x, playerController.playerInfo.getJumpUpSpeed()));
        jumpStartHeight = playerController.transform.position.y;

        playerController.StopCoroutine(JumpUpCheck());
        playerController.StartCoroutine(JumpUpCheck());
    }

    public void EndJump()
    {
        playerController.StopCoroutine(JumpUpCheck());
        playerController.setRigidGravityScaleToNormal();
    }

    public IEnumerator JumpUpCheck()
    {
        bool hasQuickSlowDown = false;
        bool hasNormalSlowDown = false;

        float normalSlowDistance = 0.5f * playerController.playerInfo.getJumpUpSpeed() * Constants.JumpUpSlowDownTime;//s=0.5*velocity*time
        while (true)
        {
            yield return null;//每次update后循环一次
            //EPlayerState state = playerController.PlayerAnimatorStatesControl.CurrentPlayerState;
            if (playerController.getRigidVelocity().y < 0.01f)//跳跃上升过程结束
            {
                playerController.setRigidGravityScaleToNormal();
                break;
            }

            float jumpHeight = playerController.transform.position.y - jumpStartHeight;

            if (jumpHeight > playerController.playerInfo.jumpMinHeight - 0.5f)//达到最小高度后才能停下
            {

                if (hasQuickSlowDown == false && PlayerInput.Instance.jump.Held == false)//急刹
                {
                    hasQuickSlowDown = true;
                    stopJumpInTime(Constants.JumpUpStopTime);
                }
                if (!hasNormalSlowDown && !hasQuickSlowDown && jumpHeight > playerController.playerInfo.getJumpHeight() - normalSlowDistance)//
                {
                    hasNormalSlowDown = true;
                    stopJumpInTime(Constants.JumpUpSlowDownTime);
                }
            }
        }

    }

    public void stopJumpInTime(float stopTime)
    {
        float acce = playerController.getRigidVelocity().y / stopTime;
        float gScale = -acce / Physics2D.gravity.y;
        // Debug.Log(gScale);
        playerController.setRigidGravityScale(gScale);
    }

}

public class PlayerFall : PlayerAction
{

    public PlayerFall(PlayerController playerController) : base(playerController) { }

    public void checkMaxFallSpeed()
    {
        if (playerController.getRigidVelocity().y < -playerController.playerInfo.maxFallSpeed)
        {
            playerController.setRigidVelocity(new Vector2(playerController.getRigidVelocity().x, -playerController.playerInfo.maxFallSpeed));
        }
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
        AirSprintLeftCount = playerController.playerInfo.maxAirSprintCount;
    }
    public void SprintStart(EPlayerState oldState)
    {
        playerController.setRigidGravityScale(0);
        if (oldState == EPlayerState.ClimbIdle)
        {
            playerController.Flip();
        }

        int x = playerController.playerInfo.playerFacingRight ? 1 : -1;

        playerController.setRigidVelocity(new Vector2(playerController.playerInfo.sprintSpeed * x, 0));

        if (playerController.isGroundedBuffer() == false)
            AirSprintLeftCount--;

        playerController.gravityLock = true;
    }

    public void SprintEnd()
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
    private bool sameSide(BreakMoonPoint b)//是否在玩家面朝的一侧
    {
        float x = b.transform.position.x - playerController.transform.position.x;
        bool result = playerController.playerInfo.playerFacingRight ? x > 0 : x < 0;
        return result;
    }

    public void breakMoonStart()
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
        totalTime = totalDistance.magnitude / playerController.playerInfo.breakMoonAvgSpeed;
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

    public void breakingMoon()
    {
        timer += Time.deltaTime;
        if (!prepareOver )
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

            if (!hasBreakTheMoon && s.magnitude >= toMoonDistance.magnitude)//击碎月球
            {
                //Debug.Log("break");
                hasBreakTheMoon = true;
                PlayerAnimatorParamsMapping.SetControl(true);
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
    public void endBreakMoon()
    {
        //  Debug.Log("end braeakMoon");
        playerController.gravityLock = false;
        playerController.setRigidGravityScaleToNormal();
    }

}

public class PlayerHeal : PlayerAction
{
    public PlayerHeal(PlayerController playerController) : base(playerController) { }
    private float healTotalTime;
    private float healTimer;
    private float healStartMana;
    public void healStart()
    {
        healTotalTime = Constants.PlayerBaseHealTime;
        healTimer = 0;
        healStartMana = playerController.playerCharacter.Mana;
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

public class PlayerSwim : PlayerAction
{
    public PlayerSwim(PlayerController playerController) : base(playerController) { }

    public void SwimStart()
    {
        playerController.IsUnderWater = true;
        //入水后0.2s内禁止向上
        //setRigidGravityScale(playerInfo.normalGravityScale/2);
    }

    public void SwimEnd()
    {
        playerController.m_Transform.localRotation = Quaternion.Euler(0, 0, 0);
        playerController.IsUnderWater = false;
        //setRigidGravityScaleToNormal();
    }
}

public class PlayerCastSkill : PlayerAction
{
    public PlayerCastSkill(PlayerController playerController) : base(playerController)
    {
        playerSkillManager = playerController.gameObject.GetComponent<PlayerSkillManager>();
    }

    private PlayerSkillManager playerSkillManager;

    public void CastSkill()
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


    public void PlungeStart()
    {
        // Debug.Log("start plunging");

        // 竖直下落
        playerController.setRigidGravityScale(0f);
        playerController.gravityLock = true;
        playerController.setRigidVelocity(new Vector2(0, -1 * playerController.playerInfo.plungeSpeed));


        plungeStrength = 0;

        plungeDistance = 0.0f;
        plungeStartPositionY = playerController.transform.position.y;
    }

    public void Plunging()
    {
        float positionY = playerController.transform.position.y;
        plungeDistance = plungeStartPositionY - positionY;

        // 更新Strength
        int i = plungeStrength;
        while (i < playerController.plungeStrengthArr.Length - 1 && plungeDistance > playerController.plungeStrengthArr[i + 1])
        {
            // Debug.Log(plungeStrength);
            i++;
        }
        plungeStrength = i;

        // 在 DestructiblePlatform 组件更新 willBreakGround，在此更新animator param
        // playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.WillBreakGroundParamHash, willBreakGround);

    }

    public void PlungeEnd()
    {

        playerController.gravityLock = false;
        playerController.setRigidGravityScaleToNormal();

        // Debug.Log("Landed! Plunge strength:" + plungeStrength + "Distance:" + plungeDistance);
        plungeStrength = 0;
        plungeDistance = 0.0f;
    }

}

public class PlayerClimb : PlayerAction
{
    public PlayerClimb(PlayerController playerController) : base(playerController) { }

    private bool canMove = false;//能否水平移动
    private float fixedJumpAcce = 0;//水平减速度
   
    public void climbIdleStart()
    {
        playerController.setRigidGravityScale(0);
        playerController.setRigidVelocity(new Vector2(0, -Constants.PlayerClimbIdleFallSpeed));
        playerController.playerStatesBehaviour.playerJump.resetJumpCount();
        playerController.playerStatesBehaviour.playerSprint.resetAirSprintLeftCount();
    }

    public void climbIdleEnd()
    {
        playerController.setRigidGravityScale(playerController.playerInfo.normalGravityScale);
        playerController.playerStatesBehaviour.playerJump.CurrentJumpCountLeft--;
    }

    public void climbJumpStart()
    {
        playerController.Flip();
        int x = playerController.playerInfo.playerFacingRight ? 1 : -1;
        Vector2 speed;
        speed.x = 2*Constants.PlayerClimbJumpFixedLength/ Constants.PlayerClimbJumpFixedTime * x;
        fixedJumpAcce = speed.x / Constants.PlayerClimbJumpFixedTime;
        float jumpSpeed = Constants.PlayerClimbJumpMaxHeight / (Constants.PlayerClimbJumpTotalTime - Constants.PlayerClimbJumpNormalSlowDownTime * 0.5f);
        speed.y = jumpSpeed;
        canMove = false;
        playerController.setRigidVelocity(speed);
        playerController.setRigidGravityScale(0);

        playerController.StopCoroutine(IEClimbJumping());
        playerController.StartCoroutine(IEClimbJumping());

    }

    public void climbJumping()
    {
        if(canMove)
        {
            playerController.CheckFlipPlayer(1f);
            playerController.CheckHorizontalMove(0.5f);
        }
    }

    public IEnumerator IEClimbJumping() 
    {
        float timer=0;
        bool hasActiveSlowDown = false;//player active stop
        bool hasNormalSlowDown = false;//passive stop
        bool isFixedJumping=true;
        bool isContinueJumping = false;
        float jumpStartYPos=playerController.transform.position.y;
        


        while (true)
        {
            yield return null;
            if (playerController.getRigidVelocity().y < 0.01f)//跳跃上升过程结束
            {
                playerController.setRigidGravityScaleToNormal();
                break;
            }
            timer += Time.deltaTime;
            float jumpHeight = playerController.transform.position.y - jumpStartYPos;

            if(timer>=Constants.PlayerClimbJumpFixedTime)
            {
                if(isFixedJumping)
                {
                    isFixedJumping = false;
                    isContinueJumping = true;
                    canMove = true;
                }

            }
            else if(!canMove)
            {
             
                Vector2 t = playerController.getRigidVelocity();
                t.x -= Time.deltaTime * fixedJumpAcce;
                playerController.setRigidVelocity(t);

                if(((PlayerInput.Instance.horizontal.Value==1)?fixedJumpAcce<0:fixedJumpAcce>0) && timer>Constants.PlayerClimbJumpCanMoveTime)
                {
                    canMove = true;
                }
           
            }

            if ( !hasActiveSlowDown  && PlayerInput.Instance.jump.Held == false)
            {
                hasActiveSlowDown = true;
                if (isFixedJumping)
                {
                    float speed = 2f * (Constants.PlayerClimbJumpFixedHeight - jumpHeight) / (Constants.PlayerClimbJumpFixedTime - timer);
                    playerController.setRigidVelocity(new Vector2(playerController.getRigidVelocity().x,speed));
                    playerController.playerStatesBehaviour.playerJump.stopJumpInTime(Constants.PlayerClimbJumpFixedTime - timer);

                }
                if(isContinueJumping)
                {
                    playerController.playerStatesBehaviour.playerJump.stopJumpInTime(Constants.JumpUpStopTime);
                }

            }
            if (!hasNormalSlowDown && !hasActiveSlowDown && timer>Constants.PlayerClimbJumpTotalTime-Constants.PlayerClimbJumpNormalSlowDownTime)//
            {
                hasNormalSlowDown = true;
                playerController.playerStatesBehaviour.playerJump.stopJumpInTime(Constants.PlayerClimbJumpNormalSlowDownTime);
            }
            
        }
       
       
    }

}

public class PlayerSprintInWater : PlayerAction {
    public PlayerSprintInWater(PlayerController playerController) : base(playerController) { }
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
    public void SprintInWaterStart(){
        playerController.setRigidGravityScale(0);
        int x = playerController.playerInfo.playerFacingRight ? 1 : -1;
        int y = 0;
        if(PlayerInput.Instance.vertical.Value != 0){
            y = PlayerInput.Instance.vertical.Value > 0 ? 1:-1;
        }
        playerController.setRigidVelocity(new Vector2(playerController.playerInfo.sprintSpeed * x,playerController.playerInfo.sprintSpeed * y));
        playerController.gravityLock = true;
        playerController.IsUnderWater = false;
    }

     public void SprintInWaterEnd()
    {
        Debug.Log(playerController.IsUnderWater);
        playerController.gravityLock = false;
        if (!playerController.IsUnderWater){
            //冲出水面重力正常
            playerController.setRigidGravityScaleToNormal();
        }else{
            playerController.setRigidGravityScale(playerController.playerInfo.gravityUnderWater);
        }
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
 
