using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{

    public class JsonImportWindow : AbstractConverterWindow<AbstractConverterWindowPrefs>
    {

        //*** Set the source file extension here:
        public override string sourceFileExtension { get { return "json"; } }

        //*** Set the EditorPrefs key to save preferences under:
        public override string prefsKey { get { return "PixelCrushers.DialogueSystem.JsonImportSettings"; } }

        //*** Customize this menu item:
        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/JSON...", false, 1)]
        public static void Init()
        {
            EditorWindow.GetWindow(typeof(JsonImportWindow), false, "JSON to DS");
        }

        //*** Basic preferences are stored in a variable named 'prefs' of type Prefs. You can
        //*** create a subclass of Prefs if you need to store additional data. If you do this,
        //*** also override the ClearPrefs(), LoadPrefs(), and SavePrefs() methods.

        //*** This is the main conversion routine.
        //*** Read prefs.SourceFile (or whatever source data you need, if you've overridden
        //*** the prefs object) and copy the data into the dialogue database object.
        protected override void CopySourceToDialogueDatabase(DialogueDatabase database)
        {
            string json;
            try
            {
                json = System.IO.File.ReadAllText(prefs.sourceFilename);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Dialogue System: Couldn't load JSON file " + prefs.sourceFilename +
                    ". Exception: " + e.Message);
                return;
            }
            JsonUtility.FromJsonOverwrite(json, database);
        }

        protected override void DrawOverwriteCheckbox()
        {
            prefs.overwrite = EditorGUILayout.Toggle(new GUIContent("Overwrite", "Overwrite database if it already exists"),
                                                     prefs.overwrite);
        }

        //*** Uncomment this method and change it if you want to change the way the converter
        //*** touches up the database after copying the source data. The base version of this
        //*** method edits the START nodes of all conversations and sets their Sequence fields
        //*** to None(). For example, if you know where the actors' portrait textures are,
        //*** You can also call FindPortraitTextures(database, portraitFolder), which will 
        //*** assign the actors' portrait images based on their Textures fields.
        //protected override void TouchUpDialogueDatabase(DialogueDatabase database) {
        //	base.TouchUpDialogueDatabase(database);
        //}

    }

}
