using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    //Allow to bubble up a OnBecameVisible call. usefull to have parent object (without renderer)
    //be notified when a child become visible
    public class VisibleBubbleUp : MonoBehaviour
    {
        public System.Action<VisibleBubbleUp> objectBecameVisible;

        private void OnBecameVisible()
        {
            objectBecameVisible(this);
        }
    }
}