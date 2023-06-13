#if !USE_CELTX
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    public class CeltxImportWindowDisabled
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Celtx GVR 2...", false, 1)]
        public static void AskEnableCeltxSupport()
        {
            if (EditorUtility.DisplayDialog("Enable Celtx GVR 2 Support", "Celtx GVR 2 import support isn't enabled yet. Would you like to enable it? After clicking Enable, re-open the Celtx import window.\n\nNOTE: Celtx import requires Netwonsoft Json.NET. Please follow the Json.NET setup steps in the Celtx Importer Guide before enabling Celtx import.", "Enable", "Cancel"))
            {
                EditorTools.TryAddScriptingDefineSymbols("USE_CELTX");
            }
        }
    }
}
#endif
