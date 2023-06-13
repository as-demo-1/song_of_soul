#if USE_AURORA
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.Aurora
{

    /// <summary>
    /// XML schema for Aurora dialog files.
    /// </summary>
    [XmlRoot("gff")]
    [System.Serializable]
    public class Dlg
    {
        [XmlAttribute("name")]
        public string name;

        [XmlAttribute("type")]
        public string type;

        [XmlAttribute("version")]
        public string version;

        [XmlElement("struct")]
        public Struct topLevelStruct;

        [XmlIgnore]
        public string EndConversation { get { return topLevelStruct.GetElementValue("EndConversation"); } }

        [XmlIgnore]
        public string EndConverAbort { get { return topLevelStruct.GetElementValue("EndConverAbort"); } }

        [XmlIgnore]
        public List<Struct> EntryList { get { return topLevelStruct.GetElementStructs("EntryList"); } }

        [XmlIgnore]
        public List<Struct> StartingList { get { return topLevelStruct.GetElementStructs("StartingList"); } }

        [XmlIgnore]
        public List<Struct> ReplyList { get { return topLevelStruct.GetElementStructs("ReplyList"); } }

        public static Dlg Load(TextAsset xmlFile)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Dlg));
            return xmlSerializer.Deserialize(new StringReader(xmlFile.text)) as Dlg;
        }

        public static Dlg Load(string filename)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Dlg));
            return xmlSerializer.Deserialize(new StreamReader(filename)) as Dlg;
        }

        public static Dlg Load(string filename, Encoding encoding)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Dlg));
            return xmlSerializer.Deserialize(new StreamReader(filename, encoding)) as Dlg;
        }
    }

    [System.Serializable]
    public class Struct
    {
        [XmlAttribute("id")]
        public string id;

        [XmlElement("element")]
        public List<Element> elements = new List<Element>();

        [XmlIgnore]
        public int ID { get { return Tools.StringToInt(id); } }

        [XmlIgnore]
        public string Comment { get { return GetElementValue("Comment"); } }

        [XmlIgnore]
        public string Speaker { get { return GetElementValue("Speaker"); } }

        [XmlIgnore]
        public bool ShowOnce { get { return string.Equals(GetElementValue("ShowOnce"), "1"); } }

        [XmlIgnore]
        public string Quest { get { return GetElementValue("Quest"); } }

        [XmlIgnore]
        public string QuestEntry { get { return GetElementValue("QuestEntry"); } }

        [XmlIgnore]
        public string Script { get { return GetElementValue("Script"); } }

        [XmlIgnore]
        public string Active { get { return GetElementValue("Active"); } }

        [XmlIgnore]
        public List<Struct> EntriesList { get { return GetElementStructs("EntriesList"); } }

        [XmlIgnore]
        public List<Struct> RepliesList { get { return GetElementStructs("RepliesList"); } }

        [XmlIgnore]
        public List<LocalString> Text { get { return GetLocalStrings("Text"); } }

        [XmlIgnore]
        public int Index { get { return Tools.StringToInt(GetElementValue("Index")); } }

        [XmlIgnore]
        public bool IsChild { get { return string.Equals(GetElementValue("IsChild"), "1"); } }

        [XmlIgnore]
        public bool End { get { return string.Equals(GetElementValue("End"), "1"); } }

        public Element GetElement(string elementName)
        {
            return elements.Find(e => string.Equals(e.name, elementName));
        }

        public string GetElementValue(string elementName)
        {
            Element element = GetElement(elementName);
            return (element != null) ? element.value : string.Empty;
        }
        public List<Struct> GetElementStructs(string elementName)
        {
            Element element = GetElement(elementName);
            return (element != null) ? element.structs : new List<Struct>();
        }

        public List<LocalString> GetLocalStrings(string elementName)
        {
            Element element = GetElement(elementName);
            if (element == null) Debug.Log(elementName + " IS NULL");
            return (element != null) ? element.localStrings : new List<LocalString>();
        }

    }

    [System.Serializable]
    public class Element
    {
        [XmlAttribute("name")]
        public string name;

        [XmlAttribute("type")]
        public string type;

        [XmlAttribute("value")]
        public string value;

        [XmlElement("struct")]
        public List<Struct> structs = new List<Struct>();

        [XmlElement("localString")]
        public List<LocalString> localStrings = new List<LocalString>();
    }

    [System.Serializable]
    public class LocalString
    {
        [XmlAttribute("languageId")]
        public string languageId;

        [XmlAttribute("value")]
        public string _data;

        [XmlElement("value")]
        public string cdata;

        public const string HungarianLanguageIDString = "260";

        [XmlIgnore]
        public string value
        {
            //---Was: get { return !string.IsNullOrEmpty(cdata) ? cdata : _data; }
            get
            {
                // Special handling for Hungarian characters that NWN doesn't export properly:
                var text = !string.IsNullOrEmpty(cdata) ? cdata : _data;
                return string.Equals(languageId, HungarianLanguageIDString)
                    ? text.Replace("Õ", "Ő").Replace("õ", "ő").Replace("û", "ű").Replace("Û", "Ű").Replace("ũ", "ű").Replace("Ũ", "Ű")
                    : text;
            }
        }

        [XmlIgnore]
        public int LanguageID { get { return Tools.StringToInt(languageId); } }
    }

}
#endif
