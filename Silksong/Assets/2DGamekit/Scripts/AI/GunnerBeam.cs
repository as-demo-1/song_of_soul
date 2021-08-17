using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class GunnerBeam : MonoBehaviour
    {
        public Transform target;

        void Update()
        {
            transform.LookAt(target);
        }

    }
}