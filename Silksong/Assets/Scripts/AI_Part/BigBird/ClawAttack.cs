using System.Collections;
using UnityEngine;
namespace BehaviorDesigner.Runtime.Tasks
{
    public class ClawAttack : Action
    {
        public float Speed = 5;
        private Animator animator;
        public Rigidbody2D body;
        private Transform Player;
        private bool IsFirstAttack = true;
        private bool reset = false;

        public override void OnAwake()
        {
            Player = GameObject.FindGameObjectWithTag("Player").transform;
            body = gameObject.GetComponentInChildren<Rigidbody2D>();
            animator = gameObject.GetComponentInChildren<Animator>();
            
        }

        IEnumerator Reset()
        {
            yield return new WaitForSeconds(1f);
            Vector2 targetup = new Vector2(body.position.x, -2f);
            Vector2 resup = Vector2.MoveTowards(body.position, targetup, Speed * Time.fixedDeltaTime);
            body.MovePosition(resup);
           // animator.ResetTrigger("Attack_Claw");
            IsFirstAttack = true;
            reset = true;
           
        }
    

        public override void OnBehaviorRestart()
        {
          
        }


        public override TaskStatus OnUpdate()
        {

            //if(reset)
           // return TaskStatus.Inactive;
            
            animator.SetTrigger("Attack_Claw");
                IsFirstAttack = false;
            Vector2 target = new Vector2(Player.position.x, Player.position.y - 1.0f);
            Vector2 res = Vector2.MoveTowards(body.position, target, Speed * Time.fixedDeltaTime);
            body.MovePosition(res);
           
          
            StartCoroutine(Reset());
            
            
            //Debug.Log(body.position.y);
            if (body.position.y == -4.246644f)
            {
                animator.ResetTrigger("Attack_Claw");

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