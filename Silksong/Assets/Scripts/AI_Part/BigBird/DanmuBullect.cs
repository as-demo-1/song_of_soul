using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanmuBullect : MonoBehaviour
{
    public float LinearVelocity = 0;

    public float Acceleration = 0;

    public float Deceleration = 0;

    public float AngularVelocity = 0;

    public float AngularAccleration = 0;

    public float MaxVelocity = int.MaxValue;

    public float LifeCycle = 5f;

    private void FixedUpdate()
    {
        LinearVelocity = Mathf.Clamp(LinearVelocity + Acceleration * Time.fixedDeltaTime, -MaxVelocity, MaxVelocity);
        AngularVelocity += AngularAccleration * Time.fixedDeltaTime;
        
        transform.Translate(Vector2.right * (LinearVelocity * Time.fixedDeltaTime));
        
        transform.rotation *= Quaternion.Euler(new Vector3(0,0,1) * (AngularVelocity * Time.fixedDeltaTime));

        
        
        LifeCycle -= Time.fixedDeltaTime;
        if(LifeCycle <= 0)
            Destroy(gameObject);

    }
}
