using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
[TaskCategory("Battle")]
public abstract class BattleConditional : Conditional
{
    protected SharedGameObject target;
    protected HpDamable damable;
    protected BattleAgent battleAgent;
    public override void OnStart()
    {
        damable = GetComponent<HpDamable>();
        battleAgent = GetComponent<BattleAgent>();
        target = (SharedGameObject)GetComponent<BehaviorTree>().GetVariable("Target");
    }
}
