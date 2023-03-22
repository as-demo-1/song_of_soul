using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;


[TaskCategory("Battle")]
public abstract class BattleAction : Action
{
    protected SharedGameObject target;
    protected BattleAgent battleAgent;
    protected BehaviorTree behaviorTree;
    protected HpDamable damable;
    protected Animator animator;

    public override void OnStart()
    {
        target = (SharedGameObject)GetComponent<BehaviorTree>().GetVariable("Target");
        battleAgent = GetComponent<BattleAgent>();
        animator=transform.GetComponentInChildren<Animator>();
        behaviorTree = GetComponent<BehaviorTree>();
        damable = GetComponent<HpDamable>();
    }
}

