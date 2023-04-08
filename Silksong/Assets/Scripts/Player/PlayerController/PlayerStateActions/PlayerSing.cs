using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSing : PlayerAction
{
    public PlayerSing(PlayerController playerController) : base(playerController) { }
    private float singTotalTime;
    private float singimer;
    public override void StateStart(EPlayerState oldState)
    {
        singTotalTime = Constants.PlayerBaseHealTime;
        singimer = 0;
    }
    public override void StateUpdate()
    {
        playerController.CheckHorizontalMove(0.4f);
        singProcess();
    }
    public void singProcess()
    {
        singimer += Time.deltaTime;
        float rate = singimer / singTotalTime;
        if (rate >= 1)
        {

        }
    }

}
