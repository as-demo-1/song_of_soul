using UnityEngine;

public class PlayerDistanceTrigger : EnemyFSMBaseTrigger
{
    public float checkNearRadius;
    public float checkFarRadius;
    public bool isReturnTrueBetweenNearAndFar=true;
    public bool isRayDetect=false;
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        triggerType = EnemyTriggers.PlayerDistanceTrigger;
    }
    public override bool IsTriggerReachInUpdate(EnemyFSMManager fsm_Manager)
    {
        Vector3 v = fsm_Manager.getTargetDir();
        if (v.sqrMagnitude > checkNearRadius * checkNearRadius && v.sqrMagnitude < checkFarRadius * checkFarRadius)
        {
            if(isRayDetect)
            {
                var ray = Physics2D.Raycast(fsm_Manager.transform.position, v.normalized, 10000,LayerMask.NameToLayer("Ground"));
                if (ray.distance * ray.distance >= v.sqrMagnitude)
                    return isReturnTrueBetweenNearAndFar;
                else
                    return !isReturnTrueBetweenNearAndFar;
            }
            else
                return isReturnTrueBetweenNearAndFar;
        }
        else
        {
            return !isReturnTrueBetweenNearAndFar;
        }
    }
}
