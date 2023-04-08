using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class SpawnStones :Action
    {
        //public List<GameObject> Locations;

        public GameObject Prefabs;
        public SharedVector3 basePos;
        public override void OnStart()
        {
            base.OnStart();
            SpawnStone();
        }

        public void SpawnStone()
        {
            for (int i = 0; i < 5; i++)
            {
                GameObject.Instantiate(Prefabs, new Vector3(
                    -20.0f + 5.0f*i + basePos.Value.x, 
                    basePos.Value.y, 
                    0.0f), Quaternion.identity);
            }

        }
        
    }
}