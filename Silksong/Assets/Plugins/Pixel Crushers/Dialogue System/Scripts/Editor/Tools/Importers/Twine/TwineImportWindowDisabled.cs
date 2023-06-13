#if !USE_TWINE
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem.Twine
{

    public class TwineImportWindowDisabled
    {

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/Twine 2 (Twison)...", false, 1)]
        public static void AskEnableTwineSupport()
        {
            if (EditorUtility.DisplayDialog("Enable Twine Support", "Twine import support isn't enabled yet. Would you like to enable it? After clicking Enable, re-open the Twine import window.", "Enable", "Cancel"))
            {
                EditorTools.TryAddScriptingDefineSymbols("USE_TWINE");
            }
        }
    }
}
#endif
