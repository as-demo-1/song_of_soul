using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//计划按<位>置状态，但仍未实现多个状态同时存在 不要轻易修改其值 修改后smb的枚举将丢失
public enum EPlayerState
{
    None = 0,
    Idle = 1,
    Run = 2,
    Jump = 4,
    Fall = 8,
    NormalAttack = 16,
}
public class PlayerStatesBehaviour : StatesBehaviour
{
    PlayerController playerController { get; set; }
    PlayerJump playerJump;
    PlayerFall playerFall;
    public void init()
    {
        playerJump = new PlayerJump(playerController);
        playerFall = new PlayerFall(playerController);
    }

    public PlayerStatesBehaviour(PlayerController playerController)
    {
        this.playerController = playerController;
        init();
    }
    public override void StatesEnterBehaviour(EPlayerState playerStates)
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
            default:
                break;
        }
    }
    //active为进入该state时第一帧开始，也就是没有把start从中分开
    public override void StatesActiveBehaviour(EPlayerState playerStates)
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
            default:
                break;
        }
    }
    public override void StatesExitBehaviour(EPlayerState playerStates)
    {

    }


}


public abstract class StatesBehaviour
{
    public abstract void StatesEnterBehaviour(EPlayerState playerStates);
    public abstract void StatesActiveBehaviour(EPlayerState playerStates);
    public abstract void StatesExitBehaviour(EPlayerState playerStates);
}

public class PlayerJump
{
    private PlayerController playerController;

    private float jumpStartHeight;
    public PlayerJump(PlayerController playerController)
    {
        this.playerController = playerController;
    }
    public void JumpStart()
    {
        // PlayerInput.Instance.jump.SetValidToFalse();
        //if (playerController.playerInfo.fixedUpSpeedJump)
            playerController.RB.gravityScale = 0;

       if(playerController.isGroundedBuffer()==false)//只有空中跳跃减跳跃次数，地上跳跃次数由IsGround set方法减去
         --playerController.CurrentJumpCountLeft;

        playerController.RB.velocity = new Vector2(playerController.RB.velocity.x, playerController.playerInfo.jumpUpSpeed);
        jumpStartHeight = playerController.transform.position.y;

        playerController.StopCoroutine(JumpCheck());
        playerController.StartCoroutine(JumpCheck());
    }

    public IEnumerator JumpCheck()
    {
        bool hasQuickSlowDown=false;
        bool hasNormalSlowDown = false;

        float normalSlowDistance = 0.5f*playerController.playerInfo.jumpUpSpeed * 0.3f;
        while(true)
        {
            yield return null;//每次update后循环一次
            if(playerController.PlayerAnimatorStatesControl.CurrentPlayerState!=EPlayerState.Jump)
            {
                playerController.RB.gravityScale = playerController.playerInfo.normalGravityScale;
                break;
            }

            float jumpHeight = playerController.transform.position.y - jumpStartHeight; 

            if(jumpHeight>playerController.playerInfo.jumpMinHeight-0.5f)//达到最小高度后才能停下
            {
               /* if((jumpHeight>playerController.playerInfo.jumpMaxHeight-0.5f || PlayerInput.Instance.jump.Held==false) && !hasSlowDown)//进入减速下落阶段
                {
                    hasSlowDown = true;ffd
                    float jumpSlowDownTime = 0.1f;//随手定的
                    float acce = playerController.RB.velocity.y / jumpSlowDownTime;
                    float gScale = -acce / Physics2D.gravity.y;
                   // Debug.Log(gScale);
                    playerController.RB.gravityScale = gScale;
                }*/
                if (PlayerInput.Instance.jump.Held == false && hasQuickSlowDown == false)//急刹
                {
                    hasQuickSlowDown = true;
                    float jumpSlowDownTime = 0.05f;//随手定的
                    float acce = playerController.RB.velocity.y / jumpSlowDownTime;
                    float gScale = -acce / Physics2D.gravity.y;
                    // Debug.Log(gScale);
                    playerController.RB.gravityScale = gScale;
                }
                if(jumpHeight > playerController.playerInfo.jumpMaxHeight - normalSlowDistance && !hasNormalSlowDown && !hasQuickSlowDown)//缓停
                {
                    hasNormalSlowDown = true;
                    float jumpSlowDownTime = 0.3f;//随手定的
                    float acce = playerController.RB.velocity.y / jumpSlowDownTime;
                    float gScale = -acce / Physics2D.gravity.y;
                    // Debug.Log(gScale);
                    playerController.RB.gravityScale = gScale;
                }
            }
        }

    }

}

public class PlayerFall
{
    private PlayerController playerController;

    public PlayerFall(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    public void checkMaxFallSpeed()
    {
        if(playerController.RB.velocity.y<-playerController.playerInfo.maxFallSpeed)
        {
            playerController.RB.velocity =new Vector2(playerController.RB.velocity.x, -playerController.playerInfo.maxFallSpeed);
        }
    }
}
