#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using System.IO;
using System.Xml.Serialization;
using UnityEditor;

namespace PixelCrushers.DialogueSystem.Articy
{

    /// <summary>
    /// This class provides editor tools to manage articy converter prefs. It allows the converter to save
    /// prefs to EditorPrefs between sessions.
    /// </summary>
    public static class ConverterPrefsTools
    {

        private const string ArticyProjectFilenameKey = "PixelCrushers.DialogueSystem.ArticyProjectFilename";
        private const string ArticyPortraitFolderKey = "PixelCrushers.DialogueSystem.ArticyPortraitFolder";
        private const string ArticyStageDirectionsModeKey = "PixelCrushers.DialogueSystem.StageDirectionsMode";
        private const string ArticyUseDefaultActorsIfNoneKey = "PixelCrushers.DialogueSystem.UseDefaultActorsIfNone";
        private const string ArticyFlowFragmentModeKey = "PixelCrushers.DialogueSystem.FlowFragmentMode";
        private const string ArticyLooseFlowKey = "PixelCrushers.DialogueSystem.ArticyConvertLooseFlow";
        private const string ArticyOtherScriptFieldsKey = "PixelCrushers.DialogueSystem.OtherScriptFields";
        private const string ArticyDocumentsSubmenuKey = "PixelCrushers.DialogueSystem.ArticyDocumentsSubmenu";
        private const string ArticyImportDocumentsKey = "PixelCrushers.DialogueSystem.ArticyImportDocuments";
        private const string ArticyTextTableDocumentKey = "PixelCrushers.DialogueSystem.ArticyTextTableDocument";
        private const string ArticyOutputFolderKey = "PixelCrushers.DialogueSystem.ArticyOutput";
        private const string ArticyOverwriteKey = "PixelCrushers.DialogueSystem.ArticyOverwrite";
        private const string ArticyConversionSettingsKey = "PixelCrushers.DialogueSystem.ArticyConversionSettings";
        private const string ArticyEncodingKey = "PixelCrushers.DialogueSystem.ArticyEncoding";
        private const string ArticyRecursionKey = "PixelCrushers.DialogueSystem.ArticyRecursion";
        private const string ArticyDropdownsKey = "PixelCrushers.DialogueSystem.ArticyDropdowns";
        private const string ArticySlotsKey = "PixelCrushers.DialogueSystem.ArticySlots";
        private const string ArticyUseTechnicalNamesKey = "PixelCrushers.DialogueSystem.UseTechnicalNames";
        private const string ArticyCustomDisplayNameKey = "PixelCrushers.DialogueSystem.ArticyCustomDisplayName";
        private const string ArticyDirectConversationLinksToEntry1Key = "PixelCrushers.DialogueSystem.DirectConversationLinksToEntry1";
        private const string ArticyConvertMarkupToRichTextKey = "PixelCrushers.DialogueSystem.ArticyConvertMarkupToRichText";
        private const string ArticySplitTextOnPipesKey = "PixelCrushers.DialogueSystem.SplitTextOnPipes";
        private const string ArticyFlowFragmentScriptKey = "PixelCrushers.DialogueSystem.ArticyFlowFragmentScript";
        private const string ArticyVoiceOverPropertyKey = "PixelCrushers.DialogueSystem.ArticyVoiceOverPropertyKey";
        private const string ArticyLocalizationXlsKey = "PixelCrushers.DialogueSystem.ArticyLocalizationXlsxKey";
        private const string ArticyEmVarsKey = "PixelCrushers.DialogueSystem.ArticyEmVars";

        public static ConverterPrefs Load()
        {
            var converterPrefs = new ConverterPrefs();
            converterPrefs.ProjectFilename = EditorPrefs.GetString(ArticyProjectFilenameKey);
            converterPrefs.PortraitFolder = EditorPrefs.GetString(ArticyPortraitFolderKey);
            converterPrefs.StageDirectionsMode = EditorPrefs.HasKey(ArticyStageDirectionsModeKey) ? (ConverterPrefs.StageDirModes)EditorPrefs.GetInt(ArticyStageDirectionsModeKey) : ConverterPrefs.StageDirModes.Sequences;
            converterPrefs.UseDefaultActorsIfNoneAssignedToDialogue = EditorPrefs.HasKey(ArticyUseDefaultActorsIfNoneKey) ? EditorPrefs.GetBool(ArticyUseDefaultActorsIfNoneKey) : true;
            converterPrefs.FlowFragmentMode = (ConverterPrefs.FlowFragmentModes)(EditorPrefs.HasKey(ArticyFlowFragmentModeKey) ? EditorPrefs.GetInt(ArticyFlowFragmentModeKey) : 0);
            converterPrefs.CreateConversationsForLooseFlow = EditorPrefs.GetBool(ArticyLooseFlowKey, false);
            converterPrefs.OtherScriptFields = EditorPrefs.GetString(ArticyOtherScriptFieldsKey, string.Empty);
            converterPrefs.DocumentsSubmenu = EditorPrefs.GetString(ArticyDocumentsSubmenuKey);
            converterPrefs.ImportDocuments = EditorPrefs.GetBool(ArticyImportDocumentsKey, true);
            converterPrefs.TextTableDocument = EditorPrefs.GetString(ArticyTextTableDocumentKey);
            converterPrefs.OutputFolder = EditorPrefs.GetString(ArticyOutputFolderKey, "Assets");
            converterPrefs.Overwrite = EditorPrefs.GetBool(ArticyOverwriteKey, false);
            converterPrefs.ConversionSettings = ConversionSettings.FromXml(EditorPrefs.GetString(ArticyConversionSettingsKey));
            converterPrefs.EncodingType = EditorPrefs.HasKey(ArticyEncodingKey) ? (EncodingType)EditorPrefs.GetInt(ArticyEncodingKey) : EncodingType.Default;
            converterPrefs.RecursionMode = EditorPrefs.HasKey(ArticyRecursionKey) ? (ConverterPrefs.RecursionModes)EditorPrefs.GetInt(ArticyRecursionKey) : ConverterPrefs.RecursionModes.On;
            converterPrefs.ConvertDropdownsAs = EditorPrefs.HasKey(ArticyDropdownsKey) ? (ConverterPrefs.ConvertDropdownsModes)EditorPrefs.GetInt(ArticyDropdownsKey) : ConverterPrefs.ConvertDropdownsModes.Int;
            converterPrefs.ConvertSlotsAs = EditorPrefs.HasKey(ArticySlotsKey) ? (ConverterPrefs.ConvertSlotsModes)EditorPrefs.GetInt(ArticySlotsKey) : ConverterPrefs.ConvertSlotsModes.DisplayName;
            converterPrefs.UseTechnicalNames = EditorPrefs.GetBool(ArticyUseTechnicalNamesKey, false);
            converterPrefs.CustomDisplayName = EditorPrefs.GetBool(ArticyCustomDisplayNameKey, false);
            converterPrefs.DirectConversationLinksToEntry1 = EditorPrefs.GetBool(ArticyDirectConversationLinksToEntry1Key, false);
            converterPrefs.ConvertMarkupToRichText = EditorPrefs.GetBool(ArticyConvertMarkupToRichTextKey, true);
            converterPrefs.SplitTextOnPipes = EditorPrefs.GetBool(ArticySplitTextOnPipesKey, true);
            converterPrefs.FlowFragmentScript = EditorPrefs.GetString(ArticyFlowFragmentScriptKey, ConverterPrefs.DefaultFlowFragmentScript);
            converterPrefs.VoiceOverProperty = EditorPrefs.GetString(ArticyVoiceOverPropertyKey, ConverterPrefs.DefaultVoiceOverProperty);
            converterPrefs.LocalizationXlsx = EditorPrefs.GetString(ArticyLocalizationXlsKey);
            converterPrefs.emVarSet = ArticyEmVarSetFromXML(EditorPrefs.GetString(ArticyEmVarsKey));
            return converterPrefs;
        }

        public static void Save(ConverterPrefs converterPrefs)
        {
            EditorPrefs.SetString(ArticyProjectFilenameKey, converterPrefs.ProjectFilename);
            EditorPrefs.SetString(ArticyPortraitFolderKey, converterPrefs.PortraitFolder);
            EditorPrefs.SetInt(ArticyStageDirectionsModeKey, (int)converterPrefs.StageDirectionsMode);
            EditorPrefs.SetBool(ArticyUseDefaultActorsIfNoneKey, converterPrefs.UseDefaultActorsIfNoneAssignedToDialogue);
            EditorPrefs.SetInt(ArticyFlowFragmentModeKey, (int)converterPrefs.FlowFragmentMode);
            EditorPrefs.SetBool(ArticyLooseFlowKey, converterPrefs.CreateConversationsForLooseFlow);
            EditorPrefs.SetString(ArticyOtherScriptFieldsKey, converterPrefs.OtherScriptFields);
            EditorPrefs.SetString(ArticyDocumentsSubmenuKey, converterPrefs.DocumentsSubmenu);
            EditorPrefs.SetBool(ArticyImportDocumentsKey, converterPrefs.ImportDocuments);
            EditorPrefs.SetString(ArticyTextTableDocumentKey, converterPrefs.TextTableDocument);
            EditorPrefs.SetString(ArticyOutputFolderKey, converterPrefs.OutputFolder);
            EditorPrefs.SetBool(ArticyOverwriteKey, converterPrefs.Overwrite);
            EditorPrefs.SetString(ArticyConversionSettingsKey, converterPrefs.ConversionSettings.ToXml());
            EditorPrefs.SetInt(ArticyEncodingKey, (int)converterPrefs.EncodingType);
            EditorPrefs.SetInt(ArticyRecursionKey, (int)converterPrefs.RecursionMode);
            EditorPrefs.SetInt(ArticyDropdownsKey, (int)converterPrefs.ConvertDropdownsAs);
            EditorPrefs.SetInt(ArticySlotsKey, (int)converterPrefs.ConvertSlotsAs);
            EditorPrefs.SetBool(ArticyUseTechnicalNamesKey, converterPrefs.UseTechnicalNames);
            EditorPrefs.SetBool(ArticyCustomDisplayNameKey, converterPrefs.CustomDisplayName);
            EditorPrefs.SetBool(ArticyDirectConversationLinksToEntry1Key, converterPrefs.DirectConversationLinksToEntry1);
            EditorPrefs.SetBool(ArticyConvertMarkupToRichTextKey, converterPrefs.ConvertMarkupToRichText);
            EditorPrefs.SetBool(ArticySplitTextOnPipesKey, converterPrefs.SplitTextOnPipes);
            EditorPrefs.SetString(ArticyFlowFragmentScriptKey, converterPrefs.FlowFragmentScript);
            EditorPrefs.SetString(ArticyVoiceOverPropertyKey, converterPrefs.VoiceOverProperty);
            EditorPrefs.SetString(ArticyLocalizationXlsKey, converterPrefs.LocalizationXlsx);
            EditorPrefs.SetString(ArticyEmVarsKey, ArticyEmVarSetToXML(converterPrefs.emVarSet));
        }

        public static void DeleteEditorPrefs()
        {
            EditorPrefs.DeleteKey(ArticyProjectFilenameKey);
            EditorPrefs.DeleteKey(ArticyPortraitFolderKey);
            EditorPrefs.DeleteKey(ArticyStageDirectionsModeKey);
            EditorPrefs.DeleteKey(ArticyUseDefaultActorsIfNoneKey);
            EditorPrefs.DeleteKey(ArticyFlowFragmentModeKey);
            EditorPrefs.DeleteKey(ArticyLooseFlowKey);
            EditorPrefs.DeleteKey(ArticyOtherScriptFieldsKey);
            EditorPrefs.DeleteKey(ArticyDocumentsSubmenuKey);
            EditorPrefs.DeleteKey(ArticyImportDocumentsKey);
            EditorPrefs.DeleteKey(ArticyTextTableDocumentKey);
            EditorPrefs.DeleteKey(ArticyOutputFolderKey);
            EditorPrefs.DeleteKey(ArticyOverwriteKey);
            EditorPrefs.DeleteKey(ArticyConversionSettingsKey);
            EditorPrefs.DeleteKey(ArticyEncodingKey);
            EditorPrefs.DeleteKey(ArticyRecursionKey);
            EditorPrefs.DeleteKey(ArticyDropdownsKey);
            EditorPrefs.DeleteKey(ArticySlotsKey);
            EditorPrefs.DeleteKey(ArticyUseTechnicalNamesKey);
            EditorPrefs.DeleteKey(ArticyCustomDisplayNameKey);
            EditorPrefs.DeleteKey(ArticyDirectConversationLinksToEntry1Key);
            EditorPrefs.DeleteKey(ArticyConvertMarkupToRichTextKey);
            EditorPrefs.DeleteKey(ArticySplitTextOnPipesKey);
            EditorPrefs.DeleteKey(ArticyFlowFragmentScriptKey);
            EditorPrefs.DeleteKey(ArticyVoiceOverPropertyKey);
            EditorPrefs.DeleteKey(ArticyLocalizationXlsKey);
            EditorPrefs.DeleteKey(ArticyEmVarsKey);
        }

        private static ArticyEmVarSet ArticyEmVarSetFromXML(string xml)
        {
            ArticyEmVarSet emVarSet = null;
            if (!string.IsNullOrEmpty(xml))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ArticyEmVarSet));
                emVarSet = xmlSerializer.Deserialize(new StringReader(xml)) as ArticyEmVarSet;
            }
            return (emVarSet != null) ? emVarSet : new ArticyEmVarSet();
        }

        private static string ArticyEmVarSetToXML(ArticyEmVarSet emVarSet)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ArticyEmVarSet));
            StringWriter writer = new StringWriter();
            xmlSerializer.Serialize(writer, emVarSet);
            return writer.ToString();
        }

    }

}
#endif
