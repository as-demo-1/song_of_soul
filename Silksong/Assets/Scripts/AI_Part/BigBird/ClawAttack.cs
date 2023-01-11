using System.Collections;
using TMPro.Examples;
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
        float time = 0;
        public override void OnAwake()
        {
            Player = GameObject.FindGameObjectWithTag("Player").transform;
            body = gameObject.GetComponentInChildren<Rigidbody2D>();
            animator = gameObject.GetComponentInChildren<Animator>();
            
        }
        

        IEnumerator Reset()
        {
            yield return new WaitForSeconds(2f);
          
            
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

               time += Time.deltaTime;

               StartCoroutine(Reset());


              // Debug.Log("time"+time);
               if (body.position.y == -4.246644f || time >= 2)
               {
                   animator.ResetTrigger("Attack_Claw");
                   Vector2 targetup = new Vector2(body.position.x, -2f);
                   Vector2 resup = Vector2.MoveTowards(body.position, targetup, Speed * Time.fixedDeltaTime);
                   body.MovePosition(resup);
                   // animator.ResetTrigger("Attack_Claw")
                   // ;
                   time = 0;
                   return TaskStatus.Success;
                   Debug.Log("1");

                //   return TaskStatus.Success;

               }
           

           return TaskStatus.Running;

        }
    }
}