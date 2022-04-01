using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearPlatformBorderTrigger :EnemyFSMBaseTrigger
{
    public PlatformBorderCheckDir checkDir;
    public bool returnFalseWhenBorder;
    public override void InitTrigger(EnemyFSMManager fsm_Manager)
    {
        base.InitTrigger(fsm_Manager);
    }


    public override bool IsTriggerReachInUpdate(EnemyFSMManager fsm_Manager)
    {
        Vector2 dir=Vector2.zero;
        switch (checkDir)
        {
            case PlatformBorderCheckDir.speed:
                {
                    dir = fsm_Manager.rigidbody2d.velocity;
                    break;
                }
            case PlatformBorderCheckDir.target:
                {
                    dir = fsm_Manager.getTargetDir();
                    break;
                }
        }

        return fsm_Manager.nearPlatformBoundary(dir)? !returnFalseWhenBorder:returnFalseWhenBorder;
    }
}

public enum PlatformBorderCheckDir
{ 
    speed,
    target,

}

