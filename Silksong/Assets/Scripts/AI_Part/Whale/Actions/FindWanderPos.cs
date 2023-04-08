using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindWanderPos : BattleAction
{
    public SharedVector2 pos;
    public override void OnStart()
    {
        base.OnStart();
        float dis = WhaleBossManager.Instance.whaleOutCameraDistanceX;
        bool facingLeft = battleAgent.currentFacingLeft();
        Vector2 wanderPos=new Vector2(0,Random.Range(WhaleBossManager.Instance.roomLeftDownPoint.y, WhaleBossManager.Instance.roomRightUpPoint.y));
        if(facingLeft)
        {
            wanderPos.x = WhaleBossManager.Instance.roomLeftDownPoint.x - dis;
        }
        else
        {
            wanderPos.x = WhaleBossManager.Instance.roomRightUpPoint.x + dis;
        }
        pos.Value = wanderPos;
    }
    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
