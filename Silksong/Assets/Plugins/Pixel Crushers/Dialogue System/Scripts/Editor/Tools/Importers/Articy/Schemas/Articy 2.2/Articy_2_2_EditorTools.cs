#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy.Articy_2_2 {
	
	/// <summary>
	/// This static utility class contains editor-side tools to convert an articy:draft 2.2 XML schema into
	/// a schema-independent ArticyData object.
	/// </summary>
	public static class Articy_2_2_EditorTools {

		public static bool IsSchema(string xmlFilename) {
			return ArticyEditorTools.FileContainsSchemaId(xmlFilename, "http://www.nevigo.com/schemas/articydraft/2.2/XmlContentExport_FullProject.xsd");
		}

        public static ArticyData LoadArticyDataFromXmlFile(string xmlFilename, Encoding encoding) {
            return Articy_2_2_Tools.ConvertExportToArticyData(LoadExportFromXmlFile(xmlFilename, encoding));
        }

        public static ExportType LoadExportFromXmlFile(string xmlFilename, Encoding encoding) {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportType));
            return xmlSerializer.Deserialize(new StreamReader(xmlFilename, encoding)) as ExportType;
        }

	}
	
}
#endif