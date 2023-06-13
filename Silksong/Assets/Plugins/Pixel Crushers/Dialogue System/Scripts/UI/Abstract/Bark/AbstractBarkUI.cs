// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This abstract class forms the base for many IBarkUI implementations.
    /// </summary>
    public abstract class AbstractBarkUI : MonoBehaviour, IBarkUI
    {

        public abstract bool isPlaying { get; }

        public abstract void Bark(Subtitle subtitle);

        public abstract void Hide();

    }

}
