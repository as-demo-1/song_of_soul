using UnityEngine;
namespace BehaviorDesigner.Runtime.Tasks
{
    public class Fly : BossAction
    {
        
        public float Speed = 5;
        private Animator animator;
        public Rigidbody2D body;
        
        public SharedVector3 basePos;
        public override void OnStart()
        {
            body = gameObject.GetComponentInChildren<Rigidbody2D>();
            animator = gameObject.GetComponentInChildren<Animator>();
            basePos = transform.position;
            if (!body)
            {
                Debug.Log("NULL");
            }
            animator.SetTrigger("Fly_Up");
            
            
        }
        
        public override TaskStatus OnUpdate()
        {
            CameraShakeManager.Instance.cameraShake(5f);
            Vector2 target = new Vector2(body.position.x, -2f);
            Vector2 res =  Vector2.MoveTowards(body.position, target, Speed * Time.fixedDeltaTime);
            body.MovePosition(res);
          
            Debug.Log(body.position.y);
           if (body.position.y == -2f)
           {
               animator.ResetTrigger("Fly_Up");
               return TaskStatus.Success;
           }
           return TaskStatus.Running;
        }
    }
}