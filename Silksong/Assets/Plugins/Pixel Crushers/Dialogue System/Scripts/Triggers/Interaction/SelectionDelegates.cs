// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine.Events;
using System;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This delegate is called when a selector targets a usable object. Example uses:
    /// - Update a HUD with the usable object's info
    /// - Make the usable object play an animation
    /// </summary>
    public delegate void SelectedUsableObjectDelegate(Usable usable);

    /// <summary>
    /// This delegate is called when a selector un-targets a usable object.
    /// </summary>
    public delegate void DeselectedUsableObjectDelegate(Usable usable);

    /// <summary>
    /// UnityEvent that has a Usable parameter.
    /// </summary>
    [Serializable]
    public class UsableUnityEvent : UnityEvent<Usable> { }

}
