using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Sender : MonoBehaviour
{
    public BullectObject BullectObject;

    private float CurrentAngle = 0;

    private float currentAngulatVelo = 0;

    private float currenttime = 0;

    private DanmuBullect bh;
    private void Awake()
    {
        CurrentAngle = BullectObject.InitRotation;
        currentAngulatVelo = BullectObject.AngularVelocity;
    }

    private void FixedUpdate()
    {
        currentAngulatVelo = Mathf.Clamp(currentAngulatVelo + BullectObject.AngularAccleration * Time.fixedDeltaTime,-BullectObject.SendMaxAngularv,BullectObject.SendMaxAngularv);
        CurrentAngle += currentAngulatVelo * Time.fixedDeltaTime;

        if (Mathf.Abs(CurrentAngle) > 720)
            CurrentAngle -= Mathf.Sign(CurrentAngle) * 360f;
         

        currenttime += Time.fixedDeltaTime;

        if (currenttime > BullectObject.SendInterval)
        {
            currenttime -= BullectObject.SendInterval;
            Send(CurrentAngle);
            
        }
        
        

    }

    public void Send(float angle)
    {
        GameObject go = Instantiate(BullectObject.Prefabs);

        go.transform.position = transform.position;
        
        go.transform.rotation = Quaternion.Euler(0,0,angle);

        bh =  go.AddComponent<DanmuBullect>();
        
        InitBullet(bh);
      
    }

    private void InitBullet(DanmuBullect info)
    {
        bh.LinearVelocity = BullectObject.LinearVelocity;
        bh.Acceleration = BullectObject.Acceleration;
        bh.AngularAccleration = BullectObject.AngularAccleration;
        bh.AngularVelocity = BullectObject.AngularVelocity;
        bh.LifeCycle = BullectObject.LifeCycle;
        bh.MaxVelocity = BullectObject.MaxVelocity;

    }
}
