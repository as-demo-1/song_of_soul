using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatesBehaviour : StatesBehaviour
{
    PlayerController PlayerController { get; set; }
    public override void StatesEnterBehaviour(PlayerState playerStates)
    {

    }
    public override void StatesActiveBehaviour(PlayerState playerStates)
    {
        switch (playerStates)
        {
            case PlayerState.None:
                break;
            case PlayerState.Idle:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.HorizontalMove(0.4f);
                break;
            case PlayerState.Run:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.FlipPlayer(1f);
                PlayerController.HorizontalMove(0.4f);
                break;
            case PlayerState.Jump:
                PlayerController.Jump();
                PlayerController.IsGrounded = false;
                PlayerController.FlipPlayer(1f);
                PlayerController.HorizontalMove(0.5f);
                break;
            case PlayerState.Fall:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.FlipPlayer(1f);
                PlayerController.HorizontalMove(0.5f);
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
