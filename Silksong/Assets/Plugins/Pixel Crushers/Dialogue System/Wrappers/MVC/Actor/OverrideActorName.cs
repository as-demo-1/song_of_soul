// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.Wrappers
{

    /// <summary>
    /// OverrideActorName was renamed to DialogueActor. This wrapper helps maintain
    /// compatibility with code that references OverrideActorName.
    /// </summary>
    [AddComponentMenu("")] // No menu. Just for compatibility.
    public class OverrideActorName : DialogueActor
    {
    }

}
