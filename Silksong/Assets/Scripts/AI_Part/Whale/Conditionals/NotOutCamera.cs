using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotOutCamera : BattleConditional
{
    private float left;
    private float right;
    private float up;
    private float down;
    public override void OnStart()
    {
        base.OnStart();
        left = WhaleBossManager.Instance.roomLeftDownPoint.x - WhaleBossManager.Instance.whaleOutCameraDistanceX;
        right = WhaleBossManager.Instance.roomRightUpPoint.x +WhaleBossManager.Instance.whaleOutCameraDistanceX;
        down = WhaleBossManager.Instance.roomLeftDownPoint.y - WhaleBossManager.Instance.whaleOutCameraDistanceY;
        up = WhaleBossManager.Instance.roomRightUpPoint.y + WhaleBossManager.Instance.whaleOutCameraDistanceY;
    }
    public override TaskStatus OnUpdate()
    {
        float x = transform.position.x;
        float y = transform.position.y;

        return (x < left || x > right || y < down || y > up) ? TaskStatus.Failure : TaskStatus.Success;

    }
}
