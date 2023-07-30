using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Monsters")]

public class PlayerDistance : Conditional
{
    public float setDistance = 5.0f;
    public enum DistanceType
    {
        Circle,
        UpperSemi
    }
    public DistanceType distanceType;
    private GameObject player;
    public override void OnAwake()
    {
        
    }

    public override void OnStart()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public override TaskStatus OnUpdate()
    {
        if (distanceType == DistanceType.UpperSemi) {
            if (player.transform.position.y < this.transform.position.y)
            {
                return TaskStatus.Failure;
            }
        }
        float distance = (this.transform.position - player.transform.position).magnitude;
        if (distance < setDistance)
        {
            return TaskStatus.Success;
        } else
        {
            return TaskStatus.Failure;
        }
    }
}
