using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public float linearVelocity = 0;
    public float acceleration = 0;
    public float angularVelocity = 0;
    public float angularAcceleration = 0;
    public float maxVelocity = int.MaxValue;
    public float lifeCycle = 5f;
    public BulletPool pool;

    private void FixedUpdate()
    {
        linearVelocity = Mathf.Clamp(linearVelocity + acceleration * Time.fixedDeltaTime, -maxVelocity, maxVelocity);
        angularVelocity += angularAcceleration * Time.fixedDeltaTime;

        transform.Translate(linearVelocity * Vector2.right * Time.fixedDeltaTime, Space.Self);
        transform.rotation *= Quaternion.Euler(new Vector3(0, 0, 1) * angularVelocity * Time.fixedDeltaTime);

        lifeCycle -= Time.fixedDeltaTime;
        if (lifeCycle <= 0)
        {
            pool.ReturnObjectToPool(this);
        }
    }
}
