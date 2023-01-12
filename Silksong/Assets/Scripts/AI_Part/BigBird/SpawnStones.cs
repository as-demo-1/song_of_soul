using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class SpawnStones :Action
    {
        public List<GameObject> Locations;

        public GameObject Prefabs;

        public override void OnStart()
        {
            base.OnStart();
            SpawnStone();
        }

        public void SpawnStone()
        {
            foreach (var stone in Locations)
            {
                GameObject.Instantiate(Prefabs, stone.transform.position, Quaternion.identity);
            }
        }
        
    }
}