// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.Wrappers
{

    /// <summary>
    /// This wrapper class keeps references intact if you switch between the 
    /// compiled assembly and source code versions of the original class.
    /// </summary>
    [RequireComponent(typeof(TextMesh))]
    [AddComponentMenu("Pixel Crushers/Dialogue System/UI/Misc/Localize Text Mesh")]
    public class LocalizeTextMesh : PixelCrushers.DialogueSystem.LocalizeTextMesh
    {
    }

}
