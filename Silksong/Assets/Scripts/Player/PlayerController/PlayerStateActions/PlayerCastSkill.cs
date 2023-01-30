using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCastSkill : PlayerAction
{

    public PlayerCastSkill(PlayerController playerController) : base(playerController)
    {
        //playerSkillManager = playerController.gameObject.GetComponent<PlayerSkillManager>();
    }

    //private PlayerSkillManager playerSkillManager;

    public override void StateStart(EPlayerState oldState)
    {
        //playerSkillManager.Cast();
    }



}
