#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System;

namespace PixelCrushers.DialogueSystem.Articy.Articy_3_1
{

    /// <summary>
    /// This static utility class contains editor-side tools to convert an articy:draft 3.1 XML schema into
    /// a schema-independent ArticyData object.
    /// </summary>
    public static class Articy_3_1_EditorTools
    {

        public static bool IsSchema(string xmlFilename)
        {
            return ArticyEditorTools.FileContainsSchemaId(xmlFilename, "http://www.nevigo.com/schemas/articydraft/3.1/XmlContentExport_FullProject.xsd");
        }

        public static ArticyData LoadArticyDataFromXmlFile(string xmlFilename, Encoding encoding, ConverterPrefs.ConvertDropdownsModes convertDropdownAs = ConverterPrefs.ConvertDropdownsModes.Int, ConverterPrefs prefs = null)
        {
            return Articy_3_1_Tools.ConvertExportToArticyData(LoadExportFromXmlFile(xmlFilename, encoding), convertDropdownAs, prefs);
        }

        public static ExportType LoadExportFromXmlFile(string xmlFilename, Encoding encoding)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportType));
                return xmlSerializer.Deserialize(new StreamReader(xmlFilename, encoding)) as ExportType;
            }
            catch (InvalidOperationException e)
            {
                Debug.LogError("Error reading articy XML. Is there an inconsistency in your articy project such as a misnamed variable in a journey? Exception: " + e.Message);
                return null;
            }
        }

    }

}
#endif