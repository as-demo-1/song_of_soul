// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This is the Dialogue System welcome window. It provides easy shortcuts to
    /// tools, documentation, and support.
    /// </summary>
    [InitializeOnLoad]
    public class WelcomeWindow : EditorWindow
    {

        private const string ShowOnStartEditorPrefsKey = "PixelCrushers.DialogueSystem.WelcomeWindow.ShowOnStart";

        private GUIStyle m_quickButtonGuiStyle = null;
        private GUIStyle quickButtonGuiStyle
        {
            get
            {
                if (m_quickButtonGuiStyle == null)
                {
                    m_quickButtonGuiStyle = new GUIStyle(GUI.skin.button);
                    m_quickButtonGuiStyle.alignment = TextAnchor.MiddleCenter;
                }
                return m_quickButtonGuiStyle;
            }
        }

        private static bool showOnStartPrefs
        {
            get { return EditorPrefs.GetBool(ShowOnStartEditorPrefsKey, true); }
            set { EditorPrefs.SetBool(ShowOnStartEditorPrefsKey, value); }
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Welcome Window", false, -2)]
        public static WelcomeWindow ShowWindow()
        {
            var window = GetWindow<WelcomeWindow>(false, "Welcome");
#if EVALUATION_VERSION || ACADEMIC
            window.minSize = new Vector2(370, 650);
#else
            window.minSize = new Vector2(370, 620);
#endif
            window.showOnStart = true; // Can't check EditorPrefs when constructing window: showOnStartPrefs;
            return window;
        }

        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            RegisterWindowCheck();
        }

        private static void RegisterWindowCheck()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.update -= CheckShowWelcomeWindow;
                EditorApplication.update += CheckShowWelcomeWindow;
            }
        }

        private static void CheckShowWelcomeWindow()
        {
            EditorApplication.update -= CheckShowWelcomeWindow;
            if (showOnStartPrefs)
            {
                ShowWindow();
            }
        }

        public bool showOnStart = true;

        private static Texture2D icon = null;
        private static GUIStyle iconButtonStyle = null;

        private void OnGUI()
        {
            DrawBanner();
            DrawButtons();
            DrawDefines();
            DrawFooter();
        }

        private void DrawBanner()
        {
            if (icon == null) icon = DialogueSystemControllerEditor.FindIcon();
            if (icon == null) return;
            if (iconButtonStyle == null)
            {
                iconButtonStyle = new GUIStyle(EditorStyles.label);
                iconButtonStyle.normal.background = icon;
                iconButtonStyle.active.background = icon;
            }
            GUI.DrawTexture(new Rect(5, 5, icon.width, icon.height), icon);
            var version = DialogueSystemMenuItems.GetVersion();
            if (!string.IsNullOrEmpty(version))
            {
                var versionSize = EditorStyles.label.CalcSize(new GUIContent(version));
                GUI.Label(new Rect(position.width - (versionSize.x + 5) - 5, 5, versionSize.x + 5, versionSize.y), version);
            }
        }

        private const float ButtonWidth = 68;
        private const float ButtonHeight = 50;

        private void DrawButtons()
        {
            GUILayout.BeginArea(new Rect(5, 40, position.width - 10, position.height - 40));
            try
            {
                EditorWindowTools.DrawHorizontalLine();
                EditorGUILayout.HelpBox("Welcome to the Dialogue System for Unity!\n\nThe buttons below are shortcuts to commonly-used functions. You can find even more in Tools > Pixel Crushers > Dialogue System.", MessageType.None);
                EditorWindowTools.DrawHorizontalLine();
                GUILayout.Label("Help", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal();
                try
                {
                    if (GUILayout.Button(new GUIContent("Quick\nStart", "Open Quick Start tutorial"), quickButtonGuiStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(3 * EditorGUIUtility.singleLineHeight)))
                    {
                        Application.OpenURL("http://www.pixelcrushers.com/dialogue_system/manual2x/html/quick_start.html");
                    }
                    if (GUILayout.Button(new GUIContent("Manual", "Open online manual"), quickButtonGuiStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(3 * EditorGUIUtility.singleLineHeight)))
                    {
                        Application.OpenURL("http://www.pixelcrushers.com/dialogue_system/manual2x/html/");
                    }
                    if (GUILayout.Button(new GUIContent("Videos", "Open video tutorial list"), quickButtonGuiStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(3 * EditorGUIUtility.singleLineHeight)))
                    {
                        Application.OpenURL("http://www.pixelcrushers.com/dialogue-system-tutorials/");
                    }
                    if (GUILayout.Button(new GUIContent("Scripting\nReference", "Open scripting & API reference"), quickButtonGuiStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(3 * EditorGUIUtility.singleLineHeight)))
                    {
                        Application.OpenURL("http://www.pixelcrushers.com/dialogue_system/manual2x/html/scripting.html");
                    }
                    if (GUILayout.Button(new GUIContent("Forum", "Go to the Pixel Crushers forum"), quickButtonGuiStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(3 * EditorGUIUtility.singleLineHeight)))
                    {
                        Application.OpenURL("http://www.pixelcrushers.com/phpbb");
                    }
                }
                finally
                {
                    GUILayout.EndHorizontal();
                }
                EditorWindowTools.DrawHorizontalLine();
                GUILayout.Label("Wizards & Resources", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal();
                try
                {
                    if (GUILayout.Button(new GUIContent("Dialogue\nEditor", "Open the Dialogue Editor window"), quickButtonGuiStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(3 * EditorGUIUtility.singleLineHeight)))
                    {
                        PixelCrushers.DialogueSystem.DialogueEditor.DialogueEditorWindow.OpenDialogueEditorWindow();
                    }
                    if (GUILayout.Button(new GUIContent("Dialogue\nManager\nWizard", "Configure a Dialogue Manager, the component that coordinates all Dialogue System activity"), quickButtonGuiStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(3 * EditorGUIUtility.singleLineHeight)))
                    {
                        DialogueManagerWizard.Init();
                    }
                    if (GUILayout.Button(new GUIContent("Player\nSetup\nWizard", "Configure a player GameObject to work with the Dialogue System"), quickButtonGuiStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(3 * EditorGUIUtility.singleLineHeight)))
                    {
                        PlayerSetupWizard.Init();
                    }
                    if (GUILayout.Button(new GUIContent("NPC\nSetup\nWizard", "Configure a non-player character or other interactive GameObject to work with the Dialogue System"), quickButtonGuiStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(3 * EditorGUIUtility.singleLineHeight)))
                    {
                        NPCSetupWizard.Init();
                    }
                    if (GUILayout.Button(new GUIContent("Free\nExtras", "Go to the Dialogue System free extras website"), quickButtonGuiStyle, GUILayout.Width(ButtonWidth), GUILayout.Height(3 * EditorGUIUtility.singleLineHeight)))
                    {
                        Application.OpenURL("http://www.pixelcrushers.com/dialogue-system-extras/");
                    }
                }
                finally
                {
                    GUILayout.EndHorizontal();
                }
                EditorWindowTools.DrawHorizontalLine();
            }
            finally
            {
                GUILayout.EndArea();
            }
        }

        private void DrawDefines()
        {
            GUILayout.BeginArea(new Rect(5, 256, position.width - 10, position.height - 256));
            EditorGUILayout.LabelField("Current Build Target: " + ObjectNames.NicifyVariableName(EditorUserBuildSettings.activeBuildTarget.ToString()), EditorStyles.boldLabel);

            var define_USE_PHYSICS2D = false;
            var define_USE_NEW_INPUT = false;
            var define_USE_ADDRESSABLES = false;
            var define_USE_TIMELINE = false;
            var define_USE_CINEMACHINE = false;
            var define_USE_ARCWEAVE = false;
            var define_USE_ARTICY = false;
            var define_USE_AURORA = false;
            var define_USE_CELTX = false;
            var define_USE_CELTX3 = false;
            var define_USE_TWINE = false;
            var define_USE_YARN = false;
            var define_TMP_PRESENT = false;
            var define_USE_STM = false;
            var defines = MoreEditorUtility.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');
            for (int i = 0; i < defines.Length; i++)
            {
                if (string.Equals(ScriptingSymbolNames.USE_PHYSICS2D, defines[i].Trim())) define_USE_PHYSICS2D = true;
                if (string.Equals(ScriptingSymbolNames.USE_NEW_INPUT, defines[i].Trim())) define_USE_NEW_INPUT = true;
                if (string.Equals(ScriptingSymbolNames.USE_ADDRESSABLES, defines[i].Trim())) define_USE_ADDRESSABLES = true;
                if (string.Equals(ScriptingSymbolNames.USE_TIMELINE, defines[i].Trim())) define_USE_TIMELINE = true;
                if (string.Equals(ScriptingSymbolNames.USE_CINEMACHINE, defines[i].Trim())) define_USE_CINEMACHINE = true;
                if (string.Equals(ScriptingSymbolNames.USE_ARCWEAVE, defines[i].Trim())) define_USE_ARCWEAVE = true;
                if (string.Equals(ScriptingSymbolNames.USE_ARTICY, defines[i].Trim())) define_USE_ARTICY = true;
                if (string.Equals(ScriptingSymbolNames.USE_AURORA, defines[i].Trim())) define_USE_AURORA = true;
                if (string.Equals(ScriptingSymbolNames.USE_CELTX, defines[i].Trim())) define_USE_CELTX = true;
                if (string.Equals(ScriptingSymbolNames.USE_CELTX3, defines[i].Trim())) define_USE_CELTX3 = true;
                if (string.Equals(ScriptingSymbolNames.USE_TWINE, defines[i].Trim())) define_USE_TWINE = true;
                if (string.Equals(ScriptingSymbolNames.USE_YARN, defines[i].Trim())) define_USE_YARN = true;
                if (string.Equals(ScriptingSymbolNames.TMP_PRESENT, defines[i].Trim())) define_TMP_PRESENT = true;
                if (string.Equals(ScriptingSymbolNames.USE_STM, defines[i].Trim())) define_USE_STM = true;
            }
#if EVALUATION_VERSION || ACADEMIC
            //define_USE_PHYSICS2D = true;
            define_USE_NEW_INPUT = false;
            define_USE_ADDRESSABLES = false;
            //define_TMP_PRESENT = true;
            define_USE_STM = false;
            define_USE_ARCWEAVE = false;
            define_USE_ARTICY = true;
            define_USE_AURORA = true;
            define_USE_CELTX = false;
            define_USE_CELTX3 = false;
            define_USE_TWINE = true;
            define_USE_YARN = false;
#endif

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent("Enable support for:", "NOTE: Enables Dialogue System support. You must still enable each package in Package Manager."));
#if EVALUATION_VERSION || ACADEMIC
            var new_TMP_PRESENT = EditorGUILayout.ToggleLeft(new GUIContent(define_TMP_PRESENT ? "TextMesh Pro (TMP_PRESENT)" : "TextMesh Pro (TMP_PRESENT) <- USING TEXTMESH PRO?", "Enable Dialogue System support for TextMesh Pro. You must still enable TextMesh Pro in Package Manager."), define_TMP_PRESENT);
            var new_USE_PHYSICS2D = EditorGUILayout.ToggleLeft(define_USE_PHYSICS2D ? "2D Physics (USE_PHYSICS2D)" : "2D Physics (USE_PHYSICS2D) <- MAKING A 2D GAME?", define_USE_PHYSICS2D);
            EditorGUI.BeginDisabledGroup(true);
            //EditorGUILayout.ToggleLeft(new GUIContent("TextMesh Pro (TMP_PRESENT)", "TextMesh Pro support is enabled in evaluation version. Your project must contain the TextMesh Pro package."), define_TMP_PRESENT);
            //EditorGUILayout.ToggleLeft(new GUIContent("2D Physics (USE_PHYSICS2D)", "Support is built in for evaluation version or Unity 2017 and earlier."), define_USE_PHYSICS2D);
            EditorGUILayout.ToggleLeft(new GUIContent("Addressables (USE_ADDRESSABLES)", "Addressables support not available in evaluation version."), define_USE_ADDRESSABLES);
            EditorGUILayout.ToggleLeft(new GUIContent("New Input System (USE_NEW_INPUT)", "New Input System support not available in evaluation version."), define_USE_NEW_INPUT);
            EditorGUI.EndDisabledGroup();
            //var new_USE_PHYSICS2D = define_USE_PHYSICS2D;
            var new_USE_CINEMACHINE = define_USE_CINEMACHINE;
            var new_USE_NEW_INPUT = define_USE_NEW_INPUT;
            var new_USE_ADDRESSABLES = define_USE_ADDRESSABLES;
#else
            var new_TMP_PRESENT = EditorGUILayout.ToggleLeft(new GUIContent(define_TMP_PRESENT ? "TextMesh Pro (TMP_PRESENT)" : "TextMesh Pro (TMP_PRESENT) <- USING TEXTMESH PRO?", "Enable Dialogue System support for TextMesh Pro. You must still enable TextMesh Pro in Package Manager."), define_TMP_PRESENT);
            var new_USE_PHYSICS2D = EditorGUILayout.ToggleLeft(define_USE_PHYSICS2D ? "2D Physics (USE_PHYSICS2D)" : "2D Physics (USE_PHYSICS2D) <- MAKING A 2D GAME?", define_USE_PHYSICS2D);
            var new_USE_ADDRESSABLES = EditorGUILayout.ToggleLeft("Addressables (USE_ADDRESSABLES)", define_USE_ADDRESSABLES);
            var new_USE_CINEMACHINE = EditorGUILayout.ToggleLeft(new GUIContent("Cinemachine (USE_CINEMACHINE)", "Enable Dialogue System support for Cinemachine. You must still enable Cinemachine in Package Manager."), define_USE_CINEMACHINE);
            var new_USE_NEW_INPUT = EditorGUILayout.ToggleLeft("New Input System (USE_NEW_INPUT)", define_USE_NEW_INPUT);
#endif

            var new_USE_TIMELINE = EditorGUILayout.ToggleLeft(new GUIContent("Timeline (USE_TIMELINE)", "Enable Dialogue System support for Timeline. You must still enable Timeline in Package Manager."), define_USE_TIMELINE);

#if EVALUATION_VERSION || ACADEMIC
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ToggleLeft(new GUIContent("Super Text Mesh (USE_STM)", "Super Text Mesh support not available in evaluation version."), define_USE_STM);
            EditorGUI.EndDisabledGroup();
            //var new_TMP_PRESENT = define_TMP_PRESENT;
            var new_USE_STM = define_USE_STM;
#else
            var new_USE_STM = EditorGUILayout.ToggleLeft(new GUIContent("Super Text Mesh (USE_STM)", "Enable Dialogue System support for Super Text Mesh. Requires Super Text Mesh in project."), define_USE_STM);
#endif

#if EVALUATION_VERSION || ACADEMIC
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ToggleLeft(new GUIContent("Arcweave (USE_ARCWEAVE)", "Enable Dialogue System support for Arcweave import."), define_USE_ARCWEAVE);
            EditorGUILayout.ToggleLeft(new GUIContent("articy:draft (USE_ARTICY)", "Enable Dialogue System support for articy:draft XML import."), define_USE_ARTICY);
            EditorGUILayout.ToggleLeft(new GUIContent("Aurora Toolset (USE_AURORA)", "Enable Dialogue System support for Aurora (Neverwinter Nights) Toolset import."), define_USE_AURORA);
            EditorGUILayout.ToggleLeft(new GUIContent("Celtx GVR 2 (USE_CELTX)", "Enable Dialogue System support for Celtx GVR 2 JSON import."), define_USE_CELTX);
            EditorGUILayout.ToggleLeft(new GUIContent("Backlight (Celtx) Gem 3 (USE_CELTX3)", "Enable Dialogue System support for Backlight Gem 3 JSON import."), define_USE_CELTX3);
            EditorGUILayout.ToggleLeft(new GUIContent("Twine (USE_TWINE)", "Enable Dialogue System support for Twine Twison import."), define_USE_TWINE);
            EditorGUILayout.ToggleLeft(new GUIContent("Yarn (USE_YARN)", "Enable Dialogue System support for YarnSpinner import."), define_USE_YARN);
            EditorGUI.EndDisabledGroup();
            var new_USE_ARCWEAVE = define_USE_ARCWEAVE;
            var new_USE_ARTICY = define_USE_ARTICY;
            var new_USE_AURORA = define_USE_AURORA;
            var new_USE_CELTX = define_USE_CELTX;
            var new_USE_CELTX3 = define_USE_CELTX3;
            var new_USE_TWINE = define_USE_TWINE;
            var new_USE_YARN = define_USE_YARN;
#else
            var new_USE_ARCWEAVE = EditorGUILayout.ToggleLeft(new GUIContent("Arcweave (USE_ARCWEAVE)", "Enable Dialogue System support for Arcweave import."), define_USE_ARCWEAVE);
            var new_USE_ARTICY = EditorGUILayout.ToggleLeft(new GUIContent("articy:draft (USE_ARTICY)", "Enable Dialogue System support for articy:draft XML import."), define_USE_ARTICY);
            var new_USE_AURORA = EditorGUILayout.ToggleLeft(new GUIContent("Aurora Toolset (USE_AURORA)", "Enable Dialogue System support for Aurora (Neverwinter Nights) Toolset import."), define_USE_AURORA);
            var new_USE_CELTX = EditorGUILayout.ToggleLeft(new GUIContent("Celtx GVR 2 (USE_CELTX)", "Enable Dialogue System support for Celtx GVR 2 JSON import."), define_USE_CELTX);
            var new_USE_CELTX3 = EditorGUILayout.ToggleLeft(new GUIContent("Backlight (Celtx) Gem 3 (USE_CELTX3)", "Enable Dialogue System support for Backlight Gem 3 JSON import."), define_USE_CELTX3);
            var new_USE_TWINE = EditorGUILayout.ToggleLeft(new GUIContent("Twine (USE_TWINE)", "Enable Dialogue System support for Twine Twison import."), define_USE_TWINE);
            var new_USE_YARN = EditorGUILayout.ToggleLeft(new GUIContent("Yarn (USE_YARN)", "Enable Dialogue System support for YarnSpinner import."), define_USE_YARN);
#endif

            var changed = EditorGUI.EndChangeCheck();

            if (new_USE_PHYSICS2D != define_USE_PHYSICS2D) MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_PHYSICS2D, new_USE_PHYSICS2D);
            if (new_USE_TIMELINE != define_USE_TIMELINE) MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_TIMELINE, new_USE_TIMELINE, true);
            if (new_USE_ARTICY != define_USE_ARTICY) MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_ARTICY, new_USE_ARTICY);
            if (new_USE_AURORA != define_USE_AURORA) MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_AURORA, new_USE_AURORA);
            if (new_USE_TWINE != define_USE_TWINE) MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_TWINE, new_USE_TWINE);
            if (new_USE_YARN != define_USE_YARN) MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_YARN, new_USE_YARN);
            if (new_TMP_PRESENT != define_TMP_PRESENT) MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.TMP_PRESENT, new_TMP_PRESENT, true);

            if (new_USE_NEW_INPUT != define_USE_NEW_INPUT)
            {
                if (new_USE_NEW_INPUT)
                {
                    if (EditorUtility.DisplayDialog("Enable New Input Package Support", "This will switch the Dialogue System to use the new Input System. You MUST have installed the new Input System package via the Package Manager window first. If you're using Unity's built-in input, click Cancel now.", "OK", "Cancel"))
                    {
                        MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_NEW_INPUT, new_USE_NEW_INPUT);
                    }
                    else
                    {
                        changed = false;
                    }
                }
                else
                {
                    MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_NEW_INPUT, new_USE_NEW_INPUT);
                }
            }
            if (new_USE_ADDRESSABLES != define_USE_ADDRESSABLES)
            {
                if (new_USE_ADDRESSABLES)
                {
                    if (EditorUtility.DisplayDialog("Enable Addressables Support", "This will enable support for Addressables. You MUST have installed the Addressables package via the Package Manager window first.", "OK", "Cancel"))
                    {
                        MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_ADDRESSABLES, new_USE_ADDRESSABLES);
                    }
                    else
                    {
                        changed = false;
                    }
                }
                else
                {
                    MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_ADDRESSABLES, new_USE_ADDRESSABLES);
                }
            }
            if (new_USE_CINEMACHINE != define_USE_CINEMACHINE)
            {
                if (new_USE_CINEMACHINE)
                {
                    if (EditorUtility.DisplayDialog("Enable Cinemachine Support", "This will enable support for Cinemachine. You MUST have installed the Cinemachine package via the Package Manager window first.", "OK", "Cancel"))
                    {
                        MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_CINEMACHINE, new_USE_CINEMACHINE);
                    }
                    else
                    {
                        changed = false;
                    }
                }
                else
                {
                    MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_CINEMACHINE, new_USE_CINEMACHINE);
                }
            }
            if (new_USE_STM != define_USE_STM)
            {
                if (new_USE_STM)
                {
                    if (EditorUtility.DisplayDialog("Enable Super Text Mesh Support", "This will enable Super Text Mesh support. Your project must already contain Super Text Mesh.\n\n*IMPORTANT*: Before pressing OK, you MUST move the Clavian folder into the Plugins folder!\n\nTo continue, press OK. If you need to install Super Text Mesh or move the folder first, press Cancel.", "OK", "Cancel"))
                    {
                        MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_STM, new_USE_STM);
                    }
                    else
                    {
                        changed = false;
                    }
                }
                else
                {
                    MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_STM, new_USE_STM);
                }
            }
            if (new_USE_ARCWEAVE != define_USE_ARCWEAVE)
            {
                if (new_USE_ARCWEAVE)
                {
                    if (EditorUtility.DisplayDialog("Enable Arcweave Import", "This will enable the ability to import Arcweave projects. Newtonsoft Json.NET MUST already be installed in your project first. See the Arcweave Import instructions in the online manual if Newtonsoft Json.NET is not installed yet.\n\n*IMPORTANT*: Only press OK if Newtonsoft Json.NET is already installed!\n\nTo continue, press OK. If you need to install Newtonsoft Json.NET first, press Cancel.", "OK", "Cancel"))
                    {
                        MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_ARCWEAVE, new_USE_ARCWEAVE);
                    }
                    else
                    {
                        changed = false;
                    }
                }
                else
                {
                    MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_ARCWEAVE, new_USE_ARCWEAVE);
                }
            }
            if (new_USE_CELTX != define_USE_CELTX)
            {
                if (new_USE_CELTX)
                {
                    if (EditorUtility.DisplayDialog("Enable Celtx GVR 2 Import", "This will enable the ability to import Celtx GVR 2 JSON exports. Newtonsoft Json.NET MUST already be installed in your project first. See the Celtx Import instructions in the online manual if Newtonsoft Json.NET is not installed yet.\n\n*IMPORTANT*: Only press OK if Newtonsoft Json.NET is already installed!\n\nTo continue, press OK. If you need to install Newtonsoft Json.NET first, press Cancel.", "OK", "Cancel"))
                    {
                        MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_CELTX, new_USE_CELTX);
                    }
                    else
                    {
                        changed = false;
                    }
                }
                else
                {
                    MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_CELTX, new_USE_CELTX);
                }
            }
            if (new_USE_CELTX3 != define_USE_CELTX3)
            {
                if (new_USE_CELTX3)
                {
                    if (EditorUtility.DisplayDialog("Enable Celtx Gem 3 Import", "This will enable the ability to import Celtx Gem 3 JSON exports. Newtonsoft Json.NET MUST already be installed in your project first. See the Celtx Import instructions in the online manual if Newtonsoft Json.NET is not installed yet.\n\n*IMPORTANT*: Only press OK if Newtonsoft Json.NET is already installed!\n\nTo continue, press OK. If you need to install Newtonsoft Json.NET first, press Cancel.", "OK", "Cancel"))
                    {
                        MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_CELTX3, new_USE_CELTX3);
                    }
                    else
                    {
                        changed = false;
                    }
                }
                else
                {
                    MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_CELTX3, new_USE_CELTX3);
                }
            }
            if (new_USE_YARN != define_USE_YARN)
            {
                if (new_USE_YARN)
                {
                    if (EditorUtility.DisplayDialog("Enable Yarn Import", "This will enable the ability to import Yarn Spinner files. Yarn Spinner for Unity must already be installed in your project first.\n\n*IMPORTANT*: Only press OK if Yarn Spinner is already installed!\n\nTo continue, press OK. If you need to install Yarn Spinner first, press Cancel.", "OK", "Cancel"))
                    {
                        MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_YARN, new_USE_YARN);
                    }
                    else
                    {
                        changed = false;
                    }
                }
                else
                {
                    MoreEditorUtility.ToggleScriptingDefineSymbol(ScriptingSymbolNames.USE_YARN, new_USE_YARN);
                }
            }

            EditorWindowTools.DrawHorizontalLine();
            GUILayout.EndArea();

            if (changed) EditorTools.ReimportScripts();
        }

        private void DrawFooter()
        {
            if (GUI.Button(new Rect(position.width - 200, position.height - 8 - 2 * EditorGUIUtility.singleLineHeight, 190, EditorGUIUtility.singleLineHeight), new GUIContent("Learn About OpenAI Addon", "Visit the Asset Store page for the Addon for OpenAI")))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/ai/dialogue-system-addon-for-openai-249287");
            }

            var newShowOnStart = EditorGUI.ToggleLeft(new Rect(5, position.height - 5 - EditorGUIUtility.singleLineHeight, position.width - (70 + 150), EditorGUIUtility.singleLineHeight), "Show at start", showOnStart);
            if (newShowOnStart != showOnStart)
            {
                showOnStart = newShowOnStart;
                showOnStartPrefs = newShowOnStart;
            }
            if (GUI.Button(new Rect(position.width - 80, position.height - 5 - EditorGUIUtility.singleLineHeight, 70, EditorGUIUtility.singleLineHeight), new GUIContent("Support", "Contact the developer for support")))
            {
                Application.OpenURL("http://www.pixelcrushers.com/support-form/");
            }
#if EVALUATION_VERSION || ACADEMIC
            if (GUI.Button(new Rect(position.width - 154, position.height - 5 - EditorGUIUtility.singleLineHeight, 70, EditorGUIUtility.singleLineHeight), new GUIContent("Buy", "Buy a license")))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/tools/ai/dialogue-system-for-unity-11672");
            }
#endif
        }

    }

}
