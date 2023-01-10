using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class Laser: Action
    {


        public LaserController LaserController;

        public override void OnStart()
        {
            if (LaserController)
            {
                LaserController.FireLaser();
                Debug.Log("1");
            }
        }
    }
}