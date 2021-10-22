using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusBehaviour : StatusBehaviour
{
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
                PlayerController.Instance.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.Instance.HorizontalMove();
                break;
            case PlayerStatus.Run:
                PlayerController.Instance.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.Instance.HorizontalMove();
                PlayerController.Instance.FlipPlayer(0f);
                break;
            case PlayerStatus.Jump:
                PlayerController.Instance.Jump();
                PlayerController.Instance.IsGrounded = false;
                PlayerController.Instance.HorizontalMove();
                PlayerController.Instance.FlipPlayer(1f);
                break;
            case PlayerStatus.Fall:
                PlayerController.Instance.CheckIsGroundedAndResetAirJumpCount();
                PlayerController.Instance.HorizontalMove();
                PlayerController.Instance.FlipPlayer(1f);
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
