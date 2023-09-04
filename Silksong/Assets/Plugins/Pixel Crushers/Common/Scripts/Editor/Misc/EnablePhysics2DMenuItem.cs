// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

#if UNITY_2018_1_OR_NEWER
    public static class EnablePhysics2DMenuItem
    {

        [MenuItem("Tools/Pixel Crushers/Common/Misc/Enable Physics2D Support...", false, 100)]
        static public void AddUSEPHYSICS2D()
        {
            if (EditorUtility.DisplayDialog("Enable Physics2D Support", "If your project uses 2D Physics, press OK to enable Pixel Crushers support for 2D Physics.", "OK", "Cancel"))
            {
                MoreEditorUtility.TryAddScriptingDefineSymbols("USE_PHYSICS2D");
                EditorUtility.DisplayDialog("Physics2D Support Enabled", "Support for 2D Physics has been enabled. You may need to right-click on the Plugins/Pixel Crushers folder and select Reimport to recompile the scripts with 2D Physics support. If you add build platforms, you may need to select this menu item again.", "OK");
            }
        }

        [MenuItem("Tools/Pixel Crushers/Common/Misc/Enable Physics2D Support...", true)]
        static bool ValidateAddUSEPHYSICS2D()
        {
            return !MoreEditorUtility.DoesScriptingDefineSymbolExist("USE_PHYSICS2D");
        }

    }
#endif
}