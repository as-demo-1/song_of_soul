using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class CheckHealth : Conditional
    {
        private Boss_BigBird BirdController;
        private GameObject Player;
        private float Health = 50;
        public override void OnStart()
        {
            Player = GameObject.FindGameObjectWithTag("Player");
           // BirdController = Player.GetComponentInChildren<Boss_BigBird>();
            
        }

        public override TaskStatus OnUpdate()
        {
            if (Health > 100)
                return TaskStatus.Success;
            else
            {
                return TaskStatus.Failure;
                
            }
        }
    }
}