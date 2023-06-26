using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class EyeLaser : Action
    {

        public LaserLRController Controller;
        public override void OnStart()
        {
            Controller.DoFire();
        }
    }
}