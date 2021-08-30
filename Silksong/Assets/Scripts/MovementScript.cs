using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovementScript
{
    //Put your static method here for reuse

    /// <summary>
    /// 移动函数，在传入的position的基础上改变位移
    /// </summary>
    /// <param name="movement">移动的Vector2</param>
    /// <param name="nextMovement">被改变的Vector2</param>
    public static void Move(Vector2 movement, ref Vector2 nextMovement)
    {
        nextMovement += movement;
    }

    /// <summary>
    /// 传送函数将物体直接传送到指定的position
    /// </summary>
    /// <param name="position">要传送的位置</param>
    /// <param name="rigidbody2D">被传送的物体的刚体</param>
    public static void Teleport(Vector2 position,  Rigidbody2D rigidbody2D)
    { 
        rigidbody2D.MovePosition(position);
    }
    /// <summary>
    /// 冲刺函数
    /// </summary>
    /// <param name="speed">冲刺的速度，冲刺的距离由冲刺速度决定</param>
    /// <param name="direction">冲刺的方向</param>
    /// <param name="rigidbody2D">要进行冲刺物体的刚体</param>
    public static void Sprint(float speed, Vector2 direction, Rigidbody2D rigidbody2D)
    {
        rigidbody2D.AddForce(new Vector2(direction.x  * speed * Time.deltaTime , direction.y));
    }
}
