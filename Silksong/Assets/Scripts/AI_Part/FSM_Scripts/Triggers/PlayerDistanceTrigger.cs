using UnityEngine;

public class PlayerDistanceTrigger : EnemyFSMBaseTrigger
{
    public float checkNearRadius;
    public float checkFarRadius;
    public bool addYBlockCheck;
    public bool returnTrueWhenYBlock;
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
        triggerType = EnemyTriggers.PlayerDistanceTrigger;
    }
    public override bool IsTriggerReach(EnemyFSMManager fsm_Manager)
    {
        Vector3 v = (fsm_Manager as EnemyFSMManager).getTargetDir();
        if(addYBlockCheck)
        {
            var rayHit = Physics2D.Raycast(fsm_Manager.transform.position, new Vector2(0, v.y), v.y, 1 << LayerMask.NameToLayer("Ground"));
            if (rayHit.distance > 0)
                return returnTrueWhenYBlock;
        }

        if (v.sqrMagnitude > checkNearRadius * checkNearRadius && v.sqrMagnitude < checkFarRadius * checkFarRadius)
        {
            return true;
        }
        else
        {
            return false;
        }


    }
}
