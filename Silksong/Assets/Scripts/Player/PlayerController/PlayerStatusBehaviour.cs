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
            case EPlayerState.None:
                break;
            case EPlayerState.Idle:
                break;
            case EPlayerState.Run:
                break;
            case EPlayerState.Jump:
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
            case EPlayerState.None:
                break;
            case EPlayerState.Idle:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.Run:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.CheckFlipPlayer(1f);
                PlayerController.CheckHorizontalMove(0.4f);
                break;
            case EPlayerState.Jump:
                PlayerController.CheckJump();
                PlayerController.IsGrounded = false;
                PlayerController.CheckFlipPlayer(1f);
                PlayerController.CheckHorizontalMove(0.5f);
                break;
            case EPlayerState.Fall:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
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

//间隔一些留下相似但是不同的state方便管理，比如转身和idle，不要轻易改变现有的enum的值
public enum EPlayerState
{
    None = 0,
    Idle = 10,
    Run = 20,
    Jump = 30,
    Fall = 40,
    NormalAttack = 100,
}

public abstract class StatesBehaviour
{
    public abstract void StatesEnterBehaviour(EPlayerState playerStates);
    public abstract void StatesActiveBehaviour(EPlayerState playerStates);
    public abstract void StatesExitBehaviour(EPlayerState playerStates);
}
