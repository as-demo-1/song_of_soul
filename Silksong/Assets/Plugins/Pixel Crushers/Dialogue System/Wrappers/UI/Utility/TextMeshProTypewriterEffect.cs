// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.Wrappers
{

#if TMP_PRESENT

    /// <summary>
    /// This wrapper class keeps references intact if you switch between the 
    /// compiled assembly and source code versions of the original class.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Dialogue System/UI/TextMesh Pro/Effects/TextMesh Pro Typewriter Effect")]
    [DisallowMultipleComponent]
    public class TextMeshProTypewriterEffect : PixelCrushers.DialogueSystem.TextMeshProTypewriterEffect
    {
    }

#else

    [HelpURL("https://pixelcrushers.com/dialogue_system/manual2x/html/dialogue_u_is.html#dialogueUITypewriterEffect")]
    [AddComponentMenu("")]
    public class TextMeshProTypewriterEffect : PixelCrushers.DialogueSystem.TextMeshProTypewriterEffect
    {
        private void Reset()
        {
            Debug.LogWarning("Support for " + GetType().Name + " must be enabled using Tools > Pixel Crushers > Dialogue System > Welcome Window.", this);
        }
    }

#endif
}
