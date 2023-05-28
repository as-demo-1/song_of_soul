#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy.Articy_1_4 {

	/// <summary>
	/// This static utility class contains editor-side tools to convert an articy:draft 1.4 XML schema into
	/// a schema-independent ArticyData object.
	/// </summary>
	public static class Articy_1_4_EditorTools {

		public static bool IsSchema(string xmlFilename) {
			return ArticyEditorTools.FileContainsSchemaId(xmlFilename, "http://www.nevigo.com/schemas/articydraft/1.4/XmlContentExport_FullProject.xsd");
		}

        public static ArticyData LoadArticyDataFromXmlFile(string xmlFilename, Encoding encoding) {
            return Articy_1_4_Tools.ConvertExportToArticyData(LoadExportFromXmlFile(xmlFilename, encoding));
        }

        public static ExportType LoadExportFromXmlFile(string xmlFilename, Encoding encoding) {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportType));
            return xmlSerializer.Deserialize(new StreamReader(xmlFilename, encoding)) as ExportType;
        }

    }

}
#endif