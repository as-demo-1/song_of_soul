using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSprint : PlayerAction
{
    public PlayerSprint(PlayerController playerController) : base(playerController) { }

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

    private int airSprintLeftCount;
    public int AirSprintLeftCount
    {
        get { return airSprintLeftCount; }
        set
        {
            airSprintLeftCount = value;
            playerController.PlayerAnimator.SetInteger(playerController.animatorParamsMapping.AirSprintLeftCountParamHash, airSprintLeftCount);
        }
    }

    public void resetAirSprintLeftCount()
    {
        AirSprintLeftCount = Constants.PlayerMaxAirSprintCount;
    }

    public override void StateStart(EPlayerState oldState)
    {
        SprintReady = false;
        playerController.setRigidGravityScale(0);
        if (oldState == EPlayerState.ClimbIdle)
        {
            playerController.Flip();
        }
        else
        {
            playerController.CheckFlipPlayer(0.5f);
        }

        int x = playerController.playerInfo.playerFacingRight ? 1 : -1;

        playerController.setRigidVelocity(new Vector2(playerController.playerInfo.sprintSpeed * x, 0));

        if (playerController.isGroundedBuffer() == false)
            AirSprintLeftCount--;

        playerController.gravityLock = true;

        playerController.dash.Play();
    }
    public override void StateEnd(EPlayerState newState)
    {
        playerController.gravityLock = false;

        playerController.setRigidGravityScaleToNormal();

        if(newState!=EPlayerState.IntoWater)
        playerController.setRigidVelocity(Vector2.zero);

        playerController.StartCoroutine(sprintCdCount());
    }

    public IEnumerator sprintCdCount()
    {
        yield return new WaitForSeconds(playerController.playerCharacter.GetSprintCd());
        SprintReady = true;
    }

}
