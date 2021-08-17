using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    //duplicate the given trasnform position, rotation and scale when this object get enable
    public class LocalTransformDuplicator : MonoBehaviour
    {
        public Transform targetTrasnform;

        private void OnEnable()
        {
            transform.localScale = targetTrasnform.localScale;
            transform.localRotation = targetTrasnform.localRotation;
            transform.localPosition = targetTrasnform.localPosition;
        }
    }
}