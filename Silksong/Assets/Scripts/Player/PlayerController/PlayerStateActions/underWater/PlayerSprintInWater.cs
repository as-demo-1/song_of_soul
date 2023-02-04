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

    public override void StateStart(EPlayerState oldState)
    {
        playerController.setRigidGravityScale(0);
        int x = playerController.playerInfo.playerFacingRight ? 1 : -1;
        int y = 0;
        if (PlayerInput.Instance.vertical.Value != 0)
        {
            y = PlayerInput.Instance.vertical.Value > 0 ? 1 : -1;
        }
        playerController.setRigidVelocity(new Vector2(playerController.playerInfo.sprintSpeed * x, playerController.playerInfo.sprintSpeed * y));
        playerController.gravityLock = true;
        playerController.IsUnderWater = false;
    }

    public override void StateEnd(EPlayerState newState)
    {
        Debug.Log(playerController.IsUnderWater);
        playerController.gravityLock = false;
        if (!playerController.IsUnderWater)
        {
            playerController.setRigidGravityScaleToNormal();
        }
        else
        {
          //  playerController.setRigidGravityScale(playerController.playerInfo.gravityUnderWater);
        }
        playerController.setRigidVelocity(Vector2.zero);
        playerController.StartCoroutine(sprintCdCount());
    }


    public IEnumerator sprintCdCount()
    {
        SprintReady = false;
        yield return new WaitForSeconds(Constants.SprintCd);
        SprintReady = true;
    }
}