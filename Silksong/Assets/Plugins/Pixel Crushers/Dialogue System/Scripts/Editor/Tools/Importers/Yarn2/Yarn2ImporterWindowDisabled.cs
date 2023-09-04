#if !USE_YARN2

// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem.Yarn
{

    public class Yarn2ImporterWindow
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Yarn 2...", false, 1)]
        public static void AskEnableYarnSupport()
        {
            if (EditorUtility.DisplayDialog("Enable Yarn 2 Support", "Yarn 2 import support isn't enabled yet. Would you like to enable it?\n\nYarn Spinner 2 and the ANTLR4 Runtime MUST be installed in your project first, and you must configure the Dialogue System assembly definitions to reference it. If you haven't set this up yet, click Cancel and refer to the Yarn 2 Import manual section.\n\nAfter clicking Enable, re-open the Yarn 2 import window.", "Enable", "Cancel"))
            {
                EditorTools.TryAddScriptingDefineSymbols("USE_YARN2");
            }
        }
    }
}
#endif
