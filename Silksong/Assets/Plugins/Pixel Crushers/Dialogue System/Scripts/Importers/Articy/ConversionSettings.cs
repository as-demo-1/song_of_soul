#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy
{

    /// <summary>
    /// This class contains articy project conversion settings. It's used by ConverterPrefs.
    /// </summary>
    public class ConversionSettings
    {

        private Dictionary<string, ConversionSetting> dict = new Dictionary<string, ConversionSetting>();

        public List<ConversionSetting> list = new List<ConversionSetting>();

        public enum DropdownOverrideMode { UseGlobalSetting, Int, TechnicalName, DisplayName }

        [Serializable]
        public class DropdownOverrideSetting
        {
            public string id = string.Empty;
            public DropdownOverrideMode mode = DropdownOverrideMode.UseGlobalSetting;
            public DropdownOverrideSetting() { }
            public DropdownOverrideSetting(string id, DropdownOverrideMode mode = DropdownOverrideMode.UseGlobalSetting)
            {
                this.id = id;
                this.mode = mode;
            }
        }

        private Dictionary<string, DropdownOverrideSetting> dropdownOverrideDict = new Dictionary<string, DropdownOverrideSetting>();

        public List<DropdownOverrideSetting> dropdownOverrideList = new List<DropdownOverrideSetting>();

        public static ConversionSettings FromXml(string xml)
        {
            ConversionSettings conversionSettings = null;
            if (string.IsNullOrEmpty(xml))
            {
                conversionSettings = new ConversionSettings();
            }
            else
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConversionSettings));
                conversionSettings = xmlSerializer.Deserialize(new StringReader(xml)) as ConversionSettings;
                if (conversionSettings != null) conversionSettings.AfterDeserialization();
            }
            return conversionSettings;
        }

        public string ToXml()
        {
            BeforeSerialization();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConversionSettings));
            StringWriter writer = new StringWriter();
            xmlSerializer.Serialize(writer, this);
            return writer.ToString();
        }

        private void BeforeSerialization()
        {
            list.Clear();
            foreach (var entry in dict)
            {
                list.Add(entry.Value);
            }
            dropdownOverrideList.Clear();
            foreach (var entry in dropdownOverrideDict)
            {
                dropdownOverrideList.Add(entry.Value);
            }
        }

        private void AfterDeserialization()
        {
            dict.Clear();
            foreach (var element in list)
            {
                dict.Add(element.Id, element);
            }
            dropdownOverrideDict.Clear();
            foreach (var element in dropdownOverrideList)
            {
                dropdownOverrideDict.Add(element.id, element);
            }
        }

        public void Clear()
        {
            dict.Clear();
            list.Clear();
            dropdownOverrideDict.Clear();
            dropdownOverrideList.Clear();
        }

        public ConversionSetting GetConversionSetting(string Id)
        {
            if (string.IsNullOrEmpty(Id)) return null;
            if (!dict.ContainsKey(Id)) dict[Id] = new ConversionSetting(Id);
            return dict[Id];
        }

        public bool ConversionSettingExists(string Id)
        {
            return !string.IsNullOrEmpty(Id) && dict.ContainsKey(Id);
        }

        public DropdownOverrideSetting GetDropdownOverrideSetting(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!dropdownOverrideDict.ContainsKey(id))
            {
                var newEntry = new DropdownOverrideSetting(id);
                dropdownOverrideDict.Add(id, newEntry);
                dropdownOverrideList.Add(newEntry);
            }
            return dropdownOverrideDict[id];
        }

        public void AllDropdownOverrides(DropdownOverrideMode mode)
        {
            foreach (var setting in dropdownOverrideList)
            {
                setting.mode = mode;
            }
        }

    }

}
#endif
