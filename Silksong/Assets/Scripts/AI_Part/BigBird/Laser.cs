using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class Laser: Action
    {


        public LaserController LaserController;

        private bool IsFire = false;
        public override void OnStart()
        {
            if (LaserController)
            {
                LaserController.FireLaser();
                
            }
        }

        public override void OnBehaviorComplete()
        {
            base.OnBehaviorComplete();
          
        }

      
    }
}