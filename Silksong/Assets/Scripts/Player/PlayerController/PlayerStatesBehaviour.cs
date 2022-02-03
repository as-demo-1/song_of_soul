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
    Sprint=60,
    BreakMoon=70,

}
public class PlayerStatesBehaviour 
{
   public PlayerController playerController { get; set; }
   public PlayerJump playerJump;
   public PlayerFall playerFall;
   public PlayerSprint playerSprint;
    public PlayerBreakMoon playerBreakMoon;
    public void init()
    {
        playerJump = new PlayerJump(playerController);
        playerFall = new PlayerFall(playerController);
        playerSprint = new PlayerSprint(playerController);
        playerBreakMoon = new PlayerBreakMoon(playerController);
    }

    public PlayerStatesBehaviour(PlayerController playerController)
    {
        this.playerController = playerController;
        init();
    }
    public void StatesEnterBehaviour(EPlayerState playerStates)
    {
        switch (playerStates)
        {
            /* case EPlayerState.None:
                   break*/
            case EPlayerState.Idle:
                break;
            case EPlayerState.Run:
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
                playerSprint.SprintStart();
                break;
            case EPlayerState.BreakMoon:
                playerBreakMoon.breakMoonStart();

                break;
            default:
                break;
        }
    }
    //active为进入该state时第一帧开始，也就是没有把start从中分开
    public  void StatesActiveBehaviour(EPlayerState playerStates)
    {
        switch (playerStates)
        {
            /* case EPlayerState.None:
                 break;*/
            case EPlayerState.Idle:
                //PlayerController.CheckIsGroundedAndResetAirJumpCount();
                playerController.CheckAddItem();
                playerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.Run:
                // PlayerController.CheckIsGroundedAndResetAirJumpCount();
                playerController.CheckAddItem();
                playerController.CheckFlipPlayer(1f);
                playerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.Jump:
                // PlayerController.IsGrounded = false;
                playerController.CheckFlipPlayer(1f);
                playerController.CheckHorizontalMove(0.5f);
                break;
            case EPlayerState.Fall:
                //PlayerController.CheckIsGroundedAndResetAirJumpCount();
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
            default:
                break;
        }
    }
    public  void StatesExitBehaviour(EPlayerState playerStates)
    {
        switch (playerStates)
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

public class PlayerJump:PlayerAction
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
            playerController. PlayerAnimator.SetInteger(playerController.animatorParamsMapping.JumpLeftCountParamHash, currentJumpCountLeft);
        }
    }

    public void resetJumpCount() => CurrentJumpCountLeft = playerController.playerInfo.maxJumpCount;

    public void JumpStart()
    {

        playerController.setRigidGravityScale(0);

       if(playerController.isGroundedBuffer()==false)//只有空中跳跃减跳跃次数，地上跳跃次数由IsGround set方法减去
         --CurrentJumpCountLeft;

        playerController.setRigidVelocity( new Vector2(playerController.getRigidVelocity().x, playerController.playerInfo.jumpUpSpeed));
        jumpStartHeight = playerController.transform.position.y;

        playerController.StopCoroutine(JumpUpCheck());
        playerController.StartCoroutine(JumpUpCheck());
    }

    public IEnumerator JumpUpCheck()
    {
        bool hasQuickSlowDown=false;
        bool hasNormalSlowDown = false;

        float normalSlowDistance = 0.5f*playerController.playerInfo.jumpUpSpeed * Constants.JumpUpSlowDownTime;//s=0.5*velocity*time
        while(true)
        {
            yield return null;//每次update后循环一次
            //EPlayerState state = playerController.PlayerAnimatorStatesControl.CurrentPlayerState;
            if (playerController.getRigidVelocity().y<0.01f)//跳跃上升过程结束
            {
                playerController.setRigidGravityScaleToNormal();
                break;
            }

            float jumpHeight = playerController.transform.position.y - jumpStartHeight; 

            if(jumpHeight>playerController.playerInfo.jumpMinHeight-0.5f)//达到最小高度后才能停下
            {

                if ( hasQuickSlowDown == false && PlayerInput.Instance.jump.Held == false )//急刹
                {
                    hasQuickSlowDown = true;
                    float jumpSlowDownTime = Constants.JumpUpStopTime;
                    float acce = playerController.getRigidVelocity().y / jumpSlowDownTime;
                    float gScale = -acce / Physics2D.gravity.y;
                    // Debug.Log(gScale);
                    playerController.setRigidGravityScale(gScale);
                }
                if(!hasNormalSlowDown && !hasQuickSlowDown && jumpHeight > playerController.playerInfo.jumpMaxHeight - normalSlowDistance )//缓停
                {
                    hasNormalSlowDown = true;
                    float jumpSlowDownTime = Constants.JumpUpSlowDownTime;
                    float acce = playerController.getRigidVelocity().y / jumpSlowDownTime;
                    float gScale = -acce / Physics2D.gravity.y;
                    // Debug.Log(gScale);
                    playerController.setRigidGravityScale(gScale);
                }
            }
        }

    }

}

public class PlayerFall:PlayerAction
{

    public PlayerFall(PlayerController playerController) : base(playerController) { }

    public void checkMaxFallSpeed()
    {
        if(playerController.getRigidVelocity().y<-playerController.playerInfo.maxFallSpeed)
        {
            playerController.setRigidVelocity( new Vector2(playerController.getRigidVelocity().x, -playerController.playerInfo.maxFallSpeed));
        }
    }
}

public class PlayerSprint : PlayerAction
{
    public PlayerSprint(PlayerController playerController):base(playerController){}

    private bool sprintReady;
    public bool SprintReady
    {
        get { return sprintReady; }
        set
        {
            sprintReady = value;
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SprintReadyParamHash,sprintReady);
        }
    }

    private int airSprintLeftCount;
    public int AirSprintLeftCount
    {
        get { return airSprintLeftCount; }
        set
        {
            airSprintLeftCount = value;
            playerController.PlayerAnimator.SetInteger(playerController.animatorParamsMapping.AirSprintLeftCountParamHash,airSprintLeftCount);
        }
    }

    public void resetAirSprintLeftCount()
    {
        AirSprintLeftCount = playerController.playerInfo.maxAirSprintCount;
    }
    public void SprintStart()
    {
        playerController.setRigidGravityScale(0);
        int x = playerController.playerInfo.playerFacingRight ? 1 : -1;
        playerController.setRigidVelocity( new Vector2(playerController.playerInfo.sprintSpeed * x, 0));

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

    public IEnumerator  sprintCdCount()
    {
        SprintReady = false;
        yield return new WaitForSeconds(Constants.SprintCd);
        SprintReady = true;
    }

}

public class PlayerBreakMoon:PlayerAction
{
    public PlayerBreakMoon(PlayerController playerController) : base(playerController) { }

    public BreakMoonPoint currentTarget;
    public List<BreakMoonPoint> availableTargets=new List<BreakMoonPoint>();

    private float timer;
    private float totalTime;
    private Vector2 totalDistance;
    private Vector2 startPosition;
    private Vector2 toMoonDistance;
    private bool hasBreakTheMoon;



    public void findCurrentTarget()
    {
       
        if (availableTargets.Count < 1)
        {
            currentTarget = null;
            return;
        }

        BreakMoonPoint temp;
        temp = availableTargets[0];
        for(int i=1;i<availableTargets.Count;i++)
        {
            BreakMoonPoint t = availableTargets[i];
            if (sameSide(t) ==sameSide(temp))
            {
                if (Vector2.Distance(t.transform.position, playerController.transform.position)
                    < Vector2.Distance(temp.transform.position, playerController.transform.position))
                {
                    temp = t;
                }
            }
            else if(sameSide(t))
            {
                temp = t;
            }
        }
        if(currentTarget)
        currentTarget.unPicked();

        currentTarget = temp;
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
        //Debug.Log("breakMoonStart");
        startPosition = playerController.transform.position;
        Vector2 target = currentTarget.transform.position;
        toMoonDistance=target- startPosition;
        Vector2 afterDistance = toMoonDistance.normalized* Constants.BreakMoonAfterDistance;
        totalDistance = afterDistance + toMoonDistance;
       /* Debug.Log(afterDistance);
        Debug.Log(toMoonDistance);
        Debug.Log(totalDistance);*/
        totalTime= totalDistance.magnitude/playerController.playerInfo.breakMoonAvgSpeed;
       // Debug.Log(totalTime);

        timer = 0;
        PlayerInput.Instance.ReleaseControls();
        playerController.setRigidGravityScale(0);
        playerController.gravityLock = true;
        playerController.setRigidVelocity(Vector2.zero);
        hasBreakTheMoon = false;

        if(sameSide(currentTarget)==false)
        {
            playerController.Flip();
        }
    }

    public void breakingMoon()
    {
        if(timer<totalTime)
        {
            timer += Time.deltaTime;
            float rate= playerController.playerInfo.breakMoonPositionCurve.Evaluate(timer / totalTime);
            //Debug.Log(rate);
            Vector2 s = totalDistance * rate;
            playerController.rigidMovePosition(startPosition + s);

            if (!hasBreakTheMoon && s.magnitude>=toMoonDistance.magnitude )//击碎月球
            {
                //Debug.Log("break");
                hasBreakTheMoon = true;
                PlayerInput.Instance.GainControls();
                currentTarget.atBreakMoonPoint();
            }
        }
        else
        {
            playerController.gravityLock = false;
            playerController.setRigidGravityScaleToNormal();
        }

    }
    public void endBreakMoon()
    {
        playerController.gravityLock = false;
        playerController.setRigidGravityScaleToNormal();
    }

}
