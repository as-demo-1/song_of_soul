#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy.Articy_2_4
{

    /// <summary>
    /// This static utility class contains editor-side tools to convert an articy:draft 2.4 XML schema into
    /// a schema-independent ArticyData object.
    /// </summary>
    public static class Articy_2_4_EditorTools
    {

        public static bool IsSchema(string xmlFilename)
        {
            return ArticyEditorTools.FileContainsSchemaId(xmlFilename, "http://www.nevigo.com/schemas/articydraft/2.4/XmlContentExport_FullProject.xsd");
        }

        public static ArticyData LoadArticyDataFromXmlFile(string xmlFilename, Encoding encoding, ConverterPrefs.ConvertDropdownsModes convertDropdownAs = ConverterPrefs.ConvertDropdownsModes.Int, ConverterPrefs prefs = null)
        {
            return Articy_2_4_Tools.ConvertExportToArticyData(LoadExportFromXmlFile(xmlFilename, encoding), convertDropdownAs, prefs);
        }

        public static ExportType LoadExportFromXmlFile(string xmlFilename, Encoding encoding)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportType));
            return xmlSerializer.Deserialize(new StreamReader(xmlFilename, encoding)) as ExportType;
        }

    }

}
#endif