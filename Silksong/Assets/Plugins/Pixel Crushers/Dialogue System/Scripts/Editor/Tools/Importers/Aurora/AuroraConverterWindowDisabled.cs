#if !USE_AURORA
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem.Aurora
{

    public class AuroraConverterWindowDisabled
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Aurora (Neverwinter Nights)...", false, 1)]
        public static void AskEnableAuroraSupport()
        {
            if (EditorUtility.DisplayDialog("Enable Aurora Support", "Aurora (Neverwinter Nights) import support isn't enabled yet. Would you like to enable it? After clicking Enable, re-open the Aurora import window.", "Enable", "Cancel"))
            {
                EditorTools.TryAddScriptingDefineSymbols("USE_AURORA");
            }
        }
    }
}
#endif
