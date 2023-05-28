// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add the attribute [LuaScriptWizard] to a string variable to
    /// use the Lua Script Wizard in the inspector.
    /// </summary>
    public class LuaScriptWizardAttribute : PropertyAttribute
    {

        public bool showReferenceDatabase = false;

        public LuaScriptWizardAttribute(bool showReferenceDatabase = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
        }
    }
}
