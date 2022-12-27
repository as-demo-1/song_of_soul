using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class MonsterAction
{
    protected MonsterController monsterController;
    public MonsterAction(MonsterController monsterController)
    {
        this.monsterController = monsterController;
    }
    public virtual void StateStart(EMonsterState oldState)
    {

    }
    public virtual void StateUpdate()
    {

    }
    public virtual void StateEnd(EMonsterState newState)
    {

    }
}
public class MosterStatesBehaviour
{
    public Dictionary<EMonsterState,MonsterAction> StateActionsDic=new Dictionary<EMonsterState,MonsterAction>();
    public MonsterController monsterController;

    public void init()
    {
        StateActionsDic.Add(EMonsterState.Idle, new MonsterIdle(monsterController));
        StateActionsDic.Add(EMonsterState.Hurt, new MonsterHurt(monsterController));
        StateActionsDic.Add(EMonsterState.Attack, new MonsterAttack(monsterController));
        StateActionsDic.Add(EMonsterState.Meet, new MonsterMeet(monsterController));
        StateActionsDic.Add(EMonsterState.Move, new MonsterMove(monsterController));
        StateActionsDic.Add(EMonsterState.Turn, new MonsterTurn(monsterController));
    }

    public MosterStatesBehaviour(MonsterController monsterController)
    {
        this.monsterController = monsterController;
        init();
    }
    public void StatesEnterBehaviour(EMonsterState newState, EMonsterState oldState)
    {
        StateActionsDic[newState].StateStart(oldState);
    }
    public void StatesActiveBehaviour(EMonsterState playerState)
    {
        StateActionsDic[playerState].StateUpdate();
    }
    public void StatesExitBehaviour(EMonsterState exitState, EMonsterState newState)
    {
        StateActionsDic[exitState].StateEnd(newState);
    }
}



public class MonsterIdle : MonsterAction
{
    public MonsterIdle(MonsterController monsterController) : base(monsterController) { }

    public override void StateUpdate()
    {
        /*
        monsterController.CheckAddItem();
        playerController.CheckHorizontalMove(0.4f);
        */
    }
}

public class MonsterHurt : MonsterAction
{
    public MonsterHurt(MonsterController monsterController) : base(monsterController) { }

    public override void StateUpdate()
    {
        /*
        monsterController.CheckAddItem();
        playerController.CheckHorizontalMove(0.4f);
        */
    }
}

public class MonsterAttack : MonsterAction
{
    public MonsterAttack(MonsterController monsterController) : base(monsterController) { }

    public override void StateUpdate()
    {
        /*
        monsterController.CheckAddItem();
        playerController.CheckHorizontalMove(0.4f);
        */
    }
}
public class MonsterMeet : MonsterAction
{
    public MonsterMeet(MonsterController monsterController) : base(monsterController) { }

    public override void StateUpdate()
    {
        /*
        monsterController.CheckAddItem();
        playerController.CheckHorizontalMove(0.4f);
        */
    }
}
public class MonsterMove : MonsterAction
{
    public MonsterMove(MonsterController monsterController) : base(monsterController) { }

    public override void StateUpdate()
    {
        /*
        monsterController.CheckAddItem();
        playerController.CheckHorizontalMove(0.4f);
        */
    }
}
public class MonsterTurn : MonsterAction
{
    public MonsterTurn(MonsterController monsterController) : base(monsterController) { }

    public override void StateUpdate()
    {
        /*
        monsterController.CheckAddItem();
        playerController.CheckHorizontalMove(0.4f);
        */
    }
}





 
