using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Monsters")]
public class DetactChase : Action 
{
    private GameObject player;

    public float detactDistance = 8.0f;
    public float attactDistance = 3.0f;
    public float speed = 5.0f;

    public override void OnStart()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public override TaskStatus OnUpdate()
    {
        float distance = (this.transform.position - player.transform.position).magnitude;
        if (distance <= detactDistance)
        {
            if (distance > attactDistance)
            {
                this.GetComponent<Rigidbody2D>().velocity = (player.transform.position - this.transform.position).normalized * speed;
            } else
            {
                return TaskStatus.Success;
            }
        }
        return TaskStatus.Running;
    }
}
