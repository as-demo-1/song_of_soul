#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using System.Text;

namespace PixelCrushers.DialogueSystem.Articy
{

    /// <summary>
    /// This static utility class loads an arbitrary articy XML as a schema-independent
    /// ArticyData object, regardless of what version of articy generated the XML file.
    /// </summary>
    public static class ArticySchemaTools
    {

        public static ArticyData LoadArticyDataFromXmlData(string xmlData, Encoding encoding, ConverterPrefs.ConvertDropdownsModes convertDropdownAs = ConverterPrefs.ConvertDropdownsModes.Int)
        {
            if (Articy_3_1.Articy_3_1_Tools.IsSchema(xmlData))
            {
                return Articy_3_1.Articy_3_1_Tools.LoadArticyDataFromXmlData(xmlData, encoding, convertDropdownAs);
            }
            else if (Articy_2_4.Articy_2_4_Tools.IsSchema(xmlData))
            {
                return Articy_2_4.Articy_2_4_Tools.LoadArticyDataFromXmlData(xmlData, encoding, convertDropdownAs);
            }
            else if (Articy_2_2.Articy_2_2_Tools.IsSchema(xmlData))
            {
                return Articy_2_2.Articy_2_2_Tools.LoadArticyDataFromXmlData(xmlData, encoding);
            }
            else if (Articy_1_4.Articy_1_4_Tools.IsSchema(xmlData))
            {
                return Articy_1_4.Articy_1_4_Tools.LoadArticyDataFromXmlData(xmlData, encoding);
            }
            else
            {
                return null;
            }
        }

        public static ArticyData LoadArticyDataFromXmlData(string xmlData, ConverterPrefs prefs)
        {
            if (Articy_3_1.Articy_3_1_Tools.IsSchema(xmlData))
            {
                return Articy_3_1.Articy_3_1_Tools.LoadArticyDataFromXmlData(xmlData, prefs.Encoding, prefs.ConvertDropdownsAs, prefs);
            }
            else if (Articy_2_4.Articy_2_4_Tools.IsSchema(xmlData))
            {
                return Articy_2_4.Articy_2_4_Tools.LoadArticyDataFromXmlData(xmlData, prefs.Encoding, prefs.ConvertDropdownsAs, prefs);
            }
            else if (Articy_2_2.Articy_2_2_Tools.IsSchema(xmlData))
            {
                return Articy_2_2.Articy_2_2_Tools.LoadArticyDataFromXmlData(xmlData, prefs.Encoding);
            }
            else if (Articy_1_4.Articy_1_4_Tools.IsSchema(xmlData))
            {
                return Articy_1_4.Articy_1_4_Tools.LoadArticyDataFromXmlData(xmlData, prefs.Encoding);
            }
            else
            {
                return null;
            }
        }

    }

}
#endif
