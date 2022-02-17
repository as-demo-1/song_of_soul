using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 电梯各个楼层的机关
/// </summary>作者：青瓜
public class LiftFloorGear : Damable
{
    public int floor;
    public Lift lift;

    [DisplayOnly]
    public float floorHeight;//世界坐标的地面y轴高度 用于对齐电梯
    void Start()
    {
        lift.setFloorGear(this);

        var rayHit = Physics2D.Raycast(transform.position, Vector2.down, 100, 1 << LayerMask.NameToLayer("Ground"));
        floorHeight = transform.position.y - rayHit.distance;
      
    }
    public override void takeDamage(DamagerBase damager)
    {
        base.takeDamage(damager);

        if (lift.currentFloor == floor)
            return;

        lift.setTargetFloor(floor);
    }

}
