// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PixelCrushers
{

    public static class MoreEditorUtility
    {

        // These two methods handle API differences:

        public static string GetScriptingDefineSymbolsForGroup(BuildTargetGroup group)
        {
#if UNITY_2023_1_OR_NEWER
            return PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(group));
#else
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#endif
        }

        public static void SetScriptingDefineSymbolsForGroup(BuildTargetGroup group, string defines)
        {
#if UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(group), defines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
#endif
        }

        /// <summary>
        /// Checks if a symbol exists in the project's Scripting Define Symbols for the current build target.
        /// </summary>
        public static bool DoesScriptingDefineSymbolExist(string symbol)
        {
            var defines = GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');
            for (int i = 0; i < defines.Length; i++)
            {
                if (string.Equals(symbol, defines[i].Trim())) return true;
            }
            return false;
        }

        public static HashSet<BuildTargetGroup> GetInstalledBuildTargetGroups()
        {
#if UNITY_2017
            Debug.Log("Updating all build targets. Please ignore messages about build targets not installed.");
#endif
            var result = new HashSet<BuildTargetGroup>();
            foreach (BuildTarget target in (BuildTarget[])Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
#if UNITY_2018_1_OR_NEWER
                if (BuildPipeline.IsBuildTargetSupported(group, target))
#endif
                {
                    result.Add(group);
                }
            }
            return result;
        }

        /// <summary>
        /// Try to add a symbol to the project's Scripting Define Symbols for all build targets.
        /// </summary>
        public static void TryAddScriptingDefineSymbols(string symbol, bool touchFiles = false)
        {
            foreach (var group in GetInstalledBuildTargetGroups())
            {
                try
                {
                    var defines = GetScriptingDefineSymbolsForGroup(group);
                    if (!string.IsNullOrEmpty(defines)) defines += ";";
                    defines += symbol;
                    SetScriptingDefineSymbolsForGroup(group, defines);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            if (touchFiles) TouchScriptsWithScriptingSymbol(symbol);
            RecompileScripts();
        }

        /// <summary>
        /// Try to remove a symbol from the project's Scripting Define Symbols for all build targets.
        /// </summary>
        public static void TryRemoveScriptingDefineSymbols(string symbol)
        {
            foreach (var group in GetInstalledBuildTargetGroups())
            {
                try
                {
                    var symbols = new List<string>(GetScriptingDefineSymbolsForGroup(group).Split(';'));
                    symbols.Remove(symbol);
                    var defines = string.Join(";", symbols.ToArray());
                    SetScriptingDefineSymbolsForGroup(group, defines);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            RecompileScripts();
        }

        /// <summary>
        /// Add or remove a scripting define symbol.
        /// </summary>
        public static void ToggleScriptingDefineSymbol(string define, bool value, bool touchFiles = false)
        {
            if (value == true) TryAddScriptingDefineSymbols(define, touchFiles);
            else TryRemoveScriptingDefineSymbols(define);
        }

        /// <summary>
        /// Triggers a script recompile.
        /// </summary>
        public static void RecompileScripts()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#if UNITY_2019_3_OR_NEWER
            EditorUtility.RequestScriptReload();
#else
            UnityEditorInternal.InternalEditorUtility.RequestScriptReload();
#endif
        }

        /// <summary>
        /// The only reliable way to force a recompile and get the editor to recognize
        /// MonoBehaviour scripts and wrappers in Plugins is to actually change those
        /// files. :/
        /// </summary>
        /// <param name="symbol">Touch files that cehck this scripting symbol.</param>
        public static void TouchScriptsWithScriptingSymbol(string symbol)
        {
            var path = Application.dataPath + "/Plugins/Pixel Crushers/";
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                path = path.Replace("/", "\\");
            }
            if (!Directory.Exists(path))
            {
                Debug.Log("It looks like you've moved this Pixel Crushers asset. In the Project view, please right-click on the folder in its new location and select Reimport.");
            }
            else
            {
                string[] filenames = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);
                var found = string.Empty;
                var recompileAtText = "// Recompile at " + DateTime.Now + "\r\n";
                var searchString = "#if " + symbol;
                foreach (string filename in filenames)
                {
                    var text = File.ReadAllText(filename);
                    if (text.Contains(searchString))
                    {
                        found += filename + "\n";
                        if (text.StartsWith("// Recompile at "))
                        {
                            var lines = File.ReadAllLines(filename);
                            lines[0] = recompileAtText;
                            File.WriteAllLines(filename, lines);
                        }
                        else
                        {
                            text = recompileAtText + text;
                            File.WriteAllText(filename, text);
                        }
                    }
                }
            }
        }

        //=============================================================

        [MenuItem("Tools/Pixel Crushers/Common/Misc/Enable TextMesh Pro Support...", false, 101)]
        static public void AddTMPPRESENT()
        {
            if (EditorUtility.DisplayDialog("Enable TextMesh Pro Support", "This will enable TextMesh Pro support. Your project must already contain the TextMesh Pro package. To continue, press OK. If you need to install TextMesh Pro first, press Cancel.", "OK", "Cancel"))
            {
                MoreEditorUtility.TryAddScriptingDefineSymbols("TMP_PRESENT");
                TouchScriptsWithScriptingSymbol("TMP_PRESENT");
                EditorUtility.DisplayDialog("TextMesh Pro Support Enabled", "TextMesh Pro support has been enabled. You may need to right-click on the two files named TextMeshProTypewriterEffect and select Reimport to be able to add them to your GameObjects. If you change build platforms, you may need to select this menu item again.", "OK");
            }
        }

        [MenuItem("Tools/Pixel Crushers/Common/Misc/Enable TextMesh Pro Support...", true)]
        static bool ValidateAddTMPPRESENT()
        {
            return !MoreEditorUtility.DoesScriptingDefineSymbolExist("TMP_PRESENT");
        }

        //=============================================================

        [MenuItem("Tools/Pixel Crushers/Common/Misc/Enable Super Text Mesh Support...", false, 101)]
        static public void AddUSESTM()
        {
            if (EditorUtility.DisplayDialog("Enable Super Text Mesh Support", "This will enable Super Text Mesh support. Your project must already contain Super Text Mesh.\n\n*IMPORTANT*: Before pressing OK, you MUST move the Clavian folder into the Plugins folder!\n\nTo continue, press OK. If you need to install Super Text Mesh or move the folder first, press Cancel.", "OK", "Cancel"))
            {
                MoreEditorUtility.TryAddScriptingDefineSymbols("USE_STM");
                EditorUtility.DisplayDialog("Super Text Mesh Support Enabled", "Super Text Mesh support has been enabled.", "OK");
            }
        }

        [MenuItem("Tools/Pixel Crushers/Common/Misc/Enable Super Text Mesh Support...", true)]
        static bool ValidateAddUSESTM()
        {
            return !MoreEditorUtility.DoesScriptingDefineSymbolExist("USE_STM");
        }

        //=============================================================

#if UNITY_2019_1_OR_NEWER

        [MenuItem("Tools/Pixel Crushers/Common/Misc/Use New Input System...", false, 102)]
        static public void AddUSENEWINPUT()
        {
            if (EditorUtility.DisplayDialog("Use New Input System", "This will switch the Input Device Manager to read from Unity's new Input System. You must have already added the Input System package.\n\nSee the configuration manual in Plugins > Pixel Crushers > Common > Documentation.", "OK", "Cancel"))
            {
                MoreEditorUtility.TryAddScriptingDefineSymbols("USE_NEW_INPUT");
                EditorUtility.DisplayDialog("Using New Input System", "See the configuration manual in Plugins > Pixel Crushers > Common > Documentation.", "OK");
            }
        }

        [MenuItem("Tools/Pixel Crushers/Common/Misc/Use New Input System...", true)]
        static bool ValidateAddUSENEWINPUT()
        {
            return !MoreEditorUtility.DoesScriptingDefineSymbolExist("USE_NEW_INPUT");
        }

#endif

    }
}
