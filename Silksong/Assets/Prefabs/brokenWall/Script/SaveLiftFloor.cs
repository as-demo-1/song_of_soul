using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLiftFloor : Damable
{
    [DisplayOnly]
    public string guid;
    public SaveLift lift;
    private void OnValidate()
    {
        guid=GetComponent<GuidComponent>().GetGuid().ToString();
    }
    public override void takeDamage(DamagerBase damager)
    {
        base.takeDamage(damager);
        lift.MoveToFloor(this);
    }
}
