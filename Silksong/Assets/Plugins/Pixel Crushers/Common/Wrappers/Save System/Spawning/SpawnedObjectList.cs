// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.Wrappers
{

    /// <summary>
    /// This wrapper for PixelCrushers.SpawnedObjectList keeps references intact if you switch 
    /// between the compiled assembly and source code versions of the original class.
    /// </summary>
    [CreateAssetMenu(fileName = "New Spawned Object List", menuName = "Pixel Crushers/Save System/Misc/Spawned Object List")]
    public class SpawnedObjectList : PixelCrushers.SpawnedObjectList
    {
    }

}
