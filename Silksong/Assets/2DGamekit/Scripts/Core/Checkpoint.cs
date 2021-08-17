using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Checkpoint : MonoBehaviour
    {
        public bool respawnFacingLeft;

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true; 
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerCharacter c = collision.GetComponent<PlayerCharacter>();
            if(c != null)
            {
                c.SetChekpoint(this);
            }
        }
    }
}