using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem.ChatMapper;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This part of the Dialogue Editor window contains the Chat Mapper export code.
	/// </summary>
	public static class ChatMapperExporter {

		public static int maxEntryCount = 0;

		public static void Export(DialogueDatabase database, string filename, bool exportActors, bool exportItems, bool exportLocations, bool exportVariables, bool exportConversations, bool includeCanvasRect = false) {
			ChatMapperProject cmp = DatabaseToChatMapperProject(database, exportActors, exportItems, exportLocations, exportVariables, exportConversations, includeCanvasRect);
			SaveChatMapperProject(filename, cmp);
		}

		private static void SaveChatMapperProject(string filename, ChatMapperProject cmp) {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ChatMapperProject));
			StreamWriter streamWriter = new StreamWriter(filename, false, System.Text.Encoding.Unicode);
			xmlSerializer.Serialize(streamWriter, cmp);
			streamWriter.Close();
		}

		private static ChatMapperProject DatabaseToChatMapperProject(DialogueDatabase database, bool exportActors, bool exportItems, bool exportLocations, bool exportVariables, bool exportConversations, bool includeCanvasRect) {
			maxEntryCount = 0;
			ChatMapperProject cmp = new ChatMapperProject();
			cmp.Title = database.name;
			cmp.Version = "1.5.1.0"; // The version of Chat Mapper XML format that imports properly.
			cmp.Author = database.author;
			cmp.EmphasisColor1 = ColorToCmpStyle(database.emphasisSettings[0].color);
			cmp.EmphasisStyle1 = EmphasisToCmpStyle(database.emphasisSettings[0]);
			cmp.EmphasisColor2 = ColorToCmpStyle(database.emphasisSettings[1].color);
			cmp.EmphasisStyle2 = EmphasisToCmpStyle(database.emphasisSettings[1]);
			cmp.EmphasisColor3 = ColorToCmpStyle(database.emphasisSettings[2].color);
			cmp.EmphasisStyle3 = EmphasisToCmpStyle(database.emphasisSettings[2]);
			cmp.EmphasisColor4 = ColorToCmpStyle(database.emphasisSettings[3].color);
			cmp.EmphasisStyle4 = EmphasisToCmpStyle(database.emphasisSettings[3]);
			cmp.Description = database.description;
			cmp.UserScript = database.globalUserScript;
			cmp.Assets = AssetsToCmp(database, exportActors, exportItems, exportLocations, exportVariables, exportConversations, includeCanvasRect);
			return cmp;
		}

		private static string ColorToCmpStyle(Color color ) {
			return Tools.ToWebColor(color).Substring(0, 7);
		}

		private static string EmphasisToCmpStyle(EmphasisSetting es) {
			return string.Format("{0}{1}{2}", (es.bold ? 'b' : '-'), (es.italic ? 'i' : '-'), (es.underline ? 'u' : '-'));
		}

		private static PixelCrushers.DialogueSystem.ChatMapper.Assets AssetsToCmp(DialogueDatabase database, bool exportActors, bool exportItems, bool exportLocations, bool exportVariables, bool exportConversations, bool includeCanvasRect) {
            PixelCrushers.DialogueSystem.ChatMapper.Assets assets = new PixelCrushers.DialogueSystem.ChatMapper.Assets();
			if (exportActors) assets.Actors = ActorsToCmp(database);
			if (exportItems) assets.Items = ItemsToCmp(database);
			if (exportLocations) assets.Locations = LocationsToCmp(database);
			if (exportVariables) assets.UserVariables = VariablesToCmp(database);
			if (exportConversations) assets.Conversations = ConversationsToCmp(database, includeCanvasRect);
			AssignConnectors(assets.Conversations);
			return assets;
		}

		//============================================================================================================
		
		private static List<ChatMapper.Actor> ActorsToCmp(DialogueDatabase database) {
			List<ChatMapper.Actor> cmpActors = new List<ChatMapper.Actor>();
			database.actors.ForEach(actor => cmpActors.Add(ActorToCmp(actor)));
			return cmpActors;
		}

		private static ChatMapper.Actor ActorToCmp(DialogueSystem.Actor actor) {
			ChatMapper.Actor cmpActor = new ChatMapper.Actor();
			cmpActor.ID = actor.id;
			cmpActor.Fields = FieldsToCmp(actor.fields);
			AddRequiredActorFields(cmpActor.Fields);
			return cmpActor;
		}

		private static void AddRequiredActorFields(List<ChatMapper.Field> cmpFields) {
			RequireField(cmpFields, 0, "Name", "Text", "New Actor", "A name to reference this actor.");
			RequireField(cmpFields, 1, "Pictures", "Files", "[]", "A collection of images that represent this actor.");
			RequireField(cmpFields, 2, "Description", "Text", "", "A short description of what makes this actor unique.");
			RequireField(cmpFields, 3, "IsPlayer", "Boolean", "False", "Mark this box if this actor is the main player character.");
		}

		//============================================================================================================
		
		private static List<ChatMapper.Item> ItemsToCmp(DialogueDatabase database) {
			List<ChatMapper.Item> cmpItems = new List<ChatMapper.Item>();
			database.items.ForEach(item => cmpItems.Add(ItemToCmp(item)));
			return cmpItems;
		}
		
		private static ChatMapper.Item ItemToCmp(DialogueSystem.Item item) {
			ChatMapper.Item cmpItem = new ChatMapper.Item();
			cmpItem.ID = item.id;
			cmpItem.Fields = FieldsToCmp(item.fields);
			AddRequiredItemFields(cmpItem.Fields);
			Field entryCount = Field.Lookup(item.fields, "Entry Count");
			if (entryCount != null) maxEntryCount = Mathf.Max(maxEntryCount, Tools.StringToInt(entryCount.value));
			return cmpItem;
		}
		
		private static void AddRequiredItemFields(List<ChatMapper.Field> cmpFields) {
			RequireField(cmpFields, 0, "Name", "Text", "New Item", "A name to reference this item.");
			RequireField(cmpFields, 1, "Pictures", "Files", "[]", "A collection of images that represent this item.");
			RequireField(cmpFields, 2, "Description", "Text", "", "A short description of what this item looks like.");
		}

		//============================================================================================================
		
		private static List<ChatMapper.Location> LocationsToCmp(DialogueDatabase database) {
			List<ChatMapper.Location> cmpLocations = new List<ChatMapper.Location>();
			database.locations.ForEach(location => cmpLocations.Add(LocationToCmp(location)));
			return cmpLocations;
		}
		
		private static ChatMapper.Location LocationToCmp(DialogueSystem.Location location) {
			ChatMapper.Location cmpLocation = new ChatMapper.Location();
			cmpLocation.ID = location.id;
			cmpLocation.Fields = FieldsToCmp(location.fields);
			AddRequiredLocationFields(cmpLocation.Fields);
			return cmpLocation;
		}
		
		private static void AddRequiredLocationFields(List<ChatMapper.Field> cmpFields) {
			RequireField(cmpFields, 0, "Name", "Text", "New Location", "A name to reference this location.");
			RequireField(cmpFields, 1, "Pictures", "Files", "[]", "A collection of images that represent this location.");
			RequireField(cmpFields, 2, "Description", "Text", "", "A short description of what this location looks like.");
		}
		
		//============================================================================================================
		
		private static List<ChatMapper.UserVariable> VariablesToCmp(DialogueDatabase database) {
			List<ChatMapper.UserVariable> cmpVariables = new List<ChatMapper.UserVariable>();
			database.variables.ForEach(variable => cmpVariables.Add(VariableToCmp(variable)));
			return cmpVariables;
		}
		
		private static ChatMapper.UserVariable VariableToCmp(DialogueSystem.Variable variable) {
			ChatMapper.UserVariable cmpUserVariable = new ChatMapper.UserVariable();
			cmpUserVariable.Fields = FieldsToCmp(variable.fields);
			AddRequiredUserVariableFields(cmpUserVariable.Fields);
			return cmpUserVariable;
		}
		
		private static void AddRequiredUserVariableFields(List<ChatMapper.Field> cmpFields) {
			RequireField(cmpFields, 0, "Name", "Text", "", "The name of this variable.");
			RequireField(cmpFields, 1, "Initial Value", "Text", "", "The default initial value of this variable.");
			RequireField(cmpFields, 2, "Description", "Text", "", "A short description of what this variable is for.");
		}
		
		//============================================================================================================
		
		private static List<ChatMapper.Conversation> ConversationsToCmp(DialogueDatabase database, bool includeCanvasRect) {
			List<ChatMapper.Conversation> cmpConversations = new List<ChatMapper.Conversation>();
			database.conversations.ForEach(conversation => cmpConversations.Add(ConversationToCmp(conversation, includeCanvasRect)));
			return cmpConversations;
		}
		
		private static ChatMapper.Conversation ConversationToCmp(DialogueSystem.Conversation conversation, bool includeCanvasRect) {
			ChatMapper.Conversation cmpConversation = new ChatMapper.Conversation();
			cmpConversation.ID = conversation.id;
			cmpConversation.NodeColor = string.IsNullOrEmpty(conversation.nodeColor) ? "Red" : conversation.nodeColor;
			cmpConversation.LockedMode = "Unlocked";
			cmpConversation.Fields = FieldsToCmp(conversation.fields);
			AddRequiredConversationFields(cmpConversation.Fields);
			RequireField(cmpConversation.Fields, conversation.fields.Count, "Overrides", "Text", JsonUtility.ToJson(conversation.overrideSettings), "Display Settings overrides.");
			cmpConversation.DialogEntries = DialogEntriesToCmp(conversation.dialogueEntries, includeCanvasRect);
			return cmpConversation;
		}
		
		private static void AddRequiredConversationFields(List<ChatMapper.Field> cmpFields) {
			RequireField(cmpFields, 0, "Title", "Text", "New Conversation", "The title of this conversation.");
			RequireField(cmpFields, 1, "Pictures", "Files", "[]", "A collection of image files that represent this conversation. The first is used as the background image.");
			RequireField(cmpFields, 2, "Description", "Text", "", "A short description of what happens in this conversation.");
			RequireField(cmpFields, 3, "Actor", "Actor", "", "The actor who is doing most of the talking, or initiates this conversation.");
			RequireField(cmpFields, 4, "Conversant", "Actor", "", "The actor who is approached or is mostly talked to.");
		}
		
		//============================================================================================================
		
		private static List<ChatMapper.DialogEntry> DialogEntriesToCmp(List<DialogueSystem.DialogueEntry> entries, bool includeCanvasRect) {
			List<ChatMapper.DialogEntry> cmpEntries = new List<DialogEntry>();
			entries.ForEach(entry => cmpEntries.Add(DialogEntryToCmp(entry, includeCanvasRect)));
			return cmpEntries;
		}

		private static ChatMapper.DialogEntry DialogEntryToCmp(DialogueSystem.DialogueEntry entry, bool includeCanvasRect) {
			ChatMapper.DialogEntry cmpEntry = new DialogEntry();
			cmpEntry.ID = entry.id;
			cmpEntry.IsRoot = entry.isRoot || ((entry.id == 0) && string.Equals(entry.Title, "START"));
			cmpEntry.IsGroup = entry.isGroup;
			cmpEntry.NodeColor = string.IsNullOrEmpty(entry.nodeColor) ? "White" : entry.nodeColor;
			cmpEntry.DelaySimStatus = entry.delaySimStatus;
			cmpEntry.FalseCondtionAction = GetValidFalseConditionAction(entry.falseConditionAction);
			cmpEntry.ConditionPriority = entry.conditionPriority.ToString();
			cmpEntry.Fields = FieldsToCmp(entry.fields);
			AddRequiredDialogEntryFields(cmpEntry.Fields);
			cmpEntry.Fields.ForEach(cmpField => { if (cmpField.Title.StartsWith("Dialogue Text")) cmpField.Type = "Localization"; });
            if (includeCanvasRect) AddCanvasRectField(entry, cmpEntry);
			cmpEntry.OutgoingLinks = LinksToCmp(entry.outgoingLinks);
			foreach (var link in cmpEntry.OutgoingLinks) {
				link.OriginConvoID = entry.conversationID;
				link.OriginDialogID = entry.id;
			}
			cmpEntry.ConditionsString = entry.conditionsString;
			cmpEntry.UserScript = entry.userScript;
			return cmpEntry;
		}

        private static void AddCanvasRectField(DialogueSystem.DialogueEntry entry, ChatMapper.DialogEntry cmpEntry)
        {
            var canvasRectField = cmpEntry.Fields.Find(f => string.Equals(f.Title, "canvasRect"));
            if (canvasRectField == null)
            {
                canvasRectField = new ChatMapper.Field();
                canvasRectField.Title = "canvasRect";
                cmpEntry.Fields.Add(canvasRectField);
            }
            canvasRectField.Value = string.Format("{0};{1}", entry.canvasRect.x, entry.canvasRect.y);
        }

        private static string GetValidFalseConditionAction(string action) {
			return (string.Equals(action, "Block") || string.Equals(action, "Passthrough")) ? action : "Block";
		}

		private static void AddRequiredDialogEntryFields(List<ChatMapper.Field> cmpFields) {
			RequireField(cmpFields, 0, "Title", "Text", "New Dialogue", "The title of this dialogue.");
			RequireField(cmpFields, 1, "Pictures", "Files", "[]", "A collection of image files that represent this dialogue.");
			RequireField(cmpFields, 2, "Description", "Text", "", "A short description of what happens in this dialogue.");
			RequireField(cmpFields, 3, "Actor", "Actor", "", "The actor who is talking.");
			RequireField(cmpFields, 4, "Conversant", "Actor", "", "The actor who is listening.");
			RequireField(cmpFields, 5, "Menu Text", "Text", "", "The text that is displayd to the player in a list as a dialogue option.");
			RequireField(cmpFields, 6, "Dialogue Text", "Localization", "", "The text that is spoken by the actor.");
			RequireField(cmpFields, 7, "Parenthetical", "Text", "", "A description of what is going on during this dialogue.");
			RequireField(cmpFields, 8, "Audio Files", "Files", "[]", "A collection of audio files that should be played during this dialogue.");
			RequireField(cmpFields, 9, "Video File", "Text", "", "A WMV video file to be played for this dialogue.");
		}

		private static List<ChatMapper.Link> LinksToCmp(List<DialogueSystem.Link> links) {
			List<ChatMapper.Link> cmpLinks = new List<ChatMapper.Link>();
			links.ForEach(link => cmpLinks.Add(LinkToCmp(link)));
			return cmpLinks;
		}

		private static ChatMapper.Link LinkToCmp(DialogueSystem.Link link) {
			ChatMapper.Link cmpLink = new ChatMapper.Link();
			cmpLink.ConversationID = link.originConversationID;
			cmpLink.OriginConvoID = link.originConversationID;
			cmpLink.DestinationConvoID = link.destinationConversationID;
			cmpLink.OriginDialogID = link.originDialogueID;
			cmpLink.DestinationDialogID = link.destinationDialogueID;
			cmpLink.IsConnector = link.isConnector;
			return cmpLink;
		}

		//============================================================================================================
		
		private static List<ChatMapper.Field> FieldsToCmp(List<DialogueSystem.Field> fields) {
			List<ChatMapper.Field> cmpFields = new List<ChatMapper.Field>();
			fields.ForEach(field => cmpFields.Add(FieldToCmp(field)));
			return cmpFields;
		}

		private static ChatMapper.Field FieldToCmp(DialogueSystem.Field field) {
			return NewCmpField(field.title, field.type.ToString(), field.value, string.Empty);
		}

		private static ChatMapper.Field NewCmpField(string title, string type, string value, string hint) {
			ChatMapper.Field cmpField = new ChatMapper.Field();
			cmpField.Type = type;
			cmpField.Hint = string.Empty;
			cmpField.Title = title;
			cmpField.Value = value;
			return cmpField;
		}

		private static void RequireField(List<ChatMapper.Field> cmpFields, int position, string title, string type, string value, string hint) {
			if (!ContainsCmpField(cmpFields, title)) cmpFields.Add(NewCmpField(title, type, value, hint));
			ChatMapper.Field cmpField = GetCmpField(cmpFields, title);
			if (cmpField == null) return;
			cmpFields.Remove(cmpField);
			cmpFields.Insert(position, cmpField);
		}

		private static bool ContainsCmpField(List<ChatMapper.Field> cmpFields, string title) {
			return GetCmpField(cmpFields, title) != null;
		}

		private static ChatMapper.Field GetCmpField(List<ChatMapper.Field> cmpFields, string title) {
			return cmpFields.Find(field => string.Equals(field.Title, title));
		}

		//============================================================================================================

		/*
		 * --- This version assigned the non-connector to the first entry in the database.
		 * --- It doesn't work with Chat Mapper because the first link in the *tree* must be the non-connector:
		private static void AssignConnectors(List<PixelCrushers.DialogueSystem.ChatMapper.Conversation> conversations) {
			HashSet<string> alreadyConnected = new HashSet<string>();
			foreach (var conversation in conversations) {
				foreach (var entry in conversation.DialogEntries) {
					foreach (var link in entry.OutgoingLinks) {
						if (link.OriginConvoID == link.DestinationConvoID) {
							string destination = string.Format("{0}.{1}", link.DestinationConvoID, link.DestinationDialogID);
							if (alreadyConnected.Contains(destination)) {
								link.IsConnector = true;
							} else {
								link.IsConnector = false;
								alreadyConnected.Add(destination);
							}
						}
					}
				}
			}
		}
		 */
		private static void AssignConnectors(List<PixelCrushers.DialogueSystem.ChatMapper.Conversation> conversations) {
			// Moved to dialogue editor ValidateDatabase().
		}

	}

}