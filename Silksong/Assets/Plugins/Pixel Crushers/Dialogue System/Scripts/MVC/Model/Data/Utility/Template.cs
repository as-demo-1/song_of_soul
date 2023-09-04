// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This class defines the template that the Dialogue Database Editor will use when creating
    /// new dialogue database assets such as actors and conversations. The Dialogue Database Editor
    /// stores a copy of the template in EditorPrefs using the TemplateTools class. The equivalent 
    /// in Chat Mapper is Project Preferences.
    /// </summary>
    [System.Serializable]
    public class Template
    {

        public bool treatItemsAsQuests = true;

        public List<Field> actorFields = new List<Field>();
        public List<Field> itemFields = new List<Field>();
        public List<Field> questFields = new List<Field>();
        public List<Field> locationFields = new List<Field>();
        public List<Field> variableFields = new List<Field>();
        public List<Field> conversationFields = new List<Field>();
        public List<Field> dialogueEntryFields = new List<Field>();

        public List<string> actorPrimaryFieldTitles = new List<string>();
        public List<string> itemPrimaryFieldTitles = new List<string>();
        public List<string> questPrimaryFieldTitles = new List<string>();
        public List<string> locationPrimaryFieldTitles = new List<string>();
        public List<string> variablePrimaryFieldTitles = new List<string>();
        public List<string> conversationPrimaryFieldTitles = new List<string>();
        public List<string> dialogueEntryPrimaryFieldTitles = new List<string>();

        public Color npcLineColor = Color.red;
        public Color pcLineColor = Color.blue;
        public Color repeatLineColor = Color.gray;

        public static Template FromDefault()
        {
            Template template = new Template();
            template.actorFields.Clear();
            template.actorFields.Add(new Field("Name", string.Empty, FieldType.Text));
            template.actorFields.Add(new Field("Pictures", "[]", FieldType.Files));
            template.actorFields.Add(new Field("Description", string.Empty, FieldType.Text));
            template.actorFields.Add(new Field("IsPlayer", "False", FieldType.Boolean));

            template.itemFields.Clear();
            template.itemFields.Add(new Field("Name", string.Empty, FieldType.Text));
            template.itemFields.Add(new Field("Pictures", "[]", FieldType.Files));
            template.itemFields.Add(new Field("Description", string.Empty, FieldType.Text));
            template.itemFields.Add(new Field("Is Item", "True", FieldType.Boolean));

            template.questFields.Clear();
            template.questFields.Add(new Field("Name", string.Empty, FieldType.Text));
            template.questFields.Add(new Field("Pictures", "[]", FieldType.Files));
            template.questFields.Add(new Field("Description", string.Empty, FieldType.Text));
            template.questFields.Add(new Field("Success Description", string.Empty, FieldType.Text));
            template.questFields.Add(new Field("Failure Description", string.Empty, FieldType.Text));
            template.questFields.Add(new Field("State", "unassigned", FieldType.Text));
            template.questFields.Add(new Field("Is Item", "False", FieldType.Boolean));

            template.locationFields.Clear();
            template.locationFields.Add(new Field("Name", string.Empty, FieldType.Text));
            //template.locationFields.Add(new Field("Pictures", "[]", FieldType.Files));
            template.locationFields.Add(new Field("Description", string.Empty, FieldType.Text));

            template.variableFields.Add(new Field("Name", string.Empty, FieldType.Text));
            template.variableFields.Add(new Field("Initial Value", string.Empty, FieldType.Text));
            template.variableFields.Add(new Field("Description", string.Empty, FieldType.Text));

            template.conversationFields.Add(new Field("Title", string.Empty, FieldType.Text));
            //template.conversationFields.Add(new Field("Pictures", "[]", FieldType.Files));
            template.conversationFields.Add(new Field("Description", string.Empty, FieldType.Text));
            template.conversationFields.Add(new Field("Actor", "0", FieldType.Actor));
            template.conversationFields.Add(new Field("Conversant", "0", FieldType.Actor));

            template.dialogueEntryFields.Add(new Field("Title", string.Empty, FieldType.Text));
            //template.dialogueEntryFields.Add(new Field("Pictures", "[]", FieldType.Files));
            template.dialogueEntryFields.Add(new Field("Description", string.Empty, FieldType.Text));
            template.dialogueEntryFields.Add(new Field("Actor", string.Empty, FieldType.Actor));
            template.dialogueEntryFields.Add(new Field("Conversant", string.Empty, FieldType.Actor));
            template.dialogueEntryFields.Add(new Field("Menu Text", string.Empty, FieldType.Text));
            template.dialogueEntryFields.Add(new Field("Dialogue Text", string.Empty, FieldType.Text));
            //template.dialogueEntryFields.Add(new Field("Parenthetical", string.Empty, FieldType.Text));
            //template.dialogueEntryFields.Add(new Field("Audio Files", "[]", FieldType.Files));
            //template.dialogueEntryFields.Add(new Field("Video File", string.Empty, FieldType.Text));
            template.dialogueEntryFields.Add(new Field("Sequence", string.Empty, FieldType.Text));

            // Note: 2021-04-10: Removed default Chat Mapper fields from DS template. No need for them to
            // take space unless you're using Chat Mapper, in which case the ChatMapperExporter will
            // automatically re-add any missing mandatory fields.

            return template;
        }

        /// <summary>
        /// Creates a new actor with the fields defined in the template.
        /// </summary>
        public Actor CreateActor(int id, string name, bool isPlayer)
        {
            Actor actor = new Actor();
            actor.fields = CreateFields(actorFields);
            actor.id = id;
            actor.Name = name;
            actor.IsPlayer = isPlayer;
            return actor;
        }

        /// <summary>
        /// Creates a new item with the fields defined in the template.
        /// </summary>
        public Item CreateItem(int id, string name)
        {
            Item item = new Item();
            item.id = id;
            item.fields = CreateFields(itemFields);
            item.Name = name;
            return item;
        }

        /// <summary>
        /// Creates a new quest with the fields defined in the template.
        /// </summary>
        public Item CreateQuest(int id, string name)
        {
            Item item = new Item();
            item.id = id;
            item.fields = CreateFields(questFields);
            item.Name = name;
            return item;
        }

        /// <summary>
        /// Creates a new location with the fields defined in the template.
        /// </summary>
        public Location CreateLocation(int id, string name)
        {
            Location location = new Location();
            location.id = id;
            location.fields = CreateFields(locationFields);
            location.Name = name;
            return location;
        }

        /// <summary>
        /// Creates a new variable (Text type) with the fields defined in the template.
        /// </summary>
        public Variable CreateVariable(int id, string name, string value)
        {
            Variable variable = new Variable();
            variable.fields = CreateFields(variableFields);
            variable.id = id;
            variable.Name = name;
            variable.InitialValue = value;
            return variable;
        }

        /// <summary>
        /// Creates a new variable with the fields defined in the template.
        /// </summary>
        public Variable CreateVariable(int id, string name, string value, FieldType type)
        {
            Variable variable = new Variable();
            variable.fields = CreateFields(variableFields);
            variable.id = id;
            variable.Name = name;
            variable.InitialValue = value;
            variable.Type = type;
            return variable;
        }

        /// <summary>
        /// Creates a new conversation with the fields defined in the template.
        /// </summary>
        public Conversation CreateConversation(int id, string title)
        {
            Conversation conversation = new Conversation();
            conversation.id = id;
            conversation.fields = CreateFields(conversationFields);
            conversation.Title = title;
            return conversation;
        }

        /// <summary>
        /// Creates a new dialogue entry with the fields defined in the template.
        /// </summary>
        public DialogueEntry CreateDialogueEntry(int id, int conversationID, string title)
        {
            DialogueEntry entry = new DialogueEntry();
            entry.fields = CreateFields(dialogueEntryFields);
            entry.id = id;
            entry.conversationID = conversationID;
            entry.Title = title;
            return entry;
        }

        public List<Field> CreateFields(List<Field> templateFields)
        {
            List<Field> fields = new List<Field>();
            foreach (var templateField in templateFields)
            {
                fields.Add(new Field(templateField.title, templateField.value, templateField.type, templateField.typeString));
            }
            return fields;
        }

        /// <returns>A value 1 higher than the highest actor ID in the database.</returns>
        public int GetNextActorID(DialogueDatabase database)
        {
            return (database != null) ? GetNextAssetID<Actor>(database.baseID, database.actors) : 0;
        }

        /// <returns>A value 1 higher than the highest item/quest ID in the database.</returns>
        public int GetNextItemID(DialogueDatabase database)
        {
            return (database != null) ? GetNextAssetID<Item>(database.baseID, database.items) : 0;
        }

        /// <returns>A value 1 higher than the highest item/quest ID in the database.</returns>
        public int GetNextQuestID(DialogueDatabase database)
        {
            return GetNextItemID(database);
        }

        /// <returns>A value 1 higher than the highest location ID in the database.</returns>
        public int GetNextLocationID(DialogueDatabase database)
        {
            return (database != null) ? GetNextAssetID<Location>(database.baseID, database.locations) : 0;
        }

        /// <returns>A value 1 higher than the highest variable ID in the database.</returns>
        public int GetNextVariableID(DialogueDatabase database)
        {
            return (database != null) ? GetNextAssetID<Variable>(database.baseID, database.variables) : 0;
        }

        /// <returns>A value 1 higher than the highest conversation ID in the database.</returns>
        public int GetNextConversationID(DialogueDatabase database)
        {
            return (database != null) ? GetNextAssetID<Conversation>(database.baseID, database.conversations) : 0;
        }

        private int GetNextAssetID<T>(int baseID, List<T> assets) where T : Asset
        {
            int highest = baseID - 1;
            for (int i = 0; i < assets.Count; i++)
            {
                highest = Mathf.Max(highest, assets[i].id);
            }
            return highest + 1;
        }

        /// <returns>A value 1 higher than the highest dialogue entry ID in the conversation.</returns>
        public int GetNextDialogueEntryID(Conversation conversation)
        {
            if (conversation == null) return 0;
            int highest = -1;
            for (int i = 0; i < conversation.dialogueEntries.Count; i++)
            {
                highest = Mathf.Max(highest, conversation.dialogueEntries[i].id);
            }
            return highest + 1;
        }
    }
}
