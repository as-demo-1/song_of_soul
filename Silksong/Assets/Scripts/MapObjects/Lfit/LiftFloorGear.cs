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
    public BoxCollider2D floorCollider;//该电梯开关所在地面的碰撞体
    public float floorHeight;//地面高度 用于对齐电梯
    void Start()
    {
        lift.gears[floor - 1] = this;//按楼层顺序给对应电梯引用

        //根据collider设置地面高度
        float floorDistance = floorCollider.offset.y;
        floorDistance += (floorCollider.size.y / 2);
        floorDistance *= floorCollider.transform.lossyScale.y;
        floorHeight = floorCollider.transform.position.y + floorDistance;
    }
    public override void takeDamage(DamagerBase damager)
    {
        base.takeDamage(damager);

        if (lift.currentFloor == floor)
            return;

        lift.setTargetFloor(floor);
    }

}
