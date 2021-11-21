using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 电梯内上下移动的开关
/// </summary>作者：青瓜
public class LfitMoveButton :Damable
{
    public bool goUp;//是否是向上按钮 否则向下
    public Lift lift;

    public override void takeDamage(DamagerBase damager)
    {
        base.takeDamage(damager);

        if(goUp)
        {
            int upFloor = (int)Mathf.Floor(lift.currentFloor) + 1;
            if (upFloor > lift.maxFloor)
                return;
            lift.setTargetFloor(upFloor);
        }
        else
        {
            int downFloor = (int)Mathf.Ceil(lift.currentFloor) - 1;
            if (downFloor <=0)
                return;
            lift.setTargetFloor(downFloor);
        }
    }
}
