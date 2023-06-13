// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.Wrappers
{

    [CreateAssetMenu(menuName = "Pixel Crushers/Common/UI/Localized Fonts")]
    /// <summary>
    /// This wrapper for PixelCrushers.LocalizedFonts keeps references intact if you
    /// switch between the compiled assembly and source code versions of the original
    /// class.
    /// </summary>
    public class LocalizedFonts : PixelCrushers.LocalizedFonts
    {
    }

}
