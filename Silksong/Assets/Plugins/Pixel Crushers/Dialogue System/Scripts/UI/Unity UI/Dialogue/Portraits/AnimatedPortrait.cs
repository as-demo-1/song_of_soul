// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Specifies the animated portrait to use for this actor.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class AnimatedPortrait : MonoBehaviour
    {

        [Tooltip("Animator controller that runs this actor's animated portrait. It should animate an Image component, not a SpriteRenderer.")]
        public RuntimeAnimatorController animatorController;
    }

}