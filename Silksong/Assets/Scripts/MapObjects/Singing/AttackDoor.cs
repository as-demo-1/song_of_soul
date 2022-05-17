using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDoor : DamageableBase
{
    public List<RisingDoor> risingDoors;
    public override void takeDamage(DamagerBase damager)
    {
        Debug.Log(damager.name);
        foreach (var door in risingDoors) door.Rising();
    }
}
