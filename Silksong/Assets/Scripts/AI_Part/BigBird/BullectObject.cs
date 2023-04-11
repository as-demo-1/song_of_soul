using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Creat Bullect")]
public class BullectObject :  ScriptableObject
{
    // Start is called before the first frame update
    [Header("初始化")] 
    
    public float LifeCycle = 0;
    
   public float LinearVelocity = 0;

   public float Acceleration = 0;

   public float Deceleration = 0;

   public float AngularVelocity = 0;

   public float AngularAccleration = 0;

   public float MaxVelocity = int.MaxValue;

   public float InitRotation = 0;

   public float SenderAngularVelocity = 0;

   public float SenderAcceleration = 0;

   public float SendMaxAngularv = int.MaxValue;

   public int Count = 0;

   public float LineAngle = 0;

   public float SendInterval = 0.1f;

   [Header("Prefab")] public GameObject Prefabs;




}
