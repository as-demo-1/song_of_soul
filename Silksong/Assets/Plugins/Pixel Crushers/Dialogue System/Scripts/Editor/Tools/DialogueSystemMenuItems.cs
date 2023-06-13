// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This class defines the menu items in the Dialogue System menu, 
    /// except for custom Editor Windows which define their own menu items.
    /// </summary>
    static public class DialogueSystemMenuItems
    {

        #region Special Builds

#if ACADEMIC
		[MenuItem("Tools/Pixel Crushers/Dialogue System/*Academic License*", false, -10)]
		static public void HelpAcademicLicense() {

		}
		
		[MenuItem("Tools/Pixel Crushers/Dialogue System/*Academic License*", true, -10)]
		static bool ValidateHelpAcademicLicense() {
			return false;
		}		

		[MenuItem("Tools/Pixel Crushers/Dialogue System/Commercial License...", false, -9)]
		static public void BuyNow() 
        {
			Application.OpenURL("https://assetstore.unity.com/packages/tools/ai/dialogue-system-for-unity-11672");
		}
#endif

#if EVALUATION_VERSION
		[MenuItem("Tools/Pixel Crushers/Dialogue System/*Evaluation Version*", false, 98)]
		static public void HelpEvaluationVersion() 
        {			
		}
		
		[MenuItem("Tools/Pixel Crushers/Dialogue System/*Evaluation Version*", true, 98)]
		static bool ValidateHelpEvaluationVersion() 
        {
			return false;
		}	

		[MenuItem("Tools/Pixel Crushers/Dialogue System/BUY NOW...", false, 99)]
		static public void BuyNow() 
        {
			Application.OpenURL("https://assetstore.unity.com/packages/tools/ai/dialogue-system-for-unity-11672");
		}
		
#endif

        #endregion

        #region Help

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Help/About", false, 0)]
        static public void HelpAbout()
        {
            ShowAboutWindow();
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Help/Manual", false, 1)]
        static public void HelpUserManual()
        {
            Application.OpenURL("http://www.pixelcrushers.com/dialogue_system/manual2x/html/");
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Help/Video Tutorials", false, 1)]
        static public void HelpVideoTutorials()
        {
            Application.OpenURL("http://www.pixelcrushers.com/dialogue-system-tutorials/");
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Help/FAQ", false, 3)]
        static public void HelpFAQ()
        {
            Application.OpenURL("https://www.pixelcrushers.com/dialogue_system/manual2x/html/faq.html");
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Help/Scripting Reference", false, 4)]
        static public void HelpScriptingReference()
        {
            Application.OpenURL("https://www.pixelcrushers.com/dialogue_system/manual2x/html/classes.html");
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Help/Release Notes", false, 15)]
        static public void HelpLateReleaseNotes()
        {
            Application.OpenURL("https://www.pixelcrushers.com/dialogue_system/manual2x/html/release_notes.html");
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Help/Late-Breaking News", false, 16)]
        static public void HelpLateBreakingNews()
        {
            Application.OpenURL("http://www.pixelcrushers.com/dialogue-system-late-breaking-news/");
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Help/Forum", false, 17)]
        static public void HelpForum()
        {
            Application.OpenURL("http://forum.unity3d.com/threads/204752-Dialogue-System-for-Unity");
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Help/Report a Bug", false, 17)]
        static public void HelpReportABug()
        {
            Application.OpenURL("http://www.pixelcrushers.com/support-form/");
        }

        // Tools > Converters, Camera Angle Editor priorities = 1, 2

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Extras...", false, 17)]
        static public void ExtrasURL()
        {
            Application.OpenURL("http://www.pixelcrushers.com/dialogue-system-extras/");
        }

        #endregion

        #region Assets

        [MenuItem("Assets/Create/Pixel Crushers/Dialogue System/Dialogue Database", false, 0)]
        public static void CreateDialogueDatabase()
        {
            CreateAsset(CreateDialogueDatabaseInstance(), "Dialogue Database");
        }

        public static DialogueDatabase CreateDialogueDatabaseInstance()
        {
            Template template = TemplateTools.LoadFromEditorPrefs();
            var wrapperType = TypeUtility.GetWrapperType(typeof(DialogueDatabase)) ?? typeof(DialogueDatabase);
            var database = ScriptableObjectUtility.CreateScriptableObject(wrapperType) as DialogueDatabase;
            database.actors.Add(template.CreateActor(1, "Player", true));
            database.variables.Add(template.CreateVariable(1, "Alert", string.Empty));
            database.ResetEmphasisSettings();
            return database;
        }

        [MenuItem("Assets/Create/Pixel Crushers/Dialogue System/Localized Text Table (deprecated)", false, 0)]
        public static void CreateLocalizedTextTable()
        {
            CreateAsset(CreateLocalizedTextTableInstance(), "Localized Text");
        }

        private static LocalizedTextTable CreateLocalizedTextTableInstance()
        {
            var wrapperType = TypeUtility.GetWrapperType(typeof(LocalizedTextTable)) ?? typeof(LocalizedTextTable);
            var table = ScriptableObjectUtility.CreateScriptableObject(wrapperType) as LocalizedTextTable;
            table.languages = new List<string> { "Default" };
            return table;
        }

        public static void CreateAsset(Object asset, string defaultAssetName)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/New {1}.asset", path, defaultAssetName));
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        #endregion

        #region Game Objects - no longer used

        //[MenuItem("GameObject/Create Other/Dialogue System/Dialogue Manager", false, 0)]
        //public static void CreateDialogueSystemGameObject()
        //{
        //    GameObject gameObject = new GameObject("Dialogue Manager", typeof(DialogueSystemController));
        //    Selection.activeGameObject = gameObject;
        //}

        //[MenuItem("GameObject/Create Other/Dialogue System/Lua Console", false, 1)]
        //public static void CreateLuaConsole()
        //{
        //    GameObject gameObject = new GameObject("Lua Console", typeof(LuaConsole));
        //    Selection.activeGameObject = gameObject;
        //}

        #endregion

        #region UnityGUI Game Objects - no longer used

        //[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Root", false, 0)]
        //public static void CreateUnityGUIRoot()
        //{
        //    AddChildGameObject("GUI Root", typeof(GUIRoot));
        //}

        //[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Label", false, 1)]
        //public static void CreateUnityGUILabel()
        //{
        //    AddChildGameObject("GUI Label", typeof(GUILabel));
        //}

        //[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Button", false, 2)]
        //public static void CreateUnityGUIButton()
        //{
        //    AddChildGameObject("GUI Button", typeof(GUIButton));
        //}

        //[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Image", false, 3)]
        //public static void CreateUnityGUIImage()
        //{
        //    AddChildGameObject("GUI Image", typeof(GUIImage));
        //}

        //[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Progress Bar", false, 4)]
        //public static void CreateUnityGUIProgressBar()
        //{
        //    AddChildGameObject("GUI Progress Bar", typeof(GUIProgressBar));
        //}

        //[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Panel", false, 5)]
        //public static void CreateUnityGUIPanel()
        //{
        //    AddChildGameObject("GUI Panel", typeof(GUIControl));
        //}

        //[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Window", false, 6)]
        //public static void CreateUnityGUIWindow()
        //{
        //    AddChildGameObject("GUI Window", typeof(GUIWindow));
        //}

        //[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Scroll View", false, 7)]
        //public static void CreateUnityGUIScrollView()
        //{
        //    AddChildGameObject("GUI Scroll View", typeof(GUIScrollView));
        //}

        //[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Text Field", false, 8)]
        //public static void CreateUnityGUITextField()
        //{
        //    AddChildGameObject("GUI Text Field", typeof(GUITextField));
        //}

        //[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Dialogue UI (top-level)", false, 9)]
        //public static void CreateUnityDialogueUI()
        //{
        //    GameObject uiObject = AddChildGameObject("Unity Dialogue UI", typeof(UnityDialogueUI));
        //    Selection.activeGameObject = uiObject;
        //    GameObject root = AddChildGameObject("GUI Root", typeof(GUIRoot));
        //    Selection.activeGameObject = root;
        //}

        //[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Quest Log Window", false, 10)]
        //public static void CreateUnityGUIQuestLogWindow()
        //{

        //    // Create GUI root:
        //    GameObject questLogWindowObject = AddChildGameObject("Quest Log Window", typeof(UnityGUIQuestLogWindow));
        //    Selection.activeGameObject = questLogWindowObject;
        //    GameObject root = AddChildGameObject("GUI Root", typeof(GUIRoot));
        //    Selection.activeGameObject = root;
        //    GameObject window = AddChildGameObject("Window", typeof(GUIWindow));
        //    GameObject abandonQuestPopup = AddChildGameObject("Abandon Quest Popup", typeof(GUIWindow));

        //    // Create quest window:
        //    Selection.activeObject = window;
        //    GameObject scrollView = AddChildGameObject("Scroll View", typeof(GUIScrollView));
        //    Selection.activeGameObject = window;
        //    ScaledRect activeButtonRect = new ScaledRect(ScaledRectAlignment.TopCenter, ScaledRectAlignment.TopRight, ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0), ScaledValue.FromNormalizedValue(0.5f), ScaledValue.FromNormalizedValue(0.1f), 0, 0);
        //    GUIButton activeButton = CreateButton(activeButtonRect, "Active Quests", "ClickShowActiveQuests", questLogWindowObject);
        //    ScaledRect completedButtonRect = new ScaledRect(ScaledRectAlignment.TopCenter, ScaledRectAlignment.TopLeft, ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0), ScaledValue.FromNormalizedValue(0.5f), ScaledValue.FromNormalizedValue(0.1f), 0, 0);
        //    GUIButton completedButton = CreateButton(completedButtonRect, "Completed Quests", "ClickShowCompletedQuests", questLogWindowObject);
        //    ScaledRect closeRect = new ScaledRect(ScaledRectAlignment.BottomCenter, ScaledRectAlignment.BottomCenter, ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0), ScaledValue.FromNormalizedValue(1), ScaledValue.FromNormalizedValue(0.1f), 0, 0);
        //    CreateButton(closeRect, "Close", "OnClose", questLogWindowObject);
        //    UnityGUIQuestLogWindow questLogWindow = questLogWindowObject.GetComponent<UnityGUIQuestLogWindow>();
        //    questLogWindow.guiRoot = root.GetComponent<GUIRoot>();
        //    questLogWindow.scrollView = scrollView.GetComponent<GUIScrollView>();
        //    questLogWindow.activeButton = activeButton;
        //    questLogWindow.completedButton = completedButton;

        //    // Create abandon quest popup:
        //    Selection.activeObject = abandonQuestPopup;
        //    ScaledRect okButtonRect = new ScaledRect(ScaledRectAlignment.BottomCenter, ScaledRectAlignment.BottomRight, ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0), ScaledValue.FromNormalizedValue(0.5f), ScaledValue.FromNormalizedValue(0.1f), 0, 0);
        //    GUIButton okButton = CreateButton(okButtonRect, "Abandon", "ClickConfirmAbandonQuest", questLogWindowObject);
        //    ScaledRect cancelButtonRect = new ScaledRect(ScaledRectAlignment.BottomCenter, ScaledRectAlignment.BottomLeft, ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0), ScaledValue.FromNormalizedValue(0.5f), ScaledValue.FromNormalizedValue(0.1f), 0, 0);
        //    GUIButton cancelButton = CreateButton(cancelButtonRect, "Cancel", "ClickCancelAbandonQuest", questLogWindowObject);
        //    GameObject questTitle = AddChildGameObject("Quest Title Label", typeof(GUILabel));
        //    questLogWindow.abandonQuestPopup.panel = abandonQuestPopup.GetComponent<GUIWindow>();
        //    questLogWindow.abandonQuestPopup.ok = okButton;
        //    questLogWindow.abandonQuestPopup.cancel = cancelButton;
        //    questLogWindow.abandonQuestPopup.questTitleLabel = questTitle.GetComponent<GUILabel>();

        //    // Select main window:
        //    Selection.activeGameObject = questLogWindowObject;
        //}

        //private static GameObject AddChildGameObject(string name, System.Type componentType)
        //{
        //    GameObject gameObject = new GameObject(name, componentType);
        //    if (Selection.activeGameObject != null)
        //    {
        //        gameObject.transform.parent = Selection.activeGameObject.transform;
        //    }
        //    else
        //    {
        //        Selection.activeGameObject = gameObject;
        //    }
        //    return gameObject;
        //}

        //private static GUIButton CreateButton(ScaledRect scaledRect, string name, string message, GameObject target)
        //{
        //    GameObject buttonObject = AddChildGameObject(name, typeof(GUIButton));
        //    GUIButton button = buttonObject.GetComponent<GUIButton>();
        //    button.scaledRect = scaledRect;
        //    button.text = name;
        //    button.message = message;
        //    button.target = target.transform;
        //    return button;
        //}

        #endregion

        #region Generic AddComponent

        public static void AddComponentToSelection<T>() where T : MonoBehaviour
        {
            if (Selection.activeGameObject == null) return;
            Selection.activeGameObject.AddComponent<T>();
        }

        #endregion

        #region Misc Menu Items

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Tools/Clear Quest Tracker PlayerPrefs Key", false, 100)]
        static public void ClearQuestTrackerPlayerPrefsKeys()
        {
            PlayerPrefs.DeleteKey("QuestTracker");
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Tools/Enable TextMesh Pro Support...", false, 100)]
        static public void AddTMPPRESENT()
        {
            if (EditorUtility.DisplayDialog("Enable TextMesh Pro Support", "This will enable the Dialogue System's TextMesh Pro support. Your project must already contain the TextMesh Pro package. To continue, press OK. If you need to install TextMesh Pro first, press Cancel.", "OK", "Cancel"))
            {
                MoreEditorUtility.TryAddScriptingDefineSymbols("TMP_PRESENT");
                EditorTools.ReimportScripts();
                EditorUtility.DisplayDialog("TextMesh Pro Support Enabled", "TextMesh Pro support has been enabled. You may need to right-click on the two files named TextMeshProTypewriterEffect and select Reimport to be able to add them to your GameObjects. If you change build platforms, you may need to select this menu item again.", "OK");
            }
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Tools/Enable TextMesh Pro Support...", true)]
        static bool ValidateAddTMPPRESENT()
        {
            return !MoreEditorUtility.DoesScriptingDefineSymbolExist("TMP_PRESENT");
        }

        //=============================================================

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Tools/Enable Super Text Mesh Support...", false, 101)]
        static public void AddUSESTM()
        {
            if (EditorUtility.DisplayDialog("Enable Super Text Mesh Support", "This will enable Super Text Mesh support. Your project must already contain Super Text Mesh.\n\n*IMPORTANT*: Before pressing OK, you MUST move the Clavian folder into the Plugins folder!\n\nTo continue, press OK. If you need to install Super Text Mesh or move the folder first, press Cancel.", "OK", "Cancel"))
            {
                MoreEditorUtility.TryAddScriptingDefineSymbols("USE_STM");
                EditorUtility.DisplayDialog("Super Text Mesh Support Enabled", "Super Text Mesh support has been enabled.", "OK");
            }
        }

        [MenuItem("Tools/Pixel Crushers/Dialogue System/Tools/Enable Super Text Mesh Support...", true)]
        static bool ValidateAddUSESTM()
        {
            return !MoreEditorUtility.DoesScriptingDefineSymbolExist("USE_STM");
        }

        #endregion

        #region About

        private static void ShowAboutWindow()
        {
            string aboutText = "Copyright (c) Pixel Crushers\npixelcrushers.com";
            var version = GetVersion();
            if (!string.IsNullOrEmpty(version))
            {
                aboutText = version + "\n\n" + aboutText;
            }
            EditorUtility.DisplayDialog("Dialogue System for Unity", aboutText, "Close");
        }

        public static string GetVersion()
        {
            var version = string.Empty;
            try
            {
                StreamReader streamReader = new StreamReader(Application.dataPath + "/Plugins/Pixel Crushers/Dialogue System/_README.txt");
                streamReader.ReadLine();    // /*
                streamReader.ReadLine();    // ---
                streamReader.ReadLine();    // Dialogue System for Unity
                version = streamReader.ReadLine().Trim(); // version
                streamReader.Close();
            }
            catch (System.Exception)
            {
            }
            return version;
        }
        #endregion

    }
}
