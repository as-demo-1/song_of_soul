using UnityEngine;
namespace BehaviorDesigner.Runtime.Tasks
{
    public class ClawAttack : Action
    {
        public float Speed = 5;
        private Animator animator;
        public Rigidbody2D body;
        private Transform Player;

        public override void OnAwake()
        {
            Player = GameObject.FindGameObjectWithTag("Player").transform;
            body = gameObject.GetComponentInChildren<Rigidbody2D>();
            animator = gameObject.GetComponentInChildren<Animator>();
            
        }

        public override void OnBehaviorRestart()
        {
          
        }

        public override TaskStatus OnUpdate()
        {

            Vector2 target = new Vector2(Player.position.x, Player.position.y - 1.0f);
            Vector2 res = Vector2.MoveTowards(body.position, target, Speed * Time.fixedDeltaTime);
            body.MovePosition(res);
            animator.SetTrigger("Attack_Claw");
            Debug.Log(body.position.y);
            if (body.position.y == -4.246644f)
            {

                Vector2 targetup = new Vector2(body.position.x, -2f);
                Vector2 resup = Vector2.MoveTowards(body.position, targetup, Speed * Time.fixedDeltaTime);
                body.MovePosition(resup);
                Debug.Log("1");
             
                return TaskStatus.Inactive;
              


            }

            return TaskStatus.Running;

        }
    }
}