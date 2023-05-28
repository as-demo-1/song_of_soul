// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections;

namespace PixelCrushers
{

    public abstract class SceneTransitionManager : MonoBehaviour
    {

        public virtual IEnumerator LeaveScene()
        {
            yield break;
        }

        public virtual IEnumerator EnterScene()
        {
            yield break;
        }

        /// <summary>
        /// Called while loading a scene. You can override this method to update a progress indicator.
        /// </summary>
        public virtual void OnLoading(float progress) { }

    }
}