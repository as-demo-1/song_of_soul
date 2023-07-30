using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Monsters")]
public class DashAttack : Action
{
    public float speed = 200f;
    public float distance;

    private GameObject player;
    private float moveTime;
    private float currentTime;
    private Vector2 vecSpeed;

    public override void OnAwake()
    {
        
    }

    public override void OnStart()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        vecSpeed = (player.transform.position - this.transform.position).normalized * speed * Time.deltaTime;
        moveTime = distance / vecSpeed.magnitude;
        currentTime = 0;
    }

    public override TaskStatus OnUpdate()
    {
        if (currentTime < moveTime)
        {
            this.GetComponent<Rigidbody2D>().velocity = vecSpeed;
            currentTime += Time.fixedDeltaTime;
            Debug.Log(currentTime);
            Debug.Log(moveTime);
            return TaskStatus.Running;
        }
        return TaskStatus.Success;
    }
}
