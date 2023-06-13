#if !USE_ARCWEAVE

// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem.ArcweaveSupport
{

    public class ArcweaveImportWindowDisabled
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Arcweave...", false, 1)]
        public static void AskEnableArcweaveSupport()
        {
            if (EditorUtility.DisplayDialog("Enable Arcweave Support", "Arcweave import support isn't enabled yet. Would you like to enable it? After clicking Enable, re-open the Arcweave import window.\n\nIMPORTANT: Make sure Newtonsoft Json.NET is installed *before* enabled Arcweave import support.", "Enable", "Cancel"))
            {
                EditorTools.TryAddScriptingDefineSymbols("USE_ARCWEAVE");
            }
        }
    }
}

#endif
