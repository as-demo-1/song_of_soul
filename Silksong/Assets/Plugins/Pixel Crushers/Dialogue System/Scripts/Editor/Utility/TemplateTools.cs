// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Utility methods to work with Template.
    /// </summary>
    public static class TemplateTools
    {

        private const string DialogueDatabaseTemplateKey = "PixelCrushers.DialogueSystem.DatabaseTemplate";

        public static Template FromDefault()
        {
            var template = Template.FromDefault();
            if (EditorGUIUtility.isProSkin)
            {
                template.npcLineColor = new Color(0.75f, 0.25f, 0.25f);
                template.pcLineColor = new Color(0.25f, 0.5f, 1f);
            }
            return template;
        }

        public static Template FromXml(string xml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Template));
            return xmlSerializer.Deserialize(new StringReader(xml)) as Template;
        }

        public static string ToXml(Template template)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Template));
            StringWriter writer = new StringWriter();
            xmlSerializer.Serialize(writer, template);
            return writer.ToString();
        }

        public static Template LoadFromEditorPrefs()
        {
            Template template = null;
            if (EditorPrefs.HasKey(DialogueDatabaseTemplateKey)) template = FromXml(EditorPrefs.GetString(DialogueDatabaseTemplateKey));
            return template ?? Template.FromDefault();
        }

        public static void SaveToEditorPrefs(Template template)
        {
            EditorPrefs.SetString(DialogueDatabaseTemplateKey, ToXml(template));
        }

    }

}
