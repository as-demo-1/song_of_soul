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

    Plunge = 130,
    ClimbIdle=140,
    ClimbJump=150,
    Sing=160,
    HeartSword=170,

    ToCat = 200,
    CatIdle = 210,
    ToHuman = 220,
    CatToHumanExtraJump = 230,

    WaterIdle = 300,
    Swim=310,
    Dive=320,
    WatreSprint=330,
    IntoWater=340,
    FloatUp=350,
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
    public Dictionary<EPlayerState, PlayerAction> StateActionsDic = new Dictionary<EPlayerState, PlayerAction>();
    public PlayerController playerController;

    public void init()
    {
        StateActionsDic.Add(EPlayerState.None, new NoneAction(playerController));
        StateActionsDic.Add(EPlayerState.Idle, new PlayerIdle(playerController));
        StateActionsDic.Add(EPlayerState.Run, new PlayerRun(playerController));
        StateActionsDic.Add(EPlayerState.NormalAttack, new PlayerNormalAttack(playerController));
        StateActionsDic.Add(EPlayerState.Heal, new PlayerHeal(playerController));
        StateActionsDic.Add(EPlayerState.Hurt, new PlayerHurt(playerController));
        StateActionsDic.Add(EPlayerState.Jump, new PlayerJump(playerController));
        StateActionsDic.Add(EPlayerState.Fall, new PlayerFall(playerController));
        StateActionsDic.Add(EPlayerState.Sing, new PlayerSing(playerController));
        StateActionsDic.Add(EPlayerState.Plunge, new PlayerPlunge(playerController));
        StateActionsDic.Add(EPlayerState.ClimbIdle, new PlayerClimbIdle(playerController));
        StateActionsDic.Add(EPlayerState.ClimbJump, new PlayerClimbJump(playerController));
        StateActionsDic.Add(EPlayerState.Sprint, new PlayerSprint(playerController));
        StateActionsDic.Add(EPlayerState.BreakMoon, new PlayerBreakMoon(playerController));
        StateActionsDic.Add(EPlayerState.HeartSword, new PlayerHeartSword(playerController));

        StateActionsDic.Add(EPlayerState.Dive, new PlayerDive(playerController));
        StateActionsDic.Add(EPlayerState.WatreSprint, new PlayerSprintInWater(playerController));
        StateActionsDic.Add(EPlayerState.WaterIdle, new PlayerWaterIdle(playerController));
        StateActionsDic.Add(EPlayerState.IntoWater, new PlayerIntoWater(playerController));
        StateActionsDic.Add(EPlayerState.FloatUp, new PlayerFloatUp(playerController));
        StateActionsDic.Add(EPlayerState.Swim, new PlayerSwim(playerController));

        StateActionsDic.Add(EPlayerState.CastSkill, new PlayerCastSkill(playerController));


        StateActionsDic.Add(EPlayerState.CatIdle, new PlayerCatIdle(playerController));
        StateActionsDic.Add(EPlayerState.ToCat, new PlayerToCat(playerController));
        StateActionsDic.Add(EPlayerState.ToHuman, new PlayerToHuman(playerController));
        StateActionsDic.Add(EPlayerState.CatToHumanExtraJump, new PlayerCatToHumanExtraJump(playerController));
        
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


    public class NoneAction:PlayerAction
    {
        public NoneAction(PlayerController playerController) : base(playerController) { }
        public override void StateEnd(EPlayerState newState)
        {
            base.StateEnd(newState);
        }

        public override void StateStart(EPlayerState oldState)
        {
            base.StateStart(oldState);
        }

        public override void StateUpdate()
        {
            base.StateUpdate();
        }
    }


}













 
