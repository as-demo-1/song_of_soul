#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using System;
using System.Text;

namespace PixelCrushers.DialogueSystem.Articy
{

    [Serializable]
    public class ArticyEmVars
    {
        public string color;
        public string bold;
        public string italic;
        public string underline;
    }

    [Serializable]
    public class ArticyEmVarSet
    {
        public ArticyEmVars[] emVars;

        public ArticyEmVarSet()
        {
            InitializeEmVars();
        }

        public void InitializeEmVars()
        {
            emVars = new ArticyEmVars[DialogueDatabase.NumEmphasisSettings];
            for (int i = 0; i < DialogueDatabase.NumEmphasisSettings; i++)
            {
                emVars[i] = new ArticyEmVars();
            }
        }
    }

    /// <summary>
    /// This class manages articy converter prefs. It allows the converter to save
    /// prefs to EditorPrefs between sessions.
    /// </summary>
    public class ConverterPrefs
    {

        public enum FlowFragmentModes { NestedConversationGroups, ConversationGroups, Quests, Ignore }

        public enum StageDirModes { Sequences, Nothing, Description }

        public enum ConvertDropdownsModes { Int, TechnicalName, DisplayName }

        public enum ConvertSlotsModes { DisplayName, ID, TechnicalName }

        public enum RecursionModes { Off, On }

        public string ProjectFilename { get; set; }
        public string PortraitFolder { get; set; }
        public bool UseDefaultActorsIfNoneAssignedToDialogue { get; set; }
        public StageDirModes StageDirectionsMode { get; set; }
        public FlowFragmentModes FlowFragmentMode { get; set; }
        public bool CreateConversationsForLooseFlow { get; set; }
        public string OtherScriptFields { get; set; }
        public string DocumentsSubmenu { get; set; }
        public bool ImportDocuments { get; set; }
        public string TextTableDocument { get; set; }
        public string OutputFolder { get; set; }
        public bool Overwrite { get; set; }
        public ConversionSettings ConversionSettings { get; set; }
        public EncodingType EncodingType { get; set; }
        public RecursionModes RecursionMode { get; set; }
        public ConvertDropdownsModes ConvertDropdownsAs { get; set; }
        public ConvertSlotsModes ConvertSlotsAs { get; set; }
        public bool UseTechnicalNames { get; set; }
        public bool SetDisplayName { get; set; }
        public bool CustomDisplayName { get; set; }
        public bool DirectConversationLinksToEntry1 { get; set; }
        public bool ConvertMarkupToRichText { get; set; }
        public bool SplitTextOnPipes { get; set; }
        public string FlowFragmentScript { get; set; }
        public string VoiceOverProperty { get; set; }
        public string LocalizationXlsx { get; set; }

        public ArticyEmVarSet emVarSet = new ArticyEmVarSet();

        public const string DefaultFlowFragmentScript = "OnFlowFragment";
        public const string DefaultVoiceOverProperty = "VoiceOverFile";

        public Encoding Encoding { get { return EncodingTypeTools.GetEncoding(EncodingType); } }

        public ConverterPrefs()
        {
            ProjectFilename = string.Empty;
            PortraitFolder = string.Empty;
            UseDefaultActorsIfNoneAssignedToDialogue = true;
            StageDirectionsMode = StageDirModes.Sequences;
            FlowFragmentMode = FlowFragmentModes.ConversationGroups;
            CreateConversationsForLooseFlow = false;
            OtherScriptFields = string.Empty;
            DocumentsSubmenu = string.Empty;
            ImportDocuments = true;
            TextTableDocument = string.Empty;
            OutputFolder = "Assets";
            Overwrite = false;
            ConversionSettings = new ConversionSettings();
            EncodingType = EncodingType.Default;
            RecursionMode = RecursionModes.On;
            ConvertDropdownsAs = ConvertDropdownsModes.Int;
            ConvertSlotsAs = ConvertSlotsModes.DisplayName;
            UseTechnicalNames = false;
            SetDisplayName = false;
            CustomDisplayName = false;
            DirectConversationLinksToEntry1 = false;
            ConvertMarkupToRichText = true;
            SplitTextOnPipes = true;
            FlowFragmentScript = DefaultFlowFragmentScript;
            VoiceOverProperty = DefaultVoiceOverProperty;
            LocalizationXlsx = string.Empty;
        }

        public void ReviewSpecialProperties(ArticyData articyData)
        {
            foreach (var articyEntity in articyData.entities.Values)
            {
                var conversionSetting = ConversionSettings.GetConversionSetting(articyEntity.id);
                if (conversionSetting.Include)
                {
                    if (ArticyConverter.HasField(articyEntity.features, "IsNPC", false)) conversionSetting.Category = EntityCategory.NPC;
                    if (ArticyConverter.HasField(articyEntity.features, "IsPlayer", true)) conversionSetting.Category = EntityCategory.Player;
                    if (ArticyConverter.HasField(articyEntity.features, "IsItem", true)) conversionSetting.Category = EntityCategory.Item;
                    if (ArticyConverter.HasField(articyEntity.features, "IsQuest", true)) conversionSetting.Category = EntityCategory.Quest;
                }
            }
        }

    }

}
#endif
