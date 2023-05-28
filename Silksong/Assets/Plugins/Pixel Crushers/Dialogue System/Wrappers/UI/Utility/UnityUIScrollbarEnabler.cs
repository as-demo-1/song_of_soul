// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.Wrappers
{

    /// <summary>
    /// Enables a scrollbar only if the content is larger than the container. This component
    /// only shows or hides the scrollbar when the component is enabled or when it receives
    /// the OnContentChanged event. Now just points to the Common Library version.
    /// 
    /// Normally this would be marked deprecated, but since it's used in so many customer
    /// dialogue UIs it's kinder to just let it point to the Common Library version.
    /// </summary>
    [AddComponentMenu("")]
    public class UnityUIScrollbarEnabler : UIScrollbarEnabler
    {
    }

}
