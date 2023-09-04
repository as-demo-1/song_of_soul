#if USE_ARTICY
// Copyright (c) Pixel Crushers. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.Articy
{

    /// <summary>
    /// This class does the actual work of converting ArticyData (version-independent 
    /// articy:draft data) into a dialogue database.
    /// </summary>
    public class ArticyConverter
    {

        #region Public Static Utility Methods

        public delegate void ProgressCallbackDelegate(string info, float progress);
        public static event ProgressCallbackDelegate onProgressCallback = delegate { };

        /// <summary>
        /// Creates a new database from an articy:draft XML file.
        /// </summary>
        /// <param name="xmlData">Articy XML data (i.e., the contents of an articy:draft XML export).</param>
        /// <param name="prefs">Optional ConverterPrefs to use. Does not use prefs.ProjectFilename.</param>
        /// <param name="template">Optional template for dialogue database assets.</param>
        /// <returns></returns>
        public static DialogueDatabase ConvertXmlDataToDatabase(string xmlData, ConverterPrefs prefs = null, Template template = null)
        {
            if (prefs == null) prefs = new ConverterPrefs();
            if (template == null) template = new Template();
            var database = DatabaseUtility.CreateDialogueDatabaseInstance();
            var articyData = ArticySchemaTools.LoadArticyDataFromXmlData(xmlData, prefs);
            if (articyData == null)
            {
                if (DialogueDebug.logWarnings) Debug.LogWarning("Dialogue System: Can't convert articy:draft project; unable to import articy:draft data.");
                return null;
            }
            ConvertArticyDataToDatabase(articyData, prefs, template, database);
            return database;
        }

        /// <summary>
        /// This static utility method creates a converter and uses it to run the conversion.
        /// </summary>
        /// <param name='articyData'>
        /// Articy data.
        /// </param>
        /// <param name='prefs'>
        /// Prefs.
        /// </param>
        /// <param name='database'>
        /// Dialogue database.
        /// </param>
        public static void ConvertArticyDataToDatabase(ArticyData articyData, ConverterPrefs prefs, Template template, DialogueDatabase database)
        {
            ArticyConverter converter = new ArticyConverter();
            converter.Convert(articyData, prefs, template, database);
        }

        #endregion

        #region Private Variables

        private const string ArticyIdFieldTitle = "Articy Id";
        private const string ArticyTechnicalNameFieldTitle = "Technical Name";
        private const string DestinationArticyIdFieldTitle = "destinationArticyId";
        private const int StartEntryID = 0;

        private ArticyData articyData;
        private ConverterPrefs prefs;
        private DialogueDatabase database;
        private Template template;
        private int conversationID;
        private int actorID;
        private int itemID;
        private int locationID;
        private static List<string> fullVariableNames = new List<string>(); // Make static to expose ConvertExpression().

        private HashSet<string> otherScriptFieldTitles = new HashSet<string>();
        private List<Conversation> documentConversations = new List<Conversation>();

        #endregion

        #region Stacks

        private List<string> flowFragmentNameStack = new List<string>();
        private List<Conversation> conversationStack = new List<Conversation>();
        private Dictionary<Conversation, int> conversationLastEntryID = new Dictionary<DialogueSystem.Conversation, int>();
        private Dictionary<string, List<DialogueEntry>> entriesByArticyId = new Dictionary<string, List<DialogueEntry>>();
        private Dictionary<string, DialogueEntry> entriesByPinID = new Dictionary<string, DialogueEntry>();
        private Dictionary<ArticyData.Jump, DialogueEntry> jumpsToProcess = new Dictionary<ArticyData.Jump, DialogueEntry>();
        private List<DialogueEntry> unusedOutputEntries = new List<DialogueEntry>();

        private void ResetStacks()
        {
            flowFragmentNameStack.Clear();
            conversationStack.Clear();
            conversationLastEntryID.Clear();
            entriesByPinID.Clear();
            jumpsToProcess.Clear();
            unusedOutputEntries.Clear();
        }

        private void PushFlowFragment(ArticyData.FlowFragment flowFragment)
        {
            if (flowFragment == null) return;
            flowFragmentNameStack.Add(flowFragment.displayName.DefaultText);
        }

        private void PopFlowFragment()
        {
            if (flowFragmentNameStack.Count < 1) return;
            flowFragmentNameStack.RemoveAt(flowFragmentNameStack.Count - 1);
        }

        private void PushConversation(Conversation conversation)
        {
            if (conversation == null) return;
            conversationStack.Add(conversation);
        }

        private void PopConversation()
        {
            if (conversationStack.Count < 1) return;
            conversationStack.RemoveAt(conversationStack.Count - 1);
        }

        private Conversation GetConversationStackTop()
        {
            return (conversationStack.Count > 0) ? conversationStack[conversationStack.Count - 1] : null;
        }

        private int GetNextConversationEntryID(Conversation conversation)
        {
            if (conversation == null) return 0;
            if (!conversationLastEntryID.ContainsKey(conversation))
            {
                conversationLastEntryID.Add(conversation, 0);
                return 0;
            }
            else
            {
                conversationLastEntryID[conversation]++;
                return conversationLastEntryID[conversation];
            }
        }

        private void ResetArticyIdIndex()
        {
            entriesByArticyId.Clear();
        }

        private void IndexDialogueEntryByArticyId(DialogueEntry entry, string articyId)
        {
            if (entriesByArticyId.ContainsKey(articyId))
            {
                if (!entriesByArticyId[articyId].Contains(entry))
                {
                    entriesByArticyId[articyId].Add(entry);
                }
            }
            else
            {
                entriesByArticyId.Add(articyId, new List<DialogueEntry>());
                entriesByArticyId[articyId].Add(entry);
            }
        }

        #endregion

        #region Top Level Conversion Methods

        /// <summary>
        /// Convert the ArticyData, using the preferences in Prefs, into a dialogue database.
        /// </summary>
        /// <param name='articyData'>Articy data.</param>
        /// <param name='prefs'>Prefs.</param>
        /// <param name='database'>Dialogue database.</param>
        public void Convert(ArticyData articyData, ConverterPrefs prefs, Template template, DialogueDatabase database)
        {
            if (articyData != null)
            {
                onProgressCallback("Converting non-dialogue elements", 0.01f);
                Setup(articyData, prefs, template, database);
                ConvertProjectAttributes();
                ConvertVariables();
                ConvertEntities();
                ConvertLocations();
                ConvertFlowFragmentsToQuests();
                ConvertDialogues();
                ResetArticyIdIndex();
                ConvertEmVarSet();
                if (!prefs.ImportDocuments) DeleteDocumentConversations();
            }
        }

        /// <summary>
        /// Sets up the conversion process.
        /// </summary>
        /// <param name='articyData'>Articy data.</param>
        /// <param name='prefs'>Prefs.</param>
        /// <param name='database'>Dialogue database.</param>
        private void Setup(ArticyData articyData, ConverterPrefs prefs, Template template, DialogueDatabase database)
        {
            this.articyData = articyData;
            this.prefs = prefs;
            this.database = database;
            database.actors = new List<Actor>();
            database.items = new List<Item>();
            database.locations = new List<Location>();
            database.variables = new List<Variable>();
            database.conversations = new List<Conversation>();
            conversationID = 0;
            actorID = 0;
            itemID = 0;
            locationID = 0;
            //documentConversation = null;
            //lastDocumentEntry = null;
            fullVariableNames.Clear();
            otherScriptFieldTitles.Clear();
            documentConversations.Clear();
            foreach (var otherScriptFieldTitle in prefs.OtherScriptFields.Split(';'))
            {
                otherScriptFieldTitles.Add(otherScriptFieldTitle.Trim());
            }
            ResetArticyIdIndex();
            this.template = template;
        }

        private void ConvertProjectAttributes()
        {
            database.version = articyData.ProjectVersion;
            database.author = articyData.ProjectAuthor;
        }

        #endregion

        #region Non-Dialogue Conversion

        /// <summary>
        /// Converts articy entities into Dialogue System actors and items/quests.
        /// </summary>
        private void ConvertEntities()
        {
            foreach (ArticyData.Entity articyEntity in articyData.entities.Values)
            {
                ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(articyEntity.id);
                if (conversionSetting.Include)
                {
                    var category = conversionSetting.Category;
                    if (HasField(articyEntity.features, "IsNPC", false)) category = EntityCategory.NPC;
                    if (HasField(articyEntity.features, "IsPlayer", true)) category = EntityCategory.Player;
                    if (HasField(articyEntity.features, "IsItem", true)) category = EntityCategory.Item;
                    if (HasField(articyEntity.features, "IsQuest", true)) category = EntityCategory.Quest;
                    switch (category)
                    {
                        case EntityCategory.NPC:
                        case EntityCategory.Player:
                            actorID++;
                            bool isPlayer = (conversionSetting.Category == EntityCategory.Player);
                            Actor actor = template.CreateActor(actorID, articyEntity.displayName.DefaultText, isPlayer);
                            Field.SetValue(actor.fields, ArticyIdFieldTitle, articyEntity.id, FieldType.Text);
                            Field.SetValue(actor.fields, ArticyTechnicalNameFieldTitle, articyEntity.technicalName, FieldType.Text);
                            Field.SetValue(actor.fields, "Description", articyEntity.text.DefaultText, FieldType.Text);
                            if (!string.IsNullOrEmpty(articyEntity.previewImage)) Field.SetValue(actor.fields, "Pictures", string.Format("[{0}]", articyEntity.previewImage), FieldType.Text);
                            SetFeatureFields(actor.fields, articyEntity.features);
                            ConvertLocalizableText(actor.fields, "Name", articyEntity.displayName);
                            if (prefs.UseTechnicalNames)
                            {
                                Field.SetValue(actor.fields, "Name", articyEntity.technicalName, FieldType.Text);
                            }
                            if (prefs.UseTechnicalNames || prefs.SetDisplayName)
                            { 
                                Field.SetValue(actor.fields, "Display Name", articyEntity.displayName.DefaultText, FieldType.Text);
                            }
                            if (prefs.CustomDisplayName) UseCustomDisplayName(actor.fields);
                            database.actors.Add(actor);
                            break;
                        case EntityCategory.Item:
                        case EntityCategory.Quest:
                            itemID++;
                            Item item = template.CreateItem(itemID, articyEntity.displayName.DefaultText);
                            Field.SetValue(item.fields, ArticyIdFieldTitle, articyEntity.id, FieldType.Text);
                            Field.SetValue(item.fields, ArticyTechnicalNameFieldTitle, articyEntity.technicalName, FieldType.Text);
                            Field.SetValue(item.fields, "Description", articyEntity.text.DefaultText, FieldType.Text);
                            Field.SetValue(item.fields, "Is Item", ((category == EntityCategory.Item) ? "True" : "False"), FieldType.Boolean);
                            if (prefs.UseTechnicalNames) Field.SetValue(item.fields, "Display Name", articyEntity.displayName.DefaultText, FieldType.Text);
                            SetFeatureFields(item.fields, articyEntity.features);
                            ConvertLocalizableText(item.fields, "Name", articyEntity.displayName);
                            if (prefs.UseTechnicalNames)
                            {
                                Field.SetValue(item.fields, "Name", articyEntity.technicalName, FieldType.Text);
                            }
                            if (prefs.UseTechnicalNames || prefs.SetDisplayName)
                            { 
                                Field.SetValue(item.fields, "Display Name", articyEntity.displayName.DefaultText, FieldType.Text);
                            }
                            if (prefs.CustomDisplayName) UseCustomDisplayName(item.fields);
                            database.items.Add(item);
                            break;
                        default:
                            Debug.LogError("Dialogue System: Internal error converting entity type '" + conversionSetting.Category + "' (Articy ID: " + articyEntity.id + ").");
                            break;
                    }
                }
            }
            foreach (var actor in database.actors) // Find actors' portraits.
            {
                FindPortraitTextureInResources(actor);
            }
        }

        /// <summary>
        /// Converts locations.
        /// </summary>
        private void ConvertLocations()
        {
            foreach (ArticyData.Location articyLocation in articyData.locations.Values)
            {
                if (prefs.ConversionSettings.GetConversionSetting(articyLocation.id).Include)
                {
                    locationID++;
                    Location location = template.CreateLocation(locationID, articyLocation.displayName.DefaultText);
                    Field.SetValue(location.fields, ArticyIdFieldTitle, articyLocation.id, FieldType.Text);
                    Field.SetValue(location.fields, ArticyTechnicalNameFieldTitle, articyLocation.technicalName, FieldType.Text);
                    Field.SetValue(location.fields, "Description", articyLocation.text.DefaultText, FieldType.Text);
                    if (prefs.UseTechnicalNames) Field.SetValue(location.fields, "Display Name", articyLocation.displayName.DefaultText, FieldType.Text);
                    SetFeatureFields(location.fields, articyLocation.features);
                    ConvertLocalizableText(location.fields, "Name", articyLocation.displayName);
                    if (prefs.UseTechnicalNames)
                    {
                        Field.SetValue(location.fields, "Name", articyLocation.technicalName, FieldType.Text);
                        Field.SetValue(location.fields, "Display Name", articyLocation.displayName.DefaultText, FieldType.Text);
                    }
                    if (prefs.CustomDisplayName) UseCustomDisplayName(location.fields);
                    database.locations.Add(location);
                }
            }
        }

        /// <summary>
        /// Converts flow fragments into items. (The quest system uses the Item[] table.)
        /// This is only called if the flow fragment mode is set to Quests.
        /// </summary>
        private void ConvertFlowFragmentsToQuests()
        {
            if (prefs.FlowFragmentMode != ConverterPrefs.FlowFragmentModes.Quests) return;
            foreach (ArticyData.FlowFragment articyFlowFragment in articyData.flowFragments.Values)
            {
                if (prefs.ConversionSettings.GetConversionSetting(articyFlowFragment.id).Include)
                {
                    itemID++;
                    Item item = template.CreateItem(itemID, articyFlowFragment.displayName.DefaultText);
                    Field.SetValue(item.fields, ArticyIdFieldTitle, articyFlowFragment.id, FieldType.Text);
                    Field.SetValue(item.fields, ArticyTechnicalNameFieldTitle, articyFlowFragment.technicalName, FieldType.Text);
                    Field.SetValue(item.fields, "Description", articyFlowFragment.text.DefaultText, FieldType.Text);
                    Field.SetValue(item.fields, "Success Description", string.Empty, FieldType.Text);
                    Field.SetValue(item.fields, "Failure Description", string.Empty, FieldType.Text);
                    Field.SetValue(item.fields, "State", "unassigned", FieldType.Text);
                    Field.SetValue(item.fields, "Is Item", "False", FieldType.Boolean);
                    SetFeatureFields(item.fields, articyFlowFragment.features);
                    ConvertLocalizableText(item.fields, "Name", articyFlowFragment.displayName);
                    database.items.Add(item);
                }
            }
        }

        private void SetFeatureFields(List<Field> fields, ArticyData.Features features)
        {
            // Note: quest State and Entry_#_State fields are fixed up in the Articy_#_#_Tools class
            // for each schema.
            foreach (ArticyData.Feature feature in features.features)
            {
                foreach (ArticyData.Property property in feature.properties)
                {
                    foreach (Field field in property.fields)
                    {
                        if (!string.IsNullOrEmpty(field.title))
                        {
                            var fieldTitle = ConvertSpecialTechnicalNames(field.title);
                            var fieldValue = IsOtherScriptField(fieldTitle) ? ConvertExpression(field.value, false) : field.value;
                            var existingField = Field.Lookup(fields, fieldTitle);
                            if (existingField != null)
                            {
                                existingField.value = fieldValue;
                            }
                            else
                            {
                                fields.Add(new Field(fieldTitle, fieldValue, field.type));
                            }
                        }
                    }
                }
            }
        }

        private void UseCustomDisplayName(List<Field> fields)
        {
            // Look for a field named 'DisplayName'. If present, replace 'Display Name' field with it.
            var customField = Field.Lookup(fields, "DisplayName");
            if (customField != null)
            {
                fields.RemoveAll(field => field.title == "Display Name");
                customField.title = "Display Name";
            }
        }

        private bool IsOtherScriptField(string fieldTitle)
        {
            return otherScriptFieldTitles.Contains(fieldTitle);
        }

        private string ConvertSpecialTechnicalNames(string technicalName)
        {
            if (string.Equals(technicalName, "Response_Menu_Sequence") ||
                string.Equals(technicalName, "Success_Description") ||
                string.Equals(technicalName, "Failure_Description") ||
                string.Equals(technicalName, "Entry_Count") ||
                Regex.Match(technicalName, @"^Entry_[0-9]").Success)
            {
                return technicalName.Replace("_", " ");
            }
            else
            {
                return technicalName;
            }
        }

        public static bool HasField(ArticyData.Features features, string fieldName, bool mustBeTrue)
        {
            foreach (ArticyData.Feature feature in features.features)
            {
                foreach (ArticyData.Property property in feature.properties)
                {
                    foreach (Field field in property.fields)
                    {
                        if (string.Equals(field.title, fieldName))
                        {
                            return mustBeTrue
                                ? string.Equals(field.value, "True", System.StringComparison.OrdinalIgnoreCase)
                                : true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Converts articy variable sets and variables into Dialogue System variables.
        /// </summary>
        private void ConvertVariables()
        {
            int variableID = 0;
            foreach (ArticyData.VariableSet articyVariableSet in articyData.variableSets.Values)
            {
                foreach (ArticyData.Variable articyVariable in articyVariableSet.variables)
                {
                    string fullName = ArticyData.FullVariableName(articyVariableSet, articyVariable);
                    fullVariableNames.Add(fullName);
                    if (prefs.ConversionSettings.GetConversionSetting(fullName).Include)
                    {
                        variableID++;
                        Variable variable = template.CreateVariable(variableID, fullName, articyVariable.defaultValue);
                        variable.Type = (articyVariable.dataType == ArticyData.VariableDataType.Boolean)
                            ? FieldType.Boolean
                                : ((articyVariable.dataType == ArticyData.VariableDataType.Integer)
                                ? FieldType.Number
                                : FieldType.Text);
                        if (!string.IsNullOrEmpty(articyVariable.description))
                        {
                            var descriptionField = Field.Lookup(variable.fields, "Description");
                            if (descriptionField != null)
                            {
                                descriptionField.value = articyVariable.description;
                            }
                            else
                            {
                                variable.fields.Add(new Field("Description", articyVariable.description, FieldType.Text));
                            }
                        }
                        database.variables.Add(variable);
                    }
                }
            }
        }

        #endregion

        #region Dialogue Conversion

        private void DeleteDocumentConversations()
        {
            database.conversations.RemoveAll(conversation => documentConversations.Contains(conversation));
        }

        /// <summary>
        /// Converts dialogues using the articy project's hierarchy.
        /// </summary>
        private void ConvertDialogues()
        {
            ResetStacks();
            onProgressCallback("Converting dialogues", 0.2f);
            ConvertDialoguesToConversations();
            onProgressCallback("Processing hierarchy", 0.3f);
            ProcessHierarchy();
            onProgressCallback("Sorting links by position", 0.7f);
            SortAllLinksByPosition();
            if (prefs.SplitTextOnPipes) SplitPipesIntoEntries();
            onProgressCallback("Converting VoiceOver properties", 0.9f);
            ConvertVoiceOverProperties();
        }

        private bool IncludeDialogue(string dialogueId)
        {
            var setting = (prefs == null) ? null : prefs.ConversionSettings.GetConversionSetting(dialogueId);
            return (setting == null) || setting.Include;
        }

        private void ConvertDialoguesToConversations()
        {
            foreach (var articyDialogue in articyData.dialogues.Values)
            {
                if (!IncludeDialogue(articyDialogue.id)) continue;
                CreateNewConversation(articyDialogue);
            }
        }

        /// <summary>
        /// Creates a new Dialogue System conversation from an articy dialogue. This also adds the
        /// conversation's mandatory first dialogue entry, "START".
        /// </summary>
        /// <returns>The new conversation.</returns>
        /// <param name='articyDialogue'>Articy dialogue.</param>
        private Conversation CreateNewConversation(ArticyData.Dialogue articyDialogue)
        {
            if (articyDialogue == null) return null;
            // Create conversation:
            conversationID++;
            var conversationTitle = string.Empty;
            conversationTitle += articyDialogue.displayName.DefaultText;
            if (articyDialogue.isDocument && !string.IsNullOrEmpty(prefs.DocumentsSubmenu))
            {
                conversationTitle = prefs.DocumentsSubmenu + "/" + conversationTitle;
            }
            Conversation conversation = template.CreateConversation(conversationID, conversationTitle);
            Field.SetValue(conversation.fields, ArticyIdFieldTitle, articyDialogue.id, FieldType.Text);
            Field.SetValue(conversation.fields, "Description", articyDialogue.text.DefaultText, FieldType.Text);
            SetConversationOverrideProperties(conversation, articyDialogue.features);
            SetFeatureFields(conversation.fields, articyDialogue.features);
            conversation.ActorID = FindActorIdFromArticyDialogue(articyDialogue, 0, 1);
            conversation.ConversantID = FindActorIdFromArticyDialogue(articyDialogue, 1, 2);
            database.conversations.Add(conversation);

            if (articyDialogue.isDocument) documentConversations.Add(conversation);

            // Create START entry:
            DialogueEntry startEntry = template.CreateDialogueEntry(GetNextConversationEntryID(conversation), conversationID, "START");
            startEntry.canvasRect = new Rect(articyDialogue.position.x, articyDialogue.position.y, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
            SetDialogueEntryParticipants(startEntry, conversation.ActorID, conversation.ConversantID);
            Field.SetValue(startEntry.fields, ArticyIdFieldTitle, articyDialogue.id, FieldType.Text);
            IndexDialogueEntryByArticyId(startEntry, articyDialogue.id);
            //-- Pins are added to input and output entries instead: ConvertPinExpressionsToConditionsAndScripts(startEntry, articyDialogue.pins);
            startEntry.outgoingLinks = new List<Link>();
            var conversationSequenceField = Field.Lookup(conversation.fields, "Sequence");
            if (conversationSequenceField != null && !string.IsNullOrEmpty(conversationSequenceField.value))
            {
                conversation.fields.Remove(conversationSequenceField);
                Field.SetValue(startEntry.fields, "Sequence", conversationSequenceField.value, FieldType.Text);
            }
            else
            {
                Field.SetValue(startEntry.fields, "Sequence", "Continue()", FieldType.Text);
            }
            conversation.dialogueEntries.Add(startEntry);

            // Convert dialogue's in and out pins to passthrough group entries:
            for (int i = 0; i < articyDialogue.pins.Count; i++)
            {
                var pin = articyDialogue.pins[i];
                if (pin.semantic == ArticyData.SemanticType.Output && prefs.RecursionMode == ConverterPrefs.RecursionModes.Off) continue;
                var entryID = GetNextConversationEntryID(conversation);
                var title = (pin.semantic == ArticyData.SemanticType.Input) ? "input" : "output";
                var entry = template.CreateDialogueEntry(entryID, conversationID, title);
                entry.canvasRect = new Rect(articyDialogue.position.x, articyDialogue.position.y, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
                SetDialogueEntryParticipants(entry, conversation.ConversantID, conversation.ActorID);
                if (pin.semantic == ArticyData.SemanticType.Input)
                {
                    ConvertPinExpressionsToConditionsAndScripts(entry, articyDialogue.pins, true, false);
                }
                else
                {
                    ConvertPinExpressionsToConditionsAndScripts(entry, articyDialogue.pins, false, true);
                }
                entry.isGroup = true;
                //Field.SetValue(entry.fields, "Sequence", "Continue()", FieldType.Text);
                Field.SetValue(entry.fields, ArticyIdFieldTitle, pin.id, FieldType.Text);

                if (pin.semantic == ArticyData.SemanticType.Input)
                {
                    var link = new Link();
                    link.originConversationID = conversationID;
                    link.originDialogueID = startEntry.id;
                    link.destinationConversationID = conversationID;
                    link.destinationDialogueID = entry.id;
                    startEntry.outgoingLinks.Add(link);
                }
                else
                {
                    unusedOutputEntries.Add(entry);
                }

                IndexDialogueEntryByArticyId(entry, pin.id);
                entry.outgoingLinks = new List<Link>();
                conversation.dialogueEntries.Add(entry);
                RecordPin(pin, entry);
            }

            return conversation;
        }

        // Extract special conversation override features and set conversation override settings:
        private void SetConversationOverrideProperties(Conversation conversation, ArticyData.Features features)
        {
            foreach (ArticyData.Feature feature in features.features)
            {
                foreach (ArticyData.Property property in feature.properties)
                {
                    for (int i = property.fields.Count - 1; i >= 0; i--)
                    {
                        var field = property.fields[i];
                        var deleteField = true;
                        switch (field.title)
                        {
                            case "ShowNPCSubtitlesDuringLine":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideSubtitleSettings = true;
                                conversation.overrideSettings.showNPCSubtitlesDuringLine = Tools.StringToBool(field.value);
                                break;
                            case "ShowNPCSubtitlesWithResponses":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideSubtitleSettings = true;
                                conversation.overrideSettings.showNPCSubtitlesWithResponses = Tools.StringToBool(field.value);
                                break;
                            case "ShowPCSubtitlesDuringLine":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideSubtitleSettings = true;
                                conversation.overrideSettings.showPCSubtitlesDuringLine = Tools.StringToBool(field.value);
                                break;
                            case "SkipPCSubtitleAfterResponseMenu":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideSubtitleSettings = true;
                                conversation.overrideSettings.skipPCSubtitleAfterResponseMenu = Tools.StringToBool(field.value);
                                break;
                            case "SubtitleCharsPerSecond":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideSubtitleSettings = true;
                                conversation.overrideSettings.subtitleCharsPerSecond = Tools.StringToFloat(field.value);
                                break;
                            case "MinSubtitleSeconds":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideSubtitleSettings = true;
                                conversation.overrideSettings.minSubtitleSeconds = Tools.StringToFloat(field.value);
                                break;
                            case "ContinueButton":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideSubtitleSettings = true;
                                conversation.overrideSettings.continueButton = StringToContinueButtonMode(field.value);
                                break;
                            //---
                            case "DefaultSequence":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideSequenceSettings = true;
                                conversation.overrideSettings.defaultSequence = field.value;
                                break;
                            case "DefaultPlayerSequence":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideSequenceSettings = true;
                                conversation.overrideSettings.defaultPlayerSequence = field.value;
                                break;
                            case "DefaultResponseMenuSequence":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideSequenceSettings = true;
                                conversation.overrideSettings.defaultResponseMenuSequence = field.value;
                                break;
                            //---
                            case "AlwaysForceResponseMenu":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideInputSettings = true;
                                conversation.overrideSettings.alwaysForceResponseMenu = Tools.StringToBool(field.value);
                                break;
                            case "IncludeInvalidEntries":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideInputSettings = true;
                                conversation.overrideSettings.includeInvalidEntries = Tools.StringToBool(field.value);
                                break;
                            case "ResponseTimeout":
                                conversation.overrideSettings.useOverrides = true;
                                conversation.overrideSettings.overrideInputSettings = true;
                                conversation.overrideSettings.responseTimeout = Tools.StringToFloat(field.value);
                                break;
                            default:
                                deleteField = false;
                                break;
                        }
                        if (deleteField)
                        {
                            property.fields.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private DisplaySettings.SubtitleSettings.ContinueButtonMode StringToContinueButtonMode(string value)
        {
            var enumValues = System.Enum.GetValues(typeof(DisplaySettings.SubtitleSettings.ContinueButtonMode));
            for (int i = 0; i < enumValues.Length; i++)
            {
                var enumMode = (DisplaySettings.SubtitleSettings.ContinueButtonMode)i;
                if (string.Equals(value, enumMode.ToString(), System.StringComparison.OrdinalIgnoreCase))
                {
                    return enumMode;
                }
            }
            return DisplaySettings.SubtitleSettings.ContinueButtonMode.Never;
        }

        private void SetDialogueEntryParticipants(DialogueEntry startEntry, int actorID, int conversantID)
        {
            startEntry.ActorID = actorID;
            startEntry.ConversantID = conversantID;
        }

        private int GetDefaultActorID(Conversation conversation)
        {
            return (conversation != null) ? conversation.ActorID : (prefs.UseDefaultActorsIfNoneAssignedToDialogue ? 1 : -1);
        }

        private int GetDefaultConversantID(Conversation conversation)
        {
            return (conversation != null) ? conversation.ConversantID : (prefs.UseDefaultActorsIfNoneAssignedToDialogue ? 2 : -1);
        }

        /// <summary>
        /// Creates a new Dialogue System conversation from an articy flow fragment. This also adds the
        /// conversation's mandatory first dialogue entry, "START".
        /// </summary>
        /// <returns>The new conversation.</returns>
        /// <param name='articyFlowFragment'>Articy flow fragment.</param>
        private Conversation FindOrCreateFlowFragmentConversation(ArticyData.FlowFragment articyFlowFragment, bool isTopLevel)
        {
            if (articyFlowFragment == null) return null;
            // Create conversation:
            conversationID++;
            var conversationTitle = articyFlowFragment.displayName.DefaultText + " Conversation";
            Conversation conversation = template.CreateConversation(conversationID, conversationTitle);
            Field.SetValue(conversation.fields, ArticyIdFieldTitle, articyFlowFragment.id, FieldType.Text);
            Field.SetValue(conversation.fields, "Description", articyFlowFragment.text.DefaultText, FieldType.Text);
            SetFeatureFields(conversation.fields, articyFlowFragment.features);
            var parentConversation = GetConversationStackTop();
            conversation.ActorID = GetDefaultActorID(parentConversation);
            conversation.ConversantID = GetDefaultConversantID(parentConversation);
            database.conversations.Add(conversation);

            // Create START entry:
            DialogueEntry startEntry = template.CreateDialogueEntry(GetNextConversationEntryID(conversation), conversationID, "START");
            SetDialogueEntryParticipants(startEntry, conversation.ActorID, conversation.ConversantID);
            Field.SetValue(startEntry.fields, ArticyIdFieldTitle, articyFlowFragment.id, FieldType.Text);
            IndexDialogueEntryByArticyId(startEntry, articyFlowFragment.id);
            ConvertPinExpressionsToConditionsAndScripts(startEntry, articyFlowFragment.pins);
            startEntry.outgoingLinks = new List<Link>();
            var conversationSequenceField = Field.Lookup(conversation.fields, "Sequence");
            if (conversationSequenceField != null && !string.IsNullOrEmpty(conversationSequenceField.value))
            {
                conversation.fields.Remove(conversationSequenceField);
                Field.SetValue(startEntry.fields, "Sequence", conversationSequenceField.value, FieldType.Text);
            }
            else
            {
                Field.SetValue(startEntry.fields, "Sequence", "Continue()", FieldType.Text);
            }
            conversation.dialogueEntries.Add(startEntry);

            // Convert dialogue's in and out pins to passthrough group entries:
            for (int i = 0; i < articyFlowFragment.pins.Count; i++)
            {
                var pin = articyFlowFragment.pins[i];
                if (pin.semantic == ArticyData.SemanticType.Output && prefs.RecursionMode == ConverterPrefs.RecursionModes.Off) continue;
                var entryID = GetNextConversationEntryID(conversation);
                var title = (pin.semantic == ArticyData.SemanticType.Input) ? "input" : "output";
                var entry = template.CreateDialogueEntry(entryID, conversationID, title);
                SetDialogueEntryParticipants(entry, conversation.ConversantID, conversation.ActorID);
                entry.isGroup = true;
                Field.SetValue(entry.fields, "Sequence", "Continue()", FieldType.Text);
                Field.SetValue(entry.fields, ArticyIdFieldTitle, pin.id, FieldType.Text);

                if (pin.semantic == ArticyData.SemanticType.Input)
                {
                    var link = new Link();
                    link.originConversationID = conversationID;
                    link.originDialogueID = startEntry.id;
                    link.destinationConversationID = conversationID;
                    link.destinationDialogueID = entry.id;
                    startEntry.outgoingLinks.Add(link);
                }
                else if (!(isTopLevel && pin.semantic == ArticyData.SemanticType.Output))
                {
                    unusedOutputEntries.Add(entry);
                }

                IndexDialogueEntryByArticyId(entry, pin.id);
                ConvertPinExpressionsToConditionsAndScripts(entry, articyFlowFragment.pins);
                entry.outgoingLinks = new List<Link>();
                conversation.dialogueEntries.Add(entry);
                RecordPin(pin, entry);
            }

            if (isTopLevel)
            {
                // Connect input pin entry to output pin entries:
                var inputEntry = conversation.dialogueEntries.Find(x => x.Title == "input");
                if (inputEntry != null)
                {
                    foreach (var outputEntry in conversation.dialogueEntries)
                    {
                        if (outputEntry.Title == "output")
                        {
                            var link = new Link();
                            link.originConversationID = conversationID;
                            link.originDialogueID = inputEntry.id;
                            link.destinationConversationID = conversationID;
                            link.destinationDialogueID = outputEntry.id;
                            inputEntry.outgoingLinks.Add(link);
                        }
                    }
                }
            }

            return conversation;
        }

        private void ProcessHierarchy()
        {
            onProgressCallback("Processing dialogue nodes", 0.4f);
            BuildDialogueEntriesFromNode(articyData.hierarchy.node, 0);
            onProgressCallback("Connecting dialogue nodes", 0.5f);
            ProcessConnections();
            onProgressCallback("Checking if jumps are group nodes", 0.6f);
            CheckJumpsForGroupNodes();
        }

        private const int MaxRecursionDepth = 1000;

        /// <summary>
        /// Processes a node in the hierarchy to build dialogue entries,
        /// also recursively processing the node's children.
        /// </summary>
        /// <param name='node'>Node to process.</param>
        private void BuildDialogueEntriesFromNode(ArticyData.Node node, int recursionDepth)
        {
            if (recursionDepth > MaxRecursionDepth)
            {
                Debug.LogError("Dialogue System: Internal error - Exceeded max recursion depth " + MaxRecursionDepth + " in ArticyConverter.BuildDialogueEntriesFromNode.");
                return;
            }
            var addedTopLevelFlowConversation = false;
            if ((node.type == ArticyData.NodeType.Dialogue) && !IncludeDialogue(node.id)) return;
            switch (node.type)
            {
                case ArticyData.NodeType.FlowFragment:
                    var flowFragment = LookupArticyFlowFragment(node.id);
                    PushFlowFragment(flowFragment);
                    if (GetConversationStackTop() != null)
                    {
                        // The stack has a conversation, so push a nested conversation:
                        if (prefs.FlowFragmentMode == ConverterPrefs.FlowFragmentModes.NestedConversationGroups && articyData.flowFragments.ContainsKey(node.id))
                        {
                            var flowFragmentConversation = FindOrCreateFlowFragmentConversation(articyData.flowFragments[node.id], false);
                            if (flowFragmentConversation != null)
                            {
                                PushConversation(flowFragmentConversation);
                                PrependFlowStackToConversationTitle(flowFragmentConversation);
                            }
                        }
                        else
                        {
                            AddFlowFragmentAsDialogueEntry(GetConversationStackTop(), flowFragment);
                        }
                    }
                    else if (prefs.CreateConversationsForLooseFlow)
                    {
                        // Otherwise, make this flow fragment a top-level conversation:
                        var flowFragmentConversation = FindOrCreateFlowFragmentConversation(articyData.flowFragments[node.id], true);
                        if (flowFragmentConversation != null)
                        {
                            PushConversation(flowFragmentConversation);
                            PrependFlowStackToConversationTitle(flowFragmentConversation);
                            addedTopLevelFlowConversation = true;
                        }
                    }
                    break;
                case ArticyData.NodeType.Dialogue:
                    var conversation = database.conversations.Find(x => string.Equals(x.LookupValue(ArticyIdFieldTitle), node.id));
                    PushConversation(conversation);
                    PrependFlowStackToConversationTitle(conversation);
                    break;
                case ArticyData.NodeType.DialogueFragment:
                    BuildDialogueEntryFromDialogueFragment(GetConversationStackTop(), LookupArticyDialogueFragment(node.id));
                    break;
                case ArticyData.NodeType.Hub:
                    BuildDialogueEntryFromHub(GetConversationStackTop(), LookupArticyHub(node.id));
                    break;
                case ArticyData.NodeType.Jump:
                    BuildDialogueEntryFromJump(GetConversationStackTop(), LookupArticyJump(node.id));
                    break;
                case ArticyData.NodeType.Condition:
                    BuildDialogueEntriesFromCondition(GetConversationStackTop(), LookupArticyCondition(node.id));
                    break;
                case ArticyData.NodeType.Instruction:
                    BuildDialogueEntryFromInstruction(GetConversationStackTop(), LookupArticyInstruction(node.id));
                    break;
            }

            // Process child nodes:
            foreach (ArticyData.Node childNode in node.nodes)
            {
                BuildDialogueEntriesFromNode(childNode, recursionDepth + 1);
            }

            // Pop from stacks as we leave node:
            switch (node.type)
            {
                case ArticyData.NodeType.FlowFragment:
                    if (!addedTopLevelFlowConversation)
                    {
                        PopFlowFragment();
                        if (prefs.FlowFragmentMode == ConverterPrefs.FlowFragmentModes.NestedConversationGroups)
                        {
                            var pushedFlowFragmentConversation = database.conversations.Find(x => string.Equals(x.LookupValue(ArticyIdFieldTitle), node.id));
                            if (pushedFlowFragmentConversation != null) PopConversation();
                        }
                    }
                    break;
                case ArticyData.NodeType.Dialogue:
                    PopConversation();
                    break;
            }
        }

        private void PrependFlowStackToConversationTitle(Conversation conversation)
        {
            var isFlowFragmentModeConversationGroups = prefs.FlowFragmentMode == ConverterPrefs.FlowFragmentModes.ConversationGroups || prefs.FlowFragmentMode == ConverterPrefs.FlowFragmentModes.NestedConversationGroups;
            if (conversation == null || !isFlowFragmentModeConversationGroups || flowFragmentNameStack.Count <= 0) return;
            var s = string.Empty;
            foreach (var flowFragmentName in flowFragmentNameStack)
            {
                s += flowFragmentName + "/";
            }
            conversation.Title = s + conversation.Title;
        }

        private void RecordPins(List<ArticyData.Pin> pins, DialogueEntry entry)
        {
            if (pins == null) return;
            for (int i = 0; i < pins.Count; i++)
            {
                RecordPin(pins[i], entry);
            }
        }

        private void RecordPin(ArticyData.Pin pin, DialogueEntry entry)
        {
            if (pin == null || entry == null || entriesByPinID.ContainsKey(pin.id)) return;
            entriesByPinID.Add(pin.id, entry);
            Field.SetValue(entry.fields, (pin.semantic == ArticyData.SemanticType.Input) ? "InputId" : "OutputId", pin.id);
        }

        private void ProcessConnections()
        {
            // Process regular connections:
            foreach (var kvp in articyData.connections)
            {
                ProcessConnectionNew(kvp.Value);
            }

            // Process jumps:
            foreach (var kvp in jumpsToProcess)
            {
                ProcessJumpConnection(kvp.Key, kvp.Value);
            }

            // Remove unused output entries:
            RemoveUnusedOutputEntries();
        }

        private void ProcessConnectionNew(ArticyData.Connection connection)
        {
            if (connection == null) return;
            if (!entriesByPinID.ContainsKey(connection.source.pinRef))
            {
                return;
            }
            if (!entriesByPinID.ContainsKey(connection.target.pinRef))
            {
                return;
            }
            var sourceEntry = entriesByPinID[connection.source.pinRef];
            var targetEntry = entriesByPinID[connection.target.pinRef];
            var linksToSelf = sourceEntry.conversationID == targetEntry.conversationID && sourceEntry.id == targetEntry.id;
            if (!linksToSelf)
            {
                var link = new Link();
                link.originConversationID = sourceEntry.conversationID;
                link.originDialogueID = sourceEntry.id;
                link.destinationConversationID = targetEntry.conversationID;
                link.destinationDialogueID = targetEntry.id;
                link.isConnector = false;
                link.priority = ArticyData.ColorToPriority(connection.color);
                sourceEntry.outgoingLinks.Add(link);
            }
            MarkTargetUsed(targetEntry);
        }

        private void ProcessJumpConnection(ArticyData.Jump jump, DialogueEntry jumpEntry)
        {
            if (jump == null || jumpEntry == null || !entriesByPinID.ContainsKey(jump.target.pinRef)) return;
            var targetEntry = entriesByPinID[jump.target.pinRef];
            Link link = new Link();
            link.originConversationID = jumpEntry.conversationID;
            link.originDialogueID = jumpEntry.id;
            link.destinationConversationID = targetEntry.conversationID;
            link.destinationDialogueID = targetEntry.id;
            link.isConnector = false;
            jumpEntry.outgoingLinks.Add(link);
            MarkTargetUsed(targetEntry);
        }

        private void MarkTargetUsed(DialogueEntry targetEntry)
        {
            unusedOutputEntries.Remove(targetEntry);
        }

        private void RemoveUnusedOutputEntries()
        {
            for (int i = 0; i < unusedOutputEntries.Count; i++)
            {
                var entry = unusedOutputEntries[i];
                var conversation = database.GetConversation(entry.conversationID);
                if (conversation == null) continue;
                conversation.dialogueEntries.Remove(entry);
            }
        }

        private ArticyData.DialogueFragment LookupArticyDialogueFragment(string id)
        {
            return articyData.dialogueFragments.ContainsKey(id) ? articyData.dialogueFragments[id] : null;
        }

        private ArticyData.Hub LookupArticyHub(string id)
        {
            return articyData.hubs.ContainsKey(id) ? articyData.hubs[id] : null;
        }

        private ArticyData.Jump LookupArticyJump(string id)
        {
            return articyData.jumps.ContainsKey(id) ? articyData.jumps[id] : null;
        }

        private ArticyData.Condition LookupArticyCondition(string id)
        {
            return articyData.conditions.ContainsKey(id) ? articyData.conditions[id] : null;
        }

        private ArticyData.Instruction LookupArticyInstruction(string id)
        {
            return articyData.instructions.ContainsKey(id) ? articyData.instructions[id] : null;
        }

        private ArticyData.Connection LookupArticyConnection(string id)
        {
            return articyData.connections.ContainsKey(id) ? articyData.connections[id] : null;
        }

        private ArticyData.FlowFragment LookupArticyFlowFragment(string id)
        {
            return articyData.flowFragments.ContainsKey(id) ? articyData.flowFragments[id] : null;
        }

        /// <summary>
        /// Converts a dialogue fragment, including fields such as text, sequence, and pins, but doesn't
        /// connect it yet.
        /// </summary>
        /// <param name='conversation'>Conversation.</param>
        /// <param name='fragment'>Fragment.</param>
        private void BuildDialogueEntryFromDialogueFragment(Conversation conversation, ArticyData.DialogueFragment fragment)
        {
            if (fragment == null || conversation == null) return;
            var entry = CreateNewDialogueEntry(conversation, fragment.displayName.DefaultText, fragment.id);
            entry.canvasRect = new Rect(fragment.position.x, fragment.position.y, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
            ConvertLocalizableText(entry, "Dialogue Text", fragment.text, true);
            ConvertLocalizableText(entry, "Menu Text", fragment.menuText, true);
            ConvertLocalizableText(entry, "Title", fragment.displayName);
            SetFeatureFields(entry.fields, fragment.features);
            switch (prefs.StageDirectionsMode)
            {
                case ConverterPrefs.StageDirModes.Sequences:
                    var defaultSequenceText = fragment.stageDirections.DefaultText;
                    if (!string.IsNullOrEmpty(defaultSequenceText) && (defaultSequenceText.Contains("(") || defaultSequenceText.Contains("{{")))
                    {
                        ConvertLocalizableText(entry, "Sequence", fragment.stageDirections);
                    }
                    break;
                case ConverterPrefs.StageDirModes.Description:
                    var description = fragment.stageDirections.DefaultText;
                    Field.SetValue(entry.fields, "Description", description);
                    break;
            }
            var scriptField = Field.Lookup(entry.fields, "Script");
            if (scriptField != null) // Script is handled differently.
            {
                entry.userScript = AddToUserScript(entry.userScript, scriptField.value);
                entry.fields.Remove(scriptField);
            }
            Actor actor = FindActorByArticyId(fragment.speakerIdRef);
            entry.ActorID = (actor != null) ? actor.id : (prefs.UseDefaultActorsIfNoneAssignedToDialogue ? conversation.ActorID : 0);
            var conversantEntity = Field.Lookup(entry.fields, "ConversantEntity");
            var conversantActor = (conversantEntity == null) ? null
                : (prefs.ConvertSlotsAs == ConverterPrefs.ConvertSlotsModes.ID) ? FindActorByArticyId(conversantEntity.value)
                : (prefs.ConvertSlotsAs == ConverterPrefs.ConvertSlotsModes.TechnicalName) ? FindActorByTechnicalName(conversantEntity.value)
                : FindActorByDisplayName(conversantEntity.value);
            if (conversantActor != null)
            {
                entry.ConversantID = conversantActor.id;
            }
            else
            {
                entry.ConversantID = prefs.UseDefaultActorsIfNoneAssignedToDialogue ? ((entry.ActorID == conversation.ActorID) ? conversation.ConversantID : conversation.ActorID) : 0;
            }
            ConvertPinExpressionsToConditionsAndScripts(entry, fragment.pins);
            RecordPins(fragment.pins, entry);

            // No longer used:
            //// Handle documents:
            //if (documentConversation != null && lastDocumentEntry != null && !DoesLinkExist(lastDocumentEntry.outgoingLinks, entry))
            //{
            //    Debug.Log("Adding link in conv " + documentConversation.Title + " entry " + lastDocumentEntry.id + " to entry " + entry.conversationID + ":" + entry.id);
            //    var link = new Link(lastDocumentEntry.conversationID, lastDocumentEntry.id, entry.conversationID, entry.id);
            //    lastDocumentEntry.outgoingLinks.Add(link);
            //    lastDocumentEntry = entry;
            //}
        }

        private bool DoesLinkExist(List<Link> outgoingLinks, DialogueEntry destination)
        {
            if (outgoingLinks == null || destination == null) return false;
            for (int i = 0; i < outgoingLinks.Count; i++)
            {
                if (outgoingLinks[i] != null && outgoingLinks[i].destinationConversationID == destination.conversationID &&
                    outgoingLinks[i].destinationDialogueID == destination.id)
                {
                    return true;
                }
            }
            return false;
        }

        private void AddFlowFragmentAsDialogueEntry(Conversation conversation, ArticyData.FlowFragment flowFragment)
        {
            if (flowFragment == null || conversation == null) return;
            var entry = CreateNewDialogueEntry(conversation, flowFragment.displayName.DefaultText, flowFragment.id);
            entry.canvasRect = new Rect(flowFragment.position.x, flowFragment.position.y, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
            ConvertLocalizableText(entry, "Title", flowFragment.displayName);
            entry.Title = "Flow: " + entry.Title;
            SetFeatureFields(entry.fields, flowFragment.features);
            var scriptField = Field.Lookup(entry.fields, "Script");
            if (scriptField != null) // Script is handled differently.
            {
                entry.userScript = AddToUserScript(entry.userScript, scriptField.value);
                entry.fields.Remove(scriptField);
            }
            entry.ActorID = conversation.ActorID;
            entry.ConversantID = (entry.ActorID == conversation.ActorID) ? conversation.ConversantID : conversation.ActorID;
            if (!string.IsNullOrEmpty(prefs.FlowFragmentScript))
            {
                entry.userScript = prefs.FlowFragmentScript + "(\"" + flowFragment.displayName.DefaultText.Replace("\"", "'") + "\")";
            }
            entry.isGroup = true;
            ConvertPinExpressionsToConditionsAndScripts(entry, flowFragment.pins);
            RecordPins(flowFragment.pins, entry);
        }

        /// <summary>
        /// Converts a hub into a group dialogue entry in a conversation.
        /// </summary>
        /// <param name='conversation'>
        /// Conversation.
        /// </param>
        /// <param name='hub'>
        /// Hub.
        /// </param>
        private void BuildDialogueEntryFromHub(Conversation conversation, ArticyData.Hub hub)
        {
            if (hub == null || conversation == null) return;
            DialogueEntry groupEntry = CreateNewDialogueEntry(conversation, hub.displayName.DefaultText, hub.id);
            groupEntry.canvasRect = new Rect(hub.position.x, hub.position.y, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
            SetFeatureFields(groupEntry.fields, hub.features);
            ConvertLocalizableText(groupEntry, "Title", hub.displayName);
            groupEntry.isGroup = true;
            ConvertPinExpressionsToConditionsAndScripts(groupEntry, hub.pins);
            RecordPins(hub.pins, groupEntry);
        }

        /// <summary>
        /// Converts a jump into a group dialogue entry in a conversation.
        /// </summary>
        /// <param name='conversation'>
        /// Conversation.
        /// </param>
        /// <param name='jump'>
        /// Jump.
        /// </param>
        private void BuildDialogueEntryFromJump(Conversation conversation, ArticyData.Jump jump)
        {
            if (jump == null || conversation == null) return;
            DialogueEntry jumpEntry = CreateNewDialogueEntry(conversation, jump.displayName.DefaultText, jump.id);
            jumpEntry.canvasRect = new Rect(jump.position.x, jump.position.y, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
            SetFeatureFields(jumpEntry.fields, jump.features);
            ConvertLocalizableText(jumpEntry, "Title", jump.displayName);
            jumpEntry.isGroup = true; // We'll set isGroup correctly in a final pass in CheckJumpsForGroupNodes.
            //jumpEntry.currentSequence = "Continue()";
            ConvertPinExpressionsToConditionsAndScripts(jumpEntry, jump.pins);
            RecordPins(jump.pins, jumpEntry);
            jumpsToProcess.Add(jump, jumpEntry);

            var flowFragment = FindFlowFragment(jump.target.idRef);
            if (flowFragment != null)
            {
                var flowEntry = CreateNewDialogueEntry(conversation, "Flow: " + flowFragment.displayName.DefaultText, flowFragment.id);
                flowEntry.canvasRect = new Rect(jump.position.x, jump.position.y + 32f, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
                SetFeatureFields(flowEntry.fields, flowFragment.features);
                flowEntry.isGroup = true;
                //flowEntry.currentSequence = "Continue()";
                ConvertPinExpressionsToConditionsAndScripts(flowEntry, flowFragment.pins);
                RecordPins(flowFragment.pins, flowEntry);
            }
        }

        /// <summary>
        /// Jumps that link only to other jumps or group nodes should be group nodes themselves.
        /// This method sets the isGroup property correctly for all jump entries.
        /// 
        /// CHANGED [2.2.1]: Jumps should always be groups unless they have a script. This is because
        /// scripts are always processed when passing through the group, which we don't want to do
        /// if we don't end up using the jump's destination entries.
        /// </summary>
        private void CheckJumpsForGroupNodes()
        {
            var jumpEntries = new HashSet<DialogueEntry>(jumpsToProcess.Values);
            foreach (var jumpEntry in jumpEntries)
            {
                if (jumpEntry == null) continue;
                jumpEntry.isGroup = string.IsNullOrEmpty(jumpEntry.userScript);
                //jumpEntry.isGroup = true;
                //for (int i = 0; i < jumpEntry.outgoingLinks.Count; i++)
                //{
                //    var destEntry = database.GetDialogueEntry(jumpEntry.outgoingLinks[i]);
                //    if (destEntry == null) continue;
                //    var linksToJump = jumpEntries.Contains(jumpEntry);
                //    var linksToGroup = destEntry.isGroup;
                //    if (!(linksToJump || linksToGroup))
                //    {
                //        jumpEntry.isGroup = false;
                //        break;
                //    }
                //}
            }
        }

        /// <summary>
        /// Converts a condition node into multiple dialogue entries - the condition entry and then
        /// some number of outgoing pins for true and false results.
        /// </summary>
        /// <param name='conversation'>Conversation.</param>
        /// <param name='condition'>Condition.</param>
        private void BuildDialogueEntriesFromCondition(Conversation conversation, ArticyData.Condition condition)
        {
            if (condition == null || conversation == null) return;

            // Main condition node:
            var conditionEntry = CreateNewDialogueEntry(conversation, condition.expression, condition.id);
            conditionEntry.canvasRect = new Rect(condition.position.x, condition.position.y, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
            conditionEntry.ActorID = conversation.ConversantID;
            conditionEntry.ConversantID = conversation.ActorID;
            conditionEntry.currentDialogueText = string.Empty;
            conditionEntry.currentMenuText = string.Empty;
            //conditionEntry.currentSequence = "Continue()";
            conditionEntry.isGroup = true;

            string trueLuaConditions = ConvertExpression(condition.expression, true);
            string falseLuaConditions = string.IsNullOrEmpty(trueLuaConditions)
                ? "false" : string.Format("({0}) == false", trueLuaConditions);

            // Separate child nodes for each output pin:
            float y = condition.position.y;
            foreach (var pin in condition.pins)
            {
                if (pin.semantic == ArticyData.SemanticType.Input)
                {
                    RecordPin(pin, conditionEntry);
                    conditionEntry.conditionsString = AddToConditions(conditionEntry.conditionsString, ConvertExpression(pin.expression, true));
                }
                else if (pin.semantic == ArticyData.SemanticType.Output)
                {
                    bool isTruePath = (pin.index == 0);
                    string title = isTruePath ? condition.expression : string.Format("!({0})", condition.expression);
                    var entry = CreateNewDialogueEntry(conversation, title, condition.id);
                    entry.canvasRect = new Rect(condition.position.x, y, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
                    y += 2f;
                    entry.ActorID = conversation.ConversantID;
                    entry.ConversantID = conversation.ActorID;
                    entry.currentDialogueText = string.Empty;
                    entry.currentMenuText = string.Empty;
                    //entry.currentSequence = "Continue()";
                    entry.isGroup = true;
                    string luaConditions = isTruePath ? trueLuaConditions : falseLuaConditions;
                    entry.conditionsString = AddToConditions(entry.conditionsString, luaConditions);
                    entry.userScript = AddToUserScript(entry.userScript, ConvertExpression(pin.expression, false));

                    Link link = new Link();
                    link.originConversationID = conditionEntry.conversationID;
                    link.originDialogueID = conditionEntry.id;
                    link.destinationConversationID = entry.conversationID;
                    link.destinationDialogueID = entry.id;
                    link.isConnector = false;
                    conditionEntry.outgoingLinks.Add(link);
                    RecordPin(pin, entry);
                }
            }
        }

        private void BuildDialogueEntryFromInstruction(Conversation conversation, ArticyData.Instruction instruction)
        {
            if (instruction == null || conversation == null) return;
            DialogueEntry entry = CreateNewDialogueEntry(conversation, instruction.expression, instruction.id);
            entry.ActorID = conversation.ConversantID;
            entry.ConversantID = conversation.ActorID;
            entry.currentDialogueText = string.Empty;
            entry.currentMenuText = string.Empty;
            entry.currentSequence = "Continue()"; // Since it's not a group, make sure we continue past it immediately.
            entry.isGroup = false; // Since groups are processed one level ahead, don't make this a group: entry.isGroup = true;
            entry.conditionsString = string.Empty;
            entry.userScript = AddToUserScript(entry.userScript, ConvertExpression(instruction.expression, false));
            ConvertPinExpressionsToConditionsAndScripts(entry, instruction.pins);
            RecordPins(instruction.pins, entry);
        }

        private string AddToConditions(string conditions, string moreConditions)
        {
            return string.IsNullOrEmpty(conditions) ? moreConditions : string.Format("({0}) and ({1})", conditions, moreConditions);
        }

        private string AddToUserScript(string script, string moreScript)
        {
            return string.IsNullOrEmpty(script) ? moreScript : string.Format("{0}; {1}", script, moreScript);
        }

        /// <summary>
        /// Creates a new dialogue entry and adds it to a conversation.
        /// </summary>
        /// <returns>The new dialogue entry.</returns>
        /// <param name='conversation'>Conversation.</param>
        /// <param name='title'>Title.</param>
        /// <param name='articyId'>Articy identifier.</param>
        private DialogueEntry CreateNewDialogueEntry(Conversation conversation, string title, string articyId)
        {
            if (conversation == null)
            {
                Debug.Log("Conversation is null! " + articyId + " / " + title);
                return null;
            }
            DialogueEntry entry = template.CreateDialogueEntry(GetNextConversationEntryID(conversation), conversation.id, title);
            SetDialogueEntryParticipants(entry, conversation.ConversantID, conversation.ActorID); // Assume speaker is conversant until changed.
            Field.SetValue(entry.fields, ArticyIdFieldTitle, articyId, FieldType.Text);
            IndexDialogueEntryByArticyId(entry, articyId);
            conversation.dialogueEntries.Add(entry);
            return entry;
        }

        /// <summary>
        /// Converts input pins as a dialogue entry's Conditions, and output pins as User Script.
        /// </summary>
        /// <param name='entry'>Entry.</param>
        /// <param name='pins'>Pins./param>
        /// <param name="convertInput">Apply pin's input conditions to entry.</param>
        /// <param name="convertOutput">Apply pin's output conditions to entry.</param>
        private void ConvertPinExpressionsToConditionsAndScripts(DialogueEntry entry, List<ArticyData.Pin> pins, bool convertInput = true, bool convertOutput = true)
        {
            foreach (ArticyData.Pin pin in pins)
            {
                switch (pin.semantic)
                {
                    case ArticyData.SemanticType.Input:
                        if (convertInput)
                        {
                            entry.conditionsString = AddToConditions(entry.conditionsString, ConvertExpression(pin.expression, true));
                        }
                        break;
                    case ArticyData.SemanticType.Output:
                        if (convertOutput)
                        {
                            entry.userScript = AddToUserScript(entry.userScript, ConvertExpression(pin.expression, false));
                        }
                        break;
                    default:
                        Debug.LogWarning("Dialogue System: Unexpected semantic type " + pin.semantic + " for pin " + pin.id + ".");
                        break;
                }
            }
        }

        /// <summary>
        /// Converts an articy expresso expression into Lua.
        /// </summary>
        /// <returns>A Lua version of the expression.</returns>
        /// <param name='expression'>articy expresso expression.</param>
        public static string ConvertExpression(string expression, bool isCondition = false)
        {
            if (string.IsNullOrEmpty(expression)) return expression;

            if (isCondition && expression.Trim().StartsWith("//") && !expression.Contains("\n")) return string.Empty;

            // If already Lua, return it:
            if (expression.Contains("Variable[")) return expression;

            // If no semicolon, convert single expression:
            if (!expression.Contains(";")) return ConvertSingleExpression(expression);

            var s = string.Empty;
            var singleExpressions = expression.Split(';'); // [TODO]: Handle semicolons nested inside string literals.
            for (int i = 0; i < singleExpressions.Length; i++)
            {
                var singleExpression = singleExpressions[i];
                if (isCondition && singleExpression.Trim().StartsWith("//")) continue;
                if (string.IsNullOrEmpty(singleExpression)) continue;
                if (s.Length > 0) s += ";\n";
                s += ConvertSingleExpression(singleExpression);
            }
            return s;
        }

        public static string ConvertSingleExpression(string expression)
        {
            if (string.IsNullOrEmpty(expression)) return expression;

            // If already Lua, return it:
            if (expression.Contains("Variable[")) return expression;

            // If no quotes, handle it as a single fragment:
            if (!expression.Contains("\"")) return ConvertExpressionFragment(expression);

            // Otherwise split on quotes except escaped quotes:
            string[] fragments = Regex.Split(expression, @"(?<=[^\\])[\""]", RegexOptions.None);

            var s = string.Empty;
            bool insideString = false;
            for (int i = 0; i < fragments.Length; i++)
            {
                s += insideString ? fragments[i] : ConvertExpressionFragment(fragments[i]);
                if (i + 1 < fragments.Length) s += '"';
                insideString = !insideString;
            }

            return s;
        }

        /// <summary>
        /// Converts an articy expresso expression into Lua without handling quotes.
        /// This is a helper method meant to be called by ConvertExpression().
        /// </summary>
        /// <returns>A Lua version of the expression.</returns>
        /// <param name='expression'>articy expresso expression.</param>
        private static string ConvertExpressionFragment(string expression)
        {
            if (string.IsNullOrEmpty(expression)) return expression;

            // If already Lua, return it:
            if (expression.Contains("Variable[")) return expression;

            // Convert comments:
            string s = expression.Trim().Replace("///", "").Replace("//", "--");

            // Convert random to math.random:
            s = s.Replace("random(", "math.random(").Replace("random (", "math.random(");

            // Convert conditionals:
            s = s.Replace("&&", " and ");
            s = s.Replace("||", " or ");
            s = s.Replace("!=", "~=");

            var incDecMatchEvaluator = new MatchEvaluator(IncDecMatchEvaluator);

            // Convert variable names: (fixed to use regex to handle variable names that are subsets of other variable names)
            foreach (string fullVariableName in fullVariableNames)
            {
                if (s.Contains(fullVariableName))
                {
                    // Convert variable++ to variable = variable + 1:
                    string pattern = @"\b" + fullVariableName + @"\b\s*(\+\+|\-\-)";
                    s = Regex.Replace(s, pattern, incDecMatchEvaluator);

                    // Convert expresso variable name to Lua:
                    pattern = @"\b" + fullVariableName + @"\b";
                    string luaVariableReference = string.Format("Variable[\"{0}\"]", fullVariableName);
                    s = Regex.Replace(s, pattern, luaVariableReference);
                }
            }

            // Convert negation (!) to "==false":
            s = s.Replace("!Variable", "not Variable");
            s = s.Replace("!(", "not (");
            const string negatedFunctionPattern = @"!\b(_\w+|[\w-[0-9_]]\w*)\b";
            s = Regex.Replace(s, negatedFunctionPattern, (match) =>
            {
                return "not " + match.Value.Substring(1);
            });

            // Convert arithmetic assignment operators (e.g., +=):
            if (ContainsArithmeticAssignment(s))
            {
                string[] tokens = s.Split(null);
                for (int i = 1; i < tokens.Length; i++)
                {
                    string token = tokens[i];
                    if (ContainsArithmeticAssignment(token))
                    {
                        char operation = token[0];
                        tokens[i] = string.Format("= {0} {1}", tokens[i - 1], operation);
                    }
                }
                s = string.Join(" ", tokens);
            }

            return s;
        }

        public static string IncDecMatchEvaluator(Match match)
        {
            var variableName = match.Value.Substring(0, match.Value.Length - 2).Trim();
            var operation = match.Value.Substring(match.Value.Length - 1);
            return variableName + " = " + variableName + " " + operation + " 1";
        }

        private static bool ContainsArithmeticAssignment(string s)
        {
            return (s != null) && (s.Contains("+=") || s.Contains("-="));
        }

        private void ConvertLocalizableText(DialogueEntry entry, string baseFieldTitle, ArticyData.LocalizableText localizableText, bool replaceNewlines = false)
        {
            if (entry == null) return;
            foreach (KeyValuePair<string, string> kvp in localizableText.localizedString)
            {
                if (string.IsNullOrEmpty(kvp.Key))
                {
                    Field.SetValue(entry.fields, baseFieldTitle, RemoveFormattingTags(kvp.Value, replaceNewlines), FieldType.Text);
                }
                else
                {
                    string localizedTitle = string.Equals("Dialogue Text", baseFieldTitle) ? kvp.Key : string.Format("{0} {1}", baseFieldTitle, kvp.Key);
                    Field.SetValue(entry.fields, localizedTitle, RemoveFormattingTags(kvp.Value, replaceNewlines), FieldType.Localization);
                }
            }
        }

        private void ConvertLocalizableText(List<Field> fields, string baseFieldTitle, ArticyData.LocalizableText localizableText)
        {
            foreach (KeyValuePair<string, string> kvp in localizableText.localizedString)
            {
                if (string.IsNullOrEmpty(kvp.Key))
                {
                    Field.SetValue(fields, baseFieldTitle, RemoveFormattingTags(kvp.Value), FieldType.Text);
                }
                else
                {
                    string localizedTitle = string.Equals("Dialogue Text", baseFieldTitle) ? kvp.Key : string.Format("{0} {1}", baseFieldTitle, kvp.Key);
                    Field.SetValue(fields, localizedTitle, RemoveFormattingTags(kvp.Value), FieldType.Localization);
                }
            }
        }

        private string RemoveFormattingTags(string s, bool replaceNewlines = false)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (replaceNewlines && s.Contains(@"\n")) s = s.Replace(@"\n", "\n");
            if (s.Contains("font-size"))
            {
                Regex regex = new Regex("{font-size:[0-9]+pt;}");
                return regex.Replace(s, string.Empty);
            }
            else
            {
                return s;
            }
        }

        /// <summary>
        /// Sets a conversation's start cutscene to None() if it's otherwise not set.
        /// </summary>
        /// <param name='conversation'>Conversation.</param>
        private static void SetConversationStartCutsceneToNone(Conversation conversation)
        {
            DialogueEntry entry = conversation.GetFirstDialogueEntry();
            if (entry == null)
            {
                Debug.LogWarning("Dialogue System: Conversation '" + conversation.Title + "' doesn't have a START dialogue entry.");
            }
            else
            {
                if (string.IsNullOrEmpty(entry.currentSequence)) entry.currentSequence = "Continue()";
            }
        }

        private Conversation FindConversationByArticyId(string articyId)
        {
            foreach (var conversation in database.conversations)
            {
                if (string.Equals(Field.LookupValue(conversation.fields, ArticyIdFieldTitle), articyId)) return conversation;
            }
            return null;
        }

        private DialogueEntry FindDialogueEntryByArticyId(Conversation conversation, string articyId)
        {
            if (conversation == null) return null;

            // Check cache first:
            if (entriesByArticyId.ContainsKey(articyId))
            {
                var list = entriesByArticyId[articyId];
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].conversationID == conversation.id) return list[i];
                }
            }

            //Then check all entries in conversation:
            foreach (DialogueEntry entry in conversation.dialogueEntries)
            {
                if (string.Equals(Field.LookupValue(entry.fields, ArticyIdFieldTitle), articyId)) return entry;
            }
            return null;
        }

        private DialogueEntry FindDialogueEntryByArticyId(string articyId)
        {
            if (entriesByArticyId.ContainsKey(articyId))
            {
                var list = entriesByArticyId[articyId];
                if (list.Count > 0) return list[0];
            }
            return null;
        }

        private List<DialogueEntry> FindAllDialogueEntriesByArticyId(string articyId)
        {
            if (entriesByArticyId.ContainsKey(articyId))
            {
                return entriesByArticyId[articyId];
            }
            return new List<DialogueEntry>();
        }

        private ArticyData.FlowFragment FindFlowFragment(string articyId)
        {
            foreach (ArticyData.FlowFragment articyFlowFragment in articyData.flowFragments.Values)
            {
                if (prefs.ConversionSettings.GetConversionSetting(articyFlowFragment.id).Include &&
                    string.Equals(articyFlowFragment.id, articyId))
                    return articyFlowFragment;
            }
            return null;
        }

        private Actor FindActorByArticyId(string articyId)
        {
            foreach (Actor actor in database.actors)
            {
                if (string.Equals(actor.LookupValue(ArticyIdFieldTitle), articyId)) return actor;
            }
            return null;
        }

        private Actor FindActorByTechnicalName(string technicalName)
        {
            foreach (Actor actor in database.actors)
            {
                if (string.Equals(actor.LookupValue(ArticyTechnicalNameFieldTitle), technicalName)) return actor;
            }
            return null;
        }

        private Actor FindActorByDisplayName(string displayName)
        {
            foreach (Actor actor in database.actors)
            {
                if (string.Equals(actor.Name, displayName)) return actor;
            }
            return null;
        }

        private int FindActorIdFromArticyDialogue(ArticyData.Dialogue articyDialogue, int index, int defaultActorID)
        {
            Actor actor = null;
            if (0 <= index && index < articyDialogue.references.Count)
            {
                actor = FindActorByArticyId(articyDialogue.references[index]);
            }
            return (actor != null) ? actor.id : (prefs.UseDefaultActorsIfNoneAssignedToDialogue ? defaultActorID : -1);
        }

        private void SplitPipesIntoEntries()
        {
            foreach (var conversation in database.conversations)
            {
                conversation.SplitPipesIntoEntries();
            }
        }

        private void SortAllLinksByPosition() // articy orders links by Y position.
        {
            foreach (var conversation in database.conversations)
            {
                SortLinksByPosition(conversation);
            }
        }

        private void SortLinksByPosition(Conversation conversation)
        {
            // Sort by each element's Y position:
            foreach (var entry in conversation.dialogueEntries)
            {
                entry.outgoingLinks.Sort(
                    delegate (Link A, Link B)
                    {
                        if (A.destinationConversationID != B.destinationConversationID) //return 0; // Only sort links in same conversation.
                        {
                            // Changed: Now sort cross-conversation links. 
                            // Keeping separate block in case this causes and issue and needs to be reverted.
                            var destA = database.GetDialogueEntry(A);
                            var destB = database.GetDialogueEntry(B);
                            if (destA == null || destB == null)
                            {
                                Debug.LogWarning("Dialogue System: Unexpected error sorting links by position. destA=" +
                                    ((destA == null) ? "null" : destA.ToString()) + " (" + A.destinationConversationID + ":" + A.destinationDialogueID + "), destB=" +
                                    ((destB == null) ? "null" : destB.ToString()) + " (" + B.destinationConversationID + ":" + B.destinationDialogueID + ") in conversation '" +
                                    conversation.Title + "' entry " + entry.id + ".");
                            }
                            return (destA == null || destB == null)
                                ? A.destinationDialogueID.CompareTo(B.destinationDialogueID)
                                    : destA.canvasRect.y.CompareTo(destB.canvasRect.y);
                        }
                        else
                        {
                            var destA = conversation.GetDialogueEntry(A.destinationDialogueID);
                            var destB = conversation.GetDialogueEntry(B.destinationDialogueID);
                            if (destA == null || destB == null)
                            {
                                Debug.LogWarning("Dialogue System: Unexpected error sorting links by position. destA=" +
                                    ((destA == null) ? "null" : destA.ToString()) + " (" + A.destinationConversationID + ":" + A.destinationDialogueID + "), destB=" +
                                    ((destB == null) ? "null" : destB.ToString()) + " (" + B.destinationConversationID + ":" + B.destinationDialogueID + ") in conversation '" +
                                    conversation.Title + "' entry " + entry.id + ".");
                            }
                            return (destA == null || destB == null)
                                ? A.destinationDialogueID.CompareTo(B.destinationDialogueID)
                                    : destA.canvasRect.y.CompareTo(destB.canvasRect.y);
                        }
                    }
                );
            }
            // Reset position because articy's positions don't necessarily map well onto the Dialogue Editor's canvas.
            foreach (var entry in conversation.dialogueEntries)
            {
                entry.canvasRect = new Rect(0, 0, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
            }
        }

        /// <summary>
        /// If a dialogue fragment has an arrow to the dialogue's endpoint, redirect it to the dialogue's first external link.
        /// If the dialogue doesn't have an external link, remove the arrow (link).
        /// </summary>
        private void RedirectLinkbacksToStartToLinkOutFromStart()
        {
            foreach (var conversation in database.conversations)
            {
                var startEntry = conversation.GetFirstDialogueEntry();
                if (startEntry == null) continue;
                var firstExternalLink = startEntry.outgoingLinks.Find(x => x.destinationConversationID != conversation.id);
                if (firstExternalLink != null) startEntry.outgoingLinks.Remove(firstExternalLink);
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (entry == startEntry) continue;
                    for (int i = entry.outgoingLinks.Count - 1; i >= 0; i--)
                    {
                        var link = entry.outgoingLinks[i];
                        if (link.destinationConversationID == conversation.id && link.destinationDialogueID == startEntry.id)
                        {
                            if (firstExternalLink == null)
                            {
                                entry.outgoingLinks.RemoveAt(i);
                            }
                            else
                            {
                                link.destinationConversationID = firstExternalLink.destinationConversationID;
                                link.destinationDialogueID = firstExternalLink.destinationDialogueID;
                            }
                        }
                    }
                }
            }
        }

        private bool DoesEntryLinkOutsideConversation(DialogueEntry entry)
        {
            if (entry == null) return false;
            foreach (var link in entry.outgoingLinks)
            {
                if (link.destinationConversationID != entry.conversationID) return true;
            }
            return false;
        }

        private void ConvertVoiceOverProperties()
        {
            foreach (var conversation in database.conversations)
            {
                foreach (var entry in conversation.dialogueEntries)
                {
                    ConvertVoiceOverProperty(entry);
                }
            }
        }

        private void ConvertVoiceOverProperty(DialogueEntry entry)
        {
            if (entry == null) return;
            var voiceOverPropertyField = Field.Lookup(entry.fields, prefs.VoiceOverProperty);
            if (voiceOverPropertyField == null) return;
            var assetID = voiceOverPropertyField.value;
            var asset = articyData.assets.ContainsKey(assetID) ? articyData.assets[assetID] : null;
            if (asset == null)
            {
                Debug.LogWarning("Dialogue System: Can't find voice-over asset with ID " + assetID + " for dialogue entry [" + entry.conversationID + ":" + entry.id + "]: '" + entry.currentDialogueText + "'.");
                return;
            }
            entry.fields.Remove(voiceOverPropertyField);
            entry.fields.Add(new Field(DialogueDatabase.VoiceOverFileFieldName, Path.GetFileNameWithoutExtension(asset.assetFilename), FieldType.Text));
        }

        private void FindPortraitTextureInResources(Actor actor)
        {
            if (actor == null || actor.portrait != null) return;
            string textureName = actor.textureName;
            if (!string.IsNullOrEmpty(textureName))
            {
                actor.portrait = LoadTexture(textureName);
            }

            // Alternate portraits:
            var s = actor.LookupValue("SUBTABLE__AlternatePortraits");
            if (!string.IsNullOrEmpty(s))
            {
                var alternatePortraitIDs = s.Split(';');
                foreach (var alternatePortraitID in alternatePortraitIDs)
                {
                    if (articyData.assets.ContainsKey(alternatePortraitID))
                    {
                        var portrait = LoadTexture(articyData.assets[alternatePortraitID].displayName.DefaultText);
                        if (portrait != null) actor.alternatePortraits.Add(portrait);
                    }
                }
            }
        }

        private Texture2D LoadTexture(string originalPath)
        {
            string filename = Path.GetFileNameWithoutExtension(originalPath).Replace('\\', '/');
            if (Application.isPlaying)
            {
                return DialogueManager.LoadAsset(filename, typeof(Texture2D)) as Texture2D;
            }
            else
            {
                return Resources.Load(filename, typeof(Texture2D)) as Texture2D;
            }
        }

        #endregion

        #region Em Var Set

        private void ConvertEmVarSet()
        {
            for (int i = 0; i < DialogueDatabase.NumEmphasisSettings; i++)
            {
                ConvertEmVars(prefs.emVarSet.emVars[i], database.emphasisSettings[i]);
            }
        }

        private void ConvertEmVars(ArticyEmVars emVars, EmphasisSetting emSetting)
        {
            if (emVars == null || emSetting == null) return;
            var colorVar = GetEmVar(emVars.color);
            var boldVar = GetEmVar(emVars.bold);
            var italicVar = GetEmVar(emVars.italic);
            var underlineVar = GetEmVar(emVars.underline);
            emSetting.color = (colorVar != null) ? Tools.WebColor(colorVar.InitialValue) : Color.white;
            emSetting.bold = (boldVar != null) ? boldVar.InitialBoolValue : false;
            emSetting.italic = (italicVar != null) ? italicVar.InitialBoolValue : false;
            emSetting.underline = (underlineVar != null) ? underlineVar.InitialBoolValue : false;
        }

        private Variable GetEmVar(string variableName)
        {
            return string.IsNullOrEmpty(variableName) ? null : database.GetVariable(variableName);
        }

        #endregion

    }

}
#endif
