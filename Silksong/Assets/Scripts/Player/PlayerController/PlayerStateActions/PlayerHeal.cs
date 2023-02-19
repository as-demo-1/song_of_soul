using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerHeal : PlayerAction
{
    public PlayerHeal(PlayerController playerController) : base(playerController) { }
    private float healTotalTime;
    private float healTimer;
    private float healStartMana;
    public override void StateStart(EPlayerState oldState)
    {
        healTotalTime = playerController.playerCharacter.GetHealTime();
        healTimer = 0;
        healStartMana = playerController.playerCharacter.Mana;
    }
    public override void StateUpdate()
    {
        playerController.CheckHorizontalMove(0.4f);
        healProcess();
    }
    public void healProcess()
    {
        healTimer += Time.deltaTime;
        float rate = healTimer / healTotalTime;
        playerController.playerCharacter.Mana = (int)Mathf.Lerp(healStartMana, (healStartMana - Constants.playerHealCostMana), rate);
        if (rate >= 1)
        {
            playerController.playerCharacter.playerDamable.addCurrentHp(playerController.playerCharacter.GetHealValue(), null);
            playerController.PlayerAnimator.Play("Idle");
        }
    }

}