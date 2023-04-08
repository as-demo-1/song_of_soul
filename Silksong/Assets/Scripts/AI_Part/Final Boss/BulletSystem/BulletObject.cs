using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Create BulletAsset")]
public class BulletObject : ScriptableObject

{
    [Header("initial config")]
    public float lifeCycle = 5;
    public float linearVelocity = 0;
    public float acceleration = 0;
    public float angularVelocity = 0;
    public float angularAcceleration = 0;
    public float maxVelocity = int.MaxValue;

    [Header("Sender")]
    public float initRotation = 0;
    public float senderAngularVelocity = 0;
    public float senderMaxAngularVelocity = int.MaxValue;
    public float senderAcceleration = 0;
    public int count = 0;
    public float lineAngle = 30;
    public float interval = 0.1f;

    [Header("prefab")]
    public GameObject prefab;
}
