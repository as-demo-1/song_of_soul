using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftEntrance : Trigger2DBase
{
    public SaveLift lift;
    public int EnterFloor;
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        lift.MoveToFloor(EnterFloor);
    }
}
