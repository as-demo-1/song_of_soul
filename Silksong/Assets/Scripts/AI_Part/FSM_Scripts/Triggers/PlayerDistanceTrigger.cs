using UnityEngine;

public class PlayerDistanceTrigger : EnemyFSMBaseTrigger
{
    public float checkNearRadius;
    public float checkFarRadius;
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        triggerType = EnemyTriggers.PlayerDistanceTrigger;
    }
    public override bool IsTriggerReach(EnemyFSMManager fsm_Manager)
    {
        Vector3 v = (fsm_Manager as EnemyFSMManager).getTargetDir();
        if (v.sqrMagnitude > checkNearRadius * checkNearRadius && v.sqrMagnitude < checkFarRadius * checkFarRadius)
            return true;
        else
            return false;
    }
}
