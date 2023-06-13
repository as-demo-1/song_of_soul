/* [REMOVE THIS LINE]
 * [REMOVE THIS LINE] To use this template, make a copy and remove the lines that start
 * [REMOVE THIS LINE] with "[REMOVE THIS LINE]". Then add your code where the comments indicate.
 * [REMOVE THIS LINE]



using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{
    //*** Rename TemplateConverterPrefs here and in the ConverterWindowTemplate class definition below.
    [Serializable]
    public class TemplateConverterPrefs : AbstractConverterWindowPrefs
    {
        //*** Add any settings your window needs to remember here.
    }

    //*** Rename ConverterWindowTemplate to the name of your converter class:
    public class ConverterWindowTemplate : AbstractConverterWindow<TemplateConverterPrefs>
    {

        //*** Set the source file extension here:
        public override string sourceFileExtension { get { return "txt"; } }

        //*** Set the EditorPrefs key to save preferences under:
        public override string prefsKey { get { return "MyKey"; } }

        //*** Customize this menu item:
        [MenuItem("Tools/Pixel Crushers/Dialogue System/Import/SET MENU NAME...", false, 1)]
        public static void Init()
        {
            EditorWindow.GetWindow(typeof(ConverterWindowTemplate), false, "Converter");
        }

        //*** Basic preferences are stored in a variable named 'prefs' of type Prefs. You can
        //*** create a subclass of Prefs if you need to store additional data. If you do this,
        //*** also override the ClearPrefs(), LoadPrefs(), and SavePrefs() methods.

        //*** This is the main conversion routine.
        //*** Read prefs.SourceFile (or whatever source data you need, if you've overridden
        //*** the prefs object) and copy the data into the dialogue database object.
        protected override void CopySourceToDialogueDatabase(DialogueDatabase database)
        {
            // Add your conversion code here.
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

        //*** This is a subclass of AbstractConverterWindow. All methods in AbstractConverterWindow
        //*** are overrideable, so you can really customize it however you want by overriding
        //*** specific methods.

    }

}



/**/
