using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 不要轻易修改其值 修改后smb的枚举将丢失
public enum EPlayerState
{
    None = 0,
    Idle = 10,
    Run = 20,
    Jump = 30,
    Fall = 40,
    NormalAttack = 50,
    Sprint = 60,
    BreakMoon = 70,
    Heal = 90,
    Hurt = 100,
    CastSkill = 110,
    Swim = 120,
    Plunge = 130,
    ClimbIdle=140,
    ClimbJump=150,

    ToCat = 200,
    CatIdle = 210,
    ToHuman = 220,
    CatToHumanExtraJump = 230,
    SprintInWater = 240,

    LookUp = 300,
    LookDown = 310,
}
public abstract class PlayerAction
{
    protected PlayerController playerController;
    public PlayerAction(PlayerController playerController)
    {
        this.playerController = playerController;
    }
    public virtual void StateStart(EPlayerState oldState)
    {

    }
    public virtual void StateUpdate()
    {

    }
    public virtual void StateEnd(EPlayerState newState)
    {

    }
}
public class PlayerStatesBehaviour
{
    public Dictionary<EPlayerState,PlayerAction> StateActionsDic=new Dictionary<EPlayerState, PlayerAction>();
    public PlayerController playerController;

    public void init()
    {
        StateActionsDic.Add(EPlayerState.Idle, new PlayerIdle(playerController));
        StateActionsDic.Add(EPlayerState.Run, new PlayerRun(playerController));
        StateActionsDic.Add(EPlayerState.NormalAttack, new PlayerNormalAttack(playerController));
        StateActionsDic.Add(EPlayerState.Heal, new PlayerHeal(playerController));
        StateActionsDic.Add(EPlayerState.Hurt, new PlayerHurt(playerController));
        StateActionsDic.Add(EPlayerState.Jump, new PlayerJump(playerController));
        StateActionsDic.Add(EPlayerState.Fall, new PlayerFall(playerController));

        StateActionsDic.Add(EPlayerState.Sprint, new PlayerSprint(playerController));
        StateActionsDic.Add(EPlayerState.BreakMoon, new PlayerBreakMoon(playerController));

        StateActionsDic.Add(EPlayerState.Swim, new PlayerSwim(playerController));
        StateActionsDic.Add(EPlayerState.SprintInWater, new PlayerSprintInWater(playerController));

        StateActionsDic.Add(EPlayerState.CastSkill, new PlayerCastSkill(playerController));

        StateActionsDic.Add(EPlayerState.Plunge, new PlayerPlunge(playerController));

        StateActionsDic.Add(EPlayerState.ClimbIdle, new PlayerClimbIdle(playerController));
        StateActionsDic.Add(EPlayerState.ClimbJump, new PlayerClimbJump(playerController));

        StateActionsDic.Add(EPlayerState.CatIdle, new PlayerCatIdle(playerController));
        StateActionsDic.Add(EPlayerState.ToCat, new PlayerToCat(playerController));
        StateActionsDic.Add(EPlayerState.ToHuman, new PlayerToHuman(playerController));
        StateActionsDic.Add(EPlayerState.CatToHumanExtraJump, new PlayerCatToHumanExtraJump(playerController));

        StateActionsDic.Add(EPlayerState.LookUp, new PlayerLookUp(playerController));
        StateActionsDic.Add(EPlayerState.LookDown, new PlayerLookDown(playerController));
    }

    public PlayerStatesBehaviour(PlayerController playerController)
    {
        this.playerController = playerController;
        init();
    }
    public void StatesEnterBehaviour(EPlayerState newState, EPlayerState oldState)
    {
        StateActionsDic[newState].StateStart(oldState);
    }
    public void StatesActiveBehaviour(EPlayerState playerState)
    {
        StateActionsDic[playerState].StateUpdate();
    }
    public void StatesExitBehaviour(EPlayerState exitState, EPlayerState newState)
    {
        StateActionsDic[exitState].StateEnd(newState);
    }


}





public class PlayerJump : PlayerAction
{
    public PlayerJump(PlayerController playerController) : base(playerController) { }

    private float jumpStartHeight;

    private int currentJumpCountLeft;
    public int CurrentJumpCountLeft
    {
        get { return currentJumpCountLeft; }
        set
        {
            currentJumpCountLeft = value;
            playerController.PlayerAnimator.SetInteger(playerController.animatorParamsMapping.JumpLeftCountParamHash, currentJumpCountLeft);
        }
    }

    public void resetJumpCount() => CurrentJumpCountLeft = playerController.playerInfo.getJumpCount();

    public void resetDoubleJump()
    {
        if (playerController.playerInfo.hasDoubleJump == false) return;
        CurrentJumpCountLeft = Constants.PlayerMaxDoubleJumpCount - Constants.PlayerMaxJumpCount;
    }

    public override void StateStart(EPlayerState oldState)
    {
        playerController.setRigidGravityScale(0);

        if (playerController.isGroundedBuffer() == false)//只有空中跳跃减跳跃次数，地上跳跃次数由IsGround set方法减去
            --CurrentJumpCountLeft;

        playerController.setRigidVelocity(new Vector2(playerController.getRigidVelocity().x, playerController.playerInfo.getJumpUpSpeed()));
        jumpStartHeight = playerController.transform.position.y;

        playerController.StopCoroutine(JumpUpCheck());
        playerController.StartCoroutine(JumpUpCheck());
    }

    public override void StateUpdate()
    {

        playerController.CheckFlipPlayer(1f);
        playerController.CheckHorizontalMove(0.5f);
    }

    public void EndJump()
    {
        playerController.StopCoroutine(JumpUpCheck());
        playerController.setRigidGravityScaleToNormal();
    }

    public IEnumerator JumpUpCheck()
    {
        bool hasQuickSlowDown = false;
        bool hasNormalSlowDown = false;

        float normalSlowDistance = 0.5f * playerController.playerInfo.getJumpUpSpeed() * Constants.JumpUpSlowDownTime;//s=0.5*velocity*time
        while (true)
        {
            yield return null;//每次update后循环一次
            //EPlayerState state = playerController.PlayerAnimatorStatesControl.CurrentPlayerState;
            if (playerController.getRigidVelocity().y < 0.01f)//跳跃上升过程结束
            {
                playerController.setRigidGravityScaleToNormal();
                break;
            }

            float jumpHeight = playerController.transform.position.y - jumpStartHeight;

            if (jumpHeight > Constants.PlayerJumpMinHeight - 0.5f)//达到最小高度后才能停下
            {

                if (hasQuickSlowDown == false && PlayerInput.Instance.jump.Held == false)//急刹
                {
                    hasQuickSlowDown = true;
                    stopJumpInTime(Constants.JumpUpStopTime);
                }
                if (!hasNormalSlowDown && !hasQuickSlowDown && jumpHeight > playerController.playerInfo.getJumpHeight() - normalSlowDistance)//
                {
                    hasNormalSlowDown = true;
                    stopJumpInTime(Constants.JumpUpSlowDownTime);
                }
            }
        }

    }

    public void stopJumpInTime(float stopTime)
    {
        float acce = playerController.getRigidVelocity().y / stopTime;
        float gScale = -acce / Physics2D.gravity.y;
        // Debug.Log(gScale);
        playerController.setRigidGravityScale(gScale);
    }

}

public class PlayerFall : PlayerAction
{

    public PlayerFall(PlayerController playerController) : base(playerController) { }
    public override void StateUpdate()
    {
        playerController.CheckFlipPlayer(1f);
        playerController.CheckHorizontalMove(0.5f);
        checkMaxFallSpeed();
    }

    public void checkMaxFallSpeed()
    {
        if (playerController.getRigidVelocity().y < -Constants.PlayerMaxFallSpeed)
        {
            playerController.setRigidVelocity(new Vector2(playerController.getRigidVelocity().x, -Constants.PlayerMaxFallSpeed));
        }
    }
}

public class PlayerIdle : PlayerAction
{
    public PlayerIdle(PlayerController playerController) : base(playerController) { }

    public override void StateUpdate()
    {
        playerController.CheckAddItem();
        playerController.CheckHorizontalMove(0.4f);
    }
}

public class PlayerRun : PlayerAction
{
    public PlayerRun(PlayerController playerController) : base(playerController) { }
    public override void StateStart(EPlayerState oldState)
    {
        playerController.playerToCat.catMoveStart();
    }
    public override void StateUpdate()
    {

        playerController.CheckAddItem();
        playerController.CheckFlipPlayer(1f);
        playerController.CheckHorizontalMove(0.4f);
        playerController.playerToCat.moveDistanceCount();
    }
}




 
