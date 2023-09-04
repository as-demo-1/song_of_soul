#if !USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem.Articy
{

    public class ArticyConverterWindowDisabled
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/articy:draft...", false, 1)]
        public static void AskEnableArticySupport()
        {
            if (EditorUtility.DisplayDialog("Enable articy:draft Support", "articy:draft import support isn't enabled yet. Would you like to enable it? After clicking Enable, re-open the articy:draft import window.", "Enable", "Cancel"))
            {
                EditorTools.TryAddScriptingDefineSymbols("USE_ARTICY");
            }
        }
    }
}
#endif
