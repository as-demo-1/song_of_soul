using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class ProjectileDanmu : Action
    {
        public PhysicalDanmuController Controller;

        public override void OnStart()
        {
            
            Controller.Fire();
        }

        public override TaskStatus OnUpdate()
        {

            return TaskStatus.Inactive;
        }
    }
    
    
    
   
}