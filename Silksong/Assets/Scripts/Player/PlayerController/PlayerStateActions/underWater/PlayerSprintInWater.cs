using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSprintInWater : PlayerAction
{
    public PlayerSprintInWater(PlayerController playerController) : base(playerController) { }
    private bool sprintReady;
    public bool SprintReady
    {
        get { return sprintReady; }
        set
        {
            sprintReady = value;
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.SprintReadyParamHash, sprintReady);
        }
    }

    private bool outWater;

    Vector2 startPos;

    public override void StateStart(EPlayerState oldState)
    {
        SprintReady = false;
        playerController.setRigidGravityScale(0);
        playerController.CheckFlipPlayer();

        Vector2 dir=Vector2.zero;

        dir.x = playerController.playerInfo.playerFacingRight ? 1 : -1;

        if (PlayerInput.Instance.vertical.Value != 0)
        {
           int y = PlayerInput.Instance.vertical.Value > 0 ? 1 : -1;
            dir = new Vector2(0, y);
        }

        playerController.setRigidVelocity(dir*playerController.playerInfo.waterSprintSpeed);
        playerController.gravityLock = true;
        outWater = false;
        startPos = playerController.transform.position;
    }

    public override void StateUpdate()
    {
        if (playerController.IsUnderWater == false && !outWater)
        {
            outWater = true;
            Vector2 v = playerController.getRigidVelocity();
            playerController.setRigidVelocity(v * 1.5f);
        }
        else if(outWater)
        {
            float dis = Vector2.Distance(playerController.transform.position, startPos);
            if(dis>=(Constants.PlayerWaterSprintDistance+Constants.PlayWaterSprintPlusDis) || playerController.getRigidVelocity().magnitude<0.1f)
            {
                Debug.Log(dis);
                playerController.PlayerAnimator.SetTrigger(playerController.animatorParamsMapping.WaterSprintPlusEndParamHash);
            }
        }
    }

    public override void StateEnd(EPlayerState newState)
    {
      
        playerController.gravityLock = false;

        if (!playerController.IsUnderWater)
        {
            playerController.setRigidGravityScaleToNormal();
        }

        if(newState!=EPlayerState.IntoWater)
        {
            Vector2 v = new Vector2(0, playerController.getRigidVelocity().y);
            if (v.y > 0) v.y = 0;
            playerController.setRigidVelocity(v);//
        }


        playerController.StartCoroutine(sprintCdCount());
    }


    public IEnumerator sprintCdCount()
    {

        yield return new WaitForSeconds(Constants.SprintCd);
        SprintReady = true;
    }
}