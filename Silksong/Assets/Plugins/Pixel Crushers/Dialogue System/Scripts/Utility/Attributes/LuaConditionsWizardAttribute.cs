// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add the attribute [LuaConditionsWizard] to a string variable to
    /// use the Lua Conditions Wizard in the inspector.
    /// </summary>
    public class LuaConditionsWizardAttribute : PropertyAttribute
    {

        public bool showReferenceDatabase = false;

        public LuaConditionsWizardAttribute(bool showReferenceDatabase = false)
        {
            this.showReferenceDatabase = showReferenceDatabase;
        }
    }
}
