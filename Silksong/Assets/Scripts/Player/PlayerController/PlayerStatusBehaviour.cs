using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatesBehaviour : StatesBehaviour
{
    PlayerController PlayerController { get; set; }
    public override void StatesEnterBehaviour(PlayerState playerStates)
    {

    }
    //active为进入该state时第一帧开始，也就是没有把start从中分开
    public override void StatesActiveBehaviour(PlayerState playerStates)
    {
        switch (playerStates)
        {
            case PlayerState.None:
                break;
            case PlayerState.Idle:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.CheckHorizontalMove(0.4f);
                break;
            case PlayerState.Run:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.CheckFlipPlayer(1f);
                PlayerController.CheckHorizontalMove(0.4f);
                break;
            case PlayerState.Jump:
                PlayerController.CheckJump();
                PlayerController.IsGrounded = false;
                PlayerController.CheckFlipPlayer(1f);
                PlayerController.CheckHorizontalMove(0.5f);
                break;
            case PlayerState.Fall:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.CheckFlipPlayer(1f);
                PlayerController.CheckHorizontalMove(0.5f);
                break;
            case PlayerState.Attack:
                break;
            default:
                break;
        }
    }
    public override void StatesExitBehaviour(PlayerState playerStates)
    {

    }

    public PlayerStatesBehaviour(PlayerController playerController) => this.PlayerController = playerController;
}

//间隔一些留下相似但是不同的state方便管理，比如转身和idle，不要轻易改变现有的enum的值
public enum PlayerState
{
    None = 0,
    Idle = 10,
    Run = 20,
    Jump = 30,
    Fall = 40,
    Attack = 100,
}

public abstract class StatesBehaviour
{
    public abstract void StatesEnterBehaviour(PlayerState playerStates);
    public abstract void StatesActiveBehaviour(PlayerState playerStates);
    public abstract void StatesExitBehaviour(PlayerState playerStates);
}
