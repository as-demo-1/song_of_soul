using BehaviorDesigner.Runtime.Tasks.Unity.Math;
using DG.Tweening;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using UnityEngine;


namespace BehaviorDesigner.Runtime.Tasks
{
    public class Walk : BossAction
    {
        public float Speed = 1;
        private Transform Player;
        private Rigidbody2D rb;
        private Animator animator;
        public override void OnStart()
        {
            Player = GameObject.FindGameObjectWithTag("Player").transform;
            rb = gameObject.GetComponentInChildren<Rigidbody2D>();
            animator = gameObject.GetComponentInChildren<Animator>();
        }

     

        public override TaskStatus OnUpdate()
        {

            if (Mathf.Abs(rb.position.x - Player.position.x) <= 2.0f)
            {
                return TaskStatus.Success;
            }
            if (Player.position.x > rb.position.x)
            {
                animator.ResetTrigger("MoveRight");
                animator.SetTrigger("MoveLeft");
            }
            else if (Player.position.x == rb.position.x)
            {
                animator.SetTrigger("MoveLeft");
                animator.ResetTrigger("MoveRight");
               
            }
            else
            {
                animator.ResetTrigger("MoveLeft");
                animator.SetTrigger("MoveRight");
            }
        
            

            Vector2 target = new Vector2(Player.position.x, rb.position.y);
            Vector2 res = Vector2.MoveTowards(rb.position, target, Speed * Time.fixedDeltaTime);
            rb.MovePosition(res);

            return TaskStatus.Running;
        }
        
        
    }
}