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
        if (!GameManager.Instance.saveSystem.getLearnedSkill(EPlayerStatus.CanSing))
            return;
        
        singTotalTime = Constants.PlayerBaseHealTime;
        singimer = 0;
        PlayerController.Instance.sing.SetActive(true);
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
            //TODO:
        }
    }

    public override void StateEnd(EPlayerState newState)
    {
        base.StateEnd(newState);
        PlayerController.Instance.sing.SetActive(false);
    }
}
