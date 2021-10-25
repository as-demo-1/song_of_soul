using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusBehaviour : StatusBehaviour
{
    PlayerController PlayerController { get; set; }
    public override void StatusEnterBehaviour(PlayerStatus playerStatus)
    {

    }
    public override void StatusActiveBehaviour(PlayerStatus playerStatus)
    {
        switch (playerStatus)
        {
            case PlayerStatus.None:
                break;
            case PlayerStatus.Idle:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.HorizontalMove(0.4f);
                break;
            case PlayerStatus.Run:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.FlipPlayer(1f);
                PlayerController.HorizontalMove(0.4f);
                break;
            case PlayerStatus.Jump:
                PlayerController.Jump();
                PlayerController.IsGrounded = false;
                PlayerController.FlipPlayer(1f);
                PlayerController.HorizontalMove(0.5f);
                break;
            case PlayerStatus.Fall:
                PlayerController.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.FlipPlayer(1f);
                PlayerController.HorizontalMove(0.5f);
                break;
            case PlayerStatus.Attack:
                break;
            default:
                break;
        }
    }
    public override void StatusExitBehaviour(PlayerStatus playerStatus)
    {

    }

    public PlayerStatusBehaviour(PlayerController playerController) => this.PlayerController = playerController;
}

public enum PlayerStatus
{
    None = 0,
    Idle = 10,
    Run = 20,
    Jump = 30,
    Fall = 40,
    Attack = 100,
}

public abstract class StatusBehaviour
{
    public abstract void StatusEnterBehaviour(PlayerStatus playerStatus);
    public abstract void StatusActiveBehaviour(PlayerStatus playerStatus);
    public abstract void StatusExitBehaviour(PlayerStatus playerStatus);
}
