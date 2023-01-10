using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftSceneTransition : SceneTransitionPoint
{
    public SaveLift lift;
    public int EnterFloor;
    public int ExitFloor;
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (lift.currentFloor == EnterFloor)
        {
            base.OnTriggerEnter2D(collision);
        }else
        {
            lift.MoveToFloor(ExitFloor);
        }
    }
}
