using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Create BulletAsset")]
public class BulletObject : ScriptableObject

{
    [Header("控制对象的初始配置")]
    [Header("对象存在的时间（秒）在被销毁之前")]
    public float lifeCycle = 5;
    [Header("对象的初始线速度")]
    public float linearVelocity = 0;
    [Header("对象的加速度")]
    public float acceleration = 0;
    [Header("对象的初始角速度")]
    public float angularVelocity = 0;
    [Header("对象的角加速度")]
    public float angularAcceleration = 0;
    [Header("对象的最大速度")]
    public float maxVelocity = int.MaxValue; 

    [Header("发送器对象")]
    [Header("发送器对象的初始旋转")]
    public float initRotation = 0;
    [Header("发送器对象的初始角速度")]
    public float senderAngularVelocity = 0;
    [Header("发送器对象的最大角速度")]
    public float senderMaxAngularVelocity = int.MaxValue;
    [Header("发送器对象的加速度")]
    public float senderAcceleration = 0;
    [Header("生成子弹的数量")]
    public int count = 0;
    [Header("每个生成的子弹之间的角度")]
    public float lineAngle = 30;
    [Header("每个生成的子弹之间的时间")]
    public float interval = 0.1f;

    [Header("prefab")]
    public GameObject prefab;
}
