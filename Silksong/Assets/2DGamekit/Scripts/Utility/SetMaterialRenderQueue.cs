using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class SetMaterialRenderQueue : MonoBehaviour {

        public Material material;
        public int queueOverrideValue;

        void Start ()
        {
            material.renderQueue = queueOverrideValue;
        }
	
	
    }
}