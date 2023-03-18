using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : PlayerAction
{
    public PlayerJump(PlayerController playerController) : base(playerController) { }

    private float jumpStartHeight;

    private int currentJumpCountLeft;

    private Coroutine jumpingCoro;
    public int CurrentJumpCountLeft
    {
        get { return currentJumpCountLeft; }
        set
        {
            currentJumpCountLeft = value;
            playerController.PlayerAnimator.SetInteger(playerController.animatorParamsMapping.JumpLeftCountParamHash, currentJumpCountLeft);
        }
    }

    public void resetAllJumpCount() => CurrentJumpCountLeft = playerController.getJumpCount();

    public void justResetDoubleJump()
    {
        if (GameManager.Instance.saveSystem.haveDoubleJump() == false) return;
        CurrentJumpCountLeft = Constants.PlayerMaxDoubleJumpCount - Constants.PlayerMaxJumpCount;
    }

    public void ClearAllJumpCount() => CurrentJumpCountLeft = 0;

    public override void StateStart(EPlayerState oldState)
    {
        playerController.setRigidGravityScale(0);

        if (playerController.isGroundedBuffer() == false)//here only change the jumpCount when in air, the change on ground is done in the isGround's set method
            --CurrentJumpCountLeft;

        playerController.setRigidVelocity(new Vector2(playerController.getRigidVelocity().x, playerController.playerInfo.getJumpUpSpeed()));
        jumpStartHeight = playerController.transform.position.y;

        if(jumpingCoro!=null)
        playerController.StopCoroutine(jumpingCoro);

        jumpingCoro= playerController.StartCoroutine(JumpUpCheck());

        //Debug.Log("跳跃参数：" + CurrentJumpCountLeft);
        if (CurrentJumpCountLeft == 0 && GameManager.Instance.saveSystem.haveDoubleJump())
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
        if (jumpingCoro != null)
            playerController.StopCoroutine(jumpingCoro);
        playerController.setRigidGravityScaleToNormal();
    }

    public IEnumerator JumpUpCheck()
    {
        bool hasQuickSlowDown = false;
        bool hasNormalSlowDown = false;
        //Debug.Log("new Jumpie");

        float normalSlowDistance = 0.5f * playerController.playerInfo.getJumpUpSpeed() * Constants.JumpUpSlowDownTime;//s=0.5*velocity*time
        while (true)
        {
            //Debug.Log("jumping");
            yield return null;//每次update后循环一次
            if (playerController.getRigidVelocity().y < 0.01f)//跳跃上升过程结束  maybe not right..
            {
               // Debug.Log("JumpEnd");
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

    /// <summary>
    /// make player's y speed to zero after stopTime ,by reset the gravityScale
    /// </summary>
    /// <param name="stopTime"></param>
    public void stopJumpInTime(float stopTime)
    {
        if (playerController.getRigidVelocity().y <= 0 || stopTime <= 0) return;
        float acce = playerController.getRigidVelocity().y / stopTime;

        float gScale = -acce / Physics2D.gravity.y;
        playerController.setRigidGravityScale(gScale);
    }

    public override void StateEnd(EPlayerState newState)
    {

    }

}
