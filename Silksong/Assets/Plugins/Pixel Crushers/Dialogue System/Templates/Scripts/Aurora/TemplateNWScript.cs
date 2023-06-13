/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this template, make a copy and remove the lines that start
 * [REMOVE THIS LINE] with "[REMOVE THIS LINE]". Then add your code where the comments indicate.
 * [REMOVE THIS LINE]



#if USE_AURORA
using UnityEngine;
using System.Reflection;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is a template for registering an NWScript() function with Lua.
    /// Add your version to a GameObject so the Start() method gets called 
    /// to register the function.
    /// </summary>
    public class TemplateNWScript : MonoBehaviour
    {

        void Start()
        {
            Lua.RegisterFunction("NWScript", this, this.GetType().GetMethod("NWScript"));
        }

        public bool NWScript(string scriptName)
        {
            // Remove the Debug.Log line and add your code here.
            // Make sure to return true or false.
            Debug.Log(string.Format("NWScript({0}) stub returning false.", scriptName));
            return false;
        }

        // The version below works by finding a C# method in this class that matches the
        // script name. For example, if your NWN conversation calls a script named 'script_01',
        // this version calls a C# method in this class named script_01(). You must define
        // these C# methods. To use this version, comment out the version of NWScript() above
        // and uncomment the version below:
        //
        //public bool NWScript(string scriptName) {
        //	// Find a method matching scriptName (e.g., "script_01"):
        //	MethodInfo methodInfo = typeof(MyNWScript).GetMethod(scriptName);
        //	if (methodInfo == null) {
        //		if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Can't find NWScript method {1}", DialogueDebug.Prefix, scriptName));
        //		return false;
        //	}
        //	// Call the method:
        //	return (bool) methodInfo.Invoke(this, null);
        //}

    }

}
#endif


/**/
