using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovementScript
{
    //Put your static method here for reuse

    /// <summary>
    /// �ƶ��������ڴ����position�Ļ����ϸı�λ��
    /// </summary>
    /// <param name="movement">�ƶ���Vector2</param>
    /// <param name="nextMovement">���ı��Vector2</param>
    public static void Move(Vector2 movement, ref Vector2 nextMovement)
    {
        nextMovement += movement;
    }

    /// <summary>
    /// ���ͺ���������ֱ�Ӵ��͵�ָ����position
    /// </summary>
    /// <param name="position">Ҫ���͵�λ��</param>
    /// <param name="rigidbody2D">�����͵�����ĸ���</param>
    public static void Teleport(Vector2 position,  Rigidbody2D rigidbody2D)
    { 
        rigidbody2D.MovePosition(position);
    }
    /// <summary>
    /// ��̺���
    /// </summary>
    /// <param name="speed">��̵��ٶȣ���̵ľ����ɳ���ٶȾ���</param>
    /// <param name="direction">��̵ķ���</param>
    /// <param name="rigidbody2D">Ҫ���г������ĸ���</param>
    public static void Sprint(float speed, Vector2 direction, Rigidbody2D rigidbody2D)
    {
        rigidbody2D.AddForce(new Vector2(direction.x  * speed * Time.deltaTime , direction.y));
    }
}
