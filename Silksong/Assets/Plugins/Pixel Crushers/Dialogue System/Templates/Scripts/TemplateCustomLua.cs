/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this template, make a copy and remove the lines that start
 * [REMOVE THIS LINE] with "[REMOVE THIS LINE]". Then add your code where the comments indicate.
 * [REMOVE THIS LINE]
 * [REMOVE THIS LINE] If your code references scripts or assets that are outside of the Plugins
 * [REMOVE THIS LINE] folder, move this script outside of the Plugins folder, too.
 * [REMOVE THIS LINE]



using UnityEngine;
using PixelCrushers.DialogueSystem;

// Rename this class to the same name that you used for the script file.
// Add the script to your Dialogue Manager. You can optionally make this 
// a static class and remove the inheritance from MonoBehaviour, in which
// case you won't add it to the Dialogue Manager.
//
// This class registers two example functions: 
//
// - DebugLog(string) writes a string to the Console using Unity's Debug.Log().
// - AddOne(double) returns the value plus one.
//
// You can use these functions as models and then replace them with your own.
public class TemplateCustomLua : MonoBehaviour // Rename this class.
{
    [Tooltip("Typically leave unticked so temporary Dialogue Managers don't unregister your functions.")]
    public bool unregisterOnDisable = false;

    void OnEnable()
    {
        // Make the functions available to Lua: (Replace these lines with your own.)
        Lua.RegisterFunction("DebugLog", this, SymbolExtensions.GetMethodInfo(() => DebugLog(string.Empty)));
        Lua.RegisterFunction("AddOne", this, SymbolExtensions.GetMethodInfo(() => AddOne((double)0)));
    }

    void OnDisable()
    {
        if (unregisterOnDisable)
        {
            // Remove the functions from Lua: (Replace these lines with your own.)
            Lua.UnregisterFunction("DebugLog");
            Lua.UnregisterFunction("AddOne");
        }
    }

    public void DebugLog(string message)
    {
        Debug.Log(message);
    }

    public double AddOne(double value)
    { // Note: Lua always passes numbers as doubles.
        return value + 1;
    }
}



/**/
