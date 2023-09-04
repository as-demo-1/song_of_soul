#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace PixelCrushers.DialogueSystem.Articy.Articy_3_1
{

    /// <summary>
    /// This static utility class contains tools to convert articy:draft 2.4/3.0 XML data into
    /// a schema-independent ArticyData object. This is also fully compatible with articy:draft 3,
	/// since 3.0.7's updated schema only adds the "String" GlobalVariableType, which was added
	/// to the Dialogue System's articy 2.4 schema already.
    /// </summary>
    public static class Articy_3_1_Tools
    {

        private static ConverterPrefs.ConvertDropdownsModes _convertDropdownAs = ConverterPrefs.ConvertDropdownsModes.Int; // Convenience variable.

        private static ConverterPrefs.ConvertSlotsModes _convertSlotsAs = ConverterPrefs.ConvertSlotsModes.DisplayName; // Convenience variable.

        private static ExportType _currentExport = null; // Convenience variable so we don't have to pass it everywhere.

        private static ConverterPrefs _prefs = null; // Convenience variable.

        private static int documentDepth = 0;

        private static bool isInTextTableDocument = false;

        public static bool IsSchema(string xmlFilename)
        {
            return ArticyTools.DataContainsSchemaId(xmlFilename, "http://www.nevigo.com/schemas/articydraft/3.1/XmlContentExport_FullProject.xsd");
        }

        public static ArticyData LoadArticyDataFromXmlData(string xmlData, Encoding encoding, ConverterPrefs.ConvertDropdownsModes convertDropdownAs = ConverterPrefs.ConvertDropdownsModes.Int, ConverterPrefs prefs = null)
        {
            return ConvertExportToArticyData(LoadFromXmlData(xmlData, encoding), convertDropdownAs, prefs);
        }

        public static ExportType LoadFromXmlData(string xmlData, Encoding encoding)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportType));
            return xmlSerializer.Deserialize(new StringReader(xmlData)) as ExportType;
        }

        public static bool IsExportValid(ExportType export)
        {
            return (export != null) && (export.Content != null) && (export.Content.Items != null);
        }

        public static ArticyData ConvertExportToArticyData(ExportType export, ConverterPrefs.ConvertDropdownsModes convertDropdownAs = ConverterPrefs.ConvertDropdownsModes.Int, ConverterPrefs prefs = null)
        {
            if (!IsExportValid(export)) return null;
            _convertDropdownAs = convertDropdownAs;
            _convertSlotsAs = (prefs != null) ? prefs.ConvertSlotsAs : ConverterPrefs.ConvertSlotsModes.DisplayName;
            _currentExport = export;
            _prefs = prefs;
            documentDepth = 0;
            ArticyData articyData = new ArticyData();
            articyData.project.createdOn = export.CreatedOn.ToString();
            articyData.project.creatorTool = export.CreatorTool;
            articyData.project.creatorVersion = export.CreatorVersion;
            foreach (object o in export.Content.Items)
            {
                ConvertProject(articyData, o as ProjectType);
                ConvertAsset(articyData, o as AssetType);
                ConvertEntity(articyData, o as EntityType, export);
                ConvertLocation(articyData, o as LocationType);
                ConvertFlowFragment(articyData, o as FlowFragmentType);
                //--- No. Documents are not dialogues. They contain dialogues: ConvertDocument(articyData, o as DocumentType);
                ConvertDialogue(articyData, o as DialogueType);
                ConvertDialogueFragment(articyData, o as DialogueFragmentType);
                ConvertHub(articyData, o as HubType);
                ConvertJump(articyData, o as JumpType);
                ConvertConnection(articyData, o as ConnectionType);
                ConvertCondition(articyData, o as ConditionType);
                ConvertInstruction(articyData, o as InstructionType);
                ConvertVariableSet(articyData, o as VariableSetType);
            }
            ConvertHierarchy(articyData, export.Hierarchy);
            return articyData;
        }

        private static void ConvertProject(ArticyData articyData, ProjectType project)
        {
            if (project != null)
            {
                articyData.project.displayName = project.DisplayName;
            }
        }

        private static void ConvertAsset(ArticyData articyData, AssetType asset)
        {
            if (asset != null)
            {
                articyData.assets.Add(asset.Id, new ArticyData.Asset(asset.Id, asset.TechnicalName,
                    ConvertLocalizableText(asset.DisplayName), ConvertLocalizableText(asset.Text),
                    ConvertFeatures(asset.Features), Vector2.zero, asset.AssetFilename));
            }
        }

        private static void ConvertEntity(ArticyData articyData, EntityType entity, ExportType export)
        {
            if (entity != null)
            {
                articyData.entities.Add(entity.Id, new ArticyData.Entity(entity.Id, entity.TechnicalName,
                    ConvertLocalizableText(entity.DisplayName), ConvertLocalizableText(entity.Text),
                    ConvertFeatures(entity.Features), Vector2.zero,
                    GetPictureFilename(export, entity.PreviewImage)));
            }
        }

        private static void ConvertLocation(ArticyData articyData, LocationType location)
        {
            if (location != null)
            {
                articyData.locations.Add(location.Id, new ArticyData.Location(location.Id, location.TechnicalName,
                    ConvertLocalizableText(location.DisplayName), ConvertLocalizableText(location.Text),
                    ConvertFeatures(location.Features), Vector2.zero));
            }
        }

        private static void ConvertFlowFragment(ArticyData articyData, FlowFragmentType flowFragment)
        {
            if (flowFragment != null)
            {
                articyData.flowFragments.Add(flowFragment.Id, new ArticyData.FlowFragment(flowFragment.Id, flowFragment.TechnicalName,
                    ConvertLocalizableText(flowFragment.DisplayName), ConvertLocalizableText(flowFragment.Text),
                    ConvertFeatures(flowFragment.Features), Vector2.zero, ConvertPins(flowFragment.Pins)));
            }
        }

        private static void ConvertDocument(ArticyData articyData, DocumentType document)
        {
            // Note: Not used. Documents appear as dialogues in XML, so use ConvertDialogue.
            if (document != null)
            {
                articyData.dialogues.Add(document.Id, new ArticyData.Dialogue(document.Id, document.TechnicalName,
                    ConvertLocalizableText(document.DisplayName), ConvertLocalizableText(document.Text),
                    new ArticyData.Features(new List<ArticyData.Feature>()),
                    Vector2.zero,
                    new List<ArticyData.Pin>(),
                    new List<string>(), true));
            }
        }

        private static void ConvertDialogue(ArticyData articyData, DialogueType dialogue)
        {
            if (dialogue != null)
            {
                articyData.dialogues.Add(dialogue.Id, new ArticyData.Dialogue(dialogue.Id, dialogue.TechnicalName,
                    ConvertLocalizableText(dialogue.DisplayName), ConvertLocalizableText(dialogue.Text),
                    ConvertFeatures(dialogue.Features), new Vector2(dialogue.Position.X, dialogue.Position.Y),
                    ConvertPins(dialogue.Pins), ConvertReferences(dialogue.References)));
            }
        }

        private static void ConvertDialogueFragment(ArticyData articyData, DialogueFragmentType dialogueFragment)
        {
            if (dialogueFragment != null)
            {
                articyData.dialogueFragments.Add(dialogueFragment.Id, new ArticyData.DialogueFragment(dialogueFragment.Id,
                    dialogueFragment.TechnicalName, ConvertLocalizableText(dialogueFragment.DisplayName),
                    ConvertLocalizableText(dialogueFragment.Text), ConvertFeatures(dialogueFragment.Features),
                    new Vector2(dialogueFragment.Position.X, dialogueFragment.Position.Y),
                    ConvertLocalizableText(dialogueFragment.MenuText),
                    ConvertLocalizableText(dialogueFragment.StageDirections), ConvertIdRef(dialogueFragment.Speaker),
                    ConvertPins(dialogueFragment.Pins)));
            }
        }

        private static void ConvertHub(ArticyData articyData, HubType hub)
        {
            if (hub != null)
            {
                articyData.hubs.Add(hub.Id, new ArticyData.Hub(hub.Id, hub.TechnicalName, ConvertLocalizableText(hub.DisplayName),
                    ConvertLocalizableText(hub.Text), ConvertFeatures(hub.Features),
                    new Vector2(hub.Position.X, hub.Position.Y), ConvertPins(hub.Pins)));
            }
        }

        private static void ConvertJump(ArticyData articyData, JumpType jump)
        {
            if (jump != null)
            {
                articyData.jumps.Add(jump.Id, new ArticyData.Jump(jump.Id, jump.TechnicalName, ConvertLocalizableText(jump.DisplayName),
                    ConvertLocalizableText(jump.Text), ConvertFeatures(jump.Features),
                    new Vector2(jump.Position.X, jump.Position.Y),
                    ConvertConnectionRef(jump.Target), ConvertPins(jump.Pins)));
            }
        }

        private static void ConvertConnection(ArticyData articyData, ConnectionType connection)
        {
            if (connection != null)
            {
                articyData.connections.Add(connection.Id, new ArticyData.Connection(connection.Id, connection.Color,
                    ConvertConnectionRef(connection.Source), ConvertConnectionRef(connection.Target)));
            }
        }

        private static ArticyData.ConnectionRef ConvertConnectionRef(ConnectionRefType connectionRef)
        {
            return (connectionRef != null)
                ? new ArticyData.ConnectionRef(connectionRef.IdRef, connectionRef.PinRef)
                : new ArticyData.ConnectionRef();
        }

        private static void ConvertCondition(ArticyData articyData, ConditionType condition)
        {
            if (condition != null)
            {
                articyData.conditions.Add(condition.Id, new ArticyData.Condition(condition.Id,
                    condition.Expression, ConvertPins(condition.Pins),
                    new Vector2(condition.Position.X, condition.Position.Y)));
            }
        }

        private static void ConvertInstruction(ArticyData articyData, InstructionType instruction)
        {
            if (instruction != null)
            {
                articyData.instructions.Add(instruction.Id, new ArticyData.Instruction(
                    instruction.Id, instruction.Expression, ConvertPins(instruction.Pins),
                    new Vector2(instruction.Position.X, instruction.Position.Y)));
            }
        }

        private static void ConvertVariableSet(ArticyData articyData, VariableSetType variableSet)
        {
            if (variableSet != null)
            {
                articyData.variableSets.Add(variableSet.Id, new ArticyData.VariableSet(variableSet.Id, variableSet.TechnicalName,
                    ConvertVariables(variableSet.Variables)));
            }
        }

        private static List<ArticyData.Variable> ConvertVariables(VariablesType variables)
        {
            List<ArticyData.Variable> articyDataVariables = new List<ArticyData.Variable>();
            if ((variables != null) && (variables.Variable != null))
            {
                foreach (VariableType variable in variables.Variable)
                {
                    articyDataVariables.Add(new ArticyData.Variable(variable.TechnicalName, variable.DefaultValue,
                        ConvertDataType(variable.DataType), GetDefaultLocalizedString(variable.Description)));
                }
            }
            return articyDataVariables;
        }

        private static ArticyData.VariableDataType ConvertDataType(VariableDataTypeType dataType)
        {
            switch (dataType)
            {
                case VariableDataTypeType.Boolean:
                    return ArticyData.VariableDataType.Boolean;
                case VariableDataTypeType.Integer:
                    return ArticyData.VariableDataType.Integer;
                case VariableDataTypeType.String:
                    return ArticyData.VariableDataType.String;
                default:
                    Debug.LogWarning(string.Format("{0}: Unexpected variable data type {1}", DialogueDebug.Prefix, dataType.ToString()));
                    return ArticyData.VariableDataType.Boolean;
            }
        }

        private static ArticyData.LocalizableText ConvertLocalizableText(LocalizableTextType localizableText)
        {
            ArticyData.LocalizableText articyDataLocalizableText = new ArticyData.LocalizableText();
            if ((localizableText != null) && (localizableText.LocalizedString != null))
            {
                foreach (LocalizedStringType ls in localizableText.LocalizedString)
                {
                    articyDataLocalizableText.localizedString.Add(ls.Lang, ArticyTools.RemoveHtml(ls.Value));
                }
            }
            return articyDataLocalizableText;
        }

        private static ArticyData.LocalizableText ConvertLocalizableText(string s)
        {
            ArticyData.LocalizableText articyDataLocalizableText = new ArticyData.LocalizableText();
            articyDataLocalizableText.localizedString.Add(string.Empty, ArticyTools.RemoveHtml(s));
            return articyDataLocalizableText;
        }

        private static List<string> ConvertReferences(ReferencesType references)
        {
            List<string> articyDataReferences = new List<string>();
            if ((references != null) && (references.Reference != null))
            {
                foreach (ReferenceType reference in references.Reference)
                {
                    articyDataReferences.Add(ConvertIdRef(reference));
                }
            }
            return articyDataReferences;
        }

        private static string ConvertIdRef(ReferenceType reference)
        {
            return (reference != null) ? reference.IdRef : string.Empty;
        }

        private static List<ArticyData.Pin> ConvertPins(PinsType pins)
        {
            List<ArticyData.Pin> articyDataPins = new List<ArticyData.Pin>();
            if ((pins != null) && (pins.Pin != null))
            {
                foreach (PinType pin in pins.Pin)
                {
                    articyDataPins.Add(new ArticyData.Pin(pin.Id, pin.Index, ConvertSemanticType(pin.Semantic), pin.Expression));
                }
            }
            return articyDataPins;
        }

        private static ArticyData.SemanticType ConvertSemanticType(SemanticType semanticType)
        {
            switch (semanticType)
            {
                case SemanticType.Input:
                    return ArticyData.SemanticType.Input;
                case SemanticType.Output:
                    return ArticyData.SemanticType.Output;
                default:
                    Debug.LogWarning(string.Format("{0}: Unexpected semantic type {1}", DialogueDebug.Prefix, semanticType.ToString()));
                    return ArticyData.SemanticType.Input;
            }
        }

        private static ArticyData.Features ConvertFeatures(FeaturesType features)
        {
            List<ArticyData.Feature> articyDataFeatures = new List<ArticyData.Feature>();
            if ((features != null) && (features.Feature != null))
            {
                foreach (FeatureType feature in features.Feature)
                {
                    var articyDataFeature = new ArticyData.Feature();
                    foreach (PropertiesType properties in feature.Properties)
                    {
                        if (properties != null && properties.Items != null && properties.Items.Length > 0)
                        {
                            var articyDataFields = new List<Field>();
                            foreach (object item in properties.Items)
                            {
                                ConvertItem(item, articyDataFields);
                            }
                            articyDataFeature.properties.Add(new ArticyData.Property(articyDataFields));
                        }
                    }
                    articyDataFeatures.Add(articyDataFeature);
                }
            }
            return new ArticyData.Features(articyDataFeatures);
        }

        private static void ConvertItem(object item, List<Field> fields)
        {
            System.Type type = item.GetType();
            if (type == typeof(BooleanPropertyType))
            {
                BooleanPropertyType booleanProperty = (BooleanPropertyType)item;
                fields.Add(new Field(booleanProperty.Name, string.Equals(booleanProperty.Value, "1").ToString(), FieldType.Boolean));
            }
            else if (type == typeof(EnumPropertyType))
            {
                EnumPropertyType enumProperty = (EnumPropertyType)item;
                if (PixelCrushers.DialogueSystem.Articy.ArticyTools.IsQuestStateArticyPropertyName(enumProperty.Name))
                {
                    fields.Add(new Field(enumProperty.Name, PixelCrushers.DialogueSystem.Articy.ArticyTools.EnumValueToQuestState(Tools.StringToInt(enumProperty.Value), GetEnumStringValue(enumProperty.Name, Tools.StringToInt(enumProperty.Value), true)), FieldType.Text));
                }
                else
                {
                    var dropdownMode = _convertDropdownAs;
                    if (_prefs != null)
                    {
                        switch (_prefs.ConversionSettings.GetDropdownOverrideSetting(enumProperty.Name).mode)
                        {
                            case ConversionSettings.DropdownOverrideMode.Int:
                                dropdownMode = ConverterPrefs.ConvertDropdownsModes.Int;
                                break;
                            case ConversionSettings.DropdownOverrideMode.TechnicalName:
                                dropdownMode = ConverterPrefs.ConvertDropdownsModes.TechnicalName;
                                break;
                            case ConversionSettings.DropdownOverrideMode.DisplayName:
                                dropdownMode = ConverterPrefs.ConvertDropdownsModes.DisplayName;
                                break;
                        }
                    }
                    switch (dropdownMode)
                    {
                        case ConverterPrefs.ConvertDropdownsModes.Int:
                            fields.Add(new Field(enumProperty.Name, enumProperty.Value, FieldType.Number));
                            break;
                        case ConverterPrefs.ConvertDropdownsModes.TechnicalName:
                            fields.Add(new Field(enumProperty.Name, GetEnumStringValue(enumProperty.Name, Tools.StringToInt(enumProperty.Value), true), FieldType.Text));
                            break;
                        case ConverterPrefs.ConvertDropdownsModes.DisplayName:
                            fields.Add(new Field(enumProperty.Name, GetEnumStringValue(enumProperty.Name, Tools.StringToInt(enumProperty.Value), false), FieldType.Text));
                            break;
                    }
                }
            }
            else if (type == typeof(LocalizableTextPropertyType))
            {
                LocalizableTextPropertyType localizableTextProperty = (LocalizableTextPropertyType)item;
                string baseFieldTitle = localizableTextProperty.Name;
                if (!string.IsNullOrEmpty(baseFieldTitle) && localizableTextProperty.LocalizedString != null)
                {
                    foreach (var localizedStringItem in localizableTextProperty.LocalizedString)
                    {
                        if (string.IsNullOrEmpty(localizedStringItem.Lang))
                        {
                            fields.Add(new Field(baseFieldTitle, localizedStringItem.Value, FieldType.Text));
                        }
                        else
                        {
                            string localizedTitle = string.Format("{0} {1}", baseFieldTitle, localizedStringItem.Lang);
                            fields.Add(new Field(localizedTitle, localizedStringItem.Value, FieldType.Localization));
                        }
                    }
                }
            }
            else if (type == typeof(ReferenceSlotPropertyType))
            {
                ReferenceSlotPropertyType slotProperty = (ReferenceSlotPropertyType)item;
                switch (_convertSlotsAs)
                {
                    case ConverterPrefs.ConvertSlotsModes.ID:
                        fields.Add(new Field(slotProperty.Name, slotProperty.IdRef, FieldType.Text));
                        break;
                    case ConverterPrefs.ConvertSlotsModes.TechnicalName:
                        fields.Add(new Field(slotProperty.Name, GetTechnicalName(slotProperty.IdRef), FieldType.Text));
                        break;
                    default:
                    case ConverterPrefs.ConvertSlotsModes.DisplayName:
                        fields.Add(new Field(slotProperty.Name, GetDisplayName(slotProperty.IdRef), FieldType.Text));
                        break;
                }
            }
            else if (type == typeof(NumberPropertyType))
            {
                NumberPropertyType numberPropertyType = (NumberPropertyType)item;
                fields.Add(new Field(numberPropertyType.Name, numberPropertyType.Value, FieldType.Number));
            }
            else if (type == typeof(ReferenceStripPropertyType))
            {
                ReferenceStripPropertyType stripPropertyType = (ReferenceStripPropertyType)item;
                fields.Add(new Field(ArticyTools.SubtableFieldPrefix + stripPropertyType.Name, GetStripStringValue(stripPropertyType), FieldType.Text));
            }
            else if (type == typeof(StringPropertyType))
            {
                StringPropertyType stringPropertyType = (StringPropertyType)item;
                fields.Add(new Field(stringPropertyType.Name, stringPropertyType.Value, FieldType.Text));
            }
        }

        private static string GetStripStringValue(ReferenceStripPropertyType stripPropertyType)
        {
            var s = string.Empty;
            if (stripPropertyType != null && stripPropertyType.Reference != null)
            {
                foreach (var reference in stripPropertyType.Reference)
                {
                    if (string.IsNullOrEmpty(reference.IdRef)) continue;
                    if (!string.IsNullOrEmpty(s)) s += ";";
                    s += reference.IdRef;
                }
            }
            return s;
        }

        private static string GetEnumStringValue(string enumName, int enumIndex, bool getTechnicalName)
        {
            var enumNumber = enumIndex;
            foreach (var o in _currentExport.Content.Items)
            {
                if (o is EnumerationPropertyDefinitionType)
                {
                    var enumDef = o as EnumerationPropertyDefinitionType;
                    if (string.Equals(enumDef.TechnicalName, enumName) && enumDef.Values != null && enumDef.Values.EnumValue != null)
                    {
                        foreach (var val in enumDef.Values.EnumValue)
                        {
                            if (val.Value == enumNumber)
                            {
                                return getTechnicalName ? val.TechnicalName : GetDefaultLocalizedString(val.DisplayName);
                            }
                        }
                    }
                }
            }
            return enumIndex.ToString();
        }

        private static string GetTechnicalName(string idRef)
        {
            foreach (var item in _currentExport.Content.Items)
            {
                if (item is EntityType && string.Equals((item as EntityType).Id, idRef)) return (item as EntityType).TechnicalName;
                if (item is FlowFragmentType && string.Equals((item as FlowFragmentType).Id, idRef)) return (item as FlowFragmentType).TechnicalName;
                else if (item is DialogueFragmentType && string.Equals((item as DialogueFragmentType).Id, idRef)) return (item as DialogueFragmentType).TechnicalName;
                else if (item is HubType && string.Equals((item as HubType).Id, idRef)) return (item as HubType).TechnicalName;
                else if (item is JumpType && string.Equals((item as JumpType).Id, idRef)) return (item as JumpType).DisplayName;
                else if (item is ZoneType && string.Equals((item as ZoneType).Id, idRef)) return (item as ZoneType).TechnicalName;
                else if (item is LocationType && string.Equals((item as LocationType).Id, idRef)) return (item as LocationType).TechnicalName;
                else if (item is SpotType && string.Equals((item as SpotType).Id, idRef)) return (item as SpotType).TechnicalName;
                else if (item is JourneyType && string.Equals((item as JourneyType).Id, idRef)) return (item as JourneyType).TechnicalName;
                else if (item is AssetType && string.Equals((item as AssetType).Id, idRef)) return (item as AssetType).TechnicalName;
                else if (item is DialogueType && string.Equals((item as DialogueType).Id, idRef)) return (item as DialogueType).TechnicalName;
            }
            return string.Equals("0x0000000000000000", idRef) ? string.Empty : idRef;
        }

        private static string GetDisplayName(string idRef)
        {
            foreach (var item in _currentExport.Content.Items)
            {
                if (item is EntityType && string.Equals((item as EntityType).Id, idRef)) return GetDefaultLocalizedString((item as EntityType).DisplayName);
                if (item is FlowFragmentType && string.Equals((item as FlowFragmentType).Id, idRef)) return GetDefaultLocalizedString((item as FlowFragmentType).DisplayName);
                else if (item is DialogueFragmentType && string.Equals((item as DialogueFragmentType).Id, idRef)) return (item as DialogueFragmentType).DisplayName;
                else if (item is HubType && string.Equals((item as HubType).Id, idRef)) return GetDefaultLocalizedString((item as HubType).DisplayName);
                else if (item is JumpType && string.Equals((item as JumpType).Id, idRef)) return (item as JumpType).DisplayName;
                else if (item is ZoneType && string.Equals((item as ZoneType).Id, idRef)) return GetDefaultLocalizedString((item as ZoneType).DisplayName);
                else if (item is LocationType && string.Equals((item as LocationType).Id, idRef)) return GetDefaultLocalizedString((item as LocationType).DisplayName);
                else if (item is SpotType && string.Equals((item as SpotType).Id, idRef)) return GetDefaultLocalizedString((item as SpotType).DisplayName);
                else if (item is JourneyType && string.Equals((item as JourneyType).Id, idRef)) return GetDefaultLocalizedString((item as JourneyType).DisplayName);
                else if (item is AssetType && string.Equals((item as AssetType).Id, idRef)) return GetDefaultLocalizedString((item as AssetType).DisplayName);
                //---Was: else if (item is DialogueType && string.Equals((item as DialogueType).Id, idRef)) return GetDefaultLocalizedString((item as DialogueType).DisplayName);
                else if (item is DialogueType && string.Equals((item as DialogueType).Id, idRef))
                { // Need to prepend conversation path:
                    return GetNameWithHierarchyPath(item as DialogueType);
                }
            }
            return string.Equals("0x0000000000000000", idRef) ? string.Empty : idRef;
        }

        private static string GetNameWithHierarchyPath(DialogueType item)
        {
            var s = GetNameWithHierarchyPathRecursion(item, _currentExport.Hierarchy.Node, 0);
            return !string.IsNullOrEmpty(s) ? s : GetDefaultLocalizedString(item.DisplayName);
        }

        private static string GetNameWithHierarchyPathRecursion(DialogueType item, NodeType node, int safeguard)
        {
            if (safeguard > 999 || node == null) return null;
            if (node.IdRef == item.Id) return GetDefaultLocalizedString(item.DisplayName);
            if (node.Node != null)
            {
                foreach (NodeType childNode in node.Node)
                {
                    var s = GetNameWithHierarchyPathRecursion(item, childNode, safeguard + 1);
                    if (!string.IsNullOrEmpty(s))
                    {
                        var myName = GetDisplayName(node.IdRef);
                        return myName.StartsWith("0x") ? s : (myName + "/" + s); // Omit top level hierarchy nodes that have no name.
                    }
                }
            }
            return null;
        }

        private static string GetDefaultLocalizedString(LocalizableTextType localizableText)
        {
            if (localizableText == null || localizableText.LocalizedString == null || localizableText.LocalizedString.Length < 1) return string.Empty;
            return localizableText.LocalizedString[0].Value;
        }

        private static string GetPictureFilename(ExportType export, PreviewImageType previewImage)
        {
            if (previewImage != null)
            {
                foreach (object o in export.Content.Items)
                {
                    if (o is AssetType)
                    {
                        AssetType asset = o as AssetType;
                        if (string.Equals(asset.Id, previewImage.IdRef)) return asset.OriginalSource; //---articy converts filename, so use  original. Was: asset.AssetFilename;
                    }
                }
            }
            return null;
        }

        private static void ConvertHierarchy(ArticyData articyData, HierarchyType hierarchy)
        {
            articyData.hierarchy.node = ConvertNode(articyData, hierarchy.Node);
        }

        private static ArticyData.Node ConvertNode(ArticyData articyData, NodeType node)
        {
            ArticyData.Node articyDataNode = new ArticyData.Node();
            if (node != null)
            {
                // Record node type:
                articyDataNode.id = node.IdRef;
                articyDataNode.type = ConvertNodeType(node.Type);

                // If this is a TextObject in our TextTableDocument, add it to the text table data:
                if (isInTextTableDocument && string.Equals(node.Type, "TextObject"))
                {
                    var textObject = LookupByIdRef(node.IdRef) as TextObjectType;
                    if (textObject != null) articyData.textTableFields.Add(GetDefaultLocalizedString(textObject.DisplayName));
                }

                // If a dialogue and inside a document, record that it's in a document:
                if (articyDataNode.type == ArticyData.NodeType.Dialogue && documentDepth > 0)
                {
                    var dialogue = articyData.dialogues.ContainsKey(node.IdRef) ? articyData.dialogues[node.IdRef] : null;
                    if (dialogue != null) dialogue.isDocument = true;
                }

                // Recurse through children:
                if (node.Node != null)
                {
                    if (node.Type == "Document")
                    {
                        documentDepth++;
                        if (!string.IsNullOrEmpty(_prefs.TextTableDocument))
                        {
                            var document = LookupByIdRef(node.IdRef) as DocumentType;
                            if (document != null && string.Equals(GetDefaultLocalizedString(document.DisplayName), _prefs.TextTableDocument)) isInTextTableDocument = true;
                        }
                    }
                    foreach (NodeType childNode in node.Node)
                    {
                        articyDataNode.nodes.Add(ConvertNode(articyData, childNode));
                    }
                    if (node.Type == "Document")
                    {
                        documentDepth--;
                        isInTextTableDocument = false;
                    }
                }
            }
            return articyDataNode;
        }

        private static ArticyData.NodeType ConvertNodeType(string nodeType)
        {
            if (string.Equals(nodeType, "FlowFragment"))
            {
                return ArticyData.NodeType.FlowFragment;
            }
            else if (string.Equals(nodeType, "Dialogue") || string.Equals(nodeType, "Document"))
            {
                return ArticyData.NodeType.Dialogue;
            }
            else if (string.Equals(nodeType, "DialogueFragment"))
            {
                return ArticyData.NodeType.DialogueFragment;
            }
            else if (string.Equals(nodeType, "Hub"))
            {
                return ArticyData.NodeType.Hub;
            }
            else if (string.Equals(nodeType, "Jump"))
            {
                return ArticyData.NodeType.Jump;
            }
            else if (string.Equals(nodeType, "Connection"))
            {
                return ArticyData.NodeType.Connection;
            }
            else if (string.Equals(nodeType, "Condition"))
            {
                return ArticyData.NodeType.Condition;
            }
            else if (string.Equals(nodeType, "Instruction"))
            {
                return ArticyData.NodeType.Instruction;
            }
            else
            {
                return ArticyData.NodeType.Other;
            }
        }

        // IMPORTANT NOTE: For efficiency, only looks up Documents and TextObjects.
        private static object LookupByIdRef(string idRef)
        {
            for (int i = 0; i < _currentExport.Content.Items.Length; i++)
            {
                var item = _currentExport.Content.Items[i];
                if (item is DocumentType)
                {
                    if (string.Equals((item as DocumentType).Id, idRef)) return item;
                }
                else if (item is TextObjectType)
                {
                    if (string.Equals((item as TextObjectType).Id, idRef)) return item;
                }
            }
            return null;
        }

    }

}
#endif
