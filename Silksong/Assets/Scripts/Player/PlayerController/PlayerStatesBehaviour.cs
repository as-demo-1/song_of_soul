using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatesBehaviour : StatesBehaviour
{
    PlayerController PlayerController { get; set; }
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
                PlayerController.JumpStart();
                break;
            case EPlayerState.Fall:
                break;
            case EPlayerState.NormalAttack:
                PlayerController.CheckFlipPlayer(1f);
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
                PlayerController.CheckAddItem();
                PlayerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.Run:
               // PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.CheckAddItem();
                PlayerController.CheckFlipPlayer(1f);
                PlayerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.Jump:
               // PlayerController.IsGrounded = false;
                PlayerController.CheckFlipPlayer(1f);
                PlayerController.CheckHorizontalMove(0.5f);
                break;
            case EPlayerState.Fall:
                //PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.CheckFlipPlayer(1f);
                PlayerController.CheckHorizontalMove(0.5f);
                break;
            case EPlayerState.NormalAttack:
                PlayerController.CheckHorizontalMove(0.5f);
                break;
            default:
                break;
        }
    }
    public override void StatesExitBehaviour(EPlayerState playerStates)
    {

    }

    public PlayerStatesBehaviour(PlayerController playerController) => this.PlayerController = playerController;
}

//按<位>置状态，允许多个状态同时存在 不要轻易修改其值 修改后smb的枚举将丢失
public enum EPlayerState
{
    None = 0,
    Idle = 1,
    Run = 2,
    Jump = 4,
    Fall = 8,
    NormalAttack = 16,
}

public abstract class StatesBehaviour
{
    public abstract void StatesEnterBehaviour(EPlayerState playerStates);
    public abstract void StatesActiveBehaviour(EPlayerState playerStates);
    public abstract void StatesExitBehaviour(EPlayerState playerStates);
}
