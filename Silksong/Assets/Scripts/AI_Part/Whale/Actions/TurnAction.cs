using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAction : BattleAction
{
    public override void OnStart()
    {
        base.OnStart();
        battleAgent.turnFace();
    }
    public override TaskStatus OnUpdate()
    {
        return base.OnUpdate();
    }
}
