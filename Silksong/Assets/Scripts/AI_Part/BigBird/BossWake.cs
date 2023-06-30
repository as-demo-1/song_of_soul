using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

    public class BossWake : Action
    {

        public Vector3 TargetLocation;
        private Rigidbody2D rb;
        
        public float Speed = 1;// 这个speed像是后续加入的变量

        public override void OnStart()
        {
            rb = gameObject.GetComponentInChildren<Rigidbody2D>(); // 在开始声明了刚体
        }

        public override TaskStatus OnUpdate()
        {
            if (rb.transform.position == TargetLocation)
                return TaskStatus.Inactive;
            Vector2 res = Vector2.MoveTowards(rb.position, TargetLocation, Speed * Time.fixedDeltaTime);
            rb.MovePosition(res);
            return TaskStatus.Running;
        /* 上文简单理解为如果boss刚体位置在选定范围的话taskstatus改为不激活
         * 而如果位置非所需位置则进行激活并且冲向目标位置，这个目标位置后文应该会同步过来，我猜测是玩家位置
         * 在位移之后taskstatus激活
         * 这东西好像是行为树运行，结合脚本名认为是boss触发后的行为，而决定boss是否激发的核心就在这个TargetLocation变量上
        */
            
        }
    }
