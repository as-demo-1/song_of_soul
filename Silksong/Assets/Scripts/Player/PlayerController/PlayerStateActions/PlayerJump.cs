using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void resetJumpCount() => CurrentJumpCountLeft = playerController.playerInfo.getJumpCount();

    public void resetDoubleJump()
    {
        if (playerController.playerInfo.hasDoubleJump == false) return;
        CurrentJumpCountLeft = Constants.PlayerMaxDoubleJumpCount - Constants.PlayerMaxJumpCount;
    }

    public override void StateStart(EPlayerState oldState)
    {
        playerController.setRigidGravityScale(0);

        if (playerController.isGroundedBuffer() == false)//只有空中跳跃减跳跃次数，地上跳跃次数由IsGround set方法减去
            --CurrentJumpCountLeft;

        playerController.setRigidVelocity(new Vector2(playerController.getRigidVelocity().x, playerController.playerInfo.getJumpUpSpeed()));
        jumpStartHeight = playerController.transform.position.y;

        playerController.StopCoroutine(JumpUpCheck());
        playerController.StartCoroutine(JumpUpCheck());

        //Debug.Log("跳跃参数：" + CurrentJumpCountLeft);
        if (CurrentJumpCountLeft == 0)
        {
            playerController.jump.Play();
        }

    }

    public override void StateUpdate()
    {

        playerController.CheckFlipPlayer(1f);
        playerController.CheckHorizontalMove(0.5f);
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

            if (jumpHeight > Constants.PlayerJumpMinHeight - 0.5f)//达到最小高度后才能停下
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
