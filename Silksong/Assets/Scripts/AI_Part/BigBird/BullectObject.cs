using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Creat Bullect")]
public class BullectObject :  ScriptableObject
{
    // Start is called before the first frame update
    [Header("初始化")] 
    
    public float LifeCycle = 0; //该子弹对象的使用生命周期

    public float LinearVelocity = 0;// 该子弹对象的线性速度

    public float Acceleration = 0;// 该子弹对象的加速度

    public float Deceleration = 0;// 该子弹对象的减速度

    public float AngularVelocity = 0;// 该子弹对象的角速度，可能涉及转向

    public float AngularAccleration = 0;// 该子弹对象的角加速度

    public float MaxVelocity = int.MaxValue;// 该子弹对象的速度上限

    public float InitRotation = 0;// 该子弹对象的初始旋转

   public float SenderAngularVelocity = 0;// 该子弹对象的射出对象的角速度，可能涉及继承射出者角动量的问题

    public float SenderAcceleration = 0// 该子弹对象的射出对象的加速度，涉及对象速度及加速度的继承

   public float SendMaxAngularv = int.MaxValue;//限制了最大的可继承角度

   public int Count = 0;

   public float LineAngle = 0;

   public float SendInterval = 0.1f;

   [Header("Prefab")] public GameObject Prefabs;




}
