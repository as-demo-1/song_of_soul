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
           Health = GetComponent<HpDamable>().CurrentHp;
        }
        
        


        public override TaskStatus OnUpdate()
        {
            Health = GetComponent<HpDamable>().CurrentHp;
            Debug.Log(Health);
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
/*检查是否击杀boss的确定脚本，在update中不断确认是否血量达到某个阈值，并对应的激活行为树*/