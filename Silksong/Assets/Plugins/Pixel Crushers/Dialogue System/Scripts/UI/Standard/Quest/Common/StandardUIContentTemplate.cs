// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Abstract base class for UI content templates. We use this base class
    /// for instantiatable templates in order to support pooling.
    /// </summary>
    public abstract class StandardUIContentTemplate : MonoBehaviour
    {

        public virtual void Despawn()
        {
            Destroy(gameObject);
        }

    }
}
