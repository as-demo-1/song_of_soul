using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EPlayerNormalAttackStage
{
    First,
    Second,
    Thrid,

    UpAttack,
}
public class PlayerNormalAttack : PlayerAction
{
    public PlayerNormalAttack(PlayerController playerController) : base(playerController) { }

    private bool normalAttackReady;

    public EPlayerNormalAttackStage currentAttackStage;
    
    public bool NormalAttackReady
    {
        get { return normalAttackReady; }
        set
        {
            normalAttackReady = value;
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.NormalAttackReadyParamHash, value);
        }
    }
    public override void StateStart(EPlayerState oldState)
    {
        playerController.CheckFlipPlayer(1f);
       // playerController.setRigidVelocity(Vector2.zero);

        AnimatorStateInfo stateInfo=playerController.PlayerAnimator.GetCurrentAnimatorStateInfo(0);
        if(stateInfo.IsName("NormalAtk_1"))
        {
            currentAttackStage = EPlayerNormalAttackStage.First;
        }
        else if(stateInfo.IsName("NormalAtk_2"))
        {
            currentAttackStage = EPlayerNormalAttackStage.Second;
        }
        else if (stateInfo.IsName("NormalAtk_3"))
        {
            currentAttackStage = EPlayerNormalAttackStage.Thrid;
        }
        else
        {
            currentAttackStage = EPlayerNormalAttackStage.UpAttack;
        }

        NormalAttackReady = false;
        playerController.StartCoroutine(CdCount());
    }
    public override void StateUpdate()
    {
        playerController.CheckHorizontalMove(0.5f);
        playerController.checkMaxFallSpeed();
    }

    public IEnumerator CdCount()
    {
        yield return new WaitForSeconds(playerController.playerCharacter.getNormalAttackCd());
        NormalAttackReady = true;
    }
}
