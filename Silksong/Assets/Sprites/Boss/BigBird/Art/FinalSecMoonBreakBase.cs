using System;
using UnityEngine;
namespace Sprites.Boss.BigBird.Art
{
    public class FinalSecMoonBreakBase : MonoBehaviour
    {

        public GameObject MoonPrefabs;

        private void Start()
        {
            spawn();
        }

        public void spawn()
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == 1)
                {
                    GameObject.Instantiate(MoonPrefabs,transform.position + Vector3.left, Quaternion.identity);
                 
                }

                if (i == 2)
                {
                    GameObject.Instantiate(MoonPrefabs,transform.position + Vector3.up, Quaternion.identity);
                 
                }
               if(i == 3)
                   GameObject.Instantiate(MoonPrefabs,transform.position + Vector3.right, Quaternion.identity);
                   if(i == 4)
                       GameObject.Instantiate(MoonPrefabs,transform.position + Vector3.down, Quaternion.identity);
            }
        }
    }
}