using System.Collections.Generic;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace BehaviorDesigner.Runtime.Tasks
{
    public class SpawnStones
    {
        public List<Vector3> Locations;

        public GameObject Prefabs;
        
        public void SpawnStone()
        {
            foreach (var stone in Locations)
            {
                GameObject.Instantiate(Prefabs, stone, Quaternion.identity);
            }
        }
        
    }
}