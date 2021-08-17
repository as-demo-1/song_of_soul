using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class StartScreenSpriteOffsetter : MonoBehaviour {

        public float spriteOffset;
        Vector3 initialPosition;
        Vector3 newPosition;

        private void Start()
        {
            initialPosition = transform.position;
        }

        void Update ()
        {
            transform.position = new Vector3 ((initialPosition.x + (spriteOffset * Input.mousePosition.x)), (initialPosition.y + (spriteOffset * Input.mousePosition.y)), initialPosition.z);
        }
    }
}