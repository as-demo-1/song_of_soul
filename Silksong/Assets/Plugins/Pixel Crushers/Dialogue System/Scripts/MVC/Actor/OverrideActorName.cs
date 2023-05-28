// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// OverrideActorName was renamed to DialogueActor. This wrapper helps maintain
    /// compatibility with code that references OverrideActorName.
    /// </summary>
    [AddComponentMenu("")] // No menu. Just for compatibility.
    public class OverrideActorName : DialogueActor
    {
        // For compatibility with legacy code:

        public string overrideName
        {
            get { return actor; }
            set { actor = value; }
        }

        public string internalName
        {
            get { return persistentDataName; }
            set { persistentDataName = value; }
        }

        public static string GetInternalName(Transform t)
        {
            return DialogueActor.GetPersistentDataName(t);
        }
    }

}
