using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

    public class BossWake : Action
    {

        public Vector3 TargetLocation;
        private Rigidbody2D rb;
        
        public float Speed = 1;


        public override void OnStart()
        {
            rb = gameObject.GetComponentInChildren<Rigidbody2D>();
        }

        public override TaskStatus OnUpdate()
        {
            if (rb.transform.position == TargetLocation)
                return TaskStatus.Inactive;
            Vector2 res = Vector2.MoveTowards(rb.position, TargetLocation, Speed * Time.fixedDeltaTime);
            rb.MovePosition(res);
            return TaskStatus.Running;

            
        }
    }
