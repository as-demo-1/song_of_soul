#if USE_UNET
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.Wrappers
{

    /// <summary>
    /// This wrapper class keeps references intact if you switch between the 
    /// compiled assembly and source code versions of the original class.
    /// </summary>
    [HelpURL("http://www.pixelcrushers.com/dialogue_system/manual2x/html/lua_network_commands.html")]
    [AddComponentMenu("Pixel Crushers/Dialogue System/Misc/Lua Network Commands")]
    public class LuaNetworkCommands : PixelCrushers.DialogueSystem.LuaNetworkCommands
    { 
    }

}
#endif