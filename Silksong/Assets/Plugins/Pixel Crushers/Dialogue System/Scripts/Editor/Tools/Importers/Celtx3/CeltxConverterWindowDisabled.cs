#if !USE_CELTX3
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    public class Celtx3ImportWindowDisabled
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Celtx Gem 3...", false, 1)]
        public static void AskEnableCeltxSupport()
        {
            if (EditorUtility.DisplayDialog("Enable Celtx Gem 3 Support", "Celtx Gem 3 import support isn't enabled yet. Would you like to enable it? After clicking Enable, re-open the Celtx import window.\n\nNOTE: Celtx import requires Netwonsoft Json.NET. Please follow the Json.NET setup steps in the Celtx Importer Guide before enabling Celtx import.", "Enable", "Cancel"))
            {
                EditorTools.TryAddScriptingDefineSymbols("USE_CELTX3");
            }
        }
    }
}
#endif
