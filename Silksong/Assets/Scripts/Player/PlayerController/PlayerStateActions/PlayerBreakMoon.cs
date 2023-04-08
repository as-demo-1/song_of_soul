using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBreakMoon : PlayerAction
{
    public PlayerBreakMoon(PlayerController playerController) : base(playerController) { }

    public BreakMoonPoint currentTarget;
    public List<BreakMoonPoint> availableTargets = new List<BreakMoonPoint>();

    private float timer;
    private float totalTime;
    private Vector2 totalDistance;
    private Vector2 startPosition;
    private Vector2 toMoonDistance;
    private bool hasBreakTheMoon;
    private bool prepareOver;

    public override void StateStart(EPlayerState oldState)
    {
        // Debug.Log("breakMoonStart");
        startPosition = playerController.transform.position;
        Vector2 target = currentTarget.transform.position;
        toMoonDistance = target - startPosition;
        Vector2 afterDistance = toMoonDistance.normalized * Constants.BreakMoonAfterDistance;
        totalDistance = afterDistance + toMoonDistance;
        /* Debug.Log(afterDistance);
         Debug.Log(toMoonDistance);
         Debug.Log(totalDistance);*/
        totalTime = totalDistance.magnitude / Constants.BreakMoonAvgSpeed;
        // Debug.Log(totalTime);

        timer = 0;
        PlayerAnimatorParamsMapping.SetControl(false);
        playerController.setRigidGravityScale(0);
        playerController.gravityLock = true;
        playerController.setRigidVelocity(Vector2.zero);
        hasBreakTheMoon = false;
        prepareOver = false;

        if (sameSide(currentTarget) == false)
        {
            playerController.Flip();
        }
    }

    public void findCurrentTarget()
    {

        if (availableTargets.Count < 1)
        {
            currentTarget = null;
            playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.HasBreakMoonPointParamHash, false);
            return;
        }

        BreakMoonPoint temp;
        temp = availableTargets[0];
        for (int i = 1; i < availableTargets.Count; i++)
        {
            BreakMoonPoint t = availableTargets[i];
            if (sameSide(t) == sameSide(temp))
            {
                if (Vector2.Distance(t.transform.position, playerController.transform.position)
                    < Vector2.Distance(temp.transform.position, playerController.transform.position))
                {
                    temp = t;
                }
            }
            else if (sameSide(t))
            {
                temp = t;
            }
        }
        if (currentTarget)
            currentTarget.unPicked();

        currentTarget = temp;
        playerController.PlayerAnimator.SetBool(playerController.animatorParamsMapping.HasBreakMoonPointParamHash, true);
        currentTarget.bePicked();

    }
    private bool sameSide(BreakMoonPoint b)//是否在玩家面朝的一侧
    {
        float x = b.transform.position.x - playerController.transform.position.x;
        bool result = playerController.playerInfo.playerFacingRight ? x > 0 : x < 0;
        return result;
    }

    public override void StateEnd(EPlayerState newState)
    {
        //  Debug.Log("end braeakMoon");
        playerController.gravityLock = false;
        playerController.setRigidGravityScaleToNormal();
    }
    public override void StateUpdate()
    {
        timer += Time.deltaTime;
        if (!prepareOver)
        {
            if (timer < Constants.BreakMoonPrePareTime) return;
            else
            {
                timer = 0;
                prepareOver = true;
            }
        }

        if (timer < totalTime)
        {
            float rate = playerController.playerInfo.breakMoonPositionCurve.Evaluate(timer / totalTime);
            //Debug.Log(rate);
            Vector2 s = totalDistance * rate;
            playerController.rigidMovePosition(startPosition + s);

            if (!hasBreakTheMoon && s.magnitude >= toMoonDistance.magnitude)//击碎月球
            {
                //Debug.Log("break");
                hasBreakTheMoon = true;
                PlayerAnimatorParamsMapping.SetControl(true);
                (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Sprint] as PlayerSprint).resetAirSprintLeftCount();
                (playerController.playerStatesBehaviour.StateActionsDic[EPlayerState.Jump] as PlayerJump).justResetDoubleJump();
                currentTarget.atBreakMoonPoint();
            }
        }
        else
        {
            // Debug.Log("start fall");
            playerController.gravityLock = false;
            playerController.setRigidGravityScaleToNormal();
        }
    }


}
