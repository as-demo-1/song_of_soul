using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class CheckHealth : Conditional
    {
        private Boss_BigBird BirdController;
        private GameObject Player;
       public float Health = 71;
        public List<int> Stage;
        public int stageindex;
        public override void OnStart()
        {
            
            Player = GameObject.FindGameObjectWithTag("Player");
           // BirdController = Player.GetComponentInChildren<Boss_BigBird>();
           Health = GameObject.FindGameObjectWithTag("Boss").GetComponentInChildren<BigBirdController>().GetHealth();
        }
        
        


        public override TaskStatus OnUpdate()
        {
           // Health = GameObject.FindGameObjectWithTag("Boss").GetComponent<BigBirdController>().GetHealth();
            //Debug.Log(Health);
            if (Health > Stage[stageindex])
                return TaskStatus.Success;
            else
            {
                return TaskStatus.Failure;
            }
        }
    }
}