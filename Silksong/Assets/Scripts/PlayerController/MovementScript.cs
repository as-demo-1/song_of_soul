using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MovementScript
{
    //Put your static method here for reuse

    /// <summary>
    /// A function that moves a specified distance
    /// </summary>
    /// <param name="movement">Move distance of Vector2</param>
    /// <param name="nextMovement">Vector2 being moved</param>
    public static void Move(Vector2 movement, ref Vector2 nextMovement)
    {
        nextMovement += movement;
    }

    /// <summary>
    /// Teleport to the specified location
    /// </summary>
    /// <param name="position">The location to teleport</param>
    /// <param name="rigidbody2D">The rigidbody of the object being teleported</param>
    public static void Teleport(Vector2 position,  Rigidbody2D rigidbody2D)
    { 
        rigidbody2D.MovePosition(position);
    }
    /// <summary>
    /// The sprint function
    /// </summary>
    /// <param name="speed">The sprint speed</param>
    /// <param name="direction">The sprint direction</param>
    /// <param name="rigidbody2D">To sprint the rigidbody of the object</param>
    public static void Sprint(float speed, Vector2 direction, Rigidbody2D rigidbody2D)
    {
        Debug.Log("sprint");
        rigidbody2D.AddForce(new Vector2(direction.x  * speed * Time.deltaTime , direction.y));
    }
}
