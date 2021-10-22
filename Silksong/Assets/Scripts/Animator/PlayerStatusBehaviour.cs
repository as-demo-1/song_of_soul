using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusBehaviour : StatusBehaviour
{
    public override void StatusEnterBehaviour(PlayerStatus playerStatus)
    {
        switch (playerStatus)
        {
            case PlayerStatus.None:
                break;
            case PlayerStatus.Idle:
                break;
            case PlayerStatus.Run:
                break;
            case PlayerStatus.Jump:
                PlayerController.Instance.Jump();
                break;
            case PlayerStatus.Fall:
                break;
            case PlayerStatus.Attack:
                break;
            default:
                break;
        }
    }
    public override void StatusActiveBehaviour(PlayerStatus playerStatus)
    {
        if (PlayerController.Instance.CheckIsGrounded())
            PlayerController.Instance.ResetJumpCount();
        switch (playerStatus)
        {
            case PlayerStatus.None:
                break;
            case PlayerStatus.Idle:
                break;
            case PlayerStatus.Run:
                PlayerController.Instance.HorizontalMove();
                if (PlayerInput.Instance.horizontal.Value == 1f & !PlayerController.Instance.playerFacingRight ||
                        PlayerInput.Instance.horizontal.Value == -1f & PlayerController.Instance.playerFacingRight)
                {
                    MovementScript.Flip(PlayerController.Instance.SpriteRenderer, ref PlayerController.Instance.playerFacingRight);
                    PlayerController.Instance.CharacterMoveAccel.SetAccelerationNormalizedTime(0f);
                }
                break;
            case PlayerStatus.Jump:
                PlayerController.Instance.HorizontalMove();
                if (PlayerInput.Instance.horizontal.Value == 1f & !PlayerController.Instance.playerFacingRight ||
                        PlayerInput.Instance.horizontal.Value == -1f & PlayerController.Instance.playerFacingRight)
                    MovementScript.Flip(PlayerController.Instance.SpriteRenderer, ref PlayerController.Instance.playerFacingRight);
                break;
            case PlayerStatus.Fall:
                PlayerController.Instance.HorizontalMove();
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
