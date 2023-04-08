using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToHuman : PlayerAction
{
    public PlayerToHuman(PlayerController playerController) : base(playerController) { }
    public override void StateStart(EPlayerState oldState)
    {
        playerController.playerToCat.colliderToHuman();
    }
    public override void StateUpdate()
    {
        playerController.CheckFlipPlayer(1f);
        playerController.CheckHorizontalMove(0.4f);
    }
    public override void StateEnd(EPlayerState newState)
    {
        (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).EndJump();
        playerController.playerToCat.extraJump();
        playerController.playerToCat.stateToHuman();
    }
}