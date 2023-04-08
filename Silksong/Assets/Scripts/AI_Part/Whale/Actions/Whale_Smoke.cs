using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whale_Smoke : BattleAction
{
    public override TaskStatus OnUpdate()
    {
        Vector2 leftDown = WhaleBossManager.Instance.roomLeftDownPoint;
        Vector2 rightUp = WhaleBossManager.Instance.roomRightUpPoint;

        float x = Random.Range(leftDown.x+5f,rightUp.x-5f);
        float y = Random.Range(leftDown.y+3f,rightUp.y-3f);
        GameObject smoke = GameObject.Instantiate(WhaleBossManager.Instance.smoke);
        smoke.transform.position=new Vector2(x,y);
        GameObject.Destroy(smoke,15f);

        WhaleBossManager.Instance.resetSmokeCdTimer();
        return TaskStatus.Success;
    }
}
