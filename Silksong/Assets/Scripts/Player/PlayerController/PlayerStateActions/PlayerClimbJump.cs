using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimbJump : PlayerAction
{
    public PlayerClimbJump(PlayerController playerController) : base(playerController) { }

    private bool canMove = false;//能否水平移动
    private float fixedJumpAcce = 0;//水平减速度
    private Coroutine jumpingCoro;

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

        if (jumpingCoro != null)
            playerController.StopCoroutine(jumpingCoro);

        jumpingCoro = playerController.StartCoroutine(IEClimbJumping());


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

    public void EndJump()
    {
        if (jumpingCoro != null)
            playerController.StopCoroutine(jumpingCoro);
        playerController.setRigidGravityScaleToNormal();
    }

    public IEnumerator IEClimbJumping()
    {
        float timer = 0;
        bool hasActiveSlowDown = false;//player active stop
        bool hasNormalSlowDown = false;//passive stop
        bool isFixedJumping = true;
        //bool isContinueJumping = false;
        float jumpStartYPos = playerController.transform.position.y;



        while (true)
        {
            yield return null;
            if (playerController.getRigidVelocity().y < 0.01f)//jumpup end
            {
                playerController.setRigidGravityScaleToNormal();
                break;
            }
            timer += Time.deltaTime;
            float jumpHeight = playerController.transform.position.y - jumpStartYPos;

            if (timer >= Constants.PlayerClimbJumpFixedTime)//the climb jump is a fixedDistance jump add a free jump ,here judge if the fixedJump is over  
            {
                if (isFixedJumping)
                {
                    isFixedJumping = false;
                    //isContinueJumping = true;
                    canMove = true;
                }

            }
            else if (!canMove)
            {
                //fixedjump horizontal move
                Vector2 t = playerController.getRigidVelocity();
                t.x -= Time.deltaTime * fixedJumpAcce;
                playerController.setRigidVelocity(t);

                //when the fixedJump start ,player can not horizontal move for a short time  if player hope to move toward wall,he can move earlyer before fixedjump over
                if (((PlayerInput.Instance.horizontal.Value == 1) ? fixedJumpAcce < 0 : fixedJumpAcce > 0) && timer > Constants.PlayerClimbJumpCanMoveTime)
                {
                    canMove = true;
                }

            }

            if (!hasActiveSlowDown && PlayerInput.Instance.jump.Held == false)
            {
                hasActiveSlowDown = true;//player dont hold the jump,jump stop

                if (isFixedJumping && Constants.PlayerClimbJumpFixedHeight - jumpHeight>0)
                {
                    float speed = 2f * (Constants.PlayerClimbJumpFixedHeight - jumpHeight) / (Constants.PlayerClimbJumpFixedTime - timer);
                    playerController.setRigidVelocity(new Vector2(playerController.getRigidVelocity().x, speed));
                    (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).stopJumpInTime(Constants.PlayerClimbJumpFixedTime - timer);

                }
                else
                {
                    (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).stopJumpInTime(Constants.JumpUpStopTime);
                }

            }
            if (!hasNormalSlowDown && !hasActiveSlowDown && timer > Constants.PlayerClimbJumpTotalTime - Constants.PlayerClimbJumpNormalSlowDownTime)
            {
                hasNormalSlowDown = true;
                (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).stopJumpInTime(Constants.PlayerClimbJumpNormalSlowDownTime);
            }

        }
    }

}
