using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLiftFloor : Damable
{
    public SaveLift lift;
    public override void takeDamage(DamagerBase damager)
    {
        base.takeDamage(damager);
        lift.MoveToFloor(this);
    }
}
