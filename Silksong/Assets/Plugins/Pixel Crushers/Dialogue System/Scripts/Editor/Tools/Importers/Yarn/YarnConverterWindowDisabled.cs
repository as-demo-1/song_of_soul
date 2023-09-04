#if !USE_YARN
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem.Yarn
{

    public class YarnConverterWindowDisabled
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Yarn 1...", false, 1)]
        public static void AskEnableYarnSupport()
        {
            if (EditorUtility.DisplayDialog("Enable Yarn Support", "Yarn 1 import support isn't enabled yet. Would you like to enable it? After clicking Enable, re-open the Yarn import window.", "Enable", "Cancel"))
            {
                EditorTools.TryAddScriptingDefineSymbols("USE_YARN");
            }
        }
    }
}
#endif
