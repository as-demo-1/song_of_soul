using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class FlyUp :Action
    {
        public float Speed = 5;
        private Rigidbody2D body;

        public override void OnStart()
        {
            body = gameObject.GetComponentInChildren<Rigidbody2D>();
        }

        public override TaskStatus OnUpdate()
        {
            Vector2 targetup = new Vector2(body.position.x, -2f);
            Vector2 resup =  Vector2.MoveTowards(body.position, targetup, Speed * Time.fixedDeltaTime);
            body.MovePosition(resup);
            Debug.Log("1");
            if (body.position.y == -2f)
            { return TaskStatus.Inactive;}

            return TaskStatus.Running;
        }
    }
}