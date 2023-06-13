#if USE_AURORA
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace PixelCrushers.DialogueSystem.Aurora
{

    /// <summary>
    /// Dlg file prefs, used in AuroraConverterPrefs.
    /// </summary>
    [System.Serializable]
    public class DlgFile
    {
        public string filename;
        public int actorID;
        public int conversantID;

        public DlgFile() { }

        public DlgFile(string filename, int actorID, int conversantID)
        {
            this.filename = filename;
            this.actorID = actorID;
            this.conversantID = conversantID;
        }
    }

    /// <summary>
    /// Jrl file prefs.
    /// </summary>
    [System.Serializable]
    public class JrlFile
    {
        public string filename;

        public JrlFile() { }

        public JrlFile(string filename)
        {
            this.filename = filename;
        }
    }

    /// <summary>
    /// Language prefs, used in AuroraConverterPrefs.
    /// </summary>
    [System.Serializable]
    public class Language
    {
        public string languageId;
        public string languageName;

        public Language() { }

        public Language(string languageId, string languageName)
        {
            this.languageId = languageId;
            this.languageName = languageName;
        }

        public static List<Language> DefaultList
        {
            get
            {
                return new List<Language>() {
                    new Language("0", "en"),
                    new Language("1", "fr"),
                    new Language("2", "de"),
                    new Language("3", "it"),
                    new Language("4", "es"),
                    new Language("5", "pl"),
                    new Language("128", "ko"),
                    new Language("129", "zh"),
                    new Language("130", "zh-cn"),
                    new Language("131", "ja"),
                    new Language("256", "ko"),
                    new Language("258", "zh"),
                    new Language("260", "zh-cn")
                };
            }
        }
    }

    /// <summary>
    /// Actor setting prefs, used in AuroraConverterPrefs.
    /// </summary>
    [System.Serializable]
    public class ActorSetting
    {
        public int id;
        public string name;
        public string tag;

        public ActorSetting() { }

        public ActorSetting(int id, string name, string tag)
        {
            this.id = id;
            this.name = name;
            this.tag = tag;
        }

        public static List<ActorSetting> DefaultList
        {
            get
            {
                return new List<ActorSetting>() {
                    new ActorSetting(1, "Player", "Player"),
                    new ActorSetting(2, "NPC", "NPC")
                };
            }
        }
    }

    /// <summary>
    /// Variable setting prefs, used in AuroraConverterPrefs
    /// </summary>
    [System.Serializable]
    public class VariableSetting
    {
        public int customId;
        public string variableName;
        public string usedInDlg;

        public VariableSetting() { }

        public VariableSetting(int customId, string variableName, string usedInDlg)
        {
            this.customId = customId;
            this.variableName = variableName;
            this.usedInDlg = usedInDlg;
        }
    }

    public enum ScriptHandlingType
    {
        LuaComments,
        CustomLuaFunction
    }

    /// <summary>
    /// This class manages Aurora converter prefs. It allows the converter to save
    /// prefs to EditorPrefs between sessions.
    /// </summary>
    [System.Serializable]
    public class AuroraConverterPrefs
    {

        private const string AuroraConverterPrefsKey = "PixelCrushers.DialogueSystem.AuroraConverterSettings";

        public string ProfilesFolder { get; set; }
        public string AlternateSearchFolder { get; set; }
        public string OutputFolder { get; set; }
        public string DatabaseFilename { get; set; }
        public bool Overwrite { get; set; }
        //--- Unused: public bool EnforceUniqueIDs { get; set; }
        public EncodingType EncodingType { get; set; }
        public ScriptHandlingType ScriptHandlingType { get; set; }
        public List<ActorSetting> Actors { get { return actors; } }
        public List<DlgFile> DlgFiles { get { return dlgFiles; } }
        public List<JrlFile> JrlFiles { get { return jrlFiles; } }
        public List<Language> Languages { get { return languages; } }
        public List<VariableSetting> Variables { get { return variables; } }

        private List<ActorSetting> actors = new List<ActorSetting>();
        private List<DlgFile> dlgFiles = new List<DlgFile>();
        private List<JrlFile> jrlFiles = new List<JrlFile>();
        private List<Language> languages = new List<Language>();
        private List<VariableSetting> variables = new List<VariableSetting>();

        public AuroraConverterPrefs() { }

        /// <summary>
        /// Clears the prefs.
        /// </summary>
        public void Clear()
        {
            ProfilesFolder = string.Empty;
            AlternateSearchFolder = string.Empty;
            OutputFolder = string.Empty;
            DatabaseFilename = string.Empty;
            Overwrite = false;
            //--- Unused: EnforceUniqueIDs = true;
            EncodingType = EncodingType.Default;
            actors = ActorSetting.DefaultList;
            dlgFiles.Clear();
            jrlFiles.Clear();
            languages = Language.DefaultList;
            variables.Clear();
        }

        /// <summary>
        /// Sets all conversants to the first NPC.
        /// </summary>
        public void SetDefaultConversant()
        {
            if (actors.Count < 2)
            {
                actors.Add(new ActorSetting(2, "NPC", "NPC"));
            }
            foreach (var dlgFile in DlgFiles)
            {
                dlgFile.conversantID = actors[1].id;
            }
        }

        /// <summary>
        /// Deletes the prefs from EditorPrefs.
        /// </summary>
        public static void DeleteEditorPrefs()
        {
            EditorPrefs.DeleteKey(AuroraConverterPrefsKey);
        }

        /// <summary>
        /// Load the prefs from EditorPrefs.
        /// </summary>
        public static AuroraConverterPrefs Load()
        {
            return FromXml(EditorPrefs.GetString(AuroraConverterPrefsKey));
        }

        /// <summary>
        /// Save the prefs to EditorPrefs.
        /// </summary>
        public void Save()
        {
            EditorPrefs.SetString(AuroraConverterPrefsKey, ToXml());
        }

        /// <summary>
        /// Loads the prefs from XML.
        /// </summary>
        /// <returns>
        /// The prefs.
        /// </returns>
        /// <param name='xml'>
        /// XML.
        /// </param>
        public static AuroraConverterPrefs FromXml(string xml)
        {
            AuroraConverterPrefs prefs = null;
            if (!string.IsNullOrEmpty(xml))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(AuroraConverterPrefs));
                prefs = xmlSerializer.Deserialize(new StringReader(xml)) as AuroraConverterPrefs;
            }
            if (prefs == null)
            {
                prefs = new AuroraConverterPrefs();
                prefs.ProfilesFolder = string.Empty;
                prefs.AlternateSearchFolder = string.Empty;
                prefs.OutputFolder = "Assets";
                prefs.DatabaseFilename = "Dialogue Database";
                prefs.Overwrite = false;
                //--- Unused: prefs.EnforceUniqueIDs = true;
                prefs.EncodingType = EncodingType.Default;
            }
            prefs.CheckLists();
            return prefs;
        }

        /// <summary>
        /// Returns the prefs in XML format.
        /// </summary>
        /// <returns>
        /// The xml.
        /// </returns>
        public string ToXml()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(AuroraConverterPrefs));
            StringWriter writer = new StringWriter();
            xmlSerializer.Serialize(writer, this);
            return writer.ToString();
        }

        /// <summary>
        /// Make sure the lists in the prefs have at least default values.
        /// </summary>		
        private void CheckLists()
        {
            if (actors.Count < 2) actors = ActorSetting.DefaultList;
            if (languages.Count < 1) languages = Language.DefaultList;
        }

        /// <summary>
        /// Looks up a variable setting by custom ID.
        /// </summary>
        /// <returns>
        /// The variable.
        /// </returns>
        /// <param name='customId'>
        /// Custom identifier number (the xxxx in <CUSTOMxxxx>).
        /// </param>
        public VariableSetting GetVariable(int customId)
        {
            return Variables.Find(v => (v.customId == customId));
        }

        public Encoding Encoding { get { return (EncodingType == EncodingType.Default) ? Encoding.Default : EncodingTypeTools.GetEncoding(EncodingType); } }

    }

}
#endif
