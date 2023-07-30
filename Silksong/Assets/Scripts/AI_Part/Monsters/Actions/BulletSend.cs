using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Monsters")]

public class BulletSend : Action
{
    public Transform initialPosition;
    public int bulletNum;
    public float sendInterval;
    public float angle;

    /*

    public enum BulletType
    {
        Circle,
        Rectangle
    }
    public BulletType bulletType;

    public enum DurationType
    {
        Time,
        Collision
    }
    public float durationTime;

    public enum BulletTrail
    {
        Horizon,
        HorizonGravity,
        Parabola
    }
    public BulletTrail bulletTrail;
    public bool playerPanatrable;
    public bool groudPanatrable;

    public enum BulletEffect
    {
        Null,
        Trailing
    }
    public BulletEffect bulletEffect;
    public SpriteRenderer spriteRenderer;
    */

    private BulletSender bulletSender;


    public override void OnAwake()
    {
        bulletSender = initialPosition.gameObject.GetComponent<BulletSender>();
    }

    public override void OnStart()
    {
        
    }

    private int alreadySend = 0;
    private float currentTime = 0;

    public override TaskStatus OnUpdate()
    {
        if (alreadySend < bulletNum)
        {
            currentTime += Time.fixedDeltaTime;
            if (currentTime > sendInterval)
            {
                currentTime -=sendInterval;
                bulletSender.Send(angle);
                alreadySend += 1;  
            }
            return TaskStatus.Running;
        }
        return TaskStatus.Success;
    }
}
