using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbJump : PlayerAction
{
    public PlayerClimbJump(PlayerController playerController) : base(playerController) { }

    private bool canMove = false;//能否水平移动
    private float fixedJumpAcce = 0;//水平减速度


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
        playerController.climp.Play();
        playerController.climpLight.Play();
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
            if (playerController.getRigidVelocity().y < 0.01f)//跳跃上升过程结束
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
