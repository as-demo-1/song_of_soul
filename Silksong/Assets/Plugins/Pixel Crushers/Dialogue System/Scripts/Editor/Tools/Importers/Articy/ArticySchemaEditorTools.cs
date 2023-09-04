#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using System.Text;

namespace PixelCrushers.DialogueSystem.Articy
{

    /// <summary>
    /// This static utility class loads an arbitrary articy XML file as a schema-independent
    /// ArticyData object, regardless of what version of articy generated the XML file.
    /// </summary>
    public static class ArticySchemaEditorTools
    {

        public static ArticyData LoadArticyDataFromXmlFile(string xmlFilename, Encoding encoding, ConverterPrefs.ConvertDropdownsModes convertDropdownAs = ConverterPrefs.ConvertDropdownsModes.Int, ConverterPrefs prefs = null)
        {
            if (Articy_3_1.Articy_3_1_EditorTools.IsSchema(xmlFilename))
            {
                return Articy_3_1.Articy_3_1_EditorTools.LoadArticyDataFromXmlFile(xmlFilename, encoding, convertDropdownAs, prefs);
            }
            else if (Articy_2_4.Articy_2_4_EditorTools.IsSchema(xmlFilename))
            {
                return Articy_2_4.Articy_2_4_EditorTools.LoadArticyDataFromXmlFile(xmlFilename, encoding, convertDropdownAs, prefs);
            }
            else if (Articy_2_2.Articy_2_2_EditorTools.IsSchema(xmlFilename))
            {
                return Articy_2_2.Articy_2_2_EditorTools.LoadArticyDataFromXmlFile(xmlFilename, encoding);
            }
            else if (Articy_1_4.Articy_1_4_EditorTools.IsSchema(xmlFilename))
            {
                return Articy_1_4.Articy_1_4_EditorTools.LoadArticyDataFromXmlFile(xmlFilename, encoding);
            }
            else
            {
                return null;
            }
        }

    }

}
#endif
