using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Battle")]
public class BattleStart : Action
{
	public override void OnStart()
	{
		WhaleBossManager.Instance.BattleStart();
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}